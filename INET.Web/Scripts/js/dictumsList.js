var _apiDataTable;

$(document).ready(function () {

    $.fn.dataTableExt.oApi.fnReloadAjax = function (oSettings, sNewSource, myParams) {
        if (oSettings.oFeatures.bServerSide) {
            if (typeof sNewSource != 'undefined' && sNewSource != null) {
                oSettings.sAjaxSource = sNewSource;
            }
            oSettings.aoServerParams = [];
            oSettings.aoServerParams.push({
                "sName": "user",
                "fn": function (aoData) {
                    for (var index in myParams) {
                        aoData.push({ "name": index, "value": myParams[index] });
                    }
                }
            });
            this.fnClearTable(oSettings);
            return;
        }
    };

    _apiDataTable = $('#list-documents').dataTable({
        "bPaginate": true,
        "iDisplayLength": 20,
        "bLengthChange": false,
        "bFilter": false,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false,
        "bServerSide": true,
        "sAjaxSource": BaseSiteURL + "/Dictum/Search",
        "sDom": '<"toolbar">frtip',
        "aoColumnDefs": [
            { "bSortable": false, "aTargets": [0, 5] }
        ]
    });

    _apiDataTable.on('preXhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', .4);
    });
    _apiDataTable.on('xhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', 1);
    })
    _apiDataTable.on('order.dt', function (e, settings, data) {
    });
    bindDocuments();


    $("#frmSearchDictums").submit(function (e) {
        e.preventDefault();
        $("#msg-search").empty();
        var msg = ""; //validate();
        if (msg == "") {
            $("#msg-search").fadeOut("slow");
            $("#msg-search").empty();

            var filters = {
                iCustomSearch_StatusId: $("#Filters_StatusId :selected").val(),
                sCustomSearch_CUE: $("#Filters_CUE").val(),
                sCustomSearch_Dictum: $("#Filters_Dictum").val(),
                sCustomSearch_Resolution: $("#Filters_Resolution").val(),
                iCustomSearch_FieldId: $("#Filters_FieldId :selected").val(),
                sCustomSearch_Identifier: $("#Filters_ImprovementPlanIdentifier").val(),
                sCustomSearch_ResolutionId: $("#Filters_ResolutionId").val()
            };

            _apiDataTable.fnReloadAjax(_apiDataTable.oSettings, filters);
        } else {
            showMessage("#msg-search", msg, "attention", false);
        }
        return false;
    });

    $("#delete-confirm-dictum").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                $.post(BaseSiteURL + "/Dictum/DeleteDictum", {
                    improvementPlanId: $(this).attr("data-improvementPlanId"),
                    documentId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        _apiDataTable.api().ajax.reload();
                        showMessage("#msg-dictums", "Se eliminó el dictamen con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-dictums", "No se pudo eliminar el Dictamen", "error", true);
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
                $.post(BaseSiteURL + "/Dictum/BlockDictum", {
                    improvementPlanId: $(this).attr("data-ImprovementPlanId"),
                    dictumId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        _apiDataTable.api().ajax.reload();
                        showMessage("#msg-dictums", "El dictamen se anuló con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-dictums", "No es posible anular el dictamen.", "error", true);
                    }

                    $("#block-confirm-dictum").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
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

        if ($("#DocumentDTO_DictumDTO_StatusId").val() == "19") {
            if (originalStatusId == "16") {
                // si está mandando de borrador a firmado lo guardamos emitido y luego firmamos
                //    SaveDictumAnnex(dictum)
                SaveDictumAnnex(dictum, function () // primero grabo archivo de anexo!
                     {
                            dictum.StatusId = 17;
                    doSaveDictum(dictum, function ()
                           {
                                dictum.StatusId = 18;
                                doSaveDictum(dictum, function () {
                                    dictum.StatusId = 19;
                                    SignDictum(dictum);
                                });
                            });
                     });
            } else {
                SignDictum(dictum);
            }
        } else {
            SaveDictumAnnex(dictum, function () { // primero grabo archivo de anexo!
                doSaveDictum(dictum);
            });
        }
    }
}
function SaveDictumAnnex(dictum,callback) {
    
    var form = $('#frmAnnex')[0];
    var data = new FormData(form);
    data.append("planId", dictum.ImprovementPlanId);
    data.append("dictumId", dictum.Id);
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: BaseSiteURL + "/Dictum/SaveDictumAnnex",
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

function doSaveDictum(dictum, callback) {
    $.ajax({
        type: "POST",
        url: BaseSiteURL + "/Dictum/SaveDictum",
        data: JSON.stringify(dictum), // Usamos JSON.stringfy por el array
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (typeof callback == 'function') {
                callback();
            } else {
                SaveDictumSuccess();
            }
        },
        error: function (e) {
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

function SaveDictumSuccess(data) {
    if (data != "") {
        _apiDataTable.api().ajax.reload();
        showMessage("#msg-dictums", "El Dictamen se guardó con éxito", "confirmation", true);
    } else {
        showMessage("#msg-dictums", "Ocurrió un error al guardar el dictamen", "error", true);
    }
    hideLoading();
    $("#new-dictum").dialog("close");
}

function SaveDictumError() {
    showMessage("#msg-dictums", "Ocurrió un error al guardar el dictamen", "error", true);
    hideLoading();
    $("#new-dictum").dialog("close");
}