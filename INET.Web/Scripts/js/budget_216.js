$(document).ready(function () {
    // Selecciono elemento en el menú
    $("ul.tabs a").removeClass("selected");
    $(".lective").addClass("selected");

    // Obtengo partida presupuestaria para ese ciclo lectivo
    $.getJSON(BaseSiteURL + '/Admin/ListBudgets216', { schoolYearId: $("#SchoolYearId :selected").val() }, function (data) {
        $('#tree').bind('tree.init', function () {
            //$(".node-textbox").blur(saveBudget);
            $(".node-textbox").focus(function () {
                $(".jqtree-selected").removeClass("jqtree-selected");
                var node = $('#tree').tree('getNodeById', $(this).attr("data-id"));
                $(node.element).addClass("jqtree-selected");
            });
        });

        $('#tree').tree({
            data: data,
            onCreateLi: function (node, $li) {
                // Append a link to the jqtree-element div.
                // The link has an url '#node-[id]' and a data property 'node-id'.  
                $li.find('.jqtree-element').prepend("<div class='node-image'></div>")
                if (node.getLevel() == 3) {
                    var ammount = node.ammount;
                    if (ammount != null)
                        ammount = Number(ammount).toFixed(2);
                    //ammount = ammount.toCurrency();

                    $li.find('.jqtree-element').append($("<input>").attr("type", "text").attr("data-id", node.id).addClass("third node-textbox").val(ammount).blur(saveBudget).mask('##0,00', { reverse: true }));

                    if (ammount != null)
                        $li.find('.node-textbox').addClass("budget-saved");
                }
            },
            onCanSelectNode: function (node) {
                $(".jqtree-selected").removeClass("jqtree-selected");
                $(node.element).addClass("jqtree-selected");
            }
        });
    });

    $("#SchoolYearId").change(function () {
        $("#SchoolYearId").after($("<img>").attr("src", BaseSiteURL + "/img/icons/ajax-loader.gif").addClass("loading"));
        $.getJSON(BaseSiteURL + '/Admin/ListBudgets216', { schoolYearId: $("#SchoolYearId :selected").val() }, function (data) {
            $('#tree').tree('loadData', data);
        });
    });

    // Disparo carga inicial
    $("#SchoolYearId").change();
});

function saveBudget() {
    if ($.trim($(this).val()) != "") {
        //var ammount = $(this).val().replace(",", ".");
        var ammount = Number($(this).val().replace(",", ".")).toFixed(2);
        $(this).val(ammount.replace('.', ','));

        //$(this).val(ammount.toCurrency());

        $(".loading-budget").remove();
        $(this).after($("<img>").attr("src", BaseSiteURL + "/img/icons/ajax-loader.gif").addClass("loading-budget"));
        var $input = $(this);

        $.post(BaseSiteURL + "/Admin/SaveBudget216", {
            budgetId: $(this).attr("data-id"),
            ammount: $(this).val()
        }).done(function (data) {
            $(".loading-budget").remove();
            $input.addClass("budget-saved");
        });
    }
}