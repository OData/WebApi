test('JQueryClientCanGetServiceDocument', function () {
    {
        $.getJSON(".", function (data) {
            {
                console.log(JSON.stringify(data));
                ok(data.value.length == 1, 'One entityset should return');
            }
        });
    }
});

test('JQueryClientCanGetDataFromFeed', function () {
    $.getJSON('ODataClientTests_Products', function (data) {
        ok(data.value.length == 0, 'By default no data returned');
    });
});