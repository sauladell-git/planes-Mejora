$(document).ready(function () {
    showServerMessageResult("#msg-forgot-password");

    $("form").submit(function () {
        if (!$("form").valid()) {
            showMessage("#msg-forgot-password", "Por favor, complete todos los campos obligatorios", "error", true);
        }
    });    
});