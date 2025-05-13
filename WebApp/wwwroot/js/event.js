document.addEventListener('DOMContentLoaded', function () {
    let eventId;
    let etappes = [];
    let expandedRow;
    let lastClickedRow;
    let isLoading = false;

    loadConfig().then(async () => {

        eventId = document.getElementById("event-id").dataset.eventId;
        if (!eventId) {
            console.error("eventId niet gevonden in DOM");
            return;
        }

        toggleTableLoading(true);
        
        loadEtappes();

        try {
            const response = await fetch(`${API_BASE_URL}/Deelnemer?eventId=${eventId}`);

            if (!response.ok) {
                throw new Error("API fout");
            }

            let deelnemers = await response.json();
            deelnemers.sort((a, b) => (b.punten || 0) - (a.punten || 0));

            const tbody = document.querySelector("#deelnemerTable tbody");
            toggleTableLoading(false);

            deelnemers.forEach(d => {
                const row = document.createElement("tr");
                row.dataset.id = d.id;

                const poolCell = document.createElement("td");
                poolCell.textContent = d.poolNaam || "Onbekende pool";

                const naamCell = document.createElement("td");
                naamCell.textContent = d.deelnemerNaam || d.name || "Onbekende deelnemer";

                const puntenCell = document.createElement("td");
                puntenCell.textContent = d.punten ?? "0";

                row.appendChild(poolCell);
                row.appendChild(naamCell);
                row.appendChild(puntenCell);

                tbody.appendChild(row);
            });
        } catch (err) {
            const tbody = document.querySelector("#deelnemerTable tbody");
            tbody.innerHTML = "<tr><td colspan='3'>Kan deelnemers niet ophalen.</td></tr>";
            console.error("Fout bij ophalen deelnemers:", err);
        } finally {
            toggleTableLoading(false);
        }

    });

    async function loadEtappes() {
        if (isLoading) return;
        isLoading = true;
        toggleLoader(true);

        try {
            console.log("Fetching stages for eventId:", eventId);
            const response = await fetch(`${API_BASE_URL}/Event/${eventId}/stages`);
            if (!response.ok) throw new Error("Network response was not ok");

            etappes = await response.json();

            renderEtappes();
        } catch (error) {
            console.error("Error fetching etappes:", error);
        } finally {
            isLoading = false;
            toggleLoader(false);
        }

    }

    function renderEtappes() {

        const stepsContainer = document.getElementById("steps-container");
        stepsContainer.innerHTML = "";

        const completedStages = etappes.filter(etappe => etappe.hasResult).length;

        const progress = (completedStages / etappes.length) * 100;

        const progressBar = document.getElementById("progress-bar");
        progressBar.style.width = `${progress}%`;

        etappes.forEach((etappe) => {
            const step = document.createElement("div");
            step.classList.add("step");

            if (etappe.hasResult) step.classList.add("completed");
            step.title = `Etappe ${etappe.stageNumber}`;
            step.textContent = etappe.stageNumber;

            stepsContainer.appendChild(step);
        });
    }

    function toggleLoader(aan = true) {
        document.getElementById("loader").style.display = aan ? "block" : "none";
    }

    function toggleTableLoading(aan = true) {
        let spinnerRow = document.getElementById("spinnerRow");

        if (aan) {
            if (!spinnerRow) {
                const tbody = document.querySelector("#deelnemerTable tbody");
                spinnerRow = document.createElement("tr");
                spinnerRow.id = "spinnerRow";
                spinnerRow.innerHtml = `
                        <tr>
                            <td colspan="3" style="text-align: center;">
                                <div class="spinner" style="margin: 10px auto;"></div>
                            </td>
                        </tr>
                    `;
            }
        } else {
            if (spinnerRow) {
                spinnerRow.remove();
            }
        }
    }

    const table = document.querySelector('#deelnemerTable tbody');
    table.addEventListener('click', async function (e) {
        const clickedRow = e.target.closest('tr');
        if (!clickedRow || isLoading) return;

        const itemId = clickedRow.dataset.id;
        if (!itemId) return;

        // Als details al getoond worden, verwijder ze
        if (expandedRow && lastClickedRow === clickedRow) {
            expandedRow.remove();
            expandedRow = null;
            lastClickedRow = null;
            return;
        }

        // Als je op dezelfde rij klikt, dan stop hier (toggle effect)
        if (expandedRow) {
            expandedRow.remove();
            expandedRow = null;
            lastClickedRow = null;
        }

        isLoading = true;
        // Haal data op van de API
        toggleLoader(true);

        try {
            const response = await fetch(`${API_BASE_URL}/Deelnemer/Picks/${itemId}/event/${eventId}`);
            if (!response.ok) throw new Error(`Server error: ${response.status}`);
            const detailsList = await response.json(); // Verwacht array van max 15 items

            const detailsRow = document.createElement('tr');
            detailsRow.classList.add('details-row');

            const detailsCell = document.createElement('td');
            detailsCell.colSpan = clickedRow.children.length;

            const columns = 3; // Aantal kolommen (aantal subtabelletjes naast elkaar)
            const perColumn = Math.ceil(detailsList.length / columns);

            // Container voor de kolommen
            const columnWrapper = document.createElement('div');
            columnWrapper.classList.add('details-column-wrapper');


            for (let i = 0; i < columns; i++) {
                const chunk = detailsList.slice(i * perColumn, (i + 1) * perColumn);

                const subTable = document.createElement('table');
                subTable.classList.add('details-table');

                const tbody = document.createElement('tbody');
                chunk.forEach(item => {
                    const row = document.createElement('tr');
                    const nameCell = document.createElement('td');
                    nameCell.textContent = item.competitorName;

                    if (item.outOfCompetition) {
                        nameCell.style.textDecoration = 'line-through';
                    }

                    const pointsCell = document.createElement('td');
                    pointsCell.textContent = item.points;

                    if (item.outOfCompetition) {
                        pointsCell.style.backgroundColor = 'lightgray';  // Grijze achtergrond
                    }

                    row.appendChild(nameCell);
                    row.appendChild(pointsCell);

                    tbody.appendChild(row);
                });

                subTable.appendChild(tbody);
                columnWrapper.appendChild(subTable);
            }

            detailsCell.appendChild(columnWrapper);
            detailsRow.appendChild(detailsCell);
            clickedRow.parentNode.insertBefore(detailsRow, clickedRow.nextSibling);

            expandedRow = detailsRow;
            lastClickedRow = clickedRow;
        } catch (err) {
            console.error('Fout bij ophalen details:', err);
        } finally {
            toggleLoader(false);
            isLoading = false;
        }
    });
});