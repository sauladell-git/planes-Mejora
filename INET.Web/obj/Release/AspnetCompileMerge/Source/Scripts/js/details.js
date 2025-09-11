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

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
    }

    showServerMessageResult("#msg-plan");
    var defaultAttributes = { plan: {}, solicitados: {} };
    $('.toogleFullView').click(function (e) {
        e.preventDefault();
        var cual = $(this);
        if (cual.attr('toggled') == 'true') {
            $('#plan-detail').animate({ height: 0, opacity: 0 }, 500, function () {
                $('#plan-detail').removeClass('inline-plan-details').css('margin-left', '-' + defaultAttributes.plan.width).css('float', 'left').css('width', defaultAttributes.plan.width).css('height', defaultAttributes.plan.height)
                    .animate({ marginLeft: 0, opacity: 1 }, 600, function () {
                        $('#solicitados-detail').css('float', 'left');
                    });
            });
            $('#solicitados-detail').stop().css('float', 'right').animate({ width: defaultAttributes.solicitados.width, marginLeft: defaultAttributes.solicitados.marginLeft, marginTop: '0' }, 600, function () {
                cual.attr('toggled', false);
            });
        } else {
            if (cual.attr('toggled') == undefined) {
                defaultAttributes.plan = {
                    width: (100 * parseFloat($('#plan-detail').css('width')) / parseFloat($('#plan-detail').parent().css('width'))) + '%',
                    height: (100 * parseFloat($('#plan-detail').css('height')) / parseFloat($('#plan-detail').parent().css('height'))) + '%'
                };
                defaultAttributes.solicitados = {
                    width: (100 * parseFloat($('#solicitados-detail').css('width')) / parseFloat($('#solicitados-detail').parent().css('width'))) + '%',
                    marginLeft: $('#solicitados-detail').css('marginLeft')
                };
            }
            $('#plan-detail').animate({ marginLeft: '-' + defaultAttributes.plan.width, opacity: 0 }, 800, function () {
                $('#plan-detail').addClass('inline-plan-details').css('margin-left', '0').css('float', 'none').css('width', '98%').css('height', '0').animate({ opacity: 1 }, 500).css('height', 'auto');
            });
            $('#solicitados-detail').stop().delay('100').animate({ width: '98%', marginLeft: '0', marginTop: '1%' }, 800, function () {
                cual.attr('toggled', true);
            });
        }
    });

    $('.select-all-solicitude').click(function (e) {
        selectAllVisibleSolicitudes();
        e.preventDefault();
        return false;
    });

    $('.select-none-solicitude').click(function (e) {
        deSelectAllVisibleSolicitudes();
        e.preventDefault();
        return false;
    });

    $("#solicitudes-group-actions").dialog({
        autoOpen: false,
        resizable: false,
        width: 500,
        maxHeight: 400,
        modal: true,
        buttons: {
            "Cerrar": function () {
                $(this).dialog("close");
            },
        }
    });

    $('.mod-selected-solicitudes').click(function (e) {
        $("#msg-solicitudes-group-actions").empty();
        $("#solicitudes-group-actions").dialog('open');
        e.preventDefault();
        return false;
    });

    $('#process-solicitudes-group').click(function (e) {
        triggerSolicitudeGroupUpdate('process');
        e.preventDefault();
        return false;
    });

    $('#approve-solicitudes-group').click(function (e) {
        triggerSolicitudeGroupUpdate('approve');
        e.preventDefault();
        return false;
    });

    $('#reject-solicitudes-group').click(function (e) {
        triggerSolicitudeGroupUpdate('reject');
        e.preventDefault();
        return false;
    });

    $('#delete-solicitudes-group').click(function (e) {
        triggerSolicitudeGroupUpdate('delete');
        e.preventDefault();
        return false;
    });

    $('#summary-table').dataTable({
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": true,
        "bInfo": false,
        "bAutoWidth": false
    });

    $('#summary-table-cue').dataTable({
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": true,
        "bInfo": false,
        "bAutoWidth": false
    });

    $("#list-reassignedSolicitudes").dataTable({
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": true,
        "bSort": true,
        "bInfo": false,
        "bAutoWidth": false
    });

    $("#SolicitudeDTO_RequestedAmount").mask('##0,00', { reverse: true });
    $("#SolicitudeDTO_ApprovedAmount").mask('##0,00', { reverse: true });

    //$("#SolicitudeDTO_RequestedPriceUnit").numericInput({ allowFloat: true });
    //$("#SolicitudeDTO_ApprovedPriceUnit").numericInput({ allowFloat: true });

    $("#SolicitudeDTO_RequestedPriceUnit").mask('##0,00', { reverse: true });
    $("#SolicitudeDTO_RequestedTotal").mask('##0,00', { reverse: true });
    $("#SolicitudeDTO_ApprovedPriceUnit").mask('##0,00', { reverse: true });
    $("#SolicitudeDTO_ApprovedTotal").mask('##0,00', { reverse: true });

    $("#SolicitudeDTO_CUE").blur(function () {
        // Cuando es un solicitado nuevo, cargo las especializaciones en base al CUE que se carga en el solicitado
        if ($("#OriginalSolicitudeCUE").val() != $.trim($(this).val()) && $("#ImprovementPlanType").val() == "Nacional" || $("#ImprovementPlanType").val() == "Jurisdiccional") {

            // Seteo el nuevo valor como el último CUE ingresado
            $("#OriginalSolicitudeCUE").val($.trim($(this).val()));

            // Busco las especializaciones para ese CUE
            $.get(BaseSiteURL + "/Plan/ListSolicitudeSpecializationsByCue", {
                improvementPlanId: $("#Id").val(),
                cue: $(this).val()
            }).done(function (data) {
                var prev_selected = $("#SolicitudeDTO_Specialization").val();
                $("#SolicitudeDTO_Specialization").empty();
                $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SolicitudeDTO_Specialization");

                $.each(data, function (i, item) {
                    $("<option>").attr("value", item.Key).text(item.Value).prop('selected', prev_selected == item.Key).appendTo("#SolicitudeDTO_Specialization");
                });
            });
        }
    });

    $("#SolicitudeDTO_StatusId").change(function () {
        // Verifico si puedo o no habilitar los campos de aprobacion de acuerdo a si quiero pasar a estado aprobado o elegible
        if ($("#OriginalStatusId").val() == "3" && ($("#SolicitudeDTO_StatusId :selected").val() == "12" || $("#SolicitudeDTO_StatusId :selected").val() == "13")) {
            // solo se se cambia a otro, no que vuelva al mismo
            if ($("#SolicitudeDTO_StatusId").val() != $("#SolicitudeDTO_StatusId").attr('data-value')) {
                $("#SolicitudeDTO_ApprovedAmount").removeAttr("disabled");
                $("#SolicitudeDTO_ApprovedPriceUnit").removeAttr("disabled");
                $("#SolicitudeDTO_ApprovedAmount").val($("#SolicitudeDTO_RequestedAmount").val());
                $("#SolicitudeDTO_ApprovedPriceUnit").val($("#SolicitudeDTO_RequestedPriceUnit").val());
                $("#SolicitudeDTO_ApprovedTotal").val($("#SolicitudeDTO_RequestedTotal").val());
            } else {
                $("#SolicitudeDTO_ApprovedAmount").val($("#SolicitudeDTO_ApprovedAmount").attr('data-value'));
                $("#SolicitudeDTO_ApprovedPriceUnit").val($("#SolicitudeDTO_ApprovedPriceUnit").attr('data-value'));
                $("#SolicitudeDTO_ApprovedTotal").val($("#SolicitudeDTO_ApprovedTotal").attr('data-value'));
            }
        } else {
            // Si paso a estado rechazado, limpio los montos aprobados
            $("#SolicitudeDTO_ApprovedAmount").val("");
            $("#SolicitudeDTO_ApprovedPriceUnit").val("");
            $("#SolicitudeDTO_ApprovedTotal").val("");

            $("#SolicitudeDTO_ApprovedAmount").attr("disabled", "disabled");
            $("#SolicitudeDTO_ApprovedPriceUnit").attr("disabled", "disabled");
        }
    });

    $("#SolicitudeDTO_LineId").change(function () {
        // Si el plan se encuentra en evaluacion, y estoy cargando un solicitado nuevo, entonces muestro el número de expediente correspondiente a esa línea
        if ($("#OriginalStatusId").val() == "3" && $("#SolicitudeDTO_LineId :selected").val() != "") {
            $("#SolicitudeDTO_FileNumber").removeClass("input-validation-error");
            $.get(BaseSiteURL + "/Plan/GetFileNumberForSolicitude", {
                improvementPlanId: $("#Id").val(),
                lineId: $("#SolicitudeDTO_LineId :selected").val()
            }).done(function (data) {
                if (data.FileNumber == null) {
                    $("#SolicitudeDTO_FileNumber").val("");
                } else {
                    $("#SolicitudeDTO_FileNumber").val(data.FileNumber);
                }
            });
        }
    });

    $("#SolicitudeDTO_SolicitudeTypeId").change(function () {
        if ($("#SolicitudeDTO_SolicitudeTypeId").val() == "2") {
            // Si es un solicitado reasignado, habilito el textbox de reasignado
            $("#SolicitudeDTO_Reassigned").removeAttr("disabled", "disabled");
            $("#SolicitudeDTO_Reassigned").attr("required", "required");
            $(".select-reassigned").removeClass("disabled");
        } else {
            // Si es un solicitado original, deshabilito el textbox de reasignado
            $("#SolicitudeDTO_Reassigned").attr("disabled", "disabled");
            $("#SolicitudeDTO_Reassigned").removeAttr("required");
            $(".select-reassigned").addClass("disabled");
            $("#SolicitudeDTO_Reassigned").removeClass("input-validation-error");
        }
    });

    // Prevengo que ingresen algo en el textbox de reasignado
    $("#SolicitudeDTO_Reassigned").keypress(function () { return false; });

    $(".requestedAmount, .approvedAmount").blur(function () {
        calculateAmountTotal($(this));
    });

    $(".requestedAmount, .approvedAmount").keyup(function () {
        calculateAmountTotal($(this));
    });

    $(".priceUnit").blur(updatePriceUnit);

    $("#nuevo-solicitado").click(function () {
        // Seteo el valor del ciclo lectivo como valor por defecto del combo
        $("#SolicitudeDTO_SchoolYearId").val($("#SchoolYearId").val());
        $("#OriginalSolicitudeCUE").val("");
        enableSolicitdeForm();
        $("#new-solicitation").dialog("open");
    });

    $(".nro-expediente").click(function () {
        // Valido que haya completado todos los número de expedientes
        $("#msg-plan").empty();
        if (validateForm("#frmFileNumbers", "#msg-plan")) {
            var fileNumbers = new Array();
            $.each($(".fileNumber"), function (index) {
                fileNumbers.push($(this).parent().parent().attr("data-lineId") + "|" + $(this).val());
            });

            showLoading();
            $.ajax({
                type: "POST",
                url: BaseSiteURL + "/Plan/SaveFileNumbers",
                dataType: "json",
                data: {
                    improvementPlanId: $("#Id").val(),
                    fileNumbers: fileNumbers
                },
                traditional: true,
                success: function (data) {
                    hideLoading();
                    if (data) {
                        showMessage("#msg-plan", "Se guardaron correctamente los números de expedientes.", "confirmation", true);
                    } else {
                        showMessage("#msg-plan", "Ocurrió un error al guardar los números de expediente. Es probable que el mismo número de expediente ya exista. Por favor, inténtelo nuevamente.", "error", true);
                    }
                },
                error: function () {
                    showMessage("#msg-plan", "Ocurrió un error al guardar los números de expediente.", "error", true);
                    hideLoading();
                }
            });
        }
    });

    $(".improvementPlan-comment").click(function () {
        $("#new-comment")
            .attr("comment-type", "ImprovementPlan")
            .dialog("open");
    });

    $("#view-solicitudes-comments").dialog({
        autoOpen: false,
        resizable: false,
        width: 500,
        maxHeight: 400,
        modal: true,
        buttons: {
            "Cerrar": function () {
                $(this).dialog("close");
            },
        }
    });

    $(".nueva-incidencia").click(function () {
        openIncidenceDialog("");
    });

    $(".status").click(openChangeStatusDialog);

    $("#delete-confirm-solicitude").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                if (!canSubmitForm(getDialogFirstButton("#delete-confirm-solicitude")))
                    return false;
                showLoading();
                $.post(BaseSiteURL + "/Plan/DeleteSolicitude", {
                    ImprovementPlanId: $("#Id").val(),
                    SolicitudeId: $(this).attr("data-solicitudeId")
                }).done(function (data) {
                    if (data != "") {
                        reloadSolicitudes();
                        updateSummaryTable();
                        showMessage("#msg-plan", "El solicitado se eliminó con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-plan", "No es posible eliminar el solicitado debido a que el mismo tiene incidencias abiertas o se encuentra en estado aprobado.", "error", true);
                    }
                    hideLoading();
                    enableSubmitButtons();
                    $("#delete-confirm-solicitude").dialog("close");
                }).fail(function () {
                    showMessage("#msg-plan", "Ocurrió un error al eliminar el solicitado.", "error", true);
                    hideLoading();
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
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

                var identifier = $(this).attr("data-identifier");
                showLoading();
                $.post(BaseSiteURL + "/Plan/DeletePlan", {
                    ImprovementPlanId: $("#Id").val()
                }).done(function (data) {
                    if (data.messageError == "") {
                        window.location.href = BaseSiteURL + "/planes-de-mejora";
                    } else {
                        showMessage("#msg-plan", "No es posible eliminar el plan " + identifier + ".", "error", true);
                    }
                    enableSubmitButtons();
                    hideLoading();
                    $("#delete-confirm").dialog("close");
                }).fail(function () {
                    showMessage("#msg-plan", "Ocurrió un error al eliminar el plan " + identifier + ".", "error", true);
                    hideLoading();
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".delete").click(deleteImprovementPlan);

    $("#evaluator-dialog").dialog({
        autoOpen: false,
        resizable: false,
        width: 370,
        open: function () {
            $("#frmEvaluators").validate().resetForm();
        },
        modal: true,
        open: function () {
            $("#msg-evaluators").empty();
            $("#frmEvaluators").validate().resetForm();
        },
        buttons: {
            "Guardar": function () {
                if (validateForm("#frmEvaluators", "#msg-evaluators")) {
                    if (!canSubmitForm(getDialogFirstButton("#evaluator-dialog")))
                        return false;
                    showLoading();
                    $.post(BaseSiteURL + "/Plan/SetEvaluator", {
                        improvementPlanId: $("#Id").val(),
                        evaluatorId: $("#EvaluatorId").val()
                    }).done(function (data) {
                        if (data) {
                            $("#evaluator_text").text($("#EvaluatorId option:selected").text());
                            showMessage("#msg-plan", "Se asignó correctamente el evaluador", "confirmation", true);
                        } else {
                            showMessage("#msg-plan", "Ocurrió un error al asignar el evaluador", "error", true);
                        }
                        hideLoading();
                        $("#evaluator-dialog").dialog("close");
                    }).fail(function () {
                        showMessage("#msg-plan", "Ocurrió un error no especificado al asignar el evaluador", "error", true);
                        hideLoading();
                    });
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#change-plan-status").dialog({
        autoOpen: false,
        resizable: false,
        width: 370,
        open: function () {
            $("#msg-plan-status").empty();
            $("#frmPlanStatus").validate().resetForm();
            $("#StatusId").val($("#OriginalStatusId").val());
        },
        modal: true,
        buttons: {
            "Guardar": function () {
                if (!canSubmitForm(getDialogFirstButton("#change-plan-status")))
                    return false;

                if (validateForm("#frmPlanStatus", "#msg-plan-status")) {
                    // Solo puedo pasar a estado en evaluación si se completaron todos los números de expediente
                    if (!validFileNumbers()) {
                        showMessage("#msg-plan-status", "Debe completar todos los número de expedientes", "attention");
                        enableSubmitButtons();
                        return;
                    }
                    showLoading();
                    $.post(BaseSiteURL + "/Plan/ChangeStatus", {
                        improvementPlanId: $("#Id").val(),
                        statusId: $("#StatusId :selected").val()
                    }).done(function (data) {
                        if (data) {
                            window.location.reload();
                        } else {
                            showMessage("#msg-plan", "Ocurrió un error al cambiar el estado.", "error", true);
                            enableSubmitButtons();
                            hideLoading();
                            $("#change-plan-status").dialog("close");
                        }
                    }).fail(function () {
                        showMessage("#msg-plan", "Ocurrió un error no especificado al cambiar el estado.", "error", true);
                        hideLoading();
                    });;
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".change-plan-status").click(function () {
        $("#change-plan-status").dialog("open");
    });

    $(".evaluator").click(function () {
        $("#evaluator-dialog").dialog("open");
    });

    $("#block-confirm-solicitude").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Anular": function () {
                if (!canSubmitForm(getDialogFirstButton("#block-confirm-solicitude")))
                    return false;

                showLoading();
                $.post(BaseSiteURL + "/Plan/BlockSolicitude", {
                    ImprovementPlanId: $("#Id").val(),
                    SolicitudeId: $(this).attr("data-solicitudeId")
                }).done(function (data) {
                    if (data != "") {
                        reloadSolicitudes();
                        updateSummaryTable();
                        showMessage("#msg-plan", "El solicitado se anuló con éxito.", "confirmation", true);
                    } else {
                        showMessage("#msg-plan", "No es posible anular el solicitado.", "error", true);
                    }
                    hideLoading();
                    enableSubmitButtons();
                    $("#block-confirm-solicitude").dialog("close");
                }).fail(function () {
                    showMessage("#msg-plan", "Ocurrió un error no especificado al anular el solicitado", "error", true);
                    hideLoading();
                });;
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#view-solicitudes-incidences").dialog({
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

    $("#new-incidence").dialog({
        autoOpen: false,
        resizable: false,
        width: 500,
        height: 370,
        modal: true,
        open: function () {
            $("#frmIncidence").validate().resetForm();
            $("#IncidenceDTO_StatusId").val("");
            $("#IncidenceDTO_IncidenceTypeId").val("");
            $("#IncidenceDTO_Details").val("");
            $("#msg-incidence").empty();
        },
        buttons: {
            "Guardar": function () {
                var isSolicicitdeIncidence = $(this).attr("data-solicitudeId") != "";
                if ($("#frmIncidence").valid()) {
                    if (!canSubmitForm(getDialogFirstButton("#new-incidence")))
                        return false;

                    showLoading();
                    $.post(BaseSiteURL + "/Plan/SaveIncidence", {
                        SolicitudeId: $(this).attr("data-solicitudeId"),
                        IncidenceTypeId: $("#IncidenceDTO_IncidenceTypeId :selected").val(),
                        ImprovementPlanId: $("#Id").val(),
                        Details: $("#IncidenceDTO_Details").val(),
                        IsSolicitudeIncidence: isSolicicitdeIncidence
                    }).done(function (data) {
                        if (isSolicicitdeIncidence) {
                            // Actualizo el listado de solicitados
                            reloadSolicitudes();

                            // Actualizo las incidencias generales en el tab de incidencias
                            $.get(BaseSiteURL + "/Plan/ListImprovementPlanIncidences", {
                                improvementPlanId: $("#Id").val()
                            }).done(function (data) {
                                $(".incidences").html(data);
                                bindIncidencesEvents();
                            });
                        } else {
                            // Actualizo el listado de incidencias
                            $(".incidences").html(data);
                            bindIncidencesEvents();
                        }

                        $("#new-incidence").dialog("close");
                        hideLoading();
                        showMessage("#msg-plan", "La incidencia se guardó con éxito", "confirmation", true);
                    }).fail(function () {
                        showMessage("#msg-incidence", "Ocurrió un error al guardar la incidencia", "error");
                        hideLoading();
                    });
                } else {
                    showMessage("#msg-incidence", "Por favor, complete todos los campos obligatorios", "error");
                }
            },
        }
    });

    $("#change-confirm").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Cerrar Incidencia": function () {
                if (!canSubmitForm(getDialogFirstButton("#change-confirm")))
                    return false;

                showLoading();
                var solicitudeId = $(this).attr("data-solicitudeId");
                $.post(BaseSiteURL + "/Plan/ChangeIncidenceStatus", {
                    improvementPlanId: $("#Id").val(),
                    incidenceId: $(this).attr("data-incidenceId"),
                    solicitudeId: $(this).attr("data-solicitudeId"),
                    returnAll: true
                }).done(function (data) {
                    if (data != "") {
                        if (solicitudeId != "") {
                            $("#view-solicitudes-incidences .solicitude-incidences").html(data);
                            bindSolicitudeIncidenceEvents();
                            showMessage("#msg-solicitude-incidences", "El incidente se cerro con éxito.", "confirmation", true);
                        } else {
                            $(".incidences").html(data);
                            bindIncidencesEvents();
                            showMessage("#msg-plan", "El incidente se cerro con éxito.", "confirmation", true);
                        }
                    } else {
                        showMessage("#msg-plan", "No es posible cerrar el incidente.", "error", true);
                    }

                    enableSubmitButtons();
                    hideLoading();
                    $("#change-confirm").dialog("close");
                }).fail(function () {
                    showMessage("#msg-plan", "Ocurrió un error no especificado al cerrar el incidente", "error", true);
                    hideLoading();
                });;
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#import-spread").dialog({
        autoOpen: false,
        resizable: false,
        width: 500,
        modal: true,
        open: function () {
            $("#msg-import").empty();
            $("#frmImportSolicitudes").validate().resetForm();
        },
        buttons: {
            "Cargar": function () {
                if ($("#frmImportSolicitudes").valid()) {
                    if (!canSubmitForm(getDialogFirstButton("#import-spread")))
                        return false;
                    showLoading();
                    // Submiteo el formulario con un timeout, porque sino no da tiempo a mostrar el icono de loading en FF
                    setTimeout(importSolicitudes, 500);
                } else {
                    showMessage("#msg-import", "Por favor, complete todos los campos obligatorios", "error", true);
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#update_solicitados_box").dialog({
        autoOpen: false,
        resizable: false,
        width: 500,
        modal: true,
        open: function () {
            $("#msg-import-updates").empty();
            $("#frmImportSolicitudesUpdates").validate().resetForm();
        },
        buttons: {
            "Cargar": function () {
                if ($("#frmImportSolicitudesUpdates").valid()) {
                    if (!canSubmitForm(getDialogFirstButton("#update_solicitados_box")))
                        return false;
                    showLoading();
                    // Submiteo el formulario con un timeout, porque sino no da tiempo a mostrar el icono de loading en FF
                    setTimeout(importSolicitudesUpdates, 5000);
                } else {
                    showMessage("#msg-import", "Por favor, complete todos los campos obligatorios", "error", true);
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#import").click(function () {
        $("#import-spread").dialog("open");
    });

    $("#update_solicitados").click(function () {
        $("#update_solicitados_box").dialog("open");
    });

    $("#view-solicitude").dialog();

    $("#new-solicitation").dialog({
        autoOpen: false,
        resizable: true,
        width: 1000,
        modal: true,
        open: function () {
            $("#frmSolicitude").validate().resetForm();

            // Si el plan es institucional, el CUE del solicitado debe ser el mismo del plan
            if ($("#ImprovementPlanType").val() == "Institucional") {
                $("#SolicitudeDTO_CUE").val($("#CUE").val());
                $("#SolicitudeDTO_CUE").attr("disabled", "disabled");
            }

            // Si es un nuevo solicitado, por defecto solo puedo crear solicitados en estados Pendiente
            if ($("#SolicitudeDTO_SolicitudeId").val() == "0") {
                $("#SolicitudeDTO_StatusId").empty();
                //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#SolicitudeDTO_StatusId");
                $("<option>").attr("value", "10").text("Pendiente").appendTo("#SolicitudeDTO_StatusId");
                $("#SolicitudeDTO_SolicitudeTypeId").attr("disabled", "disabled");
            }

            // Habilito la reasignación solo si el plan está en estado 'En Ingreso' o 'En Evaluación' y esta sin seleccionar o si es un solicitado orginal
            if (($("#OriginalStatusId").val() == "1" || $("#OriginalStatusId").val() == "3") && $("#SolicitudeDTO_SolicitudeTypeId").val() == "2") {
                $("#SolicitudeDTO_Reassigned").removeAttr("disabled", "disabled");
                $(".select-reassigned").removeClass("disabled");
            } else {
                $("#SolicitudeDTO_Reassigned").attr("disabled", "disabled");
                $(".select-reassigned").addClass("disabled");
            }

            // Campos deshabilitados por defecto
            $("#SolicitudeDTO_ApprovedAmount").attr("disabled", "disabled");
            $("#SolicitudeDTO_ApprovedPriceUnit").attr("disabled", "disabled");
        },
        close: function () {
            enableSolicitdeForm();
            clearSolicitudeForm();
            $("#SolicitudeDTO_SolicitudeId").val("0");
            $(".ui-dialog-buttonset").find("button :first").show(); // Por defecto, vuelvo a dejar siempre visible el botón de guardar            
        },
        buttons: {
            "Guardar": function () {
                $("#frmSolicitude").validate().resetForm();
                if (validSolicitude()) {
                    if (!canSubmitForm(getDialogFirstButton("#new-solicitation"))) {
                        return false;
                    }
                    showLoading();
                    $.post(BaseSiteURL + "/Plan/SaveSolicitude", {
                        ImprovementPlanId: $("#Id").val(),
                        SolicitudeId: $("#SolicitudeDTO_SolicitudeId").val(),
                        CUE: $("#SolicitudeDTO_CUE").val(),
                        SchoolYearId: $("#SolicitudeDTO_SchoolYearId :selected").val(),
                        Details: $("#SolicitudeDTO_Details").val(),
                        LineId: $("#SolicitudeDTO_LineId :selected").val(),
                        ExpenditureTypeId: $("#SolicitudeDTO_ExpenditureTypeId :selected").val(),
                        Specialization: $("#SolicitudeDTO_Specialization :selected").val(),
                        SolicitudeTypeId: $("#SolicitudeDTO_SolicitudeTypeId :selected").val(),
                        Reassigned: $("#SolicitudeDTO_Reassigned").val(),
                        FileNumber: $("#SolicitudeDTO_FileNumber").val(),
                        StatusId: $("#SolicitudeDTO_StatusId :selected").val(),
                        MeasurementUnitId: $("#SolicitudeDTO_MeasurementUnitId :selected").val(),
                        RequestedAmount: $("#SolicitudeDTO_RequestedAmount").val(),
                        RequestedPriceUnit: $("#SolicitudeDTO_RequestedPriceUnit").val(),
                        ApprovedAmount: $("#SolicitudeDTO_ApprovedAmount").val(),
                        ApprovedPriceUnit: $("#SolicitudeDTO_ApprovedPriceUnit").val()
                    }).done(function (data) {
                        if (data != "") {
                            reloadSolicitudes();

                            updateSummaryTable();
                            $("#new-solicitation").dialog("close");
                            showMessage("#msg-plan", "El solicitado se guardó con éxito", "confirmation", true);
                        } else {
                            showMessage("#msg-solicitude", "El CUE ingresado o la línea seleccionada no son válidos.", "attention");
                            hideLoading();
                        }

                    }).fail(function (xhr, textStatus, errorThrown) {
                        var responseError = xhr.responseText.substr(xhr.responseText.indexOf("<title>") + 7, xhr.responseText.indexOf("</title>") - xhr.responseText.indexOf("<title>") - 7);
                        showMessage("#msg-solicitude", responseError, "error");
                        enableSubmitButtons();
                        hideLoading();
                    });
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#new-comment").dialog({
        autoOpen: false,
        resizable: false,
        width: 350,
        modal: true,
        open: function () {
            $("#msg-comment").empty();
            $("#CommentText").val("");
            $("#chkIncludeDictum").prop('checked', false);
            $("#frmComments").validate().resetForm();

            if ($(this).attr("comment-type") == "Solicitude") {
                $("#include-in-dictum").show();
            } else {
                $("#include-in-dictum").hide();
            }
        },
        buttons: {
            "Guardar": function () {
                if ($("#frmComments").valid()) {
                    if (!canSubmitForm(getDialogFirstButton("#new-comment")))
                        return false;

                    switch ($(this).attr("comment-type")) {
                        case "ImprovementPlan":
                            saveImprovementPlanComment($("#Id").val());
                            break;
                        case "Incidence":
                            saveIncidenceComment($(this).attr("data-incidenceId"), $(this).attr("data-solicitudeId"));
                            break;
                        case "Solicitude":
                            saveSolicitudeComment($(this).attr("data-solicitudeId"));
                            break;
                    }
                }
                else {
                    showMessage("#msg-comment", "Por favor, complete todos los campos obligatorios", "error");
                }
            }
        }
    });

    $("#solicitudes-to-reassign").dialog({
        autoOpen: false,
        resizable: false,
        maxHeight: 350,
        width: 800,
        modal: true,
        open: function () {
            $("#msg-reassign").empty();
            $("#ReassignedTotal").val("");
        },
        buttons: {
            "Seleccionar": function () {
                $("#msg-reassign").empty();
                if ($(".select-reassigned:checked").length == 0) {
                    showMessage("#msg-reassign", "Por favor, seleccione al menos un solicitado", "error");
                } else {
                    // Paso Id de reasignacion al formulario de solicitados
                    var $row = $(".select-reassigned:checked").parent().parent();
                    $("#SolicitudeDTO_Reassigned").val($row.attr("data-reassignFromId"));
                    $("#ReassignedTotal").val($row.attr("data-availableTotal"));
                    $("#solicitudes-to-reassign").dialog("close");
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#view-timeline").dialog({
        autoOpen: false,
        resizable: false,
        width: 660,
        height: 400,
        modal: true,
        buttons: {
            "Cerrar": function () {
                $(this).dialog("close");
            },
        }
    });

    $("#timeline").click(function () {
        showLoading();
        $.get(BaseSiteURL + "/Plan/ListUserTimeLine/", { improvementPlanId: $("#Id").val() }).done(function (data) {
            $("#view-timeline").empty();
            $("#view-timeline").append($("<p>").addClass("celeste").text("Última modificación:"));
            $("#view-timeline").append($("<p>").text(data[0])); // Siempre deberia tener al menos un cambio de estado

            if (data.length > 1) {
                $("#view-timeline").append($("<p>").addClass("celeste").text("Modificaciones anteriores:"));

                for (var i = 1; i < data.length; i++) {
                    $("#view-timeline").append($("<p>").text(data[i]));
                }
            }

            $("#view-timeline").dialog("open");
            hideLoading();
        });
    });

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

    $(".select-reassigned").click(showReassigned);

    // Me aseguro de que se valide el formulario de números de expediente (aunque no este visible) cuando corresponda
    $("#frmFileNumbers").data("validator").settings.ignore = "";

    bindDocuments();
    bindIncidencesEvents();

    var nonSortableCols = $('.show-cue-col').length > 0 ? [0, 1, 9, 10] : [0, 1, 8, 9];

    solicitudes_table = $('#listSolicitados').dataTable({
        "bPaginate": true,
        "iDisplayLength": 20,
        "bLengthChange": false,
        "bFilter": true, // filtro de palabras
        "searchDelay": 800,
        "bSort": true,
        "bInfo": true,
        "bAutoWidth": false,
        "bServerSide": true,
        "sAjaxSource": BaseSiteURL + "/Plan/SearchSolicitudes?iCustomSearch_ImprovementPlanId=" + $("#planId").val(),
        "sDom": '<"toolbar">frtip',
        "aoColumnDefs": [
            { "bSortable": false, "aTargets": nonSortableCols }
        ]
    });

    solicitudes_table.on('preXhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', .4);
    });
    solicitudes_table.on('xhr.dt', function (e, settings, data) {
        $(e.currentTarget).fadeTo('fast', 1);
    })
    solicitudes_table.on('order.dt', function (e, settings, data) {
    });

    solicitudes_table.on('page.dt', function () {
        deSelectAllVisibleSolicitudes();
    });

    solicitudes_table.on('draw.dt', function () {
        deSelectAllVisibleSolicitudes();
        bindSolicitudes();
        showHideSolicitudesActions();
        $('.solicitude-action-view.solicitude-disabled').parent().parent().addClass('solicitude-disabled');
        $('#tabLoadingDiv').hide();
    });

    $(".plan-container").removeClass("hidden");
});

function importSolicitudes() {
    showLoading();
    $("#frmImportSolicitudes").submit();
}

function importSolicitudesUpdates() {
    // Submiteo el formulario con un timeout, porque no da tiempo a mostrar el icono de loading en FF
    $("#frmImportSolicitudesUpdates").submit();
}

function clearSolicitudeForm() {
    // Limpio mensaje de error
    $("#msg-solicitude").empty();

    // Limpio valores
    $("#SolicitudeDTO_CUE").val("");
    $("#SolicitudeDTO_FileNumber").val("");
    $("#SolicitudeDTO_SchoolYearId").val("");
    $("#SolicitudeDTO_Details").val("");
    $("#SolicitudeDTO_LineId").val("");
    $("#SolicitudeDTO_ExpenditureTypeId").val("");
    $("#SolicitudeDTO_Specialization").val("");
    $("#SolicitudeDTO_SolicitudeTypeId").val($("#SolicitudeDTO_SolicitudeTypeId option:first").val());
    $("#SolicitudeDTO_Reassigned").val("");
    $("#SolicitudeDTO_StatusId").val("");
    $("#SolicitudeDTO_MeasurementUnitId").val("");
    $("#SolicitudeDTO_RequestedAmount").val("");
    $("#SolicitudeDTO_RequestedPriceUnit").val("");
    $("#SolicitudeDTO_RequestedTotal").val("");
    $("#SolicitudeDTO_ApprovedAmount").val("");
    $("#SolicitudeDTO_ApprovedPriceUnit").val("");
    $("#SolicitudeDTO_ApprovedTotal").val("");

    // Limpio hidden con el total reasignado
    $("#ReassignedTotal").val("");
}

function validFileNumbers() {
    // Si esta en caratulización, valido que complete todos los numeros de expediente
    if (($("#UserRole").val() == "Ingreso y Monitoreo" || $("#UserRole").val() == "Admin") && $("#OriginalStatusId").val() == "2")
        return $("#frmFileNumbers").valid();

    return true;
}

function validSolicitude() {
    // Valido que el formulario sea válido
    if (!$("#frmSolicitude").valid()) {
        showMessage("#msg-solicitude", "Por favor, complete todos los campos obligatorios", "error");
        return false;
    }

    var requestedAmount = Number($("#SolicitudeDTO_RequestedAmount").val().replace(",", ".")).toFixed(2);
    var requestedPriceUnit = Number($("#SolicitudeDTO_RequestedPriceUnit").val().replace(",", ".")).toFixed(2);

    if (requestedAmount == 0) {
        showMessage("#msg-solicitude", "La cantidad pedida debe ser mayor a cero", "error");
        return false;
    }

    if (requestedPriceUnit == 0) {
        showMessage("#msg-solicitude", "El precio unitario pedido debe ser mayor a cero", "error");
        return false;
    }

    // Si es una reasignación, verifico que la cantidad solicitada y la cantidad aprobada no pueda ser mayor a la disponible
    if ($("#SolicitudeDTO_SolicitudeTypeId :selected").val() == "2") {
        //var reassignedTotal = parseFloat($("#ReassignedTotal").val().replace(",", "."));
        var reassignedTotal = Number($("#ReassignedTotal").val().replace(",", ".")).toFixed(2);
        var requestedTotal = parseFloat($("#SolicitudeDTO_RequestedTotal").val().replace("$", "").replace(",", "."));
        var approvedTotal = parseFloat($("#SolicitudeDTO_ApprovedTotal").val().replace("$", "").replace(",", "."));

        if (reassignedTotal < requestedTotal) {
            showMessage("#msg-solicitude", "El monto solicitado debe ser menor al total disponible del solicitado del cual se reasignar los fondos", "error");
            return false;
        }

        if (reassignedTotal < approvedTotal) {
            showMessage("#msg-solicitude", "El monto aprobado debe ser menor al total disponible del solicitado del cual se reasignar los fondos", "error");
            return false;
        }
    }

    //  Si el formulario es válido, verifico que haya completado los montos aprobados 
    if ($("#OriginalStatusId").val() == "3") {
        // Si quiero pasar a estado aprobado o elegible
        if ($("#SolicitudeDTO_StatusId :selected").val() == "12" || $("#SolicitudeDTO_StatusId :selected").val() == "13") {
            // Solicito los campos
            if ($.trim($("#SolicitudeDTO_ApprovedAmount").val()) == "" || $.trim($("#SolicitudeDTO_ApprovedPriceUnit").val()) == "") {
                if ($.trim($("#SolicitudeDTO_ApprovedAmount").val()) == "") {
                    $("#SolicitudeDTO_ApprovedAmount").addClass("input-validation-error");
                }

                if ($.trim($("#SolicitudeDTO_ApprovedPriceUnit").val()) == "") {
                    $("#SolicitudeDTO_ApprovedPriceUnit").addClass("input-validation-error");
                }

                showMessage("#msg-solicitude", "Por favor, complete todos los campos obligatorios", "error");
                return false;
            }

            // Valido que los importes aprobados sean menores o iguales a los solicitados          
            var approvedAmount = parseFloat($("#SolicitudeDTO_ApprovedAmount").val().replace(",", "."));
            var approvedPriceUnit = parseFloat($("#SolicitudeDTO_ApprovedPriceUnit").val().replace(",", "."));

            if (approvedAmount == 0) {
                showMessage("#msg-solicitude", "La cantidad aprobada debe ser mayor a cero", "error");
                return false;
            }

            if (approvedPriceUnit == 0) {
                showMessage("#msg-solicitude", "El precio unitario aprobado debe ser mayor a cero", "error");
                return false;
            }
        }
    }

    return true;
}

function calculateRequestedAmountTotal() {
    var amount = Number($("#SolicitudeDTO_RequestedAmount").val().replace(",", ".")).toFixed(2);
    //var priceUnit = $("#SolicitudeDTO_RequestedPriceUnit").val();
    var priceUnit = Number($("#SolicitudeDTO_RequestedPriceUnit").val().replace(",", ".")).toFixed(2);

    if (!isNaN(amount) && amount.length != 0 && !isNaN(priceUnit) && priceUnit.length != 0) {
        //var total = (amount * priceUnit).toCurrency();
        $("#SolicitudeDTO_RequestedTotal").val((priceUnit * parseInt(amount)).toFixed(2).replace('.', ',') );
    }
    else {
        $("#SolicitudeDTO_RequestedTotal").val("");
    }
}

function updatePriceUnit() {
    if ($.trim($(this).val()) != "") {
        //var priceUnit = $(this).val().replace(",", ".")
        //$(this).val(parseFloat(parseFloat(priceUnit).toFixed(4)).toCurrency());
        $(this).val(Number($(this).val().replace(",", ".")).toFixed(2).replace('.', ','));
    }
}

function openChangeStatusDialog() {
    $("#change-confirm")
        .attr("data-incidenceId", $(this).attr("data-incidenceId"))
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .dialog("open");
}

function calculateAmountTotal($txt) {
    var amount = $txt.hasClass("requestedAmount")
        ? $("#SolicitudeDTO_RequestedAmount").val()
        : $("#SolicitudeDTO_ApprovedAmount").val();

    var priceUnit = $txt.hasClass("requestedAmount")
        ? $("#SolicitudeDTO_RequestedPriceUnit").val()
        : $("#SolicitudeDTO_ApprovedPriceUnit").val();

    var $total = $txt.hasClass("requestedAmount")
        ? $("#SolicitudeDTO_RequestedTotal")
        : $("#SolicitudeDTO_ApprovedTotal");

    if (amount.length != 0 && priceUnit.length != 0) {
        //var total = "$ " + (amount * Globalize.parseFloat(priceUnit).toFixed(4)).toCurrency();
        var total = Number(amount.replace(',', '.')).toFixed(2) * Number(priceUnit.replace(',', '.')).toFixed(2);
        $total.val(total.toFixed(2).replace('.',','));
    }
    else {
        $total.val("");
    }
}

function loadStatusAndSpecializations() {
    var $source = $(this);
    showLoading();
    $.get(BaseSiteURL + "/Plan/LoadSolicitudeCombos", {
        improvementPlanId: $("#Id").val(),
        improvementPlanStatusId: $("#OriginalStatusId").val(),
        SolicitudeId: $source.attr("data-solicitudeId")
    }).done(function (data) {
        // Cargo el combo de especializaciones para ese solicitado
        $("#SolicitudeDTO_Specialization").empty();
        $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SolicitudeDTO_Specialization");

        $.each(data.Specializations, function (i, item) {
            $("<option>").attr("value", item.Key).text(item.Value).appendTo("#SolicitudeDTO_Specialization");
        });

        // Cargo combo con los estados posibles
        $("#SolicitudeDTO_StatusId").empty();
        //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#SolicitudeDTO_StatusId");

        $.each(data.Status, function (i, item) {
            $("<option>").attr("value", item.Id).text(item.Description).appendTo("#SolicitudeDTO_StatusId");
        });

        loadSolicitude($source.attr("data-solicitudeId"), $source);
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        hideLoading();
    });
}

function loadSolicitude(id, $source) {
    showLoading();
    $.get(BaseSiteURL + "/Plan/GetSolicitude", {
        solicitudeId: id
    }).done(function (data) {
        $("#SolicitudeDTO_SolicitudeId").val(data.Id);
        $("#SolicitudeDTO_CUE").val(data.CUE);
        $("#SolicitudeDTO_FileNumber").val(data.FileNumber);
        $("#SolicitudeDTO_SchoolYearId").val(data.SchoolYearId);
        $("#SolicitudeDTO_Details").val(data.Details);
        $("#SolicitudeDTO_LineId").val(data.LineId);
        $("#SolicitudeDTO_ExpenditureTypeId").val(data.ExpenditureTypeId);
        $("#SolicitudeDTO_Specialization").val(data.Specialization);
        $("#SolicitudeDTO_SolicitudeTypeId").val(data.SolicitudeTypeId);
        $("#SolicitudeDTO_Reassigned").val(data.ReassignedId);
        $("#SolicitudeDTO_StatusId").val(data.StatusId);
        $("#SolicitudeDTO_MeasurementUnitId").val(data.MeasurementUnitId);

        data.RequestedAmount = parseInt(data.RequestedAmount).toFixed(2).replace('.', ',');
        data.RequestedPriceUnit = Number(data.RequestedPriceUnit).toFixed(2).replace('.', ',');
        data.RequestedTotal = Number(data.RequestedTotal).toFixed(2).replace('.', ',');

        data.ApprovedAmount = data.ApprovedAmount ? Number(data.ApprovedAmount).toFixed(2).replace('.', ',') : "";
        data.ApprovedPriceUnit = data.ApprovedPriceUnit ? Number(data.ApprovedPriceUnit).toFixed(2).replace('.', ',') : "";
        data.ApprovedTotal = data.ApprovedAmount ? Number(data.ApprovedTotal).toFixed(2).replace('.', ',') : "";

        $("#SolicitudeDTO_RequestedAmount").val(data.RequestedAmount);
        $("#SolicitudeDTO_RequestedPriceUnit").val(data.RequestedPriceUnit);
        $("#SolicitudeDTO_RequestedTotal").val(data.RequestedTotal);

        $("#SolicitudeDTO_StatusId").attr('data-value', data.StatusId);

        $("#SolicitudeDTO_ApprovedAmount").attr('data-value', data.ApprovedAmount);
        $("#SolicitudeDTO_ApprovedPriceUnit").attr('data-value', data.ApprovedPriceUnit);
        $("#SolicitudeDTO_ApprovedTotal").attr('data-value', data.ApprovedTotal);

        $("#SolicitudeDTO_ApprovedAmount").val(data.ApprovedAmount);
        $("#SolicitudeDTO_ApprovedPriceUnit").val(data.ApprovedPriceUnit);
        $("#SolicitudeDTO_ApprovedTotal").val(data.ApprovedTotal);

        // Guardo cual era el CUE original de la solicitud, para buscar las especializaciones solo si este se modifica
        $("#OriginalSolicitudeCUE").val(data.CUE);

        var className = "";

        // Habilito o deshabilito controles
        if ($source.hasClass("solicitude-action-view") || data.canEdit == false || data.disabled == true) {
            disableSolicitudeForm();
            className = "disabled-solicitude";
        } else if ($source.hasClass("solicitude-action-edit") && data.canEdit == true && data.disabled == false) {
            enableSolicitdeForm();
            className = "enabled-solicitude";

            // si el solicitado esta aprobado se puede editar el estado, pero todo lo demás no
            if ($("#SolicitudeDTO_StatusId :selected").val() == "12") {
                disableSolicitudeForm();
                $(".ui-dialog-buttonset").find("button :first").show();
                $("#SolicitudeDTO_StatusId").prop('disabled', false);
            }
        }

        $("#new-solicitation").removeClass("disabled-solicitude").removeClass("enabled-solicitude")
            .addClass(className)
            .dialog("open");

        hideLoading();

    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        hideLoading();
    });
}

function enableSolicitdeForm() {
    $("#new-solicitation").find("input, select, textarea")
        .not(".totalAmount")
        .removeAttr("disabled", "disabled");

    $(".ui-dialog-buttonset").find("button :first").show();
}

function disableSolicitudeForm() {
    $("#new-solicitation").find("input, select, textarea")
        .not(".totalAmount")
        .attr("disabled", "disabled");

    $(".ui-dialog-buttonset").find("button :first").hide();
}

function loadSolicitudeIncidence() {
    openIncidenceDialog($(this).attr("data-solicitudeId"));
}

function deleteSolicitude() {
    $("#delete-confirm-solicitude")
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .dialog("open");
}

function deleteImprovementPlan() {
    $("#delete-confirm").find(".delete-text").text("¿Desea borrar el Plan de Mejora " + $("#Identifier").val() + "?");
    $("#delete-confirm").dialog("open");
}

function blockSolicitude() {
    $("#block-confirm-solicitude")
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .dialog("open");
}

function openIncidenceDialog(solicitudeId) {
    $("#new-incidence").attr("data-solicitudeId", solicitudeId).dialog("open");
}

function solicitudeComment() {
    $("#new-comment")
        .attr("comment-type", "Solicitude")
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .dialog("open");
}

function incidenceComment() {
    $("#new-comment")
        .attr("comment-type", "Incidence")
        .attr("data-incidenceId", $(this).attr("data-IncidenceId"))
        .attr("data-solicitudeId", $(this).attr("data-solicitudeId"))
        .dialog("open");
}

function saveImprovementPlanComment(improvementPlanId) {
    showLoading();
    $.post(BaseSiteURL + "/Plan/SaveImprovementPlanComment", {
        ImprovementPlanId: improvementPlanId,
        Text: $("#CommentText").val()
    }).done(function (data) {
        $(".comments").html(data);
        $("#new-comment").dialog("close");
        showMessage("#msg-plan", "El comentario se guardó con éxito", "confirmation", true);
        hideLoading();
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al guardar el comentario", "error", true);
        hideLoading();
    });
}

function saveIncidenceComment(incidenceId, solicitudeId) {
    showLoading();
    $.post(BaseSiteURL + "/Plan/SaveIncidenceComment", {
        ImprovementPlanId: $("#Id").val(),
        IncidenceId: incidenceId,
        SolicitudeId: solicitudeId,
        Text: $("#CommentText").val(),
        ReturnAll: true
    }).done(function (data) {
        if (solicitudeId != "") {
            // Se grabó un comentario de la incidencia de un solicitado
            $("#view-solicitudes-incidences .solicitude-incidences").empty();
            $("#view-solicitudes-incidences .solicitude-incidences").html(data);
            bindSolicitudeIncidenceEvents();
            $("#new-comment").dialog("close");
            showMessage("#msg-solicitude-incidences", "El comentario de la incidencia del solicitado se guardó con éxito", "confirmation", true);
        } else {
            // Se grabó un comentario de una incidencia general del plan
            $(".incidences").html(data);
            bindIncidencesEvents();
            $("#new-comment").dialog("close");
            showMessage("#msg-plan", "El comentario de la incidencia se guardó con éxito", "confirmation", true);
        }
        hideLoading();
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al guardar el comentario", "error", true);
        hideLoading();
    });
}

function saveSolicitudeComment(solicitudeId) {
    showLoading();
    $.post(BaseSiteURL + "/Plan/SaveSolicitudeComment", {
        ImprovementPlanId: $("#Id").val(),
        IncludeInDictum: $("#chkIncludeDictum").is(":checked"),
        SolicitudeId: solicitudeId,
        Text: $("#CommentText").val()
    }).done(function (data) {

        reloadSolicitudes();

        $("#new-comment").dialog("close");
        showMessage("#msg-plan", "El comentario de la solicitud se guardó con éxito", "confirmation", true);
        hideLoading();
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al guardar el comentario", "error", true);
        hideLoading();
    });
}

function showReassigned() {
    if (!$(this).hasClass("disabled")) {
        $.get(BaseSiteURL + "/Plan/ListSolicitudesToReassign", {
            cue: $("#SolicitudeDTO_CUE").val(),
            improvementPlanId: $("#Id").val()
        }).done(function (data) {
            $('#list-reassignedSolicitudes').dataTable().fnClearTable(); // Limpia la tabla

            // Agrego los solicitados del dictamen
            $.each(data, function (index, item) {
                // Agrego fila
                var oTable = $('#list-reassignedSolicitudes').dataTable();
                var radio = "<input type='radio' class='select-reassigned' name='radio-reassigned' />";
                var api = oTable.fnAddData([radio, item.Id, item.Details, item.Resolution, item.ApprovedTotalAsCurrency, item.ReassignedTotalAsCurrency, item.AvailableTotalAsCurrency]);

                // Agrego el atributo con el id del solicitado a la fila
                var $row = $(oTable.fnSettings().aoData[api[0]].nTr)
                $row.attr("data-reassignFromId", item.Id);
                $row.attr("data-availableTotal", item.AvailableTotal);
            });

            $("#solicitudes-to-reassign").dialog("open");
        });
    }
}

function listSolicitudeComments() {
    showLoading();
    $.get(BaseSiteURL + "/Plan/ListSolicitudeComments", {
        solicitudeId: $(this).attr("data-solicitudeId")
    }).done(function (data) {
        $("#view-solicitudes-comments .comments").empty();
        for (var i = 0; i < data.comments.length; i++) {
            var comment = data.comments[i];
            var date = comment.CommentDate;
            var user = comment.User;
            var message = comment.Text;
            var includeInDictum = (comment.IncludeInDictum) ? "Sí" : "No";

            var $li = $("<li>").append($("<div>").addClass("user").text(user).prepend($("<span>").addClass("celeste").text("Usuario: ")))
                .append($("<div>").addClass("date").text(date).prepend($("<span>").addClass("celeste").text("Fecha: ")))
                .append($("<div>").addClass("date").text(includeInDictum).prepend($("<span>").addClass("celeste").text("Incluido en el dictamen: ")))
                .append($("<div>").addClass("message").append($("<p>").text(message)).prepend($("<span>").addClass("celeste").text("Comentario: ")));



            $("#view-solicitudes-comments .comments").append($li);
        }

        if (data.comments.length == 0) {
            $("#view-solicitudes-comments .comments").append($("<li>").addClass("no-comments").text("No existen comentarios disponibles para este solicitado"));
        }
        hideLoading();
        $("#view-solicitudes-comments").dialog("open");
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar los comentarios", "error", true);
        hideLoading();
    });
}

function listSolicitudeIncidences() {
    // Actualizo las incidencias del solicitado y lo muestro en el dialogo
    showLoading();
    $.get(BaseSiteURL + "/Plan/ListSolicitudeIncidences", {
        solicitudeId: $(this).attr("data-solicitudeId")
    }).done(function (data) {
        $("#view-solicitudes-incidences .solicitude-incidences").empty();

        if ($.trim(data) == "") {
            $("#view-solicitudes-incidences .solicitude-incidences").append($("<ul>").addClass("comments").append($("<li>").addClass("no-comments").text("No existen incidencias disponibles para este solicitado")));
        } else {
            $("#view-solicitudes-incidences .solicitude-incidences").html(data);
            bindSolicitudeIncidenceEvents();
        }

        $("#view-solicitudes-incidences").dialog("open");
        hideLoading();
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar las incidencias", "error", true);
        hideLoading();
    });
}

function updateSummaryTable() {
    // Actualizo la tabla de resumen    
    showLoading();
    $.get(BaseSiteURL + "/Plan/UpdateSummary", {
        improvementPlanId: $("#Id").val()
    }).done(function (data) {
        $("#summary-wrapper").html(data);
        //$('.solicitude-checkbox').click(updateAmmount);
        hideLoading();
    }).fail(function () {
        showMessage("#msg-plan", "Ocurrió un error no especificado al recuperar los datos", "error", true);
        hideLoading();
    });
}

function toggleIncidentComments() {
    $("." + $(this).attr("data-commentIncidence")).toggle();
}

function bindIncidencesEvents() {
    $(".status").click(openChangeStatusDialog);
    $(".incidence-comment").click(incidenceComment);
    $(".icon-comment").click(toggleIncidentComments);
}

function bindSolicitudeIncidenceEvents() {
    $("#view-solicitudes-incidences .solicitude-incidences .incidence-comment").click(incidenceComment);
    $("#view-solicitudes-incidences .solicitude-incidences .status").click(openChangeStatusDialog);
    $("#view-solicitudes-incidences .solicitude-incidences .icon-comment").click(toggleIncidentComments);
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


function showHideSolicitudesActions() {
    var canReadSolComments = $('#canReadSolComments').val() == 1;
    var canWriteSolComments = $('#canWriteSolComments').val() == 1;

    var canReadSolIncidents = $('#canReadSolIncidents').val() == 1;
    var canWriteSolIncidents = $('#canWriteSolIncidents').val() == 1;

    $rows = $('#listSolicitados tbody tr');
    $rows.each(function (i, e) {
        var canEditItem = $('.solicitude-action-edit', this).length > 0;

        if (!canEditItem) $('.selectedSolicitudes', this).hide();

        if (!canReadSolComments) $('.solicitude-view-comment', this).hide();
        if (!canReadSolIncidents) $('.solicitude-view-incidences', this).hide();

        if (!canWriteSolComments || !canEditItem) $('.solicitude-action-comment', this).hide();
        if (!canWriteSolIncidents || !canEditItem) $('.solicitude-action-incidence', this).hide();
        
    });
}

function reloadSolicitudes() {
    var current_page = $('#listSolicitados').DataTable().page();
    $('#listSolicitados').DataTable().ajax.reload();
    $('#listSolicitados').DataTable().page(current_page);
    hideLoading();
}

function bindSolicitudes() {
    $(".solicitude-action-view, .solicitude-action-edit").click(loadStatusAndSpecializations);
    $(".solicitude-action-delete").click(deleteSolicitude);
    $(".solicitude-action-block").click(blockSolicitude);
    $(".solicitude-action-comment").click(solicitudeComment);
    $(".solicitude-action-incidence").click(loadSolicitudeIncidence);
    $(".solicitude-view-comment").click(listSolicitudeComments);
    $(".solicitude-view-incidences").click(listSolicitudeIncidences);
}

function selectAllVisibleSolicitudes() {
    $('#listSolicitados td input[type="checkbox"]').prop('checked', true);
}

function deSelectAllVisibleSolicitudes() {
    $('#listSolicitados td input[type="checkbox"]').prop('checked', false);
}

function triggerSolicitudeGroupUpdate(action) {
    var selected = $('#listSolicitados td input[type="checkbox"]:checked');
    $("#msg-solicitudes-group-actions").empty();
    if (selected.length > 0) {
        $('#solicitudes-group-actions').html('<p>Procesando, aguarde por favor...</p>').next().empty();
        $('#solicitude_group_action').val(action);
        $('#frmSolicitudesGroup').submit();
    } else {
        showMessage("#msg-solicitudes-group-actions", "Debe seleccionar al menos un solicitado primero.", "error");
    }
}