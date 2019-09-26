function ModifyUserChangePasswordButtonEventHandler() {    
    let  $modifyUserOldPasswordInput = $('#modifyUserOldPasswordInput');
    let $modifyUserNewPasswordInput = $('#modifyUserNewPasswordInput');
    let $modifyUserRepeatNewPasswordInput = $('#modifyUserRepeatNewPasswordInput');
    let _oldPassword = $modifyUserOldPasswordInput.val();
    let _newPassword = $modifyUserNewPasswordInput.val();
    let _newPasswordRepeat = $modifyUserRepeatNewPasswordInput.val();    
    $('#questionDialogMessage').html(`Do you want to change your password?"`);
    //ask if user want to change the password
    let $deleteQuestionDialog = $('#questionDialog')
        .css('visibility', 'visible')
        .dialog({
            title: 'Password changing',
            width: "500px",
            modal: true,
            show: {
                effect: dialogEffect,
                duration: dialogDuration
            },
            hide: {
                effect: dialogEffect,
                duration: dialogDuration
            },
            cache: false,
            resizable: false,
            buttons: [
                {
                    text: "Yes", //user want to change password
                    click: async function () {
                        if (_newPassword != _newPasswordRepeat) {//check if password and repeat password match
                            errorDialog("New password and repeat password are not the same");
                            console.log(_newPassword + " " + _newPasswordRepeat);
                            $(this).dialog('close');
                            return;
                        }

                        if (_newPassword.length < 6) {//check minimum password length
                            errorDialog("New password is too short");
                            console.log(_newPassword);
                            $(this).dialog('close');
                            return;
                        }
                        $.ajax({
                            method: "POST",
                            cache: false,
                            contentType: 'application/json',
                            data: JSON.stringify({
                                oldPassword: _oldPassword,
                                newPassword: _newPassword
                            }),
                            beforeSend: () => {//show loading animation
                                $loadingAnimationContainer.css('visibility', 'visible');                            
                            },
                            url:"/api/ModifyUserApi/ChangePassword"
                        }).fail((xhr, status, error) => {
                            errorDialog(xhr.responseJSON.message);//finish loading animation
                            $loadingAnimationContainer.css('visibility', 'hidden');
                        }).done(() => {
                            $.Toast("Message", "Password was changed successfully", "success", {
                                width: 400
                            });//finish loading animation
                            $loadingAnimationContainer.css('visibility', 'hidden');
                        })
                    
                        $(this).dialog('close');
                    }
                },
                {
                    text: "No",
                    click: function () {
                        $(this).dialog('close');
                    }

                }
            ]
        });
}