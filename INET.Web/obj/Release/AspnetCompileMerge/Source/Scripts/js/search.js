$(document).ready(function () {
    $(".search-opener").click(function () {
        $(".search").animate({
            left: 0
        }, 800, function () {
            // Animation complete.
        });
        $(".table-float").animate({
            left: 460
        }, 800, function () {
            // Animation complete.
        });
        $(".search-opener").fadeOut();
        $(".add-plan").fadeOut();
        $(".export").fadeOut();
    });

    $(".search-close").click(function () {
        $(".search").animate({
            left: -450
        }, 1000, function () {
            // Animation complete.
        });
        $(".table-float").animate({
            left: 50
        }, 1200, function () {
            // Animation complete.            
        });
        $(".search-opener").fadeIn();
        $(".add-plan").fadeIn();
        $(".export").fadeIn();
    });
});