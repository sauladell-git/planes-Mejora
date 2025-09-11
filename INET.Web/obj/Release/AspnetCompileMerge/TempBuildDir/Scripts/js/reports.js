$(document).ready(function () {
    $("#budgetByProvince").click(budgetByProvince);
    $("#nationalBudgetByProvince").click(nationalBudgetByProvince);
    $("#statementsOfAccounts").click(statementsOfAccounts);
    $("#improvementPlansByBudget").click(improvementPlansByBudget);
    $("#nationalImprovementPlansByBudget").click(nationalImprovementPlansByBudget);
    $("#CUESolicitudesReport").click(CUESolicitudesReport);

    $('#list-documents').dataTable({
        "bPaginate": true,
        "bLengthChange": false,
        "bFilter": false,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false,
        "sDom": '<"toolbar">frtip'
    });

    $("div.toolbar").html('<h1>Reportes</h1>');

    $("#budget-by-province-dialog").dialog({
        autoOpen: false,
        resizable: false,
        width: 550,
        height: 240,
        modal: true,
        open: function () {
            $("#budgetByProvince-schoolYearId option:first").attr('selected', 'selected');
            $("#budgetByProvince-budgetYearId option:first").attr('selected', 'selected');
            $("#budgetByProvince-provinces option:first").attr('selected', 'selected');
            $("#budgetByProvince-InstitutionLevelId option:first").attr('selected', 'selected');
        },
        buttons: {
            "Descargar": function () {
                var schoolYear = $("#budgetByProvince-schoolYearId :selected").val();
                var budgetYear = $("#budgetByProvince-budgetYearId :selected").val();
                var province = $("#budgetByProvince-provinces :selected").val();
                var institutionLevel = $("#budgetByProvince-InstitutionLevelId :selected").val();
                var dependenceNational = $("#budgetByProvince-dependenceNational").val();
                var downloadURL = BaseSiteURL + "/Report/BudgetByProvince?schoolYearId=" + schoolYear + "&budgetYearId=" + budgetYear + "&provinceId=" + province + "&institutionLevelId=" + institutionLevel + "&dependenceNational=" + dependenceNational;
                downloadFile("#budget-by-province-dialog", downloadURL);
            },
            "Cerrar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#statements-of-accounts-dialog").dialog({
        autoOpen: false,
        resizable: false,
        width: 550,
        height: 170,
        modal: true,
        open: function () {
            $("#statementsOfAccounts-schoolYearId option:first").attr('selected', 'selected');
            $("#statementsOfAccounts-budgetYearId option:first").attr('selected', 'selected');
        },
        buttons: {
            "Descargar": function () {
                var schoolYear = $("#statementsOfAccounts-schoolYearId :selected").val();
                var budgetYear = $("#statementsOfAccounts-budgetYearId :selected").val();
                var downloadURL = BaseSiteURL + "/Report/StatementsOfAccounts?schoolYearId=" + schoolYear + "&budgetYearId=" + budgetYear;
                downloadFile("#statements-of-accounts-dialog", downloadURL);
            },
            "Cerrar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#improvementPlansByBudget-dialog").dialog({
        autoOpen: false,
        resizable: false,
        width: 550,
        height: 290,
        modal: true,
        open: function () {
            $("#improvementPlansByBudget-schoolYearId option:first").attr('selected', 'selected');
            $("#improvementPlansByBudget-budgetYearId option:first").attr('selected', 'selected');
            $("#improvementPlansByBudget-CUE").val("");
        },
        buttons: {
            "Descargar": function () {
                if ($('input[name="improvementPlansByBudget-planTypeId"]:checked').length < 1) {
                    alert("Por favor, seleccione al menos un tipo de plan");
                    return false;
                }
                var planType = "";
                $('input[name="improvementPlansByBudget-planTypeId"]:checked').each(function (i, e) {
                    planType += (planType.length > 0 ? "," : "") + $(e).val();
                });
                var schoolYear = $("#improvementPlansByBudget-schoolYearId :selected").val();
                var budgetYear = $("#improvementPlansByBudget-budgetYearId :selected").val();
                var province = $("#improvementPlansByBudget-provinces :selected").val();
                var dependenceNational = $("#improvementPlansByBudget-dependenceNational").val();
                var CUE = $("#improvementPlansByBudget-CUE").val();
                var abreviated = $('input[name="improvementPlansByBudget-abreviated"]').is(':checked') ? 'true' : 'false';
                var downloadURL = BaseSiteURL + "/Report/ImprovementPlansByBudget?schoolYearId=" + schoolYear + "&budgetYearId=" + budgetYear + "&planTypeId=" + planType + "&provinceId=" + province + "&dependenceNational=" + dependenceNational + "&CUE=" + CUE + "&abreviated=" + abreviated;
                downloadFile("#improvementPlansByBudget-dialog", downloadURL);
            },
            "Cerrar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#CUESolicitudesReport-dialog").dialog({
        autoOpen: false,
        resizable: false,
        width: 550,
        height: 260,
        modal: true,
        open: function () {
            $("#CUESolicitudesReport-schoolYearId option:first").attr('selected', 'selected');
            $("#CUESolicitudesReport-budgetYearId option:first").attr('selected', 'selected');
            $("#CUESolicitudesReport-CUE").val("");
        },
        buttons: {
            "Descargar": function () {
                if ($('#CUESolicitudesReport-CUE').val().length < 9) {
                    alert("Por favor, indique un CUE válido");
                    return false;
                }
                var schoolYear = $("#CUESolicitudesReport-schoolYearId :selected").val();
                var budgetYear = $("#CUESolicitudesReport-budgetYearId :selected").val();
                var CUE = $("#CUESolicitudesReport-CUE").val();
                var downloadURL = BaseSiteURL + "/Report/CUESolicitudesReport?schoolYearId=" + schoolYear + "&budgetYearId=" + budgetYear + "&CUE=" + CUE;
                downloadFile("#CUESolicitudesReport-dialog", downloadURL);
            },
            "Cerrar": function () {
                $(this).dialog("close");
            }
        }
    });

});

function budgetByProvince() {
    $('#budgetByProvince-dependenceNational').val(false);
    $('#budget-by-province-dialog').dialog('option', 'title', "REPORTE EJECUCIÓN PRESUPUESTARIA PROVINCIAL");
    $("#budget-by-province-dialog").dialog("open");
    return false;
}

function nationalBudgetByProvince() {
    $('#budgetByProvince-dependenceNational').val(true);
    $('#budget-by-province-dialog').dialog('option', 'title', "REPORTE EJECUCIÓN PRESUPUESTARIA PROVINCIAL (DEPENDENCIA NACIONAL)");
    $("#budget-by-province-dialog").dialog("open");
    return false;
}

function statementsOfAccounts() {
    $("#statements-of-accounts-dialog").dialog("open");
    return false;
}

function improvementPlansByBudget() {
    $('#improvementPlansByBudget-dialog').dialog('option', 'title', "REPORTE DE PLANES");
    $('#improvementPlansByBudget-planTypeId-row').show();
    $('#improvementPlansByBudget-planTypeId-row input').prop('checked', true);
    $('#improvementPlansByBudget-dependenceNational').val(false);
    $("#improvementPlansByBudget-dialog").dialog("open");
    return false;
}

function nationalImprovementPlansByBudget() {
    $('#improvementPlansByBudget-dialog').dialog('option', 'title', "REPORTE DE PLANES (DEPENDENCIA NACIONAL)");
    $('#improvementPlansByBudget-planTypeId-row').hide();
    $('#improvementPlansByBudget-planTypeId-row input').prop('checked', true);
    $('#improvementPlansByBudget-dependenceNational').val(true);
    $("#improvementPlansByBudget-dialog").dialog("open");
    return false;
}

function CUESolicitudesReport() {
    $("#CUESolicitudesReport-dialog").dialog("open");
    return false;
}

function downloadFile(dialog, url) {
    if (!canSubmitForm(getDialogFirstButton(dialog)))
        return false;

    showLoading();
    $.fileDownload(url, {
        successCallback: function (url) {
            enableSubmitButtons();
            hideLoading();
        },
        failCallback: function (html, url) {
            enableSubmitButtons();
            hideLoading();
            alert('Ocurrió un error generando el reporte. Intentelo nuevamente.');
        }
    });
}