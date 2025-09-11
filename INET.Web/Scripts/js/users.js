$(document).ready(function () {
    $("ul.tabs a").removeClass("selected");
    $(".newuser").addClass("selected");
    showServerMessageResult("#msg-list-users");

    $("#Role").on("change", applyRoleConstrains);
    $('#provinces').multiselect({
        keepRenderingSort: true,
        submitAllLeft: false
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

                $.post(BaseSiteURL + "/Admin/DeleteUser", {
                    UserId: $(this).attr("data-userId")
                }).done(function (data) {
                    if (data.messageError == "")
                        window.location.href = BaseSiteURL + "/admin/users";
                    else
                        showMessage("#msg-list-users", data.messageError, "Error", true);

                    enableSubmitButtons();
                    $("#delete-confirm").dialog("close");
                });
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#user").tabs();

    $("#user").dialog({
        autoOpen: false,
        resizable: false,
        width: 880,
        modal: true,        
        open: function(){
            clearUsersForm();

            var isNewUser = $(this).attr("data-userId") === "0";
            if (isNewUser) {                
                $("#Password").prop('disabled', false);
                $("#fsAccess").hide();
                $("#IsEnabled").get(0).checked = true;

                // Cambio el titulo del dialogo
                $("#user").dialog("option", "title", "NUEVO USUARIO");
            } else {
                // No permito cambiar el nombre de usuario en caso de que sea el usuario logueado
                if ($("#CurrentUser").val() == $(this).attr("data-userName"))
                    $("#UserName").attr("disabled", "disabled");
                else
                    $("#UserName").removeAttr("disabled");

                $("#Password").prop('disabled', true);
                $("#fsAccess").show();

                // Cargo los valores en el popup
                $("#UserName").val($(this).attr("data-userName"));
                $("#Name").val($(this).attr("data-Name"));
                $("#LastName").val($(this).attr("data-lastName"));                
                $("#Role").val($(this).attr("data-role"));
                
                var fieldsIds = $(this).attr("data-fieldsIds").split(",");
                for (var i = 0; i < fieldsIds.length; i++) {
                    if (fieldsIds[i] != "")
                        $("#chk_field_" + fieldsIds[i]).get(0).checked = true;
                }

                var levelsIds = $(this).attr("data-levelsIds").split(",");
                for (var i = 0; i < levelsIds.length; i++) {
                    if (levelsIds[i] != "")
                        $("#chk_level_" + levelsIds[i]).get(0).checked = true;
                }

                var provincesIds = $(this).attr("data-provincesIds").split(",");
                var provincesIdsFinal = new Array();
                for (var i = 0; i < provincesIds.length; i++) {
                    if (provincesIds[i] != "")
                        provincesIdsFinal.push(provincesIds[i]);
                }
                $("#provinces").val(provincesIds);
                setTimeout(function () {
                    $("#provinces_rightSelected").trigger('click');
                }, 100);


                if ($(this).attr("data-isEnabled").toLowerCase() == "true") {                    
                    $("#IsEnabled").get(0).checked = true;
                } else {                    
                    $("#IsEnabled").get(0).checked = false;
                }
                applyRoleConstrains();
                
                // Cambio el titulo del dialogo
                $("#user").dialog("option", "title", "EDITAR USUARIO");
            }
        },
        buttons: {
            "Guardar": function () {
                if (!canSubmitForm(getDialogFirstButton("#user")))
                    return false;

                if ($("#frmUsers").valid()) {
                    var userId = $(this).attr("data-userId");
                    var isNewUser = userId === "0";
                    var isActive = (isNewUser) ? true : $("#IsEnabled").is(":checked");                    
                    var fieldsIds = new Array();
                    $('#fsFields input:checked').each(function () {
                        fieldsIds.push($(this).attr('name').split("_")[2]);
                    });
                    var levelsIds = new Array();
                    $('#fsLevels input:checked').each(function () {
                        levelsIds.push($(this).attr('name').split("_")[2]);
                    });
                    var provincesIds = new Array();
                    $('#provinces_to option').each(function () {
                        provincesIds.push($(this).val());
                    });

                    $.ajax({
                        type: "POST",
                        url: BaseSiteURL + "/Admin/SaveUser",
                        data: {
                            UserId: userId,
                            UserName: $("#UserName").val(),
                            Name: $("#Name").val(),
                            LastName: $("#LastName").val(),
                            Password: $("#Password").val(),
                            Role: $("#Role :selected").val(),
                            SelectedFieldsIds: fieldsIds,
                            SelectedLevelsIds: levelsIds,
                            SelectedProvincesIds: provincesIds,
                            IsEnabled: isActive
                        },
                        dataType: "json",
                        traditional: true,
                        success: function (data) {
                            if (data.messageError == "")
                                window.location.href = BaseSiteURL + "/admin/users";
                            else
                                showMessage("#msg-list-users", data.messageError, "Error", true);

                            enableSubmitButtons();
                            $("#user").dialog("close");
                        },
                        error: function () {
                            alert("Ocurrió un error, revise el formulario por favor.");
                            enableSubmitButtons();
                        }
                    });
                } else {
                    showMessage("#msg-user", "Por favor, complete todos los campos obligatorios. El nombre de usuario debe ser un e-mail.", "error");
                    enableSubmitButtons();
                }
            },
            "Cancelar": function () {
                $(this).dialog("close");
            }
        }
    });  

    $("#nuevo-usuario").click(function () {
        $("#user").attr("data-userId", "0").dialog("open");
    });

    $(".edit-user").click(function () {
        var user = $(this).parent().parent();

        $("#user").attr("data-userId", user.attr("data-userid"))
                        .attr("data-userName", user.attr("data-userName"))
                        .attr("data-name", user.attr("data-name"))
                        .attr("data-lastName", user.attr("data-lastName"))
                        .attr("data-role", user.attr("data-role"))
                        .attr("data-provincesIds", user.attr("data-provincesIds"))
                        .attr("data-fieldsIds", user.attr("data-fieldsIds"))
                        .attr("data-levelsIds", user.attr("data-levelsIds"))
                        .attr("data-isEnabled", user.attr("data-isEnabled"))
                        .dialog("open");
    });

    $(".delete").click(function () {
        var userRow = $(this).parent().parent();
        $("#delete-confirm").find("p").text("¿Desea eliminar el usuario " + userRow.attr("data-name") + " " + userRow.attr("data-lastName") + "?");
        $("#delete-confirm").attr("data-userId", userRow.attr("data-userId")).dialog("open");       
    });
});


function applyRoleConstrains() {
    var role = $("#Role").val();
    switch (role) {
        case "Admin":
            $("#provinces_leftAll").trigger('click');
            $("#fsFields input").attr("checked", false);
            $("#fsLevels input").attr("checked", false);
            $('#tabs-extended').hide();
            break;
        case "Administrativo":
            $("#provinces_leftAll").trigger('click');
            $("#fsFields input").attr("checked", false);
            $("#fsLevels input").attr("checked", false);
            $('#tabs-extended').hide();
            break;
        default:
            $("#provinces_leftAll").trigger('click');
            $('#tabs-extended').show();
            break;
    }
}


function clearUsersForm() {
    $("#msg-user").empty();
    $("#frmUsers").validate().resetForm();

    // Limpio el formulario
    $("#UserName").val("");
    $("#Name").val("");
    $("#LastName").val("");
    $("#Password").val("");
    $("#Role").val("");

    $('#fsFields input').each(function () {
        $(this).get(0).checked = false;
    });

    $('#fsLevels input').each(function () {
        $(this).get(0).checked = false;
    });

    $("#provinces_leftAll").trigger('click');
}