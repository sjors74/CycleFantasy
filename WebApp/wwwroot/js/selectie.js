const MAX_SELECTED = 15;

window.state = window.state || {};
window.state.selected =
    JSON.parse(document.getElementById("SelectedRidersJson").value || "[]");


function getSelected() {
    return window.state.selected || [];
}

function setSelected(selected) {
    window.state.selected = selected;
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


export async function voegWillekeurigeRennersToe(eventId) {
    let selected = getSelected();
    const selectedSet = new Set(selected);

    const nogNodig = 15 - selected.length;        

    if (nogNodig <= 0) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/CompetitorsInEvent/${eventId}/40`);
        const candidates = await response.json();
        console.log("candidates", candidates.length);

        let available = candidates.filter(r =>
            !selectedSet.has(r.competitorInTeamId));

        for (let i = available.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [available[i], available[j]] = [available[j], available[i]];
        }

        const toeTeVoegen = available.slice(0, nogNodig);

        selected = [
            ...selected,
            ...toeTeVoegen.map(r => r.competitorInTeamId)
        ];

        setSelected([...new Set(selected)]);

        console.log("nieuwe selectie", getSelected().length);

        updateUI();

    } catch (err) {
        console.error("Fout bij ophalen willekeurige renners:", err);
    }
}
