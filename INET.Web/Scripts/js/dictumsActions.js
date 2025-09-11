/* 
    Este archivo contiene las acciones comunes de los dictamenes que se utilizan en la 
    página de detalles como en el listado de dictamenes. Las acciones particulares de cada 
    una se  encuentran en dictumDetails.js para los detalles y dictumsList para el listado.
*/

$(document).ready(function () {

    $("#DocumentDTO_DictumDTO_SignDate").datepicker({ dateFormat: 'dd/mm/yy' });

    $("#DocumentDTO_DictumDTO_StatusId").change(function () {
        showDictumTabs($("#new-dictum").attr("data-documentId"));
    });
    
    $("#new-dictum").dialog({
        autoOpen: false,
        resizable: false,
        width: 1000,
        modal: true,
        open: function () {

            if ($(this).attr("data-documentId") == "0") {
                $("#DocumentDTO_Number").val("");
                $("#DocumentDTO_DictumDTO_SignDate").val("");
                $("#DocumentDTO_DictumDTO_FileNumber").val("");
                $("#DocumentDTO_DictumDTO_TemplateTypeId").val("").change();
                $("#DocumentDTO_DictumDTO_StatusId").val("");
                $("#li-body-dictum").hide();
                $("#li-preview").hide();
                $("#li-eligibility").hide();
                $("#li-sign").hide();
                $("#msg-dictum").empty();
                LoadCombos(0);
                $("#DocumentDTO_DictumDTO_FileNumber").textify(false);
                toggleSignedDocumentField("");
                $("#annexDocument-link").attr("href", "");

            } else {
                showDictumTabs($(this).attr("data-documentId"));
                $("#DocumentDTO_DictumDTO_FileNumber").textify();
            }
            $("#frmDictum").validate().resetForm();

            $("#li-solicitudes").find("a").click();
            enableDictum($(this).attr("data-documentId") == "0");

            if ($('#list-dictums > tbody > tr').length > 0) {
                $("#solicitude-selector").show();
            } else {
                $("#solicitude-selector").hide();
            }

            $(".solicitudes-all-selectors").hide();

        },
        close: function () {
            // Por defecto, vuelvo a dejar siempre visible el botón de guardar
            $(".ui-dialog-buttonset").find("button:first").show();
        },
        buttons: {
            "Guardar": function () {
                saveDictum();
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".select_all_solicitudes_in_dictum").click(function () {
        $(".solicitude-checkbox").filter(':visible').prop("checked", true);
        updateAmmount()
        return false;
    });

    $(".select_none_solicitudes_in_dictum").click(function () {
        $(".solicitude-checkbox").prop("checked", false);
        updateAmmount()
        return false;
    });

    $("#dictum-preview").click(previewDictum);

    // Inicializo el tinyMCE
    initializeTinyMCE("textarea#dictum-tinymce");

    // Me aseguro de que se valide el formulario de elegibilidad (aunque no este visible) cuando corresponda
    $("#frmEligibility").data("validator").settings.ignore = "";

    // Dictamenes
    $("#DocumentDTO_DictumDTO_TemplateTypeId").change(listDictumTemplates);
    $("#DocumentDTO_DictumDTO_FileNumber").change(listDocumentSolicitudes);

    bindDictums();
});

var LoadCombos = function (dictumId, callback) {
    $.get(BaseSiteURL + "/Plan/LoadDictumCombos", {
        dictumId: dictumId
    }).done(function (data) {

        // Cargo el combo de Estados
        $("#DocumentDTO_DictumDTO_StatusId").empty();
        $.each(data.Statuses, function (i, item) {
            $("<option>").attr("value", item.Id).text(item.Description).appendTo("#DocumentDTO_DictumDTO_StatusId");
        });

        if (typeof callback != "undefined") callback();

    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        showMessage("#msg-dictums", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        hideLoading();
    });;
}

function enableDictum(enable) {
    if (enable) {
        $("#DocumentDTO_DictumDTO_TemplateTypeId").removeAttr("disabled");
        $("#DocumentDTO_DictumDTO_TemplateId").removeAttr("disabled");
        $("#DocumentDTO_DictumDTO_FileNumber").removeAttr("disabled");
        $("#DocumentDTO_DictumDTO_StatusId").removeAttr("disabled");
    } else {
        $("#DocumentDTO_DictumDTO_TemplateTypeId").attr("disabled", "disabled");
        $("#DocumentDTO_DictumDTO_TemplateId").attr("disabled", "disabled");
        $("#DocumentDTO_DictumDTO_FileNumber").attr("disabled", "disabled");

        // Si estoy visualizando el dictamen, o el mismo esta firmado o anulado, entonces no puedo cambiar el estado
        if ($("#new-dictum").attr("data-action") == "view" || $("new-dictum").attr("data-originalStatusId") == "19" || $("new-dictum").attr("data-originalStatusId") == "20") {
            $("#DocumentDTO_DictumDTO_StatusId").attr("disabled", "disabled");
        } else {
            $("#DocumentDTO_DictumDTO_StatusId").removeAttr("disabled");
        }
    }

}

function blockDictum() {
    $("#block-confirm-dictum")
        .attr("data-documentId", $(this).attr("data-documentId"))
        .attr("data-improvementPlanId", $(this).attr("data-improvementPlanId"))
        .dialog("open");
}

function listDictumTemplates() {
    // Cargo las plantillas dependiendo el tipo de template
    $("#DocumentDTO_DictumDTO_TemplateId").empty();
    $("<option>").attr("value", "").text("Seleccionar...").appendTo("#DocumentDTO_DictumDTO_TemplateId");

    if ($("#DocumentDTO_DictumDTO_TemplateTypeId").val() != "") {
        var wsUrl = "/Plan/ListTemplates"; // es edición
        var params = {
            templateTypeId: $("#DocumentDTO_DictumDTO_TemplateTypeId").val()
        }
        if ($("#CurrentTemplateId").val() == "") { // es creación
            wsUrl = "/Plan/ListTemplatesForCreation";
            params.planId = $("#planId").val();
        }
        $.get(BaseSiteURL + wsUrl, params).done(function (data) {
            $.each(data, function (i, item) {
                $("<option>").attr("value", item.Id).text(item.Name).appendTo("#DocumentDTO_DictumDTO_TemplateId");
            });

            // Seteo el valor en el combo en caso de que lo este editando
            if ($("#CurrentTemplateId").val() != "")
                $("#DocumentDTO_DictumDTO_TemplateId").val($("#CurrentTemplateId").val());
        }).fail(function () {
            alert("Ocurrió un error recuperando la información requerida");
        });
    }

    if ($("#DocumentDTO_DictumDTO_TemplateTypeId :selected").val()) listDocumentSolicitudes();
}

function updateAmmount() {
    var totalAmmount = parseFloat(0);
    var selected = $("#list-dictums").find("input[type='checkbox']:checked");
    $.each(selected, function (i, item) {
        if ($(this).parent().parent().attr("data-approvedTotal") != undefined) {
            var ammount = $(this).parent().parent().attr("data-approvedTotal").replace(",", ".");
            totalAmmount += parseFloat(parseFloat(ammount).toFixed(4));
        }
    });

    $("#dictum-ammount").text("$" + parseFloat(parseFloat(totalAmmount).toFixed(4)).toCurrency());
}

function bindDictums() {
    var _apiDataTable = $('#list-dictums').dataTable({
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": false,
        "bInfo": false,
        "bAutoWidth": false
    });

    _apiDataTable.on('search.dt', function (e, settings, data) {
        showLoading();
    });

    _apiDataTable.on('draw.dt', function (e, settings, data) {
        hideLoading();
    });
}

function validateDictum() {
    var originalStatusId = $('#new-dictum').attr('data-originalstatusid');

    if (validateForm("#frmDictum", "#msg-dictum", "Por favor, complete todos los campos obligatorios y seleccione al menos un solicitado")) {
        // Valido que el cuerpo del dictamen tenga contenido para pasar a pendiente de aprobación
        var dictumBody = tinyMCE.get('dictum-tinymce');
        dictumBody.save();

        if ($("#DocumentDTO_DictumDTO_StatusId").val() == "17") {
            if ($.trim(dictumBody.getContent()) == "") {
                showMessage("#msg-dictum", "Por favor, ingrese el cuerpo del dictamen para poder continuar", "error");
                return null;
            }
        }

        if ($("#DocumentDTO_DictumDTO_StatusId").val() == "19") {

            if (originalStatusId == "16") {
                if ($.trim(dictumBody.getContent()) == "") {
                    showMessage("#msg-dictum", "Por favor, ingrese el cuerpo del dictamen para poder continuar", "error");
                    return null;
                }
            }

            if ($("#DocumentDTO_DictumDTO_SignDate").val() == '') {
                showMessage("#msg-dictum", "Por favor, seleccione una fecha de firma", "error");
                return null;
            }
            if ($('#signedDocument').val() == "") {
                showMessage("#msg-dictum", "Por favor, debe cargar el documento firmado digitalmente", "error");
                return null;
            }
        }

        var validateElegibility = $("#DocumentDTO_DictumDTO_StatusId").val() == "21" || $("#DocumentDTO_DictumDTO_StatusId").val() == "22" || $("#DocumentDTO_DictumDTO_StatusId").val() == "23";
        if (validateElegibility && (!$("#frmEligibility").valid() || ($.trim($("#ExternalNumber").val()) == "" && $.trim($("#DocumentDTO_DictumDTO_ExternalEntity").val()) == ""))) {
            showMessage("#msg-dictum", "Por favor, complete todos los campos obligatorios.", "error");
            return null;
        }

        var arraySelected = new Array();
        if ($("#new-dictum").attr("data-documentid") == "0") {
            var selected = $("#list-dictums").find("input[type='checkbox']:checked");

            if (selected.length == 0) {
                showMessage("#msg-dictum", "Por favor, complete todos los campos obligatorios y seleccione al menos un solicitado", "error");
                return null;
            }

            $.each(selected, function (i, item) {
                arraySelected.push($(this).parent().parent().attr("data-selectorId"));
            });
        }

        var dictum = {
            Id: $("#new-dictum").attr("data-documentId"),
            ImprovementPlanId: $("#new-dictum").attr("data-improvementPlanId"),
            TemplateTypeId: $("#DocumentDTO_DictumDTO_TemplateTypeId :selected").val(),
            Number: $("#DocumentDTO_Number").val(),
            StatusId: $("#DocumentDTO_DictumDTO_StatusId").val(),
            Body: dictumBody.getContent(),
            Ammount: parseFloat($("#dictum-ammount").text().replace("$", "")),
            FileNumber: $("#DocumentDTO_DictumDTO_FileNumber :selected").val(),
            Balance: parseFloat(0),
            TemplateId: $("#DocumentDTO_DictumDTO_TemplateId :selected").val(),
            RequiredEligibilityId: $("#DocumentDTO_DictumDTO_RequiredEligibilityId :selected").val(),
            ExternalNumber: $("#ExternalNumber").val(),
            ExternalEntity: $("#ExternalEntity").val(),
            SignDate: $("#DocumentDTO_DictumDTO_SignDate").val(),
            SolicitudesIds: arraySelected
        };

        return dictum;
    }

    return null;
}

function editDictum() {
    showLoading();
    var dictumBody = tinyMCE.get('dictum-tinymce');

    var dictumId = $(this).attr("data-documentId");
    var improvementPlanId = $(this).attr("data-improvementPlanId");
    var templateId = $(this).attr("data-templateId");
    var action = $(this).hasClass("view-dictum") ? "view" : "edit";

    if (action == "edit") {
        $("#new-dictum").find("select, input").removeAttr("disabled");
        dictumBody.getBody().setAttribute('contenteditable', true);
        $(".ui-dialog-buttonset").find("button:first").show();
    } else {
        $("#new-dictum").find("select, input").attr("disabled", "disabled");
        dictumBody.getBody().setAttribute('contenteditable', false);
        $(".ui-dialog-buttonset").find("button:first").hide();
    }

    $.get(BaseSiteURL + "/Plan/GetDictumEditData", {
        dictumId: dictumId
    }).done(function (data) {
        // Limpio mensaje de error
        $("#msg-dictum").empty();
        $(".dynamic-button").remove();

        // Seteo el valor en el hidden para poder cargarlo en el combo de plantillas que se carga por Ajax
        $("#CurrentTemplateId").val(data.TemplateId);

        // Seteo el Id del documento
        $("#new-dictum").attr("data-documentId", dictumId);
        $("#new-dictum").attr("data-templateId", templateId);

        // Seteo el resto de los combos
        $("#DocumentDTO_DictumDTO_TemplateTypeId").val(data.TemplateTypeId).change();

        // Limpio combo de expedientes y lo cargo con lo que corresponda
        $("#DocumentDTO_DictumDTO_FileNumber").empty();
        $("<option>").attr("value", "").text("Seleccionar...").appendTo("#DocumentDTO_DictumDTO_FileNumber");
        $.each(data.FileNumbers, function (index, item) {
            $("<option>").attr("value", data.FileNumbers[index].Key).text(data.FileNumbers[index].Value).appendTo("#DocumentDTO_DictumDTO_FileNumber");
        });
        $("#DocumentDTO_DictumDTO_FileNumber").val(data.FileNumber);

        // Seteo los valores de elegibilidad si tiene
        $("#ExternalNumber").val(data.ExternalNumber);
        $("#ExternalEntity").val(data.ExternalEntity);

        // Seteo el cuerpo del dictamen en el editor
        var content = tinyMCE.get('dictum-tinymce');
        content.setContent((data.Body != null) ? data.Body : "");

        $('#list-dictums').dataTable().fnClearTable(); // Limpia la tabla

        // Agrego los solicitados del dictamen

        $.each(data.Solicitudes, function (index, item) {
            // Agrego fila
            var oTable = $('#list-dictums').dataTable();
            var api = oTable.fnAddData(["", item.Id, item.Line, item.Details, item.Status, item.RequestedTotal, item.ApprovedTotal]);
            // Agrego el atributo con el id del solicitado a la fila
            var $row = $(oTable.fnSettings().aoData[api[0]].nTr)
            $row.attr("data-selectorId", item.Id);
            $row.attr("data-requestedTotal", item.RequestedTotal);

            var approvedTotal = 0;
            if (item.ApprovedTotal != null)
                approvedTotal = item.ApprovedTotal;
            $row.attr("data-approvedTotal", approvedTotal);

            // Chequeo los solicitados            
            var $checkbox = $row.find("input[type='checkbox']");
            $checkbox.prop('checked', true);
            $checkbox.click(updateAmmount);
        });

        // Muestro el monto total
        var totalAmmount = parseFloat(0);
        var rows = $("#list-dictums > tbody > tr");
        $.each(rows, function (i, item) {
            if ($(this).attr("data-approvedTotal") != undefined) {
                var ammount = $(this).attr("data-approvedTotal").replace(",", ".");
                totalAmmount += parseFloat(parseFloat(ammount).toFixed(4));
            }
        });
        $("#dictum-ammount").text("$" + parseFloat(parseFloat(totalAmmount).toFixed(4)).toCurrency());

        LoadCombos(dictumId, function () {
            $("#new-dictum").attr("data-improvementPlanId", improvementPlanId).attr("data-originalStatusId", data.StatusId).attr("data-action", action).dialog("open");
            $("#DocumentDTO_DictumDTO_StatusId").val(data.StatusId);
            showDictumTabs(data.DictumId);
            hideLoading();
        });

        $("#DocumentDTO_DictumDTO_SignDate").val(data.SignDate);
        if (data.SignDate != '') {
            $("#DocumentDTO_DictumDTO_SignDate").attr("disabled", "disabled");
            $("#signedDocument").attr("disabled", "disabled");
        } else {
            $("#DocumentDTO_DictumDTO_SignDate").removeAttr("disabled");
            $("#signedDocument").removeAttr("disabled");
        }

        toggleSignedDocumentField(data.SignedDocument);
        //DIA anexos del dictamen!
        if(data.AnnexDocument != null) {
            $("#annexDocumentDiv-link").show();
            $("#annexDocument").val('');
            $("#annexDocument-link").attr("href", $("#annexDocument-link").attr('data-urlbase') +data.AnnexDocument);
        } else {
            $("#annexDocumentDiv-link").hide();
            $("#annexDocument").val('');
            $("#annexDocument-link").attr("href", "");
        }

    }).fail(function () {
        hideLoading();
        alert("Ocurrió un error cargando el dictamen.");
    });

    return false;
}

function toggleSignedDocumentField(document) {
    $("#signedDocument").val("");
    if (document != "" && document != null) {
        $("#signedDocumentDiv-input").hide();
        $("#signedDocumentDiv-link").show();
        $("#signedDocument-link").attr("href", $("#signedDocument-link").attr('data-urlbase') + document);
    } else {
        $("#signedDocumentDiv-input").show();
        $("#signedDocumentDiv-link").hide();
        $("#signedDocument-link").attr("href", "");
    }
}

function deleteDictum() {
    $("#delete-confirm-dictum")
        .attr("data-documentId", $(this).attr("data-documentid"))
        .attr("data-improvementPlanId", $(this).attr("data-improvementPlanId"))
        .dialog("open");
}

function previewDictum(e) {
    e.preventDefault();
    var dictumBody = tinyMCE.get('dictum-tinymce');
    dictumBody.save();
    var content = $.trim(dictumBody.getContent());
    if (content != "") {
        $("#htmlContent").val(content);
        $("#documentId").val($(this).parent().parent().parent().attr("data-documentId"));
        $("#templateId").val($(this).parent().parent().parent().attr("data-templateId"));
        $("#previewDocumentVariables").empty();
        $("#frmPreview").submit();
    } else {
        showMessage("#msg-dictum", "Debe ingresar un contenido para poder visualizar el dictamen.", "attention", true);
    }
}

function showDictumTabs(dictumId) {
    var status = $("#DocumentDTO_DictumDTO_StatusId :selected").val();
    var originalStatusId = $('#new-dictum').attr('data-originalstatusid');

    switch (status) {
        case "16": //Borrador
            if ($("#new-dictum").attr("data-action") == "view") {
                $("#li-body-dictum").hide();
                $("#li-preview").hide();
            } else {
                $("#li-body-dictum").show();
                $("#li-preview").show();
            }
            $("#li-eligibility").hide();
            $("#li-sign").hide();
            break;

        case "17": // Pendiente de Aprobación
            // Oculto tabs
            if ($("#new-dictum").attr("data-action") == "view") {
                $("#li-body-dictum").hide();
                $("#li-preview").hide();
            } else {
                $("#li-body-dictum").show();
                $("#li-preview").show();
            }
            $("#li-eligibility").hide();
            $("#li-sign").hide();
            break;

        case "18":  //Emitido
            // Oculto tabs       
            if (originalStatusId != "18") {
                $("#li-body-dictum").show();
                $("#li-preview").show();
            } else {
                $("#li-body-dictum").hide();
                $("#li-preview").hide();
            }
            $("#li-eligibility").hide();
            $("#li-sign").hide();
            break;

        case "19": // Firmado
            //if ($("#new-dictum").attr("data-action") == "edit")
            //    $("#DocumentDTO_DictumDTO_StatusId").val("");

            // Oculto tabs       
            if (originalStatusId == "16") {
                // esta firmando desde borrador
                $("#li-body-dictum").show();
                $("#li-preview").show();
            } else {
                $("#li-body-dictum").hide();
                $("#li-preview").hide();
            }
            $("#li-eligibility").hide();
            $("#li-sign").show();
            break;

        case "20": // Anulado
            $("#li-body-dictum").hide();
            $("#li-preview").hide();
            $("#li-eligibility").hide();
            $("#li-sign").hide();
            break;

        case "21": // Pendiente Eligibilidad            
        case "22": // Aprobado Eligibilidad            
        case "23": // Rechazado Eligibilidad  
            $("#li-body-dictum").hide();
            $("#li-preview").hide();
            $("#li-eligibility").show();
            $("#li-sign").hide();
            break;

        default:
            break;
    }
}