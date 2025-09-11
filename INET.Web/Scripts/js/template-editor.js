var _selectedVariable = null;

$(document).ready(function () {
    showServerMessageResult("#msg-template");

   // $(document).tooltip("destroy"); // Los tooltip interfieren con los title delk TinyMCE, por eso los deshabilito
    $(".back").tooltip(); // Aplico el tooltip solo para los botones del menú
    
    tfm_path = $("#FileManagerUrl").val(); // Esta variable es la que especifica la aplicacion que se utiliza para subir las imagenes
    
    // Inicializo tinyMCE
    initializeTinyMCE("textarea#Content");

    $("#TemplateTypeId").change(function () {
        if ($("#TemplateTypeId :selected").val() != "") {
            $.get(BaseSiteURL + "/Admin/ListTemplateTypeFieldsAndBlocks", {
                templateTypeId: $("#TemplateTypeId :selected").val()
            }).done(function (data) {
                $("#keyWordsLabel").removeClass("hidden");
                $("#keyWords").empty();

                $.each(data.Fields, function (i, item) {
                    $("<label>").addClass("label").text(item.Field + ' => ' + item.Description).appendTo("#keyWords");
                });

                $("#ActionBlocks").empty();
                if (data.Blocks.length > 0) {
                    $("#ActionBlocksLabel").removeClass("hidden");                    
                    $.each(data.Blocks, function (i, item) {
                        $("<label>").addClass("label").text(item.Block + ' => ' + item.Description).appendTo("#ActionBlocks");
                    });
                } else {
                    $("#ActionBlocksLabel").addClass("hidden");
                }
            });
        }
        else {
            $("#keyWordsLabel").addClass("hidden");
            $("#keyWords").empty();

            $("#ActionBlocksLabel").addClass("hidden");
            $("#ActionBlocks").empty();
        }
    });

    $("#TemplateTypeId").change();

    $("#frmTemplate").submit(function () {
        tinyMCE.triggerSave();
        if ($("#frmTemplate").valid() && $.trim(tinyMCE.activeEditor.getContent()) != "") {
            if (validateTemplateKeywords()) {
                var arrayTemplateVariables = new Array();
                var templateVariables = $("#template-variables-wrapper").find(".custom-input");                
                $("#hidden-templates-variables").empty();
                $.each(templateVariables, function (index, item) {
                    setInput("", index, "Variables[" + index + "].TemplateVariableId", $(this).attr("data-id"), "#hidden-templates-variables");
                    setInput("", index, "Variables[" + index + "].VariableName", $(this).val(), "#hidden-templates-variables");
                });

                return true;
            } else {
                showMessage("#msg-template", "El template no es válido. Asegúrese de que contenga las pabras clave [CUERPO] y [FIRMA_[N]] (cuando corresponda).", "attention", true);
                return false;
            }
        } else {
            showMessage("#msg-template", "Por favor, complete todos los campos obligatorios y asegúrese de ingresar contenido", "Error", true);
            return false;
        }
    });

    $("#delete-confirm").dialog({
        autoOpen: false,
        resizable: false,
        minWidth: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                if (!canSubmitForm(getDialogFirstButton("#delete-confirm")))
                    return false;

                var templateId = $(this).attr("data-templateId");
                $.post(BaseSiteURL + "/Admin/DeleteTemplate", {
                    templateId: $("#Id").val()
                }).done(function (data) {
                    enableSubmitButtons();
                    $("#delete-confirm").dialog("close");
                    window.location.href = BaseSiteURL + "/Admin/TemplatesList";
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".delete").click(function () {
        $("#delete-confirm").dialog("open");
    });

    $(".btn-preview").click(function () {
        // Visualizo el documento submiteando otro form con los cambios actuales
        tinyMCE.triggerSave();
        var content = $.trim(tinyMCE.activeEditor.getContent());
        var format = $(this).data('format');
        if (content != "") {
            $("#htmlFormat").val(format != "" ? format : "pdf");
            $("#prevTemplateId").val($("#Id").val());
            $("#htmlContent").val(content);
            $("#htmlHeader").val($("#Header").val());
            $("#frmPreview").submit();
        } else {
            showMessage("#msg-template", "Debe ingresar un contenido para poder visualizar la plantilla.", "attention", true);
        }
    });

    $("#delete-confirm-variable").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {                
                _selectedVariable.parent().remove();
                $("#delete-confirm-variable").dialog("close");
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".delete-variable").click(removeVariable);
    $("#addTemplateFieldVariable").click(addTemplateVariable);    
});

function validateTemplateKeywords() {
    var content = $.trim(tinyMCE.activeEditor.getContent());
    // Si es un dictamen, el cuerpo debe ser obligatorio
    if($("#TemplateTypeId :selected").val() != "4" && content.indexOf("[CUERPO]") == -1)
        return false;

    if($("#TemplateTypeId :selected").val() != "2" && $("#TemplateTypeId :selected").val() != "4") {
        return content.indexOf("[FIRMA_1]") > -1 || content.indexOf("[FIRMA_2]") > -1 || content.indexOf("[FIRMA_3]") > -1 || content.indexOf("[FIRMA_4]") > -1 || content.indexOf("[FIRMA_5]") > -1 || content.indexOf("[FIRMA_6]") > -1;
    }

    return true;
}

function addTemplateVariable() {
    var $variable = $("<div>").addClass("variable");
    $variable.append($("<input>").attr("type", "textbox").attr("maxlength", "100").attr("data-id", "0").addClass("custom-input"));
    $variable.append($("<img>").attr("src", BaseSiteURL + "/Img/icons/icon-delete.png").attr("title", "Eliminar").addClass("delete-variable").click(removeVariable));
    $("#template-variables-wrapper").append($variable);
}

function removeVariable() {
    _selectedVariable = $(this);
    $("#delete-confirm-variable").dialog("open");
}