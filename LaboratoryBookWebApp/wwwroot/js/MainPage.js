var $loadingAnimationContainer;
var dialogEffect = 'fade';
var dialogDuration = 300;

var LaboratoryBookName = window.location.pathname.split("/").pop();
function errorDialog(message) {
    $('#ErrorMessage').html(message);
    $('#ErrorDialog').css("visibility", "visible");
    $('#ErrorDialog').dialog({
        width: "500px",
        modal: true,
        cache: false,
        resizable: false,
        buttons: [
            {
                text: "OK",
                click: function () {

                    $(this).dialog("close");
                }
            }
        ]
    });
}

document.addEventListener('DOMContentLoaded', function () {
    window.addEventListener('load', () => {
        document.getElementsByTagName('body')[0].style.display = 'block';
    });
});

$(document).ready(() => {
    $loadingAnimationContainer = $('#loadingAnimationContainer');
    $(window).on("load",() => {       
        $('body').css('display', 'block');
    })
});