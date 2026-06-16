// modal.js
window.ModalHelper = (function () {

    /*** Specifieke controles voor Stage modals ***/
    function initStageModalControls(container) {
        if (!container) return;

        const check = container.querySelector('#noScoreCheck');
        const wrapper = container.querySelector('#noScoreDescriptionWrapper');
        if (!check || !wrapper) return;

        const toggleDescription = () => wrapper.style.display = check.checked ? 'block' : 'none';

        toggleDescription();

        check.addEventListener('change', toggleDescription);
    }

    /*** AJAX modal loader ***/
    function setupModal(modalId, contentId) {
        const modalEl = document.getElementById(modalId);
        if (!modalEl) return;

        modalEl.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const url = button?.dataset.url;
            if (!url) return;

            const contentEl = document.getElementById(contentId);
            if (!contentEl) return;

            contentEl.innerHTML = `
                <div class="d-flex justify-content-center align-items-center" style="min-height:150px;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Laden...</span>
                    </div>
                </div>
            `;

            fetch(url)
                .then(res => res.text())
                .then(html => {
                    contentEl.innerHTML = html;
                    onModalLoad(modalId, contentEl);
                    setupAjaxForm(contentId);
                })
                .catch(() => contentEl.innerHTML = '<div class="text-danger">Kon inhoud niet laden.</div>');
        });
    }

    /*** AJAX formulier submit ***/
    function setupAjaxForm(modalContentId) {
        const container = document.getElementById(modalContentId);
        if (!container) return;

        const form = container.querySelector('form');
        if (!form) return;

        // voorkom dubbele binding
        if (form.dataset.ajaxBound === "true") return;
        form.dataset.ajaxBound = "true";

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: new FormData(form),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    const data = await response.json();

                    if (data.success) {
                        const modalEl = container.closest('.modal');
                        if (modalEl) {
                            const model = bootstrap.Modal.getInstance(modalEl);
                            model.hide();
                        }

                        if (data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        }
                        return;
                    }

                    Swal.fire('Fout', data.message || 'Opslaan mislukt', 'error');
                    return;
                }

                const html = await response.text();

                // Vervang inhoud veilig (zonder jQuery events)
                const fragment = document.createRange()
                    .createContextualFragment(html);

                container.replaceChildren(fragment);

                const countEl = container.querySelector('#stageCount');
                if (countEl) {
                    const count = countEl.dataset.stageCount;

                    const badge = document.querySelector(
                        '[data-bs-target="#manageStagesModal"] .badge'
                    );

                    if (badge) {
                        badge.textContent = count;
                    }
                }

                // Herinitialiseer JS op nieuwe HTML
                setupAjaxForm(modalContentId);
                const modalEl = container.closest('.modal');
                if (modalEl) {
                    onModalLoad(modalEl.id, container);
                }


            } catch (err) {
                console.error(err);
                Swal.fire('Fout', 'Er is iets misgegaan bij het opslaan.', 'error');
            }

        })
    }

    /*** Delete actie via SweetAlert2 ***/
    function setupDeleteButtons() {
        document.addEventListener('click', function (e) {
            const btn = e.target.closest('.delete-stage-btn');
            if (!btn) return;

            const stageId = btn.dataset.stageId;

            Swal.fire({
                title: 'Weet je het zeker?',
                text: "Deze etappe wordt permanent verwijderd!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Ja, verwijderen',
                cancelButtonText: 'Annuleren'
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch(`/Stages/DeleteAjax/${stageId}`, {
                        method: 'POST',
                        headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value }
                    })
                        .then(res => res.text())
                        .then(html => {
                            const fragment = document
                                .createRange()
                                .createContextualFragment(html);

                            const container = document.getElementById('manageStagesContent');
                            container.replaceChildren(fragment);

                            const countEl = container.querySelector('#stageCount');
                            if (countEl) {
                                const badge = document.querySelector(
                                    '[data-bs-target="#manageStagesModal"] .badge'
                                );
                                if (badge) badge.textContent = countEl.dataset.stageCount;

                            }
                            // JS opnieuw binden
                            ModalHelper.setupDeleteButtons();
                            ModalHelper.setupAjaxForm('manageStagesContent');
                        });
                }
            });
        });
    }

    /*** Functie om extra JS per modal te initialiseren ***/
    function onModalLoad(modalId, contentEl) {
        if (modalId === "editStageModal" || modalId === "manageStagesModal") {
            initStageModalControls(contentEl);
        }
    }

    /*** Public API ***/
    return {
        setupModal,
        setupAjaxForm,
        setupDeleteButtons,
        onModalLoad
    };

})();

// Init bij DOM ready
document.addEventListener("DOMContentLoaded", function () {
    if (!window.ModalHelper) {
        console.error("ModalHelper not loaded");
        return;
    }

    ModalHelper.setupModal('manageTeamsModal', 'manageTeamsContent');
    ModalHelper.setupModal('manageStagesModal', 'manageStagesContent');
    ModalHelper.setupModal('editStageModal', 'editStageModalContent');
    ModalHelper.setupDeleteButtons();
});
