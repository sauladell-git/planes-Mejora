$(document).ready(function () {
    bindTable();

    $("#new-comment").dialog({
        autoOpen: false,
        resizable: false,
        width: 350,
        modal: true,
        open: function () {
            $("#msg-comment").empty();
            $("#CommentText").val("");
            $("#frmComments").validate().resetForm();
        },
        buttons: {
            "Guardar": function () {
                if (!canSubmitForm(getDialogFirstButton("#new-comment")))
                    return false;

                if ($("#frmComments").valid()) {
                    saveIncidenceComment($(this).attr("data-incidenceId"), $(this).attr("data-solicitudeId"), $(this).attr("data-improvementPlanId"));
                }
                else {
                    showMessage("#msg-comment", "Por favor, complete todos los campos obligatorios", "error");
                    enableSubmitButtons();
                }                
            }
        }
    });

    $("#change-confirm").dialog({
        autoOpen: false,
        resizable: false,
        width: 340,
        modal: true,
        buttons: {
            "Cerrar Incidencia": function () {
                if (!canSubmitForm(getDialogFirstButton("#change-confirm")))
                    return false;
                showLoading();
                var solicitudeId = $(this).attr("data-solicitudeId");
                $.post(BaseSiteURL + "/Plan/ChangeIncidenceStatus", {
                    improvementPlanId: $(this).attr("data-improvementPlanId"),
                    incidenceId: $(this).attr("data-incidenceId"),
                    solicitudeId: $(this).attr("data-solicitudeId"),
                    returnAll: false
                }).done(function (data) {
                    if (data != "") {                        
                        $("#incidences-wrapper").html(data);
                        bindTable();
                        bindIncidenceDialog();                        
                        $("#view-incidence").dialog("close");
                        showMessage("#msg-incidences-list", "El incidente se cerró con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-incidence", "No es posible cerrar el incidente.", "error", true);
                    }
                    hideLoading();
                    enableSubmitButtons();
                    $("#change-confirm").dialog("close");
                }).fail(function () {
                    showMessage("#msg-incidence", "Ocurrió un error modificando el registro", "error", true);
                    hideLoading();
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    bindIncidenceDialog();
});

function viewIncidence() {
    // Actualizo las incidencias del solicitado y lo muestro en el dialogo
    showLoading();
    $.get(BaseSiteURL + "/Plan/GetIncidence", {
        IncidenceId: $(this).parent().parent().parent().attr("data-incidenceId")
    }).done(function (data) {
        $("#view-incidence .incidence").empty();
        $("#view-incidence .incidence").html(data);
        bindIncidenceEvents();
        hideLoading();
        $("#view-incidence").dialog("open");
    }).fail(function () {
        showMessage("#msg-incidence", "Ocurrió un error modificando el registro", "error", true);
        hideLoading();
    });

    return false;
}

function bindIncidenceEvents() {
    $("#view-incidence .incidence .incidence-comment").click(incidenceComment);
    $("#view-incidence .incidence .status").click(openChangeStatusDialog);
    $("#view-incidence .incidence .icon-comment").click(toggleIncidentComments);
}

function bindIncidenceDialog() {
    $("#view-incidence").dialog({
        autoOpen: false,
        resizable: false,
        width: 600,
        maxHeight: 500,
        modal: true,
        buttons: {
            "Cerrar": function () {
                $(this).dialog("close");
            },
        }
    });
}

function incidenceComment() {
    $("#new-comment")
                .attr("data-incidenceId", $(this).attr("data-IncidenceId"))
                .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
                .attr("data-improvementPlanId", $(this).attr("data-improvementPlanId"))
                .dialog("open");
}

function openChangeStatusDialog() {
    $("#change-confirm")
        .attr("data-incidenceId", $(this).attr("data-incidenceId"))
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .attr("data-improvementPlanId", $(this).attr("data-improvementPlanId"))
        .dialog("open");
}

function bindTable() {    
    $('#list-incidences').dataTable({
        "bPaginate": true,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false,
        "sDom": '<"toolbar">frtip',        
        "aoColumnDefs": [{
            "bSearchable": false, "aTargets": [0]            
        }]
    });
    $("div.toolbar").html('<h1>Registro de Incidencias</h1>');

    // Mantengo vivo el evento de visualizar al paginar
    $('#list-incidences').on('click', '.view-incidence', viewIncidence);
}

function toggleIncidentComments() {
    $("." + $(this).attr("data-commentIncidence")).toggle();
}

function saveIncidenceComment(incidenceId, solicitudeId, improvementPlanId) {
    showLoading();
    $.post(BaseSiteURL + "/Plan/SaveIncidenceComment", {
        ImprovementPlanId: improvementPlanId,
        IncidenceId: incidenceId,
        SolicitudeId: solicitudeId,
        Text: $("#CommentText").val(),
        ReturnAll: false
    }).done(function (data) {
        $("#view-incidence .incidence").empty();
        $("#view-incidence .incidence").html(data);
        bindIncidenceEvents();
        $("#new-comment").dialog("close");
        enableSubmitButtons();
        hideLoading();
        showMessage("#msg-incidence", "El comentario de la incidencia se guardó con éxito", "confirmation", true);
    }).fail(function () {
        showMessage("#msg-incidence", "Ocurrió un error modificando el registro", "error", true);
        hideLoading();
    });
}