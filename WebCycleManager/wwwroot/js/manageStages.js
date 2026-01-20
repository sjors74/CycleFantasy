(function ($) {
    if (typeof $ === 'undefined') {
        console.error('manageStages.js: jQuery not found');
        return;
    }

    var managePartialUrl = '/Events/ManageStagesPartial';
    var editAjaxUrl = '/Stages/EditAjax';
    var createAjaxUrl = '/Stages/CreateAjax';
    var deleteAjaxUrl = '/Stages/DeleteAjax';

    // Unbind previous handlers in this namespace, then bind.
    $(document).off('.manageStages');

    // Helper: init bindings that must be attached after the manage partial is inserted
    function initManagePartialBindings(container) {
        // ensure single binding for add form: unbind all then bind one namespaced handler
        var $form = $(container).find('#addStageForm');
        if ($form.length) {
            $form.off('submit.manageStagesCustom');
            $form.on('submit.manageStagesCustom', function (e) {
                e.preventDefault();
                e.stopImmediatePropagation();

                var $f = $(this);
                if ($f.data('submitting')) return;
                $f.data('submitting', true);

                var $submitButtons = $f.find('button[type="submit"], input[type="submit"]').prop('disabled', true);
                var url = $f.attr('action') || createAjaxUrl;
                var $manageModal = $('#manageStagesModal');

                $.ajax({
                    url: url,
                    method: 'POST',
                    data: $f.serialize(),
                    success: function (data) {
                        if (typeof data === 'string') {
                            $manageModal.find('#manageStagesContent').html(data);
                        } else {
                            location.reload();
                        }
                    },
                    error: function () {
                        alert('Fout bij toevoegen etappe.');
                    },
                    complete: function () {
                        $f.data('submitting', false);
                        $submitButtons.prop('disabled', false);
                        // re-init bindings for newly inserted content
                        initManagePartialBindings($('#manageStagesContent'));
                    }
                });
            });
        }
    }

    // Load manage-partial when manage modal opens
    $(document).on('show.bs.modal.manageStages', '#manageStagesModal', function (e) {
        var trigger = $(e.relatedTarget);
        var eventId = trigger?.data('event-id') || trigger?.data('eventid');
        if (!eventId) return;

        var $modal = $(this);
        $modal.find('#manageStagesContent').html('<div class="text-center p-3">Laden...</div>');

        $.get(managePartialUrl, { eventId: eventId })
            .done(function (html) {
                $modal.find('#manageStagesContent').html(html);
                initManagePartialBindings($modal.find('#manageStagesContent'));
            })
            .fail(function () {
                $modal.find('#manageStagesContent').html('<div class="alert alert-danger">Kon etappes niet laden.</div>');
            });
    });

    // Load edit form into edit modal
    $(document).on('click.manageStages', '.edit-stage-btn', function (e) {
        e.preventDefault();
        var url = $(this).data('url');
        if (!url) return;

        var $editModal = $('#editStageModal');
        $editModal.find('#editStageContent').html('<div class="text-center p-3">Laden...</div>');

        $.get(url)
            .done(function (html) {
                $editModal.find('#editStageContent').html(html);
                $editModal.modal('show');
            })
            .fail(function () {
                $editModal.find('#editStageContent').html('<div class="alert alert-danger">Kon formulier niet laden</div>');
                $editModal.modal('show');
            });
    });

    // Submit edit-form via AJAX (delegated, still ok)
    $(document).on('submit.manageStages', '#editStageForm', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        var $form = $(this);
        var url = $form.attr('action') || editAjaxUrl;
        var $editModal = $('#editStageModal');

        if ($form.data('submitting')) return;
        $form.data('submitting', true);
        var $submitButtons = $form.find('button[type="submit"], input[type="submit"]').prop('disabled', true);

        $.ajax({
            url: url,
            method: 'POST',
            data: $form.serialize(),
            success: function (data, status, xhr) {
                var contentType = (xhr.getResponseHeader('content-type') || '').toLowerCase();

                // JSON success
                if (contentType.indexOf('application/json') !== -1 || (typeof data === 'object' && data && data.success)) {
                    var parsed = (typeof data === 'string') ? JSON.parse(data) : data;
                    if (parsed && parsed.success) {
                        var s = parsed.stage;
                        var $row = $('#stage-' + s.id);
                        if ($row.length) {
                            $row.find('td').eq(0).text(s.date);
                            $row.find('td').eq(1).text(s.name);
                            $row.find('td').eq(2).html(s.start + ' → ' + s.finish);
                        } else {
                            var eventId = $form.find('input[name="EventId"]').val();
                            if (eventId) {
                                $.get(managePartialUrl, { eventId: eventId }).done(function (html) {
                                    $('#manageStagesContent').html(html);
                                });
                            } else {
                                location.reload();
                            }
                        }
                        $editModal.modal('hide');
                        return;
                    }
                }

                // HTML response -> either edit partial (validation errors) or manage partial
                var htmlString = (typeof data === 'string') ? data : null;
                if (!htmlString) {
                    location.reload();
                    return;
                }

                var $returned = $('<div>').append($.parseHTML(htmlString, document, true));
                var isManagePartial = $returned.find('#manageStagesContainer').length > 0;

                if (isManagePartial) {
                    $('#manageStagesContent').html(htmlString);
                    $editModal.modal('hide');
                    // re-init bindings for the new content
                    initManagePartialBindings($('#manageStagesContent'));
                } else {
                    $editModal.find('#editStageContent').html(htmlString);
                }
            },
            error: function () {
                alert('Fout bij opslaan etappe.');
            },
            complete: function () {
                $form.data('submitting', false);
                $submitButtons.prop('disabled', false);
            }
        });
    });

    // Delete stage via SweetAlert2 (prettier confirmation)
    $(document).on('click.manageStages', '.delete-stage-btn', function (e) {
        e.preventDefault();
        var stageId = $(this).data('stage-id');
        if (!stageId) return;

        if (window.Swal) {
            Swal.fire({
                title: 'Verwijderen?',
                text: 'Weet je zeker dat je deze etappe wilt verwijderen?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Ja, verwijderen',
                cancelButtonText: 'Annuleer'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $.ajax({
                        url: deleteAjaxUrl,
                        method: 'POST',
                        data: { id: stageId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() },
                        success: function (data) {
                            var htmlString = (typeof data === 'string') ? data : null;
                            if (htmlString && htmlString.indexOf('id="manageStagesContainer"') !== -1) {
                                $('#manageStagesContent').html(htmlString);
                                initManagePartialBindings($('#manageStagesContent'));
                            } else {
                                location.reload();
                            }
                        },
                        error: function () {
                            Swal.fire('Fout', 'Kon etappe niet verwijderen', 'error');
                        }
                    });
                }
            });
        } else {
            if (confirm('Weet je zeker dat je deze etappe wilt verwijderen?')) {
                $.ajax({
                    url: deleteAjaxUrl,
                    method: 'POST',
                    data: { id: stageId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() },
                    success: function (data) {
                        var htmlString = (typeof data === 'string') ? data : null;
                        if (htmlString && htmlString.indexOf('id="manageStagesContainer"') !== -1) {
                            $('#manageStagesContent').html(htmlString);
                            initManagePartialBindings($('#manageStagesContent'));
                        } else {
                            location.reload();
                        }
                    },
                    error: function () {
                        alert('Kon etappe niet verwijderen');
                    }
                });
            }
        }
    });

})(jQuery);