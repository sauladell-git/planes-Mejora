$(document).ready(function () {
    $("#delete-confirm-dictum").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                if (!canSubmitForm(getDialogFirstButton("#delete-confirm-dictum")))
                    return false;

                $.post(BaseSiteURL + "/Plan/DeleteDictum", {
                    improvementPlanId: $("#Id").val(),
                    documentId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        $(".documents-wrapper").html(data);
                        bindDocuments();
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
                        showMessage("#msg-plan", "Se eliminó el dictamen con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-list", "No se pudo eliminar el Dictamen", "error", true);
                    }

                    $("#delete-confirm-dictum").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#block-confirm-dictum").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Anular": function () {
                if (!canSubmitForm(getDialogFirstButton("#block-confirm-dictum")))
                    return false;

                $.post(BaseSiteURL + "/Plan/BlockDictum", {
                    improvementPlanId: $("#block-confirm-dictum").attr("data-improvementPlanId"),
                    dictumId: $("#block-confirm-dictum").attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        $(".documents-wrapper").html(data);
                        bindDocuments();
                        showMessage("#msg-plan", "El dictamen se anuló con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-plan", "No es posible anular el dictamen.", "error", true);
                    }

                    $("#block-confirm-dictum").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".nuevo-dictamen").click(function () {
        $("#CurrentTemplateId").val(""); // Limpio el valor del combo de templates
        var dictumBody = tinyMCE.get('dictum-tinymce');
        dictumBody.setContent(''); // Limpio el cuerpo del dictamen
        $('#list-dictums').dataTable().fnClearTable(); // Limpia la tabla
        $("#new-dictum").attr("data-documentId", "0").attr("data-improvementPlanId", $("#Id").val()).attr("data-originalStatusId", "16").dialog("open");
    });
});

function saveDictum() {
    var dictum = validateDictum();
    var originalStatusId = $('#new-dictum').attr('data-originalstatusid');

    $('#list-dictums_filter input[type=search]').val('').trigger('keyup')
    if (dictum != null) {
        if (!canSubmitForm(getDialogFirstButton("#new-dictum")))
            return false;
        showLoading();

        if (dictum.StatusId == "19") {
            if (originalStatusId == "16") {
                //SaveDictumAnnex(dictum);
                // si está mandando de borrador a firmado lo guardamos emitido y luego firmamos
                dictum.StatusId = 17;
                doSaveDictum(dictum, function () {
                    dictum.StatusId = 18;
                    doSaveDictum(dictum, function () {
              //          SaveDictumAnnex(dictum);
                        dictum.StatusId = 19;
                        SignDictum(dictum);
                    });
                });
            } else {
                SignDictum(dictum);
            }
        } else {
            SaveDictumAnnex(dictum, function () {
                    doSaveDictum(dictum);
                               });
        }
    }
}

function doSaveDictum(dictum, callback) {
    $.ajax({
        type: "POST",
        url: BaseSiteURL + "/Plan/SaveDictum",
        data: JSON.stringify(dictum),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (typeof callback == 'function') {
                callback();
            } else {
                SaveDictumSuccess(data);
            }
        },
        error: function () {
            SaveDictumError();
        }
    });
}

function SignDictum(dictum) {
    var form = $('#frmSign')[0];
    var data = new FormData(form);
    data.append("planId", dictum.ImprovementPlanId);
    data.append("dictumId", dictum.Id);
    data.append("statusId", dictum.StatusId);
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: BaseSiteURL + "/Plan/SignDictum",
        data: data,
        processData: false,
        contentType: false,
        cache: false,
        timeout: 600000,
        success: function (data) {
            SaveDictumSuccess(data);
        },
        error: function (e) {
            SaveDictumError();
        }
    });
}


function SaveDictumAnnex(dictum,callback) {

    var form = $('#frmAnnex')[0];
    var data = new FormData(form);
    data.append("planId", dictum.ImprovementPlanId);
    data.append("dictumId", dictum.Id);
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: BaseSiteURL + "/Plan/SaveDictumAnnex",
        data: data,
        processData: false,
        contentType: false,
        cache: false,
        timeout: 600000,
        success: function (data) {
            
                callback();
            
        },
        error: function (e) {
            SaveDictumError();
        }
    });
}
function SaveDictumSuccess(data) {
    if (data != "") {
        $(".documents-wrapper").html(data);
        bindDocuments();
        showMessage("#msg-plan", "El Dictamen se guardó con éxito", "confirmation", true);
    } else {
        showMessage("#msg-plan", "Ocurrió un error al guardar el dictamen. Verifique que los Números de Expediente hayan sido cargados.", "error", true);
    }
    hideLoading();
    $("#new-dictum").dialog("close");
}

function SaveDictumError() {
    showMessage("#msg-plan", "Ocurrió un error al guardar el dictamen", "error", true);
    hideLoading();
    $("#new-dictum").dialog("close");
}