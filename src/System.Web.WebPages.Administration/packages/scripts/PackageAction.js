$(document).ready(function () {
    $('#submitForm input[type="reset"]').click(function (event) {
        location.href = $(this).attr('data-returnurl');
    });
});
