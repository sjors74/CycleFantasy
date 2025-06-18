async function laadEtappeData() {
    let etappeNummer, stageId, data;

    try {
        toggleGlobalLoader(true);

        const params = new URLSearchParams(window.location.search);
        etappeNummer = parseInt(params.get("nummer"));

        const eventJson = localStorage.getItem("events");
        let event = null;
        const eventId = parseInt(localStorage.getItem("eventId"));

        if (eventJson) {
            const events = JSON.parse(eventJson);
            event = events.find(e => e.eventId == eventId);
        }

        if (!event) {
            throw new Error(`Event met id ${eventId} niet gevonden in localstorage`);
        }

        if (!Array.isArray(event.stages)) {
            throw new Error(`Event bevat geen geldige stage-array: ${JSON.stringify(event)}`);
        }

        if (!etappeNummer) {
            document.body.innerHTML = "<p>Geen etappe-nummer opgegeven.</p>";
            throw new Error("Geen etappe-nummer.");
        }

        const stage = event.stages.find(s => s.stageNumber === etappeNummer);
        if (!stage) {
            throw new Error(`Geen stage gevonden voor etappe ${etappeNummer}`);
        }

        document.getElementById("etappe-datum").textContent = `${stage.vanNaar}`;
        stageId = stage.stageId;

        await loadConfig();

        document.getElementById("etappe-title").textContent = `Etappe ${etappeNummer} – Resultaten`;

        const response = await fetch(`${API_BASE_URL}/api/Results/${stageId}/uitslag`);
        if (!response.ok) {
            console.error("API error:", response.status);
            return;
        }

        data = await response.json();
        if (!Array.isArray(data)) {
            console.error("API gaf geen lijst terug:", data);
            return;
        }

        const lijst = document.getElementById("renner-lijst");
        lijst.innerHTML = "";

        data.forEach(item => {
            const tr = document.createElement("tr");

            tr.innerHTML = `
                <td>${item.positie}</td>
                <td>${item.competitorName}</td>
                <td>${item.teamName ?? "Geen team"}</td>
                <td>${item.score} pt</td>
            `;

            lijst.appendChild(tr);
        });

        setupNavigation(etappeNummer, event.stages);
    } catch (error) {
        console.error(error);
    } finally {
        toggleGlobalLoader(false);
    }
}

function setupNavigation(etappeNummer, stages) {
    const nav = document.getElementById("navigatie-links");
    nav.innerHTML = "";

    const getStageId = nummer => stages.find(s => s.stageNumber === nummer)?.stageId ?? null;

    const prevNummer = etappeNummer - 1;
    const nextNummer = etappeNummer + 1;
    const prevId = getStageId(prevNummer);
    const nextId = getStageId(nextNummer);

    if (prevId) {
        const prev = document.createElement("a");
        prev.className = "btn etappe-nav-knop";
        prev.href = `Etappe?nummer=${prevNummer}`;
        prev.textContent = "← Vorige";
        prev.addEventListener("click", e => {
            e.preventDefault();
            window.history.pushState({ etappeNummer: prevNummer }, "", prev.href);
            laadEtappeData();
        });
        nav.appendChild(prev);
    }

    if (nextId) {
        const next = document.createElement("a");
        next.className = "btn etappe-nav-knop";
        next.href = `Etappe?nummer=${nextNummer}`;
        next.textContent = "Volgende →";
        next.addEventListener("click", e => {
            e.preventDefault();
            window.history.pushState({ etappeNummer: nextNummer }, "", next.href);
            laadEtappeData();
        });
        nav.appendChild(next);
    }
}

window.addEventListener("popstate", function (event) {
    if (event.state?.etappeNummer) {
        laadEtappeData();
    }
});

document.addEventListener("DOMContentLoaded", () => {
    laadEtappeData();

    const eventId = localStorage.getItem("eventId");
    if (eventId) {
        const terugLink = document.getElementById("terug-naar-event");
        terugLink.href = `/Event?eventId=${eventId}`;
    }
});
