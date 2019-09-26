
function ModifyUserChangeNameButtonEventHandler() {
    $.ajax({
        method: "GET",
        cache: false,
        url: "/ModifyUser/ChangeUserName",
        beforeSend: () => {//show loading animation until response   
            $loadingAnimationContainer.css('visibility', 'visible');
        }
    }).done((changeNameHtml) => {
        var $modifyUserChangeUserName = $('#modifyUserChangeUserName')
            .css('visibility', 'visible')
            .html(changeNameHtml)
            .dialog({
                width: 400,
                modal: true,
                show: {
                    effect: dialogEffect,
                    duration: dialogDuration
                },
                hide: {
                    effect: dialogEffect,
                    duration: dialogDuration
                },
                title: "Change name",
                cache: false,
                resizable: false,
                buttons: [
                    {
                        text: "Change",
                        click:async function () {//check if new user name is valid
                            let $changeUserNameInput = $('#changeUserNameInput', $(this));
                            let userNamePattern = /^[a-zA-Z0-9]{3,20}$/;
                            //test if new user name matches the pattern
                            if (userNamePattern.test($changeUserNameInput.val())) {
                                $changeUserNameInput.removeClass('red-border-color');
                            }
                            else {
                                $changeUserNameInput.addClass('red-border-color');
                                return;
                            }                            
                            var _newName = $changeUserNameInput.val();

                            if ($('#modifyUserUserNameDiv').html() == _newName) {
                                errorDialog("New name should differ from the old name");
                                return;
                            }

                            $('#questionDialogMessage').html(`Do you want to change your user name to \"${_newName}\?"`);
                            //ask if user want to change the name
                            let $deleteQuestionDialog = $('#questionDialog')
                                .css('visibility', 'visible')
                                .dialog({
                                    title: 'User name changing',
                                    width: "500px",
                                    modal: true,
                                    cache: false,
                                    resizable: false,
                                    buttons: [
                                        {
                                            text: "Yes", //user want to change name
                                            click: async function () {//ajax request to change the name
                                                let changeUserNameResponse = await $.ajax({
                                                    method: "POST",
                                                    cache: false,
                                                    contentType: 'application/json',
                                                    data: JSON.stringify({
                                                        newName:_newName
                                                    }),
                                                    url: "/api/ModifyUserApi/ModifyUserChangeNameAsync",
                                                    beforeSend: () => {//show loadning animation
                                                        $loadingAnimationContainer.css('visibility', 'visible');
                                                    }
                                                }).done((result) => {
                                                    console.log("success");
                                                    $.Toast("Message", "User name was changed successfully", "success", {
                                                        width: 400,
                                                        stack:true
                                                    });
                                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                                    $('#modifyUserUserNameDiv').html(_newName);
                                                    $('#ToolBarUserName').html(_newName);
                                                }).fail((xhr, status, error) => {                                                    
                                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                                    errorDialog(error);
                                                    errorDialog(xhr.responseJSON.message);                                                                                               
                                                });
                                                $(this).dialog('close');
                                            }
                                        },
                                        {
                                            text: "No",//user do not want to change the name
                                            click: function () {
                                                $(this).dialog('close');
                                            }
                                        }
                                    ]
                                });                     
                        }
                    },
                    {
                        text: "Cancel",
                        click: function () {
                            $(this).dialog('close');
                        }
                    },
                ]
            });     
        $loadingAnimationContainer.css('visibility', 'hidden');
    }).fail((xhr, status, error) => {
        errorDialog(error);
        $loadingAnimationContainer.css('visibility', 'hidden');
    })
}