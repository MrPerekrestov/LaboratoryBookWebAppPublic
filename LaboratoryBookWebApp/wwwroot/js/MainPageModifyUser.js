var $modifyUserOldPassword;
var $modifyUserNewPassword;
var $modifyUserRepeatNewPassword;

$(document).ready(() => {
    $('#manageUserButton').click(() => {       
        $.ajax({
            method: "GET",
            cache: false,
            url: "/LaboratoryBook/ModifyUser/",
            beforeSend: () => {//show loading anumation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).done((manageUserHtml) => {
            $('#mainPageModifyUserContainer')
                .html(manageUserHtml)
                .dialog({
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    width: 550,
                    modal: true,
                    resizable: false,
                    cache: false,
                    buttons: [
                        {
                            text: "Close",
                            click: function () {
                                $(this).dialog('close');
                            }
                        }
                    ]
                });
            $('#modifyUserChangeNameButton')
                .off('click')
                .click(() => ModifyUserChangeNameButtonEventHandler());
            $('#modifyUserChangePasswordButton')
                .off('click')
                .click(() => ModifyUserChangePasswordButtonEventHandler());
            $loadingAnimationContainer.css('visibility', 'hidden');
        }).fail((xhr, status, error) => {
            errorDialog(error);
            $loadingAnimationContainer.css('visibility', 'hidden');
        })
    })
});