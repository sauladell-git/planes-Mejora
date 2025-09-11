var _apiDataTable;

$(document).ready(function () {

    $(".chosen-select").chosen({ no_results_text: "Sin resultados..." });

    showServerMessageResult("#msg-list");    

    $("#opencal").datepicker({
        beforeShow: function () {
            //$(document).tooltip("destroy");
            $("#opencal").attr("title", "");
            $(".search-close").tooltip();
            $(".admin-icons a").tooltip();
        },
        onSelect: function (dateText, inst) {
            var pieces = dateText.split('/');
            $('#Filters_ReceptionDateDay').val(pieces[1]);
            $('#Filters_ReceptionDateMonth').val(pieces[0]);
            $('#Filters_ReceptionDateYear').val(pieces[2]);
        },
        onClose: function () {
            $("#opencal").attr("title", "Abrir Calendario");            
            //$(document).tooltip();
        }
    });

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

    _apiDataTable = $('#list-plans').dataTable({
        "bPaginate": true,
        "iDisplayLength": 20,
        "bLengthChange": false,
        "bFilter": false,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false,
        "bServerSide": true,
        "sAjaxSource": BaseSiteURL + "/Plan/ListPaginated",
        "sDom": '<"toolbar">frtip',
        "aoColumnDefs": [
            { "bSortable": false, "aTargets": [0, 4, 12] }
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

    $("div.toolbar").html('<h1>Planes de Mejora</h1>');

    
    $("#Filters_EvaluatorId").append($("<option>").val("-1").text("Sin Asignar"));

    $("#frmSearchPlans").submit(function (e) {
        e.preventDefault();
        $("#msg-search").empty();
        var msg = ""; //validate();
        if (msg == "") {
            $("#msg-search").fadeOut("slow");
            $("#msg-search").empty();

            // Obtengo la fecha en formato yyyy-MM-dd si el usuario seleccionó los tres combos
            var receptionDate = null;
            if ($("#Filters_ReceptionDateDay :selected").val() != "" && $("#Filters_ReceptionDateMonth :selected").val() != "" && $("#Filters_ReceptionDateYear :selected").val() != "")
                receptionDate = $("#Filters_ReceptionDateYear :selected").val() + "-" + $("#Filters_ReceptionDateMonth :selected").val() + "-" + $("#Filters_ReceptionDateDay :selected").val();

            var filters = {
                sCustomSearch_Identifier: $("#Filters_Identifier").val(),
                sCustomSearch_Summary: $("#Filters_Summary").val(),
                sCustomSearch_FileNumber: $("#Filters_FileNumber").val(),
                sCustomSearch_CUE: $("#Filters_CUE").val(),
                iCustomSearch_SchoolYearId: $("#Filters_SchoolYearId :selected").val(),
                iCustomSearch_EvaluatorId: $("#Filters_EvaluatorId :selected").val(),
                iCustomSearch_ImprovementPlanTypeId: $("#Filters_ImprovementPlanTypeId :selected").val(),
                dCustomSearch_ReceptionDate: receptionDate,
                iCustomSearch_FieldId: $("#Filters_FieldId :selected").val(),
                iCustomSearch_StatusId: $("#Filters_StatusId :selected").val(),
                sCustomSearch_Dictum: $("#Filters_Dictum").val(),
                sCustomSearch_Articulator: $("#Filters_Articulator").val(),
                sCustomSearch_ArticulatorExp: $("#Filters_ArticulatorExp").val()
            };

            _apiDataTable.fnReloadAjax(_apiDataTable.oSettings, filters);
          
        } else {
            showMessage("#msg-search", msg, "attention", false);
        }
        return false;
    });

    $("#delete-confirm").dialog({
        autoOpen: false,
        resizable: false,
        width: 380,
        modal: true,
        buttons: {
            "Eliminar": function () {
                if (!canSubmitForm(getDialogFirstButton("#delete-confirm")))
                    return false;

                var $row = $($(this).attr("data-PlanId"));
                var identifier = $(this).attr("data-identifier");
                $.post(BaseSiteURL + "/Plan/DeletePlan", {
                    ImprovementPlanId: $(this).attr("data-PlanId")
                }).done(function (data) {
                    if (data.messageError == "") {                        
                        $row.remove();
                        _apiDataTable.fnDraw();
                        showMessage("#msg-list", "El plan " + identifier + " se eliminó con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-list", data.messageError, "error", true);
                    }
                    enableSubmitButtons();
                    $("#delete-confirm").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    // Mantengo vivo el evento de visualizar al paginar
    $('#list-plans').on('click', '.delete', openDeleteDialog);
});

function validate() {
    // Valido que al menos se haya seleccionado algún campo para la búsqueda
    var isValidField = false;
    var isValidDate = true;

    if ($.trim($("#Filters_Identifier").val()) != "")
        isValidField = true;

    if ($.trim($("#Filters_CUE").val()) != "")
        isValidField = true;

    if ($("#Filters_SchoolYearId :selected").val() != "")
        isValidField = true;

    if ($("#Filters_EvaluatorId :selected").val() != "")
        isValidField = true;

    if ($("#Filters_ImprovementPlanTypeId :selected").val() != "")
        isValidField = true;

    if ($("#Filters_ReceptionDateDay :selected").val() != "" && $("#Filters_ReceptionDateMonth :selected").val() != "" && $("#Filters_ReceptionDateYear :selected").val() != "") {
        var date = $("#Filters_ReceptionDateDay :selected").val() + "/" + $("#Filters_ReceptionDateMonth :selected").val() + "/" + $("#Filters_ReceptionDateYear :selected").val();
        var parsed = Globalize.parseDate(date)
        isValidField = true;
        isValidDate = (parsed != null && !/Invalid|NaN/.test(parsed));             
    }

    if ($("#Filters_FieldId :selected").val() != "")
        isValidField = true;

    if ($("#Filters_StatusId :selected").val() != "")
        isValidField = true;

    if (isValidField && isValidDate)
        return "";

    if (!isValidDate)
        return "Por favor, asegúrese que la fecha ingresada sea válida";

    return "Por favor, seleccione al menos un campo para poder buscar";
}

function openDeleteDialog() {
    var identifier = $(this).parent().parent().find("td").get(1).innerHTML;    
    $("#delete-confirm").find(".delete-text").text("¿Desea borrar el Plan de Mejora " + identifier + "?");

    $("#delete-confirm")
        .attr("data-planId", $(this).parent().parent().attr("id"))
        .attr("data-identifier", identifier)
        .dialog("open");
}

function bindImprovementPlans() {
    $(".delete").click(openDeleteDialog);   
    $('#list-plans').dataTable({
        "bPaginate": true,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false
    });
    $('#list-plans').on('preXhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', .4);
    });
    $('#list-plans').on('xhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', 1);
    })
    $('#list-plans').on('order.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', .4);
    });
}