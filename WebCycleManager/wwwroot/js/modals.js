// modal.js
window.ModalHelper = (function () {

    /*** Specifieke controles voor Stage modals ***/
    function initStageModalControls(modalContentId) {
        const container = document.getElementById(modalContentId);
        if (!container) return;

        const check = container.querySelector('#noScoreCheck');
        const wrapper = container.querySelector('#noScoreDescriptionWrapper');
        if (!check || !wrapper) return;

        const toggleDescription = () => wrapper.style.display = check.checked ? 'block' : 'none';
        toggleDescription();

        if (check._toggleListener) check.removeEventListener('change', check._toggleListener);
        check._toggleListener = toggleDescription;
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

        container.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', function (e) {
                e.preventDefault();

                const formData = new FormData(form);
                fetch(form.action, { method: 'POST', body: formData })
                    .then(res => res.json())
                    .then(data => {
                        const modal = bootstrap.Modal.getOrCreateInstance(container.closest('.modal'));
                        if (data.success) {
                            if (data.stage) {
                                // update tabelrij bij stage
                                const row = document.getElementById('stage-' + data.stage.id);
                                if (row) {
                                    row.innerHTML = `
                                        <td>${data.stage.date}</td>
                                        <td>${data.stage.name}</td>
                                        <td>${data.stage.start} → ${data.stage.finish}</td>
                                        <td class="text-end">
                                            <button class="btn btn-sm btn-outline-primary edit-stage-btn" data-url="/Stages/EditStage/${data.stage.id}">
                                                <i class="bi bi-pencil"></i>
                                            </button>
                                            <button type="button" class="btn btn-danger btn-sm delete-stage-btn" data-stage-id="${data.stage.id}">
                                                <i class="bi bi-trash"></i>
                                            </button>
                                        </td>
                                    `;
                                }
                            }

                            modal.hide();
                            if (data.redirectUrl) {
                                window.location.href = data.redirectUrl;
                            } else {
                                Swal.fire({ icon: 'success', title: 'Actie voltooid!' });
                            }
                        } else {
                            // validation errors
                            container.innerHTML = data.html || `<div class="text-danger">${data.message}</div>`;
                            onModalLoad(modalContentId, container);
                        }
                    })
                    .catch(() => Swal.fire('Fout', 'Er is iets misgegaan bij het opslaan.', 'error'));
            });
        });
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
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                const row = document.getElementById(`stage-${stageId}`);
                                if (row) row.remove();
                                Swal.fire('Verwijderd!', 'De etappe is verwijderd.', 'success');
                            } else {
                                Swal.fire('Fout', 'Kon de etappe niet verwijderen', 'error');
                            }
                        });
                }
            });
        });
    }

    /*** Functie om extra JS per modal te initialiseren ***/
    function onModalLoad(modalId, contentEl) {
        if (modalId === "editStageModal" || modalId === "manageStagesModal") {
            initStageModalControls(contentEl.id);
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
    ModalHelper.setupDeleteButtons();
});
