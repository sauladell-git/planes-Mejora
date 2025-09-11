$(document).ready(function () {
    $("#delete-confirm-resolution").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                if (!canSubmitForm(getDialogFirstButton("#delete-confirm-resolution")))
                    return false;

                $.post(BaseSiteURL + "/Plan/DeleteResolution", {
                    improvementPlanId: $("#Id").val(),
                    documentId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        $(".documents-wrapper").html(data);
                        $('#list-documents').dataTable({
                            "bPaginate": false,
                            "bLengthChange": false,
                            "bFilter": true,
                            "bSort": true,
                            "bInfo": false,
                            "bAutoWidth": false,
                            "aoColumnDefs": [{
                                "bSearchable": false, "aTargets": [0]
                            }]
                        });
                        bindDocuments();                        
                        showMessage("#msg-plan", "Se eliminó la disposición con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-list", "No se pudo eliminar la disposición.", "error", true);
                    }

                    enableSubmitButtons();
                    $("#delete-confirm-resolution").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#block-confirm-resolution").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Anular": function () {
                if (!canSubmitForm(getDialogFirstButton("#block-confirm-resolution")))
                    return false;

                $.post(BaseSiteURL + "/Plan/BlockResolution", {
                    improvementPlanId: $("#Id").val(),
                    resolutionId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        $(".documents-wrapper").html(data);
                        bindDocuments();
                        showMessage("#msg-plan", "La disposición se anuló con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-plan", "No es posible anular la disposición.", "error", true);
                    }

                    enableSubmitButtons();
                    $("#block-confirm-resolution").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".nuevo-resolucion").click(function () {
        $("#CurrentTemplateId").val(""); // Limpio el valor del combo de templates                        
        $("#new-resolution").attr("data-documentId", "0").attr("data-improvementPlanId", $("#Id").val()).attr("data-originalStatusId", "24").dialog("open");
    });
});

function saveResolution() {
    
    $('#list-resolutions_filter input[type=search]').val('').trigger('keyup');

    var resolution = validateResolution();
    if (resolution != null) {
        if (!canSubmitForm(getDialogFirstButton("#new-resolution"))) {
            
            return false;
        }

        // Si quiero firmar el documento, submiteo solo el formulario de firma ya que contiene un archivo y no puede hacerse por AJAX
        if ($("#DocumentDTO_ResolutionDTO_StatusId :selected").val() == "27") {
            //$("#resolutionId").val($("#new-resolution").attr("data-documentId"));
            //$("#resolutionPlanId").val($("#Id").val());
            //$("#frmAnnex").submit();

            if (validateResolutionSign()) {
                $("#resolutionId").val($("#new-resolution").attr("data-documentId"));
                $("#resolutionPlanId").val($("#Id").val());
                // Cambio la accion del formulario
                $("#frmAnnex").attr("action", BaseSiteURL + "/Resolution/SignResolution");
                $("#frmAnnex").submit();
            }
            
            return false;
        }

        $.ajax({
            type: "POST",
            url: BaseSiteURL + "/Plan/SaveResolution",
            data: JSON.stringify(resolution), // Usamos JSON.stringfy por el array
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data != "") {
                    $(".documents-wrapper").html(data);
                    bindDocuments();
                    showMessage("#msg-plan", "La disposición se guardó con éxito", "confirmation", true);
                } else {
                    showMessage("#msg-plan", "Ocurrió un error al guardar la disposición", "error", true);
                }

                $("#new-resolution").dialog("close");
                
            }
        });
    }
}