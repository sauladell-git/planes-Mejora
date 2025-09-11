$(document).ready(function () {
    Globalize.culture("es-AR");
});

Number.prototype.toCurrency = function () {
    return Globalize.format(this.valueOf(), "n").toString();
}