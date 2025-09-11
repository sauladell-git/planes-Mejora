/* 
    Este archivo contiene las acciones comunes de los dictamenes que se utilizan en la 
    página de detalles como en el listado de resoluciones. Las acciones particulares de cada  una 
    se  encuentran en resolutionsDetails.js para los detalles y resolutionsList.js para el listado.
*/

$(document).ready(function () {

    $("#signDate").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#annexSignDate").datepicker({ dateFormat: 'dd/mm/yy' });

    $("#DocumentDTO_ResolutionDTO_StatusId").change(changeResolutionStatus);

    $("#new-resolution").dialog({
        autoOpen: false,
        resizable: false,
        width: 1000,
        modal: true,
        open: function () {
            if ($(this).attr("data-documentId") == "0") {
                //$("#DocumentDTO_Number").val("");
                //$("#DocumentDTO_ResolutionDTO_FileNumber").val("");
                $("#DocumentDTO_ResolutionDTO_TemplateTypeId").val(4).change().hide().prev().hide();
                $("#DocumentDTO_ResolutionDTO_TemplateId").val(99).change().hide().prev().hide();
                showResolutionTabs(0, $(this).attr("data-originalStatusId"));
                $("#signDate").val();
                $("#annexSignDate").val();
                $("#fileSigned").val();
                $("#annex").val();
                $("#li-variables").hide();
                $("#li-res-preview").hide();
                $("#li-annex").hide();
                $("#li-resolution-number").hide();
                $("#msg-resolution").empty();
            } else {
                if ($("#DocumentDTO_ResolutionDTO_TemplateId").val() != 99) {
                    $("#DocumentDTO_ResolutionDTO_TemplateTypeId").show().prev().show();
                    $("#DocumentDTO_ResolutionDTO_TemplateId").show().prev().show();
                }
                showResolutionTabs($(this).attr("data-documentId"), $(this).attr("data-originalStatusId"));
            }
            $("#frmResolution").validate().resetForm();

            $("#li-resolutions").find("a").click();
            enableResolution($(this).attr("data-documentId") == "0");

            // Muestro el monto total
            var totalAmmount = parseFloat(0);
            var rows = $("#list-resolutions > tbody > tr");
            $.each(rows, function (i, item) {
                if ($(this).attr("data-requestedTotal") != undefined) {
                    var ammount = $(this).attr("data-requestedTotal").replace(",", ".");
                    totalAmmount += parseFloat(parseFloat(ammount).toFixed(4));
                }
            });

            $("#resolution-ammount").text("$" + parseFloat(parseFloat(totalAmmount).toFixed(4)).toCurrency());
        },
        close: function () {
            // Por defecto, vuelvo a dejar siempre visible el botón de guardar
            $(".ui-dialog-buttonset").find("button :first").show();
        },
        buttons: {
            "Guardar": function () {
                saveResolution();
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#resolution-preview").click(previewResolution);

    // Me aseguro de que se valide el formulario del anexo cuando corresponda, aunque no este el tab seleccionado
    $("#frmAnnex").data("validator").settings.ignore = "";

    // Me aseguro de que se valide el formulario del número de disposición cuando corresponda, aunque no este el tab seleccionado
    //$("#frmResolutionNumber").data("validator").settings.ignore = "";

    // Combos 
    $("#DocumentDTO_ResolutionDTO_TemplateTypeId").change(listResolutionTemplates);
    $("#DocumentDTO_ResolutionDTO_TemplateId").change(listDocumentDictums);

    // Fecha de Envío
    //$("#DocumentDTO_ResolutionDTO_ShipDate").datepicker({ dateFormat: 'dd/mm/yy' });

    bindResolutions();
});

function enableResolution(enable) {
    if (enable) {
        $("#DocumentDTO_ResolutionDTO_TemplateTypeId").removeAttr("disabled");
        $("#DocumentDTO_ResolutionDTO_TemplateId").removeAttr("disabled");
        $("#DocumentDTO_ResolutionDTO_StatusId").removeAttr("disabled");
        //$("#DocumentDTO_ResolutionDTO_FileNumber").removeAttr("disabled");
    } else {
        $("#DocumentDTO_ResolutionDTO_TemplateTypeId").attr("disabled", "disabled");
        $("#DocumentDTO_ResolutionDTO_TemplateId").attr("disabled", "disabled");
        //$("#DocumentDTO_ResolutionDTO_FileNumber").attr("disabled", "disabled");

        // Si estoy visualizando la disposición, o la mismo está firmada, protocolizada o anulada, entonces no puedo cambiar el estado
        if ($("#new-resolution").attr("data-action") == "view"
                //|| $("new-resolution").attr("data-originalStatusId") == "27"
                || $("new-resolution").attr("data-originalStatusId") == "28"
            || $("new-resolution").attr("data-originalStatusId") == "29"
        ) {
            $("#DocumentDTO_ResolutionDTO_StatusId").attr("disabled", "disabled");
        } else {
            $("#DocumentDTO_ResolutionDTO_StatusId").removeAttr("disabled");
        }
    }
}

function blockResolution() {
    $("#block-confirm-resolution")
        .attr("data-documentId", $(this).attr("data-documentId"))
        .dialog("open");
}

function listResolutionTemplates() {
    // Cargo las plantillas dependiendo el tipo de template
    $("#DocumentDTO_ResolutionDTO_TemplateId").empty();
    $("<option>").attr("value", "").text("Seleccionar...").appendTo("#DocumentDTO_ResolutionDTO_TemplateId");

    if ($("#DocumentDTO_ResolutionDTO_TemplateTypeId").val() != "") {
        var wsUrl = "/Plan/ListTemplates";
        var params = {
            templateTypeId: $("#DocumentDTO_ResolutionDTO_TemplateTypeId").val()
        };
        if ($('#planId').length > 0 && ('#planId').val() != '') {
            wsUrl = "/Plan/ListTemplatesForCreation";
            params.planId = $('#planId').val();
        }
        showLoading();

        $.ajax({
            type: 'GET',
            url: BaseSiteURL + wsUrl,
            data: params,
            success: function (data) {
                $.each(data, function (i, item) {
                    $("<option>").attr("value", item.Id).text(item.Name).appendTo("#DocumentDTO_ResolutionDTO_TemplateId");
                });

                // Seteo el valor en el combo en caso de que lo este editando
                if ($("#CurrentTemplateId").val() != "")
                    $("#DocumentDTO_ResolutionDTO_TemplateId").val($("#CurrentTemplateId").val());
            },
            async: false
        }).done(function () {
            hideLoading();
        });;

        //$.get(BaseSiteURL + wsUrl, params).done(function (data) {
        //    $.each(data, function (i, item) {
        //        $("<option>").attr("value", item.Id).text(item.Name).appendTo("#DocumentDTO_ResolutionDTO_TemplateId");
        //    });

        //    // Seteo el valor en el combo en caso de que lo este editando
        //    if ($("#CurrentTemplateId").val() != "")
        //        $("#DocumentDTO_ResolutionDTO_TemplateId").val($("#CurrentTemplateId").val());
        //}).done(function () {
        //    hideLoading();
        //});
    }

    listDocumentDictums();
}

function listDocumentDictums() {
    if ($("#new-resolution").attr("data-documentId") == "0" && $("#DocumentDTO_ResolutionDTO_TemplateId :selected").val() != "") {

        var params = {
            templateId: $("#DocumentDTO_ResolutionDTO_TemplateId :selected").val(),
        };
        showLoading();
        $.get(BaseSiteURL + "/Resolution/ListDictumsForResolutionSelection", params).done(function (data) {
            $("#dictums-selector").html(data);
            $('.dictum-checkbox').click(updateResolutionAmmount);
            bindResolutions();
        }).done(function () {
            hideLoading();
        });
    } else {
        $('#list-resolutions').dataTable().fnClearTable();
    }
}

function updateResolutionAmmount() {
    var totalAmmount = parseFloat(0);
    var selected = $("#list-resolutions").find("input[type='checkbox']:checked");
    $.each(selected, function (i, item) {
        var _total = $(this).parent().parent().attr("data-requestedTotal");
        var ammount = _total != undefined && _total != '' ? _total.replace(",", ".") : 0;
        totalAmmount += parseFloat(parseFloat(ammount).toFixed(4));
    });

    $("#resolution-ammount").text("$" + parseFloat(parseFloat(totalAmmount).toFixed(4)).toCurrency());
}

function bindResolutions() {
    $('#list-resolutions').dataTable({
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": false,
        "bInfo": false,
        "bAutoWidth": false
    });
}

function validateResolution() {
    if (validateForm("#frmResolution", "#msg-resolution", "Por favor, complete todos los campos obligatorios y seleccione al menos un dictamen")) {
        // Valido que pueda guardar en borrador
        var arraySelected = new Array();
        var selected = $("#list-resolutions").find("input[type='checkbox']:checked");
        $.each(selected, function (i, item) {
            arraySelected.push($(this).parent().parent().attr("data-selectorId"));
        });

        if ($("#new-resolution").attr("data-originalStatusId") == "24" && selected.length == 0) {
            showMessage("#msg-resolution", "Por favor, complete todos los campos obligatorios y seleccione al menos un dictamen", "error");
            return null;
        }

        var documentVariables = $("#resolution-variables-wrapper").find(".custom-input");
        var arrayDocumentVariables = new Array();
        $.each(documentVariables, function (index, item) {
            arrayDocumentVariables.push({ TemplateVariableId: $(this).attr("data-id"), VariableText: $(this).val() });
        });

        var resolution = {
            Id: $("#new-resolution").attr("data-documentId"),
            ImprovementPlanId: $("#new-resolution").attr("data-improvementPlanId"),
            ResolutionNumber: $("#DocumentDTO_ResolutionDTO_ResolutionNumber").val(),
            StatusId: $("#DocumentDTO_ResolutionDTO_StatusId").val(),
            Body: "",
            AmmountExecuted: parseFloat($("#resolution-ammount").text().replace("$", "")),
            TemplateId: $("#DocumentDTO_ResolutionDTO_TemplateId :selected").val(),
            ShipDate: $("#DocumentDTO_ResolutionDTO_ShipDate").val(),
            Variables: arrayDocumentVariables,
            DictumsIds: arraySelected
        };

        return resolution;
    }

    return null;
}

function validateResolutionSign() {
    //if ($('#signDate').val() == "" || $('#annexSignDate').val() $('#DocumentDTO_ResolutionDTO_ResolutionNumber').val() == "" || ($('#annex').val() == "" && !$('#annex-link').is(':visible')) || ($('#fileSigned').val() == "" && !$('#signedDocument-link').is(':visible'))) {
        if ($('#signDate').val() == ""  || ($('#fileSigned').val() == "" && !$('#signedDocument-link').is(':visible'))) {
        showMessage("#msg-resolution", "Por favor, complete todos los campos obligatorios", "error");
        return false;
    } else {
        return true;
    }
}

function editResolution() {
    showLoading();
    var resolutionId = $(this).attr("data-documentId");
    var improvementPlanId = $(this).attr("data-improvementPlanId");
    var statusId = $(this).attr("data-originalStatusId");
    var templateId = $(this).attr("data-templateId");
    var action = $(this).hasClass("view-resolution") ? "view" : "edit";

    if (action == 'view') {
        $("#new-resolution").find("select, input").attr("disabled", "disabled");
        showResolutionTabs(resolutionId, statusId);
        $(".ui-dialog-buttonset").find("button :first").hide();
    } else {
        $("#new-resolution").find("select, input").removeAttr("disabled");
        $(".ui-dialog-buttonset").find("button :first").show();
    }

    $.get(BaseSiteURL + "/Resolution/GetResolutionEditData", {
        resolutionId: resolutionId
    }).done(function (data) {
        // Limpio mensaje de error
        $("#msg-resolution").empty();
        $(".dynamic-button").remove();

        // Seteo el valor en el hidden para poder cargarlo en el combo de plantillas que se carga por Ajax
        $("#CurrentTemplateId").val(data.TemplateId);

        // Seteo el Id del documento
        $("#new-resolution").attr("data-documentId", resolutionId);
        $("#new-resolution").attr("data-templateId", templateId);

        // Seteo el resto de los combos
        $("#DocumentDTO_ResolutionDTO_TemplateTypeId").val(data.TemplateTypeId);
        $("#DocumentDTO_ResolutionDTO_StatusId").val(data.StatusId);
        $("#DocumentDTO_ResolutionDTO_ShipDate").val(data.ShipDate);
        $("#DocumentDTO_ResolutionDTO_ResolutionNumber").val(data.ResolutionNumber);
        $("#signDate").val(data.SignatureDate);
        $("#annexSignDate").val(data.AnnexSignatureDate);

        toggleSignedDocumentField(data.SignedDocument);
        toggleAnnexField(data.Annex);
        

        //// Limpio combo de expedientes y lo cargo con lo que corresponda
        //$("#DocumentDTO_ResolutionDTO_FileNumber").empty();
        //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#DocumentDTO_ResolutionDTO_FileNumber");
        //$.each(data.FileNumbers, function (index, item) {
        //    $("<option>").attr("value", data.FileNumbers[index].Key).text(data.FileNumbers[index].Value).appendTo("#DocumentDTO_ResolutionDTO_FileNumber");
        //});
        //$("#DocumentDTO_ResolutionDTO_FileNumber").val(data.FileNumber);


        // Limpio combo de templates y cargo lo que corresponda
        $("#DocumentDTO_ResolutionDTO_TemplateId").empty();
        $("<option>").attr("value", "").text("Seleccionar...").appendTo("#DocumentDTO_ResolutionDTO_TemplateId");
        $.each(data.Templates, function (index, item) {
            $("<option>").attr("value", data.Templates[index].Id).text(data.Templates[index].Name).appendTo("#DocumentDTO_ResolutionDTO_TemplateId");
        });
        $("#DocumentDTO_ResolutionDTO_TemplateId").val(data.TemplateId);

        // Seteo numero de disposición externa
        $("#DocumentDTO_ResolutionDTO_ResolutionNumber").val(data.ResolutionNumber);

        $('#list-resolutions').dataTable().fnClearTable(); // Limpia la tabla

        // Agrego los solicitados del dictamen
        $.each(data.Dictums, function (index, item) {
            // Agrego fila
            var oTable = $('#list-resolutions').dataTable();

            var chk = "";
            if (data.StatusId == "24") { // Borrador
                chk = "<input type='checkbox' class='checkbox dictum-checkbox' checked='checked' style='margin-left:50px;' />";
            }

            var api = oTable.fnAddData([chk, item.Identifier, item.Status, item.SignatureDate, item.Ammount]);
            // Agrego el atributo con el id del solicitado a la fila
            var $row = $(oTable.fnSettings().aoData[api[0]].nTr)
            $row.attr("data-selectorId", item.Id);
            $row.attr("data-requestedTotal", item.Ammount);

            // Chequeo los solicitados            
            var $checkbox = $row.find("input[type='checkbox']");
            $checkbox.prop('checked', true);
            $checkbox.click(updateResolutionAmmount);
        });

        // Cargo las variables existentes para este tipo de documento
        $("#resolution-variables-wrapper").empty();
        $.each(data.TemplateVariables, function (index, item) {
            var $variable = $("<div>").addClass("variable");
            $variable.append($("<label>").text(item.Variable));
            $variable.append($("<input>").attr("type", "textbox")
                                         .attr("maxlength", "500")
                                         .attr("data-id", item.Id)
                                         .attr("name", "DocumentVariables")
                                         .addClass("custom-input"));

            $("#resolution-variables-wrapper").append($variable);
        });

        $.each(data.DocumentVariables, function (index, item) {
            $("input[data-id='" + item.TemplateVariableId + "']").val(item.Text);
        });

        if (action == 'view')
            $("#resolution-variables-wrapper").find("input").attr("disabled", "disabled");
        else
            $("#resolution-variables-wrapper").find("input").removeAttr("disabled");

        updateResolutionAmmount();

        $("#new-resolution").attr("data-improvementPlanId", improvementPlanId).attr("data-originalStatusId", data.StatusId).attr("data-action", action).dialog("open");

        hideLoading();
    });

    return false;
}

function toggleSignedDocumentField(document) {
    $("#fileSigned").val("");
    if (document != "" && document != null) {
        // $("#signedDocumentDiv-input").hide();
        $("#signedDocumentDiv-link").show();
        $("#signedDocument-link").attr("href", $("#signedDocument-link").attr('data-urlbase') + document);
    } else {
        // $("#signedDocumentDiv-input").show();
        $("#signedDocumentDiv-link").hide();
        $("#signedDocument-link").attr("href", "");
    }
}

function toggleAnnexField(document) {
    $("#annex").val("");
    if (document != "" && document != null) {
        // $("#annexDiv-input").hide();
        $("#annexDiv-link").show();
        $("#annex-link").attr("href", $("#annex-link").attr('data-urlbase') + document);
    } else {
        // $("#annexDiv-input").show();
        $("#annexDiv-link").hide();
        $("#annex-link").attr("href", "");
    }
}

function deleteResolution() {
    $("#delete-confirm-resolution")
        .attr("data-documentId", $(this).attr("data-documentid"))
        .dialog("open");
}

function previewResolution(e) {
    e.preventDefault();

    $("#htmlContent").val("");
    $("#documentId").val($(this).parent().parent().parent().attr("data-documentId"));
    $("#templateId").val($(this).parent().parent().parent().attr("data-templateId"));
    $("#previewDocumentVariables").empty();

    // Agrego variables de documentos
    var documentVariables = $("#resolution-variables-wrapper").find(".custom-input");
    $.each(documentVariables, function (index, item) {
        setInput("", index, "Variables[" + index + "].TemplateVariableId", $(this).attr("data-id"), "#previewDocumentVariables");
        setInput("", index, "Variables[" + index + "].VariableText", $(this).val(), "#previewDocumentVariables");
    });

    $("#frmPreview").submit();
}

var LoadStatusCombo = function (resolutionId, callback) {
    $.get(BaseSiteURL + "/Resolution/LoadStatusCombo", {
        resolutionId: resolutionId
    }).done(function (data) {
        // Cargo el combo de Estados
        $("#DocumentDTO_ResolutionDTO_StatusId").empty();
        $.each(data.Statuses, function (i, item) {
            $("<option>").attr("value", item.Id).text(item.Description).appendTo("#DocumentDTO_ResolutionDTO_StatusId");
        });
        if (typeof callback != "undefined") callback();
    }).fail(function () {
        showMessage("#msg-resolutions", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        hideLoading();
    });
}

function changeResolutionStatus(evt) {
    var statusId = $("#DocumentDTO_ResolutionDTO_StatusId").val();
    switch (statusId) {
        case "26":
            $("#li-res-preview").show();
            break;
        case "27":
            $("#li-resolution-number").show();
            $("#li-annex").show();
            break;
        default:
            $("#li-variables").hide();
            $("#li-res-preview").hide();
            $("#li-annex").hide();
            $("#li-resolution-number").hide();
            break;
    }
    
}

function showResolutionTabs(resolutionId, statusId) {
    if (statusId == "0" || statusId == "") return;

    if (resolutionId != "0") {
        $('.dictum-checkbox').prop('disabled', true);
    } else {
        $('.dictum-checkbox').prop('disabled', false);
    }

    LoadStatusCombo(resolutionId, function () {
        $("#DocumentDTO_ResolutionDTO_StatusId").val(statusId);
    });

    $("#li-variables").hide();
    $("#li-res-preview").hide();
    $("#li-annex").hide();
    $("#li-resolution-number").hide();
    
    switch (statusId) {
        case "26":
            //$("#li-res-preview").show();
            break;
        case "27": // Firmado
            if ($("#DocumentDTO_ResolutionDTO_ResolutionNumber").val() != "" || $("#DocumentDTO_ResolutionDTO_ShipDate").val() != "")
            $("#li-annex").show();
            break;

        case "28": // Protocolizado
            // Oculto tabs            
            if ($("#DocumentDTO_ResolutionDTO_ResolutionNumber").val() != "" || $("#DocumentDTO_ResolutionDTO_ShipDate").val() != "")
                $("#li-resolution-number").show();
            break;

        case "29":
            // Oculto tabs
            if ($("#DocumentDTO_ResolutionDTO_ResolutionNumber").val() != "" || $("#DocumentDTO_ResolutionDTO_ShipDate").val() != "")
                $("#li-resolution-number").show();
            break;

        default:
            break;
    }
}