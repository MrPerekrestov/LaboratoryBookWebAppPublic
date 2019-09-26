var dialogEffect = 'fade';
var dialogDuration = 300;

$(document).ready(function () {

    $(".login-container").animate(
        {
            top: '50%',
            opacity: '1'
        });

    function bodyKeydownEventHandler(e) {
        if (e.keyCode == 13) {
            $("#submit").click();            
        }
    }

    document
        .querySelector('body')
        .addEventListener('keydown', bodyKeydownEventHandler);

    $("#submit").click(function () {
        var username = $("#UserName").val();
        var password = $("#Password").val();
        if ((password == "") || (username == "")) {
            $(".login-message").html("Login or password are empty");
            $(".login-message").addClass("red");
            return;
        }
        else {
            $(".login-message").html("Trying to authorize...");
            $(".login-message").removeClass("red");
        }
        $.ajax({
            method: "POST",
            url: '/Login/Authentication',
            dataType: 'html',
            data: {
                Password: password,
                UserName: username
            },
            success: function (data, textStatus, jqXHR) {                         
                    $('#myPartialContainer').html(data);
                    $(".login-message").html("Authorized!");
                    $("#submit").prop('disabled', true);
                    $("#Password").prop('disabled', true);
                    $("#UserName").prop('disabled', true);
                    document.querySelector('body').removeEventListener('keydown', bodyKeydownEventHandler);
                    $('#lgn-container').hide("fast");
            },
            error: function (xhr, errorStatus, thrownError) {
                if (xhr.status == 500) {
                    $(".login-message").html(JSON.parse(xhr.responseText).message);
                    $(".login-message").addClass("red");
                }
                else {
                    errorDialog(errorStatus + ": " + thrownError);
                }                
            }
        });       
    });
});


