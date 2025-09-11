$(document).ready(function () {
    // Valido si se volvio a cargar la página y el server devolvio algún error
    showServerMessageResult("#msg-login");

    $("form").submit(function () {
        if (!$("form").valid())
            showMessage("#msg-login", "Por favor, complete todos los campos obligatorios", "error", true);
    });
});