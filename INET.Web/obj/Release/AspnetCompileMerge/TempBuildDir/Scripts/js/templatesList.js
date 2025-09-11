$(document).ready(function () {
    showServerMessageResult("#msg-list-templates");

    // Selecciono elemento en el menú
    $("ul.tabs a").removeClass("selected");
    $(".templates").addClass("selected");

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
                    templateId: templateId
                }).done(function (data) {
                    if (data.messageError == "") {
                        var table = $('#list').dataTable();
                        var target_row = $("tr[data-templateId='" + templateId + "']").get(0); // this line did the trick
                        var aPos = table.fnGetPosition(target_row);
                        table.fnDeleteRow(aPos);
                        showMessage("#msg-list-templates", "Se eliminó la plantilla con éxito.", "confirmation", true);
                    }
                    else {
                        showMessage("#msg-list-users", data.messageError, "Error", true);
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

    // Mantengo vivo el evento de eliminar al paginar
    $('#list').on('click', '.delete', deleteTemplate);
});

function deleteTemplate() {
    $("#delete-confirm")
          .attr("data-templateId", $(this).parent().parent().attr("data-templateId"))
          .dialog("open");
}