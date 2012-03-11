$(function () {
    $('#source, #view').change(function () {
        $(this).closest('form').submit();
    });

    $('#searchReset').click(function (event) {
        $('#search').val('');
        $(this).closest('form').submit();
    });

    $('#package-list form').submit(function (event) {
        event.preventDefault();
        var form = $(event.target);
        
        var getParams = {
            source: $('#source').val(),
            search: $('#search').val(),
            package: form.find('input[name="package"]').val(),
            version: form.find('input[name="version"]').val(),
            page: form.find('input[name="page"]').val(),
            packageName: form.find('input[name="packageName"]').val()
        };
        location.href = form.attr('action') + '?' + $.param(getParams);
    });

    $('#package-list h4').click(function (event) {
        var form = $(event.target).closest('li').find('form').submit();
    });
});

