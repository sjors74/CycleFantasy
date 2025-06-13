let eventId;
let etappes = [];
let isLoading = false;

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

document.addEventListener('DOMContentLoaded', function () {
    (async () => {

        toggleGlobalLoader(true);

        await loadConfig();

        const eventElement = document.getElementById("event-id");
        if (!eventElement) {
            console.warn("event.js: #event-id bestaat niet op deze pagina.");
            toggleGlobalLoader(false);
            return;
        }

        eventId = eventElement.dataset.eventId;
        localStorage.setItem("eventId", eventId);

        if (!eventId) {
            console.error("eventId niet gevonden in dataset van #event-id");
            toggleGlobalLoader(false);
            return;
        }
        await loadEtappes();

        let event = null;
        const cachedEvents = localStorage.getItem('events');

        if (cachedEvents) {
            const events = JSON.parse(cachedEvents);
            event = events.find(e => e.eventId == eventId);
            if (event) {
                console.log("Event geladen uit cache:", event.eventId);
            } else {
                console.warn(`Geen event gevonden met eventId ${eventId} in cache.`);
            }
        }

        if (!event) {
            // Event niet gevonden in cache, ophalen van API
            const eventRes = await fetch(`${API_BASE_URL}/api/Event/${eventId}`);
            if (!eventRes.ok) {
                console.error("Fout bij ophalen van event");
                toggleGlobalLoader(false);
                return;
            }
            event = await eventRes.json();
            console.log("Event geladen door api:", event);

            let updatedEvents = cachedEvents ? JSON.parse(cachedEvents) : [];
            updatedEvents = updatedEvents.filter(e => e.eventId != event.eventId);
            updatedEvents.push(event);
            localStorage.setItem('events', JSON.stringify(updatedEvents));
        }

        document.getElementById('eventName').textContent = event.eventName;
        document.getElementById('slogan').textContent = event.slogan;
        const options = { day: 'numeric', month: 'long', year: 'numeric' };
        document.getElementById('startDate').textContent = new Date(event.startDate).toLocaleDateString('nl-NL', options);
        document.getElementById('endDate').textContent = new Date(event.endDate).toLocaleDateString('nl-NL', options);

        if (event.deelnemers && event.deelnemers.length > 0) {
            renderDeelnemers(event.deelnemers, eventId);
            console.log("Deelnemers geladen");
        } else {
            document.getElementById("deelnemer-list").innerHTML = `<p class="text-muted p-3">Geen deelnemers gevonden.</p>`;
        }

        toggleGlobalLoader(false);
    })();
    function showPlaceholders() {
        const stepsContainer = document.getElementById("steps-container");
        stepsContainer.innerHTML = "";
        for (let i = 0; i < 5; i++) {
            const placeholder = document.createElement("div");
            placeholder.classList.add("step", "placeholder");
            stepsContainer.appendChild(placeholder);
        }
    }

    async function loadEtappes() {
        if (isLoading) return;
        isLoading = true;
        showPlaceholders();

        try {
            const eventsJson = localStorage.getItem('events');
            if (!eventsJson) {
                console.error("Geen events gevonden in localStorage");
                return;
            }
            const events = JSON.parse(eventsJson);
            const event = events.find(e => e.eventId == eventId);
            if (!event) {
                console.error("Event niet gevonden");
                return;
            }

            etappes = event.stages;
        } catch (error) {
            console.error("Error fetching etappes:", error);
        } finally {
            isLoading = false;
            renderEtappes();
        }
    }

    function renderEtappes() {

        const stepsContainer = document.getElementById("steps-container");
        stepsContainer.innerHTML = "";

        const completedStages = etappes.filter(etappe => etappe.hasResult).length;

        const progress = (completedStages / etappes.length) * 100;

        const progressBar = document.getElementById("progress-bar");
        progressBar.style.width = `${progress}%`;

        const fragment = document.createDocumentFragment();

        etappes.forEach((etappe) => {
            const step = document.createElement("div");
            step.classList.add("step");
            if (etappe.hasResult) {
                step.classList.add("completed");

                const link = document.createElement("a");
                link.href = `/Etappe?nummer=${etappe.stageNumber}&stageId=${etappe.stageId}`;
                link.textContent = etappe.stageNumber;
                step.appendChild(link);
            } else {
                step.textContent = etappe.stageNumber;
            }
            fragment.appendChild(step);
        });
        stepsContainer.appendChild(fragment);
    }

    function renderDeelnemers(deelnemers, eventId) {
        const list = document.getElementById("deelnemer-list");
        list.innerHTML = "";
        deelnemers
            .sort((a, b) => (b.punten || 0) - (a.punten || 0))
            .forEach(deelnemer => {
                const collapseId = `collapse-${deelnemer.id}`;
                const li = document.createElement("li");
                li.classList.add("list-group-item", "p-2");

                li.innerHTML = `
                                    <div class="row align-items-center"
                         data-bs-toggle="collapse"
                         data-bs-target="#${collapseId}"
                         aria-expanded="false"
                         aria-controls="${collapseId}"
                         style="cursor: pointer;"
                         data-deelnemer-id="${deelnemer.id}"
                         data-event-id="${eventId}">
                        <div class="col-md-4 text-uppercase">${deelnemer.poolNaam || "onbekende pool"}</div>
                        <div class="col-md-6 text-uppercase">${deelnemer.deelnemerNaam || "onbekende deelnemer"}</div>
                        <div class="col-md-2 text-end ms-auto">
                            <span class="fw-bold fs-4">${deelnemer.punten ?? 0}</span>
                        </div>
                    </div>
                    <div class="collapse mt-2" id="${collapseId}" data-bs-parent="#deelnemer-list">
                        <div class="card card-body details-content" data-loaded="false">
                        </div>
                    </div>
                `;
                list.appendChild(li);
            });
        initializeCollapseHandlers();
    }

    function initializeCollapseHandlers() {
        const collapseElements = document.querySelectorAll('.collapse');
        collapseElements.forEach(collapse => {
            collapse.addEventListener('show.bs.collapse', async function () {

                const targetId = `#${collapse.id}`;
                const triggerDiv = document.querySelector(`[data-bs-target="${targetId}"]`);

                if (!triggerDiv) return;

                const deelnemerId = triggerDiv.dataset.deelnemerId;
                const eventId = triggerDiv.dataset.eventId;
                const detailsDiv = collapse.querySelector('.details-content');

                if (!detailsDiv || detailsDiv.dataset.loaded === "true") return;

                toggleGlobalLoader(true);

                try {
                    const response = await fetch(`${API_BASE_URL}/api/Deelnemer/Picks/${deelnemerId}/event/${eventId}`);
                    if (!response.ok) throw new Error("Fout bij ophalen picks");

                    const data = await response.json();

                    detailsDiv.innerHTML = data.map(pick => {
                        const isOut = pick.outOfCompetition === true;
                        const rowStyle = isOut ? 'background-color: #eee; text-decoration: line-through;' : '';
                        return `
                            <div class="row mb-1" style="${rowStyle}">
                                <div class="col-md-1">
                                    <img src="${FLAGS_BASE_URL}/24x18/${pick.countryCode.toLowerCase()}.png" class="img-fluid" style="max-height: 40px;" />
                                </div>
                                <div class="col-md-4 fw-bold">${pick.competitorName}</div>
                                <div class="col-md-5 text-muted">${pick.competitorTeam}</div>
                                <div class="col-md-2 text-end">${pick.points}</div>
                            </div>
                        `;
                    }).join('');
                    detailsDiv.dataset.loaded = "true";
                } catch (err) {
                    detailsDiv.innerHTML = `<p class="text-danger">Details ophalen mislukt.</p>`;
                    console.error(err);
                } finally {
                    toggleGlobalLoader(false);
                }
            });
        });
    }
});