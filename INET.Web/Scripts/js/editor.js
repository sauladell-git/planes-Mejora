$(document).ready(function () {
    // Agregado para validar que la fecha seleccionada en los combos sea válida
    $.validator.methods.date = function (value, element) {
        if ($("#ReceptionDateDay").val() == "" || $("#ReceptionDateMonth").val() == "" || $("#ReceptionDateYear").val() == "")
            return true;

        var date = $("#ReceptionDateDay").val() + "/" + $("#ReceptionDateMonth").val() + "/" + $("#ReceptionDateYear").val();
        var parsed = Globalize.parseDate(date)
        return this.optional(element) || (parsed != null && !/Invalid|NaN/.test(parsed));
    }


    // Disparo carga inicial
    $("#cbPronafe").change();
    pronafeCheck();
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

    $("#cbPronafe").change(function () {

        clearFieldsLines();
        pronafeCheck();

    });
    function clearFieldsLines()
    {
        $("#SubField").empty();
        $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubField");

        $("#LineId").empty();
        $("<option>").attr("value", "").text("Seleccionar...").appendTo("#LineId");

        $("#FieldId").val("");
        $("#LineId").val("");
       
      

    }

    function pronafeCheck() {
        if ($("#cbPronafe").is(':checked')) {
            $("#divContainer216").hide();
            $("#FieldId").prop("disabled", false);
            $("#LineId").prop("disabled", false);

        } else {
            $("#divContainer216").show();
            $("#FieldId").prop("disabled", true);
            $("#LineId").prop("disabled", true);

            $("#Line22Id").val("");
        }


    }

    $("#FieldId").change(function () {
        // Ejes manejan la linea y el subEje, si  no es pronafe seguir adelante
        if (!$("#cbPronafe").is(':checked'))
            return;

        if ($("#FieldId :selected").val() != 0) {

            // Busco las lineas 
            $.get(BaseSiteURL + "/Plan/ListLinesByField", {
                FieldId: $("#FieldId").val()
                
            }).done(function (data) {
                var prev_selected = $("#").val();
                $("#LineId").empty();
                $("<option>").attr("value", "").text("Seleccionar...").appendTo("#LineId");

                $.each(data, function (i, item) {
                    $("<option>").attr("value", item.Key).text(item.Value).appendTo("#LineId");
                });
            }).fail(function (jqXHR, textStatus, error) {
            showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos" + error, "error", true);
            hideLoading();
        });


            //// Busco los subEjes
            //$.get(BaseSiteURL + "/Plan/ListSubField", {
            //    FieldId: $("#FieldId").val()

            //}).done(function (data) {
                
            //    $("#SubField").empty();
            //    $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubField");

            //    $.each(data, function (i, item) {
            //        $("<option>").attr("value", item.Key).text(item.Value).appendTo("#SubField");
            //    });
            //}).fail(function (jqXHR, textStatus, error) {
            //    showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos" + error, "error", true);
            //    hideLoading();
            //});

        }
        else {
            //$("#SubField").empty();
            //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubField");
            $("#LineId").empty();
            $("<option>").attr("value", "").text("Seleccionar...").appendTo("#LineId");
           
        }

       
    });


    $("#Line22Id").change(function () {
        // En Base a la linea 22 lleno todo lo demas !


        if ($("#Line22Id :selected").val() != 0) {

            // Busco el SubEje
            $.get(BaseSiteURL + "/Plan/Line22DTO", {
                Line22Id: $("#Line22Id").val()

            }).done(function (data) {

                var prev_selected = $("#").val();

                $("#FieldId").val(data.FieldId);

                $("#SubField").empty();
                $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubField");

                $.each(data.SubFields, function (i, item) {
                    $("<option>").attr("value", item.Key).text(item.Value).appendTo("#SubField");
                });

            
          

                $("#LineId").empty();
                $("<option>").attr("value", "").text("Seleccionar...").appendTo("#LineId");

                $.each(data.Lines, function (i, item) {
                    $("<option>").attr("value", item.Key).text(item.Value).appendTo("#LineId");
                });

                $("#SubField").val(data.SubFieldId);
                $("#LineId").val(data.LineId);

                // des-habilito los combos  antes tomo los valores

                $("#hdnFieldId").val(data.FieldId);
                $("#hdnSubFieldId").val(data.SubFieldId);
                $("#hdnLineId").val(data.LineId);

                //var selectedLineId = $('#LineId').val();
                //$('#LineId').prop("disabled", "disabled");
                //$("#hdnLineId").val(selectedLineId);

                //var selectedFieldId = $('#FieldId').val();
                //$('#FieldId').prop("disabled", "disabled");
                //$("#hdnFieldId").val(selectedFieldId);

                //var selectedLineId = $('#SubField').val();
                //$('#SubField').prop("disabled", "disabled");
                //$("#hdnSubFieldId").val(selectedFooId);

                //$("#SubField").attr("readonly", "readonly")
                //$("#LineId").attr("readonly", "readonly")
                //$("#FieldId").attr("readonly", "readonly")

            }).fail(function (jqXHR, textStatus, error) {
                showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos de Eje" + error, "error", true);
                hideLoading();
            });


            //// Busco el Eje
            //$.get(BaseSiteURL + "/Plan/ListLinesBySubField", {
            //    SubFieldId: $("#SubField").val()

            //}).done(function (data) {
            //    var prev_selected = $("#").val();
            //    $("#Line22Id").empty();
            //    $("<option>").attr("value", "").text("Seleccionar...").appendTo("#Line22Id");

            //    $.each(data, function (i, item) {
            //        $("<option>").attr("value", item.Key).text(item.Value).appendTo("#Line22Id");
            //    });
            //}).fail(function (jqXHR, textStatus, error) {
            //    showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos de Eje" + error, "error", true);
            //    hideLoading();
            //});


            //// Busco el Linea
            //$.get(BaseSiteURL + "/Plan/ListLinesBySubField", {
            //    SubFieldId: $("#SubField").val()

            //}).done(function (data) {
            //    var prev_selected = $("#").val();
            //    $("#Line22Id").empty();
            //    $("<option>").attr("value", "").text("Seleccionar...").appendTo("#Line22Id");

            //    $.each(data, function (i, item) {
            //        $("<option>").attr("value", item.Key).text(item.Value).appendTo("#Line22Id");
            //    });
            //}).fail(function (jqXHR, textStatus, error) {
            //    showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos de Eje" + error, "error", true);
            //    hideLoading();
            //});


        

        }
        else {
            $("#SubFields").empty();
            $("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubFields");
            $("#Lines").empty();
            $("<option>").attr("value", "").text("Seleccionar...").appendTo("#Lines");
            $("#Fields").empty();
            $("<option>").attr("value", "").text("Seleccionar...").appendTo("#Fields");
         
        }


    });

    //$("#SubField").change(function () {
    //    // Ejes manejan la linea y el subEje


    //    if ($("#SubField :selected").val() != 0) {

    //        // Busco las lineas 
    //        $.get(BaseSiteURL + "/Plan/ListLinesBySubField", {
    //            SubFieldId: $("#SubField").val()

    //        }).done(function (data) {
    //            var prev_selected = $("#").val();
    //            $("#Line22Id").empty();
    //            $("<option>").attr("value", "").text("Seleccionar...").appendTo("#Line22Id");

    //            $.each(data, function (i, item) {
    //                $("<option>").attr("value", item.Key).text(item.Value).appendTo("#Line22Id");
    //            });
    //        }).fail(function (jqXHR, textStatus, error) {
    //            showMessage("#msg-plan", " UPS! Ocurrió un error no especificado al recuperar los datos" + error, "error", true);
    //            hideLoading();
    //        });




    //    }
    //    else {
    //        //$("#SubFields").empty();
    //        //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#SubFields");
    //        //$("#Lines").empty();
    //        //$("<option>").attr("value", "").text("Seleccionar...").appendTo("#Lines");

    //    }


    //});




});