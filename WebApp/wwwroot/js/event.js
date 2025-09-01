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

        async function fetchWithRetry(url, retries = 3, delay = 1000) {
            for (let i = 0; i < retries; i++) {
                try {
                    const res = await fetch(url);
                    if (!res.ok) throw new Error(`Status ${res.status}`);
                    return await res.json();
                } catch (err) {
                    console.warn(`Poging ${i + 1} mislukt:`, err);
                    if (i < retries - 1) await sleep(delay);
                }
            }
            throw new Error("Alle pogingen mislukt voor " + url);
        }

        if (!event) {
            try {
                await fetch(`${API_BASE_URL}/config/ping`);
                await sleep(500);

                event = await fetchWithRetry(`${API_BASE_URL}/api/Event/${eventId}`, 3, 1000);
                console.log("Event geladen via API:", event);

                let updatedEvents = cachedEvents ? JSON.parse(cachedEvents) : [];
                updatedEvents = updatedEvents.filter(e => e.eventId != event.eventId);
                updatedEvents.push(event);
                localStorage.setItem('events', JSON.stringify(updatedEvents));

            } catch (err) {
                console.error("Kon event niet laden via API:", err);

                if (cachedEvents) {
                    const fallbackEvents = JSON.parse(cachedEvents);
                    event = fallbackEvents.find(e => e.eventId == eventId);

                    if (event) {
                        console.warn("Event geladen uit cache als fallback:", event);
                        showWarning("Eventinformatie is mogelijk verouderd.");
                    }
                }

                if (!event) {
                    document.body.innerHTML = `
                        <div class="container mt-5">
                            <h2 class="text-danger">Kon evenement niet laden</h2>
                            <p>Er is mogelijk een probleem met de server of je verbinding.</p>
                            <button onclick="location.reload()" class="btn btn-primary mt-3">Probeer opnieuw</button>
                        </div>
                    `;
                    toggleGlobalLoader(false);
                    return;
                }
            }
        }

        document.getElementById('eventName').textContent = event.eventName;
        document.getElementById('slogan').textContent = event.slogan;
        const options = { day: 'numeric', month: 'long', year: 'numeric' };
        document.getElementById('startDate').textContent = new Date(event.startDate).toLocaleDateString('nl-NL', options);
        document.getElementById('endDate').textContent = new Date(event.endDate).toLocaleDateString('nl-NL', options);

        if (event.deelnemers && event.deelnemers.length > 0) {
            renderDeelnemers(event.deelnemers, eventId, event);
        } else {
            document.getElementById("deelnemer-list").innerHTML = `<p class="text-muted p-3">Geen deelnemers gevonden.</p>`;
        }

        document.getElementById("top15-button").addEventListener("click", () => {
            const eventId = document.getElementById("event-id").dataset.eventId;
            window.location.href = `/Top15?eventId=${eventId}`;
        });

        toggleGlobalLoader(false);

        // Toon een waarschuwing indien data uit cache komt
        function showWarning(message) {
            const div = document.createElement("div");
            div.className = "alert alert-warning text-center mt-3";
            div.innerText = message;
            document.body.prepend(div);
        }

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

    function renderDeelnemers(deelnemers, eventId, event) {
        const list = document.getElementById("deelnemer-list");
        const podiumContainer = document.getElementById("podium-container");

        list.innerHTML = "";
        podiumContainer.innerHTML = "";

        const sorted = [...deelnemers].sort((a, b) => (b.punten || 0) - (a.punten || 0));

        if (event.showPodium) {
            podiumContainer.style.display = "flex";

            const top3 = [
                { deelnemer: sorted[1], plaats: 2 },
                { deelnemer: sorted[0], plaats: 1 },
                { deelnemer: sorted[2], plaats: 3 }
            ];

            const rest = sorted.slice(3);

            top3.forEach(({ deelnemer, plaats }) => {
                const wrapper = document.createElement("div");
                wrapper.className = `place place-${plaats}`;
                wrapper.innerHTML = `
                    <div class="plaats-badge">${plaats}</div>
                    <div class="naam">${deelnemer.poolNaam || "onbekende pool"}</div>
                    <div class="naam">${deelnemer.deelnemerNaam || "onbekende deelnemer"}</div>
                    <div class="punten">${deelnemer.punten ?? 0}</div>
                `;
                wrapper.addEventListener("click", () => handlePodiumClick(deelnemer, plaats, eventId));
                podiumContainer.appendChild(wrapper);
            });

            rest.forEach((deelnemer, index) => {
                const plaats = index + 4;
                list.appendChild(makeDeelnemerListItem(deelnemer, plaats, eventId));
            });
        } else {
            podiumContainer.style.display = "none";

            sorted.forEach((deelnemer, index) => {
                const plaats = index + 1;
                list.appendChild(makeDeelnemerListItem(deelnemer, plaats, eventId));
            });
        }
        initializeCollapseHandlers();
    }

    function renderRenner(pick) {
        const isOut = pick.outOfCompetition === true;
        const rowStyle = isOut ? 'background-color: #eee; text-decoration: line-through;' : '';
        const flagUrl = `${FLAGS_BASE_URL}/24x18/${pick.countryCode.toLowerCase()}.png`;

        return `
        <div class="renner-item border-bottom py-2 mb-2" style="${rowStyle}">
            <div class="d-flex justify-content-between align-items-start">
                <!-- Linkerzijde -->
                <div class="d-flex w-100">
                    <img src="${flagUrl}" class="img-fluid me-2 mt-1" style="max-height: 24px; width: 24px;" />

                    <div class="d-flex flex-column flex-md-row d-md-grid w-100"
                         style="display: grid; grid-template-columns: 1fr 250px; gap: 0.5rem;">
                        <div class="fw-bold">${pick.pcsName
                            ? `<a href="https://procyclingstats.com/rider/${pick.pcsName}" target="_blank" class="text-dark text-decoration-none">${pick.competitorName}</a>`
                            : pick.competitorName}</div>
                        <div class="text-muted small">${pick.competitorTeam}</div>
                    </div>
                </div>

                <div class="text-end ms-2" style="min-width: 50px;">
                    <span class="fw-bold fs-5">${pick.points}</span>
                </div>
            </div>
        </div>
    `;
    }
    function handlePodiumClick(deelnemer, plaats, eventId) {
        const lijst = document.getElementById("deelnemer-list");

        document.querySelectorAll('#deelnemer-list .podium-collapse').forEach(el => el.remove());

        lijst?.querySelectorAll('.collapse.show').forEach(c => {
            new bootstrap.Collapse(c, { toggle: false }).hide();
        });

        document.querySelectorAll('.podium .place').forEach(p => p.classList.remove('active'));
        const podiumElement = document.querySelector(`.podium .place-${plaats}`);
        if (podiumElement) {
            podiumElement.classList.add('active');
        }

        const wrapperLi = document.createElement("li");
        wrapperLi.className = "list-group-item p-2 podium-collapse";

        const collapseDiv = document.createElement("div");
        const collapseId = `collapse-podium-${deelnemer.id}`;
        collapseDiv.className = "collapse mt-2 w-100";
        collapseDiv.id = collapseId;
        collapseDiv.setAttribute("data-bs-parent", "#deelnemer-list");
        collapseDiv.innerHTML = `
            <div class="card card-body details-content" data-loaded="false">
                <em>Laden...</em>
            </div>
        `;

        wrapperLi.appendChild(collapseDiv);

        lijst.insertBefore(wrapperLi, lijst.firstElementChild);

        const detailsDiv = collapseDiv.querySelector(".details-content");

        (async () => {
            try {
                toggleGlobalLoader(true);

                const response = await fetch(`${API_BASE_URL}/api/Deelnemer/Picks/${deelnemer.id}/event/${eventId}`);
                if (!response.ok) throw new Error("Fout bij ophalen picks");

                const data = await response.json();
                detailsDiv.innerHTML = data.map(renderRenner).join('');
                detailsDiv.dataset.loaded = "true";

            } catch (err) {
                detailsDiv.innerHTML = `<p class="text-danger">Details ophalen mislukt.</p>`;
                console.error(err);
            } finally {
                toggleGlobalLoader(false);
            }

            setTimeout(() => {
                new bootstrap.Collapse(collapseDiv, { toggle: true });
            }, 250);
        })();
    }

    function makeDeelnemerListItem(deelnemer, plaats, eventId) {
        const collapseId = `collapse-${deelnemer.id}`;
        const li = document.createElement("li");
        const punten = deelnemer.punten ?? 0;
        const laatsteScore = deelnemer.laatsteScore ?? 0;
        const laatsteScoreHtml = (laatsteScore > 0)
            ? `<div class="dagscore text-success small position-absolute end-0 top-50 translate-middle-y ms-2">(+${laatsteScore})</div>`
            : ''; 
        li.className = "list-group-item p-2";
        li.innerHTML = `
        <div class="row align-items-center"
            data-bs-toggle="collapse"
            data-bs-target="#${collapseId}"
            aria-expanded="false"
            aria-controls="${collapseId}"
            style="cursor: pointer;"
            data-deelnemer-id="${deelnemer.id}"
            data-event-id="${eventId}">
            <div class="col-md-5 text-uppercase">${deelnemer.poolNaam || "onbekende pool"}</div>
            <div class="col-md-5 text-uppercase">${deelnemer.deelnemerNaam || "onbekende deelnemer"}</div>
            <div class="col-md-2 position-relative">
                <div class="fw-bold fs-4 text-end pe-5">${punten}</div>
                ${laatsteScoreHtml}
            </div>        
        </div>        
        <div class="collapse mt-2" id="${collapseId}" data-bs-parent="#deelnemer-list">
            <div class="card card-body details-content" data-loaded="false"></div>
        </div>
        `;
        return li;
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

                    if (data.length === 0) {
                        detailsDiv.innerHTML = `<p class="text-muted fst-italic mb-0">Geen renners geselecteerd.</p>`;
                    } else {
                        detailsDiv.innerHTML = data.map(renderRenner).join('');
                    }
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