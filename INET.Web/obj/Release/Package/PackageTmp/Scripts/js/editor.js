$(document).ready(function () {
    // Agregado para validar que la fecha seleccionada en los combos sea válida
    $.validator.methods.date = function (value, element) {
        if ($("#ReceptionDateDay").val() == "" || $("#ReceptionDateMonth").val() == "" || $("#ReceptionDateYear").val() == "")
            return true;

        var date = $("#ReceptionDateDay").val() + "/" + $("#ReceptionDateMonth").val() + "/" + $("#ReceptionDateYear").val();
        var parsed = Globalize.parseDate(date)
        return this.optional(element) || (parsed != null && !/Invalid|NaN/.test(parsed));
    }

    $('input[dataReadstatus="readonly"]').attr("readonly", "readonly");

    showServerMessageResult("#msg-plan");

    $("#opencal").datepicker({
        beforeShow: function () {            
            //$(document).tooltip("destroy");
            $("#opencal").attr("title", "");
            $(".back").tooltip();
            $(".admin-icons a").tooltip();
        },
        onSelect: function (dateText, inst) {
            var pieces = dateText.split('/');
            $('#ReceptionDateDay').val(pieces[1]);
            $('#ReceptionDateMonth').val(pieces[0]);
            $('#ReceptionDateYear').val(pieces[2]);
        },
        onClose: function () {
            $("#opencal").attr("title", "Abrir Calendario");            
            //$(document).tooltip();
        }
    });

    $("#ImprovementPlanTypeId").change(function () {
        if ($("#ImprovementPlanTypeId :selected").val() == 1) {
            $("#CUE").val("000000000");
            $("#CUE").attr("readonly", "readonly")
        }
        else if ($("#CUE").attr('dataReadstatus') != 'readonly')
        {
            $("#CUE").val("");
            $("#CUE").removeAttr("readonly");
        }
    });

    $("#delete-confirm").dialog({
        autoOpen: false,
        resizable: false,
        width: 380,
        modal: true,
        buttons: {
            "Eliminar": function () {                
                $.post(BaseSiteURL + "/Plan/DeletePlan", {
                    ImprovementPlanId: $("#Id").val()
                }).done(function (data) {
                    if (data != "") {
                        window.location.href = "/Plan/List";
                    } else {
                        showMessage("#msg-plan", "No es posible eliminar el plan " + $("#Identifier").val() + ".", "error", true);
                    }
                    $("#delete-confirm").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".delete").click(function () {        
        $("#delete-confirm").find(".delete-text").text("¿Desea borrar el Plan de Mejora " + $("#Identifier").val() + "?");
        $("#delete-confirm").dialog("open");
    });

    $("form").submit(function () {
        if (!$("#frmEditor").valid()) {
            showMessage("#msg-plan", "Por favor, complete los campos requeridos para continuar", "error");
            return false;
        } else {
            if (!$(".btn-save").hasClass("disabled-button")) {
                $(".btn-save").addClass("disabled-button");                
                return true;
            } else {
                return false;
            }
        }
    });
});