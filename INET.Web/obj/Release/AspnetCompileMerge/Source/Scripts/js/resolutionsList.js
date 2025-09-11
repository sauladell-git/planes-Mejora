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
        "sAjaxSource": BaseSiteURL + "/Resolution/Search",
        "aaSorting": [[4, "desc"]],
        "sDom": '<"toolbar">frtip',
        "aoColumnDefs": [
            { "bSortable": false, "aTargets": [0,3, 6] }
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

    $("#frmSearchResolutions").submit(function (e) {
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

    $("#delete-confirm-resolution").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                $.post(BaseSiteURL + "/Resolution/DeleteResolution", {
                    documentId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        var current_page = _apiDataTable.api().page();
                        _apiDataTable.api().ajax.reload();
                        showMessage("#msg-resolutions", "Se eliminó la disposición con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-resolutions", "No se pudo eliminar la disposición.", "error", true);
                    }

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
                $.post(BaseSiteURL + "/Resolution/BlockResolution", {
                    resolutionId: $(this).attr("data-documentId")
                }).done(function (data) {
                    if (data != "") {
                        var current_page = _apiDataTable.api().page();
                        _apiDataTable.api().ajax.reload();
                        showMessage("#msg-resolutions", "La disposición se anuló con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-resolutions", "No es posible anular la disposición.", "error", true);
                    }

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
        $("#new-resolution").attr("data-documentId", 0).attr("data-improvementPlanId", "").attr("data-originalStatusId", "24").dialog("open");
    });

});

function saveResolution() {
    var originalStatusId = $('#new-resolution').attr('data-originalstatusid');

    $('#list-resolutions_filter input[type=search]').val('').trigger('keyup');
    var resolution = validateResolution();

    if (resolution != null) {
        if (!canSubmitForm(getDialogFirstButton("#new-resolution"))) {
            
            return false;
        }

        showLoading();
        // Si quiero firmar el documento, submiteo solo el formulario de firma ya que contiene un archivo y no puede hacerse por AJAX
        if ($("#DocumentDTO_ResolutionDTO_StatusId :selected").val() == "27") {
            if (validateResolutionSign()) {
                $("#resolutionId").val($("#new-resolution").attr("data-documentId"));
                $("#resolutionPlanId").val(0);

                // Cambio la accion del formulario
                $("#frmAnnex").attr("action", BaseSiteURL + "/Resolution/SignResolution");

                var form = $('#frmAnnex')[0];
                var formData = new FormData(form);

                if (originalStatusId == "24") {
                    resolution.StatusId = 25;
                    doSaveResolution(resolution, function () {
                        resolution.StatusId = 26;
                        doSaveResolution(resolution, function () {
                            SignResolution(formData);
                        });
                    });
                } else {
                    SignResolution(formData);
                }

            } else {
                hideLoading();
                enableSubmitButtons();
            }
            return false;
        } else {
            doSaveResolution(resolution);
        }
    }
}

function doSaveResolution(resolution, callback) {
    $.ajax({
        type: "POST",
        url: BaseSiteURL + "/Resolution/SaveResolution",
        data: JSON.stringify(resolution), // Usamos JSON.stringfy por el array
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data != "") {
                if (typeof callback == 'function') {
                    callback();
                } else {
                    var current_page = _apiDataTable.api().page();
                    _apiDataTable.api().ajax.reload();
                    showMessage("#msg-resolutions", "La disposición se guardó con éxito", "confirmation", true);
                }
            } else {
                showMessage("#msg-resolutions", "Ocurrió un error al guardar la disposición", "error", true);
            }

            enableSubmitButtons();
            $("#new-resolution").dialog("close");
            hideLoading();
        }
    });
}

function SignResolution(formData) {
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: BaseSiteURL + "/Resolution/SignResolution",
        data: formData,
        processData: false,
        contentType: false,
        cache: false,
        timeout: 600000,
        success: function (data) {
            if (data != "") {
                _apiDataTable.api().ajax.reload();
                showMessage("#msg-resolutions", "La disposición se guardó con éxito", "confirmation", true);
            } else {
                showMessage("#msg-resolutions", "Ocurrió un error al guardar la disposición", "error", true);
            }
            hideLoading();
            enableSubmitButtons();
            $("#new-resolution").dialog("close");
        },
        error: function (e) {
            showMessage("#msg-resolutions", "Ocurrió un error al guardar la disposición", "error", true);
            hideLoading();
            $("#new-resolution").dialog("close");
            enableSubmitButtons();
        }
    });
}