let currentEtappeId = null;
let currentTab = 'tab1';
let stages = [];

document.addEventListener("DOMContentLoaded", () => {
    const activeHeader = document.querySelector('.tab-header li.active');
    if (activeHeader) currentTab = activeHeader.dataset.tab || "tab1";

    initTabSwitching();
    initTerugKnoppen();

    const params = new URLSearchParams(window.location.search);
    const startNummer = parseInt(params.get("nummer"));
    const etappeId = parseInt(params.get("stageId"));

    navigateToEtappe(etappeId, { replaceHistory: true });
});
function initTabSwitching() {
    const tabHeaders = document.querySelectorAll(".tab-header li");
    tabHeaders.forEach(header => {
        header.addEventListener("click", () => {
            const newTab = header.dataset.tab;
            if (!newTab) return;

            currentTab = newTab;
            setActiveTabUI(currentTab);

            window.history.pushState({ etappeNummer: currentEtappeId, tab: currentTab }, "", buildUrl(currentEtappeId, currentTab));

            if (currentTab === "tab1") {
                laadEtappeData(currentEtappeId);
            } else {
                laadTussenstandData(currentEtappeId);
            }

            setupNavigation();
        });
    });
}

function setActiveTabUI(tabName) {
    document.querySelectorAll(".tab-header li").forEach(li => {
        if (li.dataset.tab === tabName) li.classList.add("active");
        else li.classList.remove("active");
    });

    document.querySelectorAll(".tab-content").forEach(panel => {
        if (panel.id === tabName) panel.classList.add("active");
        else panel.classList.remove("active");
    });
}

async function navigateToEtappe(nummer, opts = { replaceHistory: false }) {
    if (!nummer || Number.isNaN(Number(nummer))) {
        await ensureStagesLoaded();
        if (stages.length) {
            nummer = stages[0].stageId;
        } else {
            console.error("Geen etappes gevonden om te tonen.");
            return;
        }
    }
    nummer = parseInt(nummer, 10);
    currentEtappeId = nummer;

    if (opts.replaceHistory) {
        window.history.replaceState({ etappeNummer: nummer, tab: currentTab }, "", buildUrl(nummer, currentTab));
    } else {
        window.history.pushState({ etappeNummer: nummer, tab: currentTab }, "", buildUrl(nummer, currentTab));
    }

    setActiveTabUI(currentTab);

    await ensureStagesLoaded();

    if (currentTab === "tab1") {
        await laadEtappeData(nummer);
    } else {
        await laadTussenstandData(nummer);
    }

    setupNavigation();
}

function buildUrl(nummer, tab) {
    const base = window.location.pathname.split("/").pop() || "Etappe";
    return `${base}?nummer=${nummer}`;
}


function setupNavigation() {
    const containers = document.querySelectorAll(".etappe-knop-groep");
    containers.forEach(c => c.innerHTML = "");

    if (!stages || !stages.length || !currentEtappeId) return;

    // Huidige stage bepalen
    const currentStage = stages.find(s => s.stageId === currentEtappeId);
    if (!currentStage) return;

    const orderedStages = [...stages].sort((a, b) => a.stageOrder - b.stageOrder);

    const currentIndex = orderedStages.findIndex(s => s.stageId === currentEtappeId);

    const prevStage = orderedStages[currentIndex - 1];
    const nextStage = orderedStages[currentIndex + 1];

    if (prevStage) createNavButton(`← ${prevStage.stageName ?? "Vorige"}`, prevStage.stageId, containers);
    if (nextStage) createNavButton(`${nextStage.stageName ?? "Volgende"} →`, nextStage.stageId, containers);
}

function createNavButton(text, nummer, containers) {
    containers.forEach(nav => {
        const a = document.createElement("a");
        a.className = "btn etappe-nav-knop";
        a.href = buildUrl(nummer);
        a.textContent = text;
        a.addEventListener("click", e => {
            e.preventDefault();
            navigateToEtappe(nummer);
        });
        nav.appendChild(a);
    });
}

async function ensureStagesLoaded() {
    if (stages && stages.length) return;
    try {
        const eventJson = localStorage.getItem("events");
        const eventId = parseInt(localStorage.getItem("eventId"));
        if (!eventJson || !eventId) {
            console.warn("LocalStorage events/eventId ontbreekt");
            stages = [];
            return;
        }
        const events = JSON.parse(eventJson);
        const event = events.find(e => e.eventId == eventId);
        if (!event || !Array.isArray(event.stages)) {
            stages = [];
            return;
        }
        stages = event?.stages ?? [];
    } catch (err) {
        console.error("Fout bij inlezen stages uit localStorage:", err);
        stages = [];
    }
}

async function laadEtappeData(stageId) {
    if (!stageId) return;
    toggleGlobalLoader && toggleGlobalLoader(true);
    try {
        const stage = stages.find(s => s.stageId === stageId);
        if (!stage) {
            console.error("Stage niet gevonden:", stageId);
            document.getElementById("etappe-title").textContent = `Etappe ${stage.stageNumber}`;
            return;
        }

        document.getElementById("etappe-datum").textContent = stage.vanNaar ?? "";
        document.getElementById("etappe-title").textContent = `Etappe ${stage.stageNumber} – Resultaten`;

        const lijst = document.getElementById("renner-lijst");
        lijst.innerHTML = `<tr><td colspan="4">Bezig met laden...</td></tr>`;

        const response = await fetch(`${API_BASE_URL}/api/Results/${stage.stageId}/uitslag`);
        if (!response.ok) {
            console.error("API error:", response.status);
            lijst.innerHTML = `<tr><td colspan="4">Fout bij laden uitslag</td></tr>`;
            return;
        }
        const data = await response.json();
        lijst.innerHTML = "";
        if (!Array.isArray(data) || data.length === 0) {
            lijst.innerHTML = `<tr><td colspan="4">Geen resultaten</td></tr>`;
            return;
        }

        data.forEach(item => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${item.positie ?? ""}</td>
                <td>${item.competitorName ?? ""}</td>
                <td>${item.teamName ?? "Geen team"}</td>
                <td>${item.score ?? ""} pt</td>
            `;
            lijst.appendChild(tr);
        });
    } catch (error) {
        console.error("Fout bij laadEtappeData: ", err);
    } finally {
        toggleGlobalLoader && toggleGlobalLoader(false);
    }
}

async function laadTussenstandData(stageId) {
    if (!stageId) return;

    const stage = stages.find(s => s.stageId === stageId);
    if (!stage) {
        console.error("Stage niet gevonden:", stageId);
        return;
    }

    document.getElementById("tussenstand-title").textContent = `Tussenstand pool na etappe ${stage.stageNumber}`;

    const tussenlijst = document.getElementById("tussenstand-lijst");
    tussenlijst.innerHTML = `<tr><td colspan="4">Bezig met laden...</td></tr>`;

    try {
        const eventId = parseInt(localStorage.getItem("eventId"));
        if (!eventId) {
            tussenlijst.innerHTML = `<tr><td colspan="4">Event niet gevonden</td></tr>`;
            return;
        }

        const response = await fetch(`${API_BASE_URL}/api/results/${eventId}/event/${stageId}/stage`);
        if (!response.ok) {
            console.error("API error:", response.status);
            tussenlijst.innerHTML = `<tr><td colspan="4">Fout bij laden tussenstand</td></tr>`;
            return;
        }

        const data = await response.json();
        tussenlijst.innerHTML = "";
        if (!Array.isArray(data) || data.length === 0) {
            tussenlijst.innerHTML = `<tr><td colspan="4">Geen tussenstand beschikbaar</td></tr>`;
            return;
        }

        let lastPoints = null;
        let lastRank = 0;
        let actualRank = 0;
        data.forEach(item => {
            actualRank++;
            if (item.punten !== lastPoints) {
                lastRank = actualRank;
                lastPoints = item.punten;
            }
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${lastRank}</td>
                <td>${item.poolNaam}</td>
                <td>${item.deelnemerNaam}</td>
                <td>${item.punten}</td>
            `;
            tussenlijst.appendChild(tr);
        });
    } catch (err) {
        console.error("Fout bij laadTussenstandData", err);
        tussenlijst.innerHTML = `<tr><td colspan="4">Fout bij laden resultaten</td></tr>`;
    }
}


function initTerugKnoppen() {
    const eventId = localStorage.getItem("eventId");
    const terugLinks = document.querySelectorAll("#terug-naar-event, #terug-naar-event-2");
    terugLinks.forEach(link => {
        if (eventId) link.href = `/Event?eventId=${eventId}`;

        link.addEventListener("click", (e) => {
            e.preventDefault();
            if (eventId) {
                window.location.href = `/Event?eventId=${eventId}`;
            } else {
                window.history.back();
            }
        });
    });
}