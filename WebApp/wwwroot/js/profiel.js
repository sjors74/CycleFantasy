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
