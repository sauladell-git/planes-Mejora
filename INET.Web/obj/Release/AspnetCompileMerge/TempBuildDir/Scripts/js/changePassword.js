$(document).ready(function () {
    showServerMessageResult("#msg-password");

    $("#frmChangePassword").submit(function () {
        if (!$("#frmChangePassword").valid()) {
            var errorMessage = "Por favor, complete todos los campos obligatorios y verifique que su contraseña tenga al menos 6 caracteres";
            var errorList = $("#frmChangePassword").validate().errorList;

            for (var i = 0; i < errorList.length; i++) {
                if (errorList[i].message == "The new password and confirmation password do not match.")
                    errorMessage = "Los campos nueva contraseña y repetir nueva contraseña conincien";
            }

            showMessage("#msg-password", errorMessage, "error");
        }
    });
});