const MAX_SELECTED = 15;

window.state = window.state || {};
window.state.selected =
    JSON.parse(document.getElementById("SelectedRidersJson").value || "[]");


function getSelected() {
    return window.state.selected || [];
}

function setSelected(selected) {
    window.state.selected = selected;

    updateSuggestieButton();
}

function updateUI() {

    const selected = getSelected();
    const max = 15;

    const hiddenInput = document.getElementById("SelectedRidersJson");
    const header = document.getElementById("riderCountHeader");

    hiddenInput.value = JSON.stringify(selected);
    header.innerText = selected.length;

    const atMax = selected.length >= max;

    document.querySelectorAll(".renner-checkbox").forEach(cb => {

        const id = parseInt(cb.value);

        cb.checked = selected.includes(id);

        cb.disabled = atMax && !cb.checked;

    });
}

function checkboxHandler(e) {
    const id = parseInt(e.target.value);
    if (e.target.checked) {
        if (!selected.includes(id) && selected.length < MAX_SELECTED) {
            selected.push(id);
        }
    } else {
        selected = selected.filter(x => x !== id);
    }
    updateUI();
}

function registerCheckboxEvents() {
    const checkboxes = document.querySelectorAll(".renner-checkbox");

    checkboxes.forEach(cb => {
        const id = parseInt(cb.value);
        cb.checked = selected.includes(id);
        cb.removeEventListener("change", checkboxHandler);
        cb.addEventListener("change", checkboxHandler);
    });
}

document.addEventListener("change", function (e) {

    if (!e.target.classList.contains("renner-checkbox"))
        return;

    const id = parseInt(e.target.value);

    let selected = getSelected();

    if (e.target.checked) {

        if (!selected.includes(id))
            selected.push(id);

    } else {

        selected = selected.filter(x => x !== id);
    }

    setSelected(selected);
    updateUI();
});

let currentRandomSelection = [];
let currentRandomEventId = null;


/* =========================================================
   WILLEKEURIGE RENNERS
   ========================================================= */

/**
 * Haalt willekeurige renners op.
 *
 * Het aantal renners dat wordt teruggegeven is afhankelijk
 * van hoeveel renners er al geselecteerd zijn.
 *
 * 3 geselecteerd  -> 12 suggesties
 * 12 geselecteerd -> 3 suggesties
 * 15 geselecteerd -> 0 suggesties
 */
async function haalWillekeurigeRenners(eventId) {

    const selected = getSelected();
    const selectedSet = new Set(selected);

    const nogNodig = MAX_SELECTED - selected.length;

    console.log("Aantal geselecteerd:", selected.length);
    console.log("Nog nodig:", nogNodig);

    if (nogNodig <= 0) {
        return [];
    }

    const response = await fetch(
        `${API_BASE_URL}/api/CompetitorsInEvent/${eventId}/40`
    );

    if (!response.ok) {
        throw new Error(
            `Fout bij ophalen renners: ${response.status}`
        );
    }

    const candidates = await response.json();

    console.log("Aantal kandidaten:", candidates.length);

    // Renners die al geselecteerd zijn uitsluiten
    let available = candidates.filter(r =>
        !selectedSet.has(r.competitorInTeamId)
    );

    console.log(
        "Beschikbare kandidaten:",
        available.length
    );

    // Willekeurig schudden
    for (let i = available.length - 1; i > 0; i--) {

        const j = Math.floor(Math.random() * (i + 1));

        [available[i], available[j]] =
            [available[j], available[i]];
    }

    // Alleen het aantal teruggeven dat nog nodig is
    return available.slice(0, nogNodig);
}


/**
 * Vult de inhoud van de random-modal.
 *
 * Wordt ook gebruikt wanneer de gebruiker op
 * "Nogmaals" klikt. De modal zelf blijft dan open.
 */
function updateRandomModal(renners) {

    console.log("MODAL RENNER DATA:", renners[0]);

    currentRandomSelection = renners;

    const container =
        document.getElementById("randomRenners");

    if (!container) {
        console.error(
            "Element #randomRenners niet gevonden."
        );
        return;
    }

    container.innerHTML = renners.map(r => {

        const flagUrl = r.countryShort
            ? `${FLAGS_BASE_URL}/24x18/${r.countryShort.toLowerCase()}.png`
            : '';

        return `
        <div class="border-bottom py-2 d-flex align-items-center">

            ${flagUrl
                ? `<img src="${flagUrl}"
                        class="me-2"
                        style="width: 24px; height: 18px;"
                        alt="">`
                : ''
            }

            <div>
                <div class="fw-bold">
                    ${r.competitorName}
                </div>

                <div class="small text-muted">
                    ${r.competitorTeam ?? ''}
                </div>
            </div>

        </div>
    `;
    }).join("");
}


/**
 * Opent de modal met de eerste suggestie.
 */
function toonRandomModal(renners) {

    console.log(
        "Modal openen met",
        renners.length,
        "renners"
    );

    updateRandomModal(renners);

    const modalElement =
        document.getElementById("randomModal");

    if (!modalElement) {

        console.error(
            "Element #randomModal niet gevonden."
        );

        return;
    }

    if (typeof bootstrap === "undefined") {

        console.error(
            "Bootstrap JavaScript is niet geladen."
        );

        return;
    }

    const modal =
        bootstrap.Modal.getOrCreateInstance(
            modalElement
        );

    modal.show();
}


/**
 * Wordt vanuit de CSHTML aangeroepen door:
 *
 * onclick="voegWillekeurigeRennersToe(@Model.EvenementId)"
 */
export async function voegWillekeurigeRennersToe(eventId) {

    console.log(
        "Suggestie aangeklikt. EventId:",
        eventId
    );

    const selected = getSelected();

    const nogNodig =
        MAX_SELECTED - selected.length;

    console.log(
        "Huidige selectie:",
        selected.length
    );

    console.log(
        "Nog nodig:",
        nogNodig
    );

    if (nogNodig <= 0) {

        console.log(
            "Er zijn al 15 renners geselecteerd."
        );

        return;
    }

    currentRandomEventId = eventId;

    try {

        const renners =
            await haalWillekeurigeRenners(eventId);

        console.log(
            "Willekeurige suggestie:",
            renners
        );

        if (renners.length === 0) {

            console.log(
                "Geen beschikbare renners gevonden."
            );

            return;
        }

        toonRandomModal(renners);

    }
    catch (err) {

        console.error(
            "Fout bij ophalen willekeurige renners:",
            err
        );
    }
}


/* =========================================================
   MODAL KNOPPEN
   ========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    () => {
        updateSuggestieButton();

        /* -------------------------------------------------
           OK
           ------------------------------------------------- */

        document
            .getElementById("btnRandomOk")
            ?.addEventListener(
                "click",
                () => {

                    console.log(
                        "Random selectie bevestigen:",
                        currentRandomSelection
                    );

                    if (
                        !currentRandomSelection ||
                        currentRandomSelection.length === 0
                    ) {
                        return;
                    }

                    let selected =
                        getSelected();

                    // Voorgestelde renners toevoegen
                    selected = [
                        ...selected,
                        ...currentRandomSelection.map(
                            r =>
                                r.competitorInTeamId
                        )
                    ];

                    // Dubbele IDs voorkomen
                    selected =
                        [...new Set(selected)];

                    setSelected(selected);

                    console.log(
                        "Nieuwe selectie:",
                        getSelected().length
                    );

                    // UI opnieuw tekenen
                    updateUI();
                    updateSuggestieButton();

                    // Modal sluiten
                    const modalElement =
                        document.getElementById(
                            "randomModal"
                        );

                    if (modalElement) {

                        const modal =
                            bootstrap.Modal.getInstance(
                                modalElement
                            );

                        modal?.hide();
                    }

                    // State leegmaken
                    currentRandomSelection = [];
                    currentRandomEventId = null;
                }
            );


        /* -------------------------------------------------
           NOGMAALS
           ------------------------------------------------- */

        document
            .getElementById("btnRandomAgain")
            ?.addEventListener(
                "click",
                async () => {

                    console.log(
                        "Nieuwe random suggestie aanvragen."
                    );

                    if (!currentRandomEventId) {

                        console.error(
                            "Geen eventId beschikbaar."
                        );

                        return;
                    }

                    try {

                        const renners =
                            await haalWillekeurigeRenners(
                                currentRandomEventId
                            );

                        console.log(
                            "Nieuwe suggestie:",
                            renners
                        );

                        if (
                            !renners ||
                            renners.length === 0
                        ) {
                            return;
                        }

                        /*
                         * Alleen de inhoud van de modal
                         * vervangen.
                         *
                         * De modal blijft dus open.
                         */
                        updateRandomModal(
                            renners
                        );

                    }
                    catch (err) {

                        console.error(
                            "Fout bij opnieuw ophalen willekeurige renners:",
                            err
                        );
                    }
                }
            );
    }
);

function updateSuggestieButton() {
    const button = document.getElementById("suggestieBtn");

    if (!button) {
        return;
    }

    const aantalGeselecteerd = getSelected().length;

    button.disabled = aantalGeselecteerd >= MAX_SELECTED;
}