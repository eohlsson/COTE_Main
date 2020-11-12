$(function () {
    $.getJSON("/Index/EfficientPaging", null, function (d) {
        // add the dynamic WebGrid to the body
        $("body").append(d.Data);

        // create the footer
        var footer = createFooter(d.Count);

        $("#DataTable tfoot a").live("click", function (e) {
            e.preventDefault();
            var data = {
                page: $(this).text()
            };

            $.getJSON("/Index/EfficientPaging", data, function (html) {
                // add the data to the table    
                $("#DataTable").remove();
                $("body").append(html.Data);

                // re-add the footer
                $('#DataTable thead').after(footer);
            });
        });
    });
});

function createFooter(d) {
    var rowsPerPage = 5;
    var footer = "<tfoot>";
    for (var i = 1; i < (d + 1); i++) {
        footer = footer + "<a href=#>" + i + "</a>&nbsp;";
    }
    footer = footer + "</tfoot>";
    $("#DataTable thead").after(footer);
    return footer;
}   