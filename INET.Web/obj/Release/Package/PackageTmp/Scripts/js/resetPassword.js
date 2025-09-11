$(document).ready(function () {
    showServerMessageResult("#msg-reset-password");

    $("form").submit(function () {
        if (!$("form").valid()) {
            showMessage("#msg-reset-password", "Por favor, complete todos los campos obligatorios y verifique que la nueva contraseña tenga al menos 6 caracteres", "error", true);
        }
    });
});