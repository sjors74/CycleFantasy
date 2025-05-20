document.addEventListener('DOMContentLoaded', function () {
    let eventId;
    let etappes = [];
    let isLoading = false;

    loadConfig().then(async () => {

        const eventElement = document.getElementById("event-id");
        if (!eventElement) {
            console.warn("event.js: #event-id bestaat niet op deze pagina.");
            return;
        }

        eventId = eventElement.dataset.eventId;
        if (!eventId) {
            console.error("eventId niet gevonden in dataset van #event-id");
            return;
        }
        loadEtappes();
    });

    const collapseElements = document.querySelectorAll('.collapse');
    collapseElements.forEach(collapse => {
        collapse.addEventListener('show.bs.collapse', async function () {
            
            const targetId = `#${collapse.id}`;
            const triggerDiv = document.querySelector(`[data-bs-target="${targetId}"]`);

            if (!triggerDiv) {
                console.warn("Geen triggerDiv gevonden met data-bs-target");
                return;
            }
            const deelnemerId = triggerDiv.dataset.deelnemerId;
            const eventId = triggerDiv.dataset.eventId;
            const detailsDiv = collapse.querySelector('.details-content');
            if (!detailsDiv || detailsDiv.dataset.loaded === "true") return;
            toggleLoader(true);
            try {
                const response = await fetch(`${API_BASE_URL}/Deelnemer/Picks/${deelnemerId}/event/${eventId}`);
                if (!response.ok) throw new Error("Fout bij ophalen");

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
                            <div class="col-md-4 text-muted">${pick.competitorTeam}</div>
                            <div class="col-md-2 text-end">${pick.points}</div>
                        </div>
                `;
                }).join('');

                detailsDiv.dataset.loaded = "true";
                toggleLoader(false);
            } catch (err) {
                detailsDiv.innerHTML = `<p class="text-danger">Details ophalen mislukt.</p>`;
                console.error(err);
            } finally {
                toggleLoader(false);
            }
        });
    });

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
            const response = await fetch(`${API_BASE_URL}/Event/${eventId}/stages`);
            if (!response.ok) throw new Error("Network response was not ok");
            etappes = await response.json();
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
            if (etappe.hasResult) step.classList.add("completed");
            step.title = `Etappe ${etappe.stageNumber}`;
            step.textContent = etappe.stageNumber;

            fragment.appendChild(step);
        });
        stepsContainer.appendChild(fragment);
    }

    function toggleLoader(aan = true) {
        document.getElementById("loader").style.display = aan ? "block" : "none";
    }
});