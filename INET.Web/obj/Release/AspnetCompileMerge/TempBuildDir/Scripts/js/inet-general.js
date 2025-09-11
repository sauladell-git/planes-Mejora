var _edit = false;
var _messageTimeOut;
var _idRow = 0;

jQuery(document).ready(function () {

    $.fn.textify = function (flag) {
        if (typeof flag == 'undefined' || flag == true) {
            // enable
            if (this.parent().find('.textified').length == 0) {
                var value = (this.prop('nodeName').toLowerCase() == 'select') ? this.children().filter(':selected').text() : this.val();
                $('<div class="textified">' + value + '</div>').insertAfter(this);
                this.hide();
            }
        } else {
            // disable
            this.show();
            this.parent().find('.textified').remove();
        }
    };

    $("#delete-confirm").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Eliminar": function () {
                $.post(BaseSiteURL + "/Plan/DeletePlan", {
                    ImprovementPlanId: $(this).attr("data-PlanId")
                }).done(function (data) {
                    window.location.href = BaseSiteURL + "/Plan/List";
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $(".block").click(openBlockDialog);

    $("#block-confirm-plan").dialog({
        autoOpen: false,
        resizable: false,
        width: 300,
        modal: true,
        buttons: {
            "Anular": function () {
                if (!canSubmitForm(getDialogFirstButton("#block-confirm-plan")))
                    return false;

                $.post(BaseSiteURL + "/Plan/BlockPlan", {
                    ImprovementPlanId: $("#Id").val()
                }).done(function (data) {
                    if (data.errorMessage == "") {
                        window.location.href = window.location.href;                        
                    } else {
                        showMessage("#msg-plan", data.errorMessage, "error", true);
                    }

                    enableSubmitButtons();
                    $("#block-confirm-plan").dialog("close");
                });
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
        buttons: {
            "Cargar": function () {
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#import").click(function () {
        $("#import-spread").dialog("open");
    });

    $("#plan-detail").tabs();
    $("#dictum-tabs").tabs();
    $("#resolution-tabs").tabs();
    
    $("#solicitados-detail").tabs({
        select: function (event, ui) {
            $('.scrollable').getNiceScroll().show();
        }
    });
    var hash_parts = location.hash.split('&', 2); //2 - limit, may be changed if more than two arguments.
    var tab = hash_parts[0];               // Tab number part of url.  Array starts at 0 for 1st element.
    var anc = hash_parts[1];               // Anchor name.
    //var tabId = tab.split("-").pop() - 1;      // Tab no. relating to Jquery ui index no. (starts at zero for tab 1.)  

    //$("#plan-detail, #solicitados-detail").tabs("option", "active", tabId);  // Select the tab.
    //$('html, body').animate({'scrollTop': $(anc).offset().top}, 1000); // Animated scroll to anchor.    
});

function getURLParameterValue(paramName) {
    return unescape(window.location.search.replace(new RegExp("^(?:.*[&\\?]" + escape(paramName).replace(/[\.\+\*]/g, "\\$&") + "(?:\\=([^&]*))?)?.*$", "i"), "$1"));
}

function showServerMessageResult(container) {
    var message = getMessageResult();
    if (message != null)
        showMessage(container, message.Text, message.Type, message.FadeOut);
}

function openBlockDialog() {
    $("#block-confirm-plan")
        .attr("data-planId", $(this).parent().parent().attr("data-planId"))
        .dialog("open");
}

function getMessageResult() {
    if ($("#MessageResultText").length == 0)
        return null;

    var message = new Object();
    message.Text = $("#MessageResultText").val();
    message.Type = $("#MessageResultType").val();
    message.FadeOut = $("#MessageResultFadeOut").val().toLowerCase() === "true";

    return message;
}

function validateForm(formSelector, messageWrapperSelector, message) {
    if (!$(formSelector).valid()) {
        var errorMessage = (message != undefined) ? message : "Por favor, complete todos los campos obligatorios";
        showMessage(messageWrapperSelector, errorMessage, "error");
        return false;
    }

    return true;
}

function initializeTinyMCE(selector) {
    tinymce.init({
        selector: selector,
        theme: "modern",
        language: "es",
        width: '100%',
        height: 300,
        plugins: [
             "advlist autolink image lists textcolor charmap print pagebreak",
             "searchreplace wordcount code fullscreen insertdatetime",
             "table contextmenu directionality paste"
        ],
        style_formats: [
                {
                    title: "Font Family", items: [
                    { title: 'Tahoma', inline: 'span', styles: { 'font-family': 'Tahoma' } },
                    { title: 'Times New Roman', inline: 'span', styles: { 'font-family': 'Times New Roman' } },
                    { title: 'Arial', inline: 'span', styles: { 'font-family': 'Arial' } },
                    { title: 'Arial Black', inline: 'span', styles: { 'font-family': 'Arial Black' } },
                    { title: 'Comic Sans MS', inline: 'span', styles: { 'font-family': 'Comic Sans MS' } },
                    { title: 'Verdana', inline: 'span', styles: { 'font-family': 'Verdana' } },
                    { title: 'Courier New', inline: 'span', styles: { 'font-family': 'Courier New' } }]
                },
                {
                    title: "Font Sizes", items: [
                    { title: '8px', inline: 'span', styles: { 'font-size': '8px' } },
                    { title: '10px', inline: 'span', styles: { 'font-size': '10px' } },
                    { title: '12px', inline: 'span', styles: { 'font-size': '12px' } },
                    { title: '14px', inline: 'span', styles: { 'font-size': '14px' } },
                    { title: '16px', inline: 'span', styles: { 'font-size': '16px' } },
                    { title: '18px', inline: 'span', styles: { 'font-size': '18px' } },
                    { title: '20px', inline: 'span', styles: { 'font-size': '20px' } },
                    { title: '24px', inline: 'span', styles: { 'font-size': '24px' } }                    
                    ]
                }
        ],
        removed_menuitems: 'newdocument',
        content_css: BaseSiteURL + "/content/tinyMCE-custom.css",
        theme_advanced_resizing: false,
        theme_advanced_toolbar_location: "top",
        mode: "none",
        toolbar: "insertfile undo redo | styleselect | bold italic | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | emoticons tinyfilemanager.net"
    });

    $(document).ajaxSuccess(function () {
        enableSubmitButtons();
    });
}

function listDocumentSolicitudes() {
    $('#list-dictums').dataTable().fnClearTable();
    
    if ($("#new-dictum").attr("data-documentId") == "0" && $("#DocumentDTO_DictumDTO_TemplateTypeId").val() != "") {

        var data = {
            improvementPlanId: $("#new-dictum").attr("data-improvementPlanId"),
            templateTypeId: $("#DocumentDTO_DictumDTO_TemplateTypeId :selected").val(),
            fileNumber: $("#DocumentDTO_DictumDTO_FileNumber :selected").val()
        };
        showLoading();
        $.get(BaseSiteURL + "/Plan/ListSolicitudesForSelection", data).done(function (data) {
            $("#solicitude-selector").show();
            $("#solicitude-selector").html(data);
            $(".solicitudes-all-selectors").show();
            $('.solicitude-checkbox').click(updateAmmount);
            updateAmmount();
            bindDictums();
            hideLoading();
        }).fail(function () {
            alert("Ocurrió un error recuperando la información requerida");
            enableSubmitButtons();
            hideLoading();
        });
    }
}

function bindDocuments() {
    // Mantengo vivos los eventos al paginar
    if (typeof editDictum == "function") {
        // Bindeo los eventos de dictamenes (si es que existen)
        $('#list-documents').on('click', '.edit-dictum', editDictum);
        $('#list-documents').on('click', '.view-dictum', editDictum);
        $('#list-documents').on('click', '.delete-dictum', deleteDictum);
        $('#list-documents').on('click', '.block-dictum', blockDictum);
    }

    if (typeof editResolution == "function") {
        // Bindeo los eventos de resoluciones (si es que existen)
        $('#list-documents').on('click', '.view-resolution', editResolution);
        $('#list-documents').on('click', '.edit-resolution', editResolution);
        $('#list-documents').on('click', '.delete-resolution', deleteResolution);
        $('#list-documents').on('click', '.block-resolution', blockResolution);
    }
}

function getDialogFirstButton(dialogSelector) {
    return $(dialogSelector).parent().find(".ui-dialog-buttonset button :first");
}

function canSubmitForm($submitButton, prependLoadingTo) {
    if ($submitButton.hasClass("disabled-button"))
        return false;

    $submitButton.addClass("disabled-button");
    
    var $img = $("<img>").attr("src", BaseSiteURL + "/img/icons/ajax-loader.gif").addClass("loading button-loading");
    if ($submitButton.hasClass("ui-button-text")) {
        // Es un diálogo, así que hago el prepend en base a eso
        $submitButton.parent().parent().prepend($img);
    } else {
        if (appendLoadingTo == undefined) {
            $submitButton.prepend($img);
        } else {
            $(prependLoadingTo).prepend($img);
        }
    }

    return true;
}

function enableSubmitButtons() {
    $(".disabled-button").removeClass("disabled-button");
}

/// <summary>Muestra un mensaje en la pantalla.</summary>
/// <param name="container" type="String">El selector en JQuery que actuará de container para el mensaje. Por ejemplo: "#container"</param>
/// <param name="message" type="String">El mensaje a mostrar</param>
/// <param name="type" type="String">El tipo de mensaje. Puede ser: 'error', 'attention' o 'confirmation'</param>
/// <param name="fadeOut" type="String">Determina si el mensaje desaparecerá luego de unos segundos</param>
function showMessage(container, message, type, fadeOut) {
    $(container).empty();
    $(container).css("display", "block");
    //$(container).append($("<div>").addClass("alert " + type).text(message).hide().fadeIn("slow"));
    $(container).html($("<div>").addClass("alert " + type).text(message).hide().fadeIn("slow"));

    if (fadeOut != undefined && fadeOut) {
        clearTimeout(_messageTimeOut);
        _messageTimeOut = setTimeout(function () { $(container).fadeOut("slow") }, 4000);
    }
}

// Genera un hidden 
function setInput(dataid, pos, key, value, prependTo) {
    $("<input>").attr("type", "hidden").attr("data-pos", pos).attr("data-id", dataid).attr("id", key).attr("name", key).val(value).prependTo(prependTo);
}

// Simplify formatting
String.format = function () {
    var replacements = arguments;
    return arguments[0].replace(/\{(\d+)\}/gm, function (string, match) {
        return replacements[parseInt(match) + 1];
    });
}

//Trim a string and return the value
function trim(str) {
    return str.replace(/^\s*|\s*$/g, "");
}

function isNumber(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}
