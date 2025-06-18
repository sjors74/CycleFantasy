const MAX_SELECTED = 15;
let selected = [];

function updateUI() {
    const headerCount = document.getElementById("riderCountHeader");
    const hiddenInput = document.getElementById("SelectedRidersJson");
    const checkboxes = document.querySelectorAll(".renner-checkbox");
    const message = document.getElementById("maxSelectedMessage");

    if (headerCount) headerCount.innerText = selected.length;
    if (hiddenInput) hiddenInput.value = JSON.stringify(selected);

    const opMax = selected.length >= MAX_SELECTED;

    checkboxes.forEach(cb => {
        const id = parseInt(cb.value);
        const isGeselecteerd = selected.includes(id);

        cb.checked = isGeselecteerd;
        cb.disabled = opMax && !isGeselecteerd;
    });

    if (message) {
        message.style.display = opMax ? "block" : "none";
    }
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

document.addEventListener("DOMContentLoaded", () => {
    const hiddenInput = document.getElementById("SelectedRidersJson");
    if (hiddenInput && hiddenInput.value) {
        try {
            selected = JSON.parse(hiddenInput.value);
        } catch {
            selected = [];
        }
    }

    registerCheckboxEvents();
    updateUI();

    document.querySelectorAll('.show-more-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const teamId = btn.dataset.teamId;
            const eventId = btn.dataset.eventId;
            const loadedIds = btn.dataset.loadedIds
                .split(',')
                .map(id => parseInt(id))
                .filter(id => !isNaN(id));

            const formData = new FormData();
            formData.append('teamId', teamId);
            formData.append('eventId', eventId);
            loadedIds.forEach(id => formData.append('alreadyLoadedIds', id));

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }

            const response = await fetch('?handler=LaadMeerRenners', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error("Server error:", errorText);
                return;
            }

            const renners = await response.json();
            const container = document.getElementById(`team-${teamId}-extra-renners`);

            renners.forEach(renner => {
                const formCheck = document.createElement('div');
                formCheck.className = 'form-check mb-1';

                const input = document.createElement('input');
                input.type = 'checkbox';
                input.className = 'form-check-input renner-checkbox';
                input.value = renner.competitorId;
                input.checked = selected.includes(renner.competitorId);

                const label = document.createElement('label');
                label.className = 'form-check-label';
                label.textContent = renner.competitorName;

                formCheck.appendChild(input);
                formCheck.appendChild(label);
                container.appendChild(formCheck);
            });

            btn.style.display = 'none';
            registerCheckboxEvents();
        });
    });
});

export async function voegWillekeurigeRennersToe(eventId) {
    const nogNodig = 15 - selected.length;
    if (nogNodig <= 0) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/CompetitorsInEvent/${eventId}/${nogNodig}`);
        const randomRenners = await response.json();

        const extra = randomRenners.filter(r =>
            !selected.includes(r.competitorId)
        );

        const toeTeVoegen = extra.slice(0, nogNodig);
        const nieuweIds = toeTeVoegen.map(r => r.competitorId);
        selected = [...selected, ...nieuweIds];

        updateUI();
        const hiddenInput = document.getElementById("SelectedRidersJson");
        hiddenInput.value = JSON.stringify(selected);
    } catch (err) {
        console.error("Fout bij ophalen willekeurige renners:", err);
    }
}
