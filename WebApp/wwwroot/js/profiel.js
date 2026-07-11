function handlePoolClick(e, el) {
    e.preventDefault();
    const url = el.getAttribute("href");
    console.log("Navigeren naar:", url);

    // eventueel logica, bv. opslaan van accordion state
    sessionStorage.setItem("openAccordionId", el.closest(".accordion-collapse").id);

    window.location.href = url;
}

let geselecteerdeDeelnemerId = null;

function verwijderDeelnemer(deelnemerId) {
    geselecteerdeDeelnemerId = deelnemerId;

    const verwijderModal = new bootstrap.Modal(document.getElementById('verwijderModal'));
    verwijderModal.show();
}

document.addEventListener('DOMContentLoaded', () => {
    const bevestigBtn = document.getElementById('bevestigVerwijderBtn');
    if (bevestigBtn) {
        bevestigBtn.addEventListener('click', async function () {
            if (!geselecteerdeDeelnemerId) return;

            try {
                const response = await fetch(`${API_BASE_URL}/api/event/${geselecteerdeDeelnemerId}`, {
                    method: 'DELETE'
                });

                if (response.ok) {
                    const element = document.getElementById(`accordion-deelnemer-${geselecteerdeDeelnemerId}`);
                    if (element) element.remove();

                    bootstrap.Modal.getInstance(document.getElementById('verwijderModal')).hide();
                } else {
                    console.error("Verwijderen mislukt.");
                }
            } catch (error) {
                console.error("Fout:", error);
            } finally {
                bootstrap.Modal.getInstance(document.getElementById('verwijderModal')).hide();
                geselecteerdeDeelnemerId = null;
            }
        });
    }
});

function editPoolName(id) {

    document.getElementById(`editPool-${id}`).classList.remove("d-none");

    const input = document.getElementById(`poolInput-${id}`);
    const error = document.getElementById(`poolError-${id}`);

    error.classList.add("d-none");

    input.focus();

    input.oninput = function () {
        error.classList.add("d-none");
    };
}

function cancelPoolName(id) {

    document
        .getElementById(`editPool-${id}`)
        .classList.add("d-none");
}

async function savePoolName(id) {

    const naam =
        document
            .getElementById(`poolInput-${id}`)
            .value
            .trim();

    if (naam === "")
        return;

    const token = document.querySelector(
                   'input[name="__RequestVerificationToken"]'
                  ).value;

    const response = await fetch
        ("?handler=RenamePool",
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },

                body: JSON.stringify({
                    deelnemerId: id,
                    nieuweNaam: naam
                })
            });

    const result = await response.json();

    const error = document.getElementById(`poolError-${id}`);

    if (!result.success) {

        error.textContent = result.message;
        error.classList.remove("d-none");

        return;
    }

    error.classList.add("d-none");
      
    document
        .getElementById(`poolName-${id}`)
        .textContent = result.poolNaam;

    cancelPoolName(id);
}