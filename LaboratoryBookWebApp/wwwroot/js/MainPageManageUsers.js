var $manageUsersAvailableUsers;
var $manageUsersAvailableStatus;
//user status changed
function ManageUsersStatusChangedEventHandler(e) {
    let _userId = $('option:selected', $manageUsersAvailableUsers).attr('userId');
    let _userName = $('option:selected', $manageUsersAvailableUsers).attr('userName');;
    let _userStatusId = $('option:selected', $manageUsersAvailableStatus).attr('accessId');
    $.ajax({
        method: "PUT",
        cache: false,
        contentType: "application/json",
        url:"/api/ManageUsersApi/UpdateUserStatus/",
        data: JSON.stringify({            
            UserName: _userName,
            UserStatusId: _userStatusId,
            UserId: _userId
        }),
        beforeSend: () => { //show loading animation until response   
            $loadingAnimationContainer.css('visibility', 'visible');
        }
    }).done((result) => {
        $loadingAnimationContainer.css('visibility', 'hidden')
        $('option:selected', $manageUsersAvailableUsers).attr('userStatusId', _userStatusId);
        let statusName = $('option:selected', $manageUsersAvailableStatus).val();
        $.Toast("Message", `${_userName}'s status was changed to ${statusName}`, "success", {
            width: 420,
            stack: true
        });
    }).fail((xhr, status, error) => {
        $loadingAnimationContainer.css('visibility', 'hidden')
        let errorString = `${xhr.status}: ${xhr.responseJSON.message}`;
        errorDialog(errorString);
        ManageUsersChangeSeletedUser();
    });    
}
//selected user changed
function ManageUsersChangeSeletedUser(e) {
    let seletedUserStatusId = $('option:selected', $manageUsersAvailableUsers).attr('userStatusId');    
    $(`option[accessId = "${seletedUserStatusId}"]`, manageUsersAvailableStatus).prop('selected', true);
}
// create user logic
function CreateUser() {
    return function () {
        console.log($(this));
        let _userName = $('#CreateUserUserName', $(this)).val();
        let _password = $('#CreateUserUserPassword', $(this)).val();
        let _statusId = $('#CeateUserUserStatus option:selected', $(this)).attr('statusId');
        console.log(`${_userName} ${_password} ${_statusId}`);
        let userNamePattern = /^[a-zA-Z0-9]{3,20}$/;
        if (!userNamePattern.test(_userName)) {
            errorDialog("User name should contain from 3 to 20 alphanumeric characters.");
            $('#CreateUserUserName', $(this)).addClass('red-border-color');
            return;
        }
        $('#CreateUserUserName', $(this)).removeClass('red-border-color');
        if (_password.length < 7) {
            errorDialog("Password should be longer than 7 characters");
            $('#CreateUserUserPassword', $(this)).addClass('red-border-color');
            return;
        }
        $('#CreateUserUserPassword', $(this)).removeClass('red-border-color');
        $.ajax({
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                UserName: _userName,
                Password: _password,
                Status: _statusId
            }),
            url: "/api/ManageUsersApi/CreateUser/",
            beforeSend: () => {
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).done((userId) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            let newUserOption = document.createElement("option");
            newUserOption.setAttribute('userId', userId);
            newUserOption.setAttribute('userName', _userName);
            newUserOption.setAttribute('userStatusId', _statusId);
            let newOptionText = document.createTextNode(_userName);
            newUserOption.append(newOptionText);
            let availableUsersSelect = document.getElementById('manageUsersAvailableUsers');
            availableUsersSelect.append(newUserOption);
            $.Toast("Message", `User "${_userName}" was successfully created`, "success", {
                width: 420,
                stack: true
            });
        }).fail((xhr, status, error) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            errorDialog(xhr.responseJSON.message);
        });
    };
}

$(document).ready(() => {
    $('#manageUsersButton').click(() => {
        $.ajax({
            method: "GET",
            cache: false,
            url: "/LaboratoryBook/ManageUsers/",
            beforeSend: () => {
                //show loading animation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).done((manageUsersPartialHtml) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            $('#manageUsersContainer')
                .html(manageUsersPartialHtml)
                .show()
                .dialog({
                    width: 400,
                    modal: true,
                    cache: false,
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    resizable: false,
                    buttons: [
                        {
                            text: "Remove",
                            click: function () {
                                let _userId = $('option:selected', $manageUsersAvailableUsers).attr('userId');
                                let _userName = $('option:selected', $manageUsersAvailableUsers).attr('userName');
                                let _userStatusId = $('option:selected', $manageUsersAvailableUsers).attr('userStatusId');
                                $('#questionDialogMessage').html('Do you want to delete user <b>' + _userName + '</b>?');
                                let $deleteQuestionDialog = $('#questionDialog')
                                    .css('visibility', 'visible')
                                    .dialog({
                                        title: 'User deleting',
                                        width: "500px",
                                        modal: true,
                                        cache: false,
                                        resizable: false,
                                        buttons: [
                                            {
                                                text: "Yes",
                                                click: function () {
                                                    let userDataJson = JSON.stringify({
                                                        UserId: _userId,
                                                        UserName: _userName,
                                                        UserStatusId: _userStatusId
                                                    });
                                                    let deleteUserRequest = new XMLHttpRequest();                                                   
                                                    //create post request using vanilla JS method
                                                    deleteUserRequest.open("POST", "/api/ManageUsersApi/DeleteUser/", true);
                                                    deleteUserRequest.setRequestHeader('Content-Type', 'application/json');
                                                    deleteUserRequest.setRequestHeader("X-Requested-With", "XMLHttpRequest");
                                                    deleteUserRequest.onloadstart = function ()  {
                                                        //show loading animation until response   
                                                        $loadingAnimationContainer.css('visibility', 'visible');
                                                    }

                                                    deleteUserRequest.onerror = function() {
                                                        //stop loading animation
                                                        $loadingAnimationContainer.css('visibility', 'hidden');
                                                        errorDialog("Delete user request syntax error");
                                                    }

                                                    deleteUserRequest.onload = function () {
                                                        //stop loading animation
                                                        $loadingAnimationContainer.css('visibility', 'hidden');
                                                        if (deleteUserRequest.status == 200) {
                                                            $(`option[userId = "${_userId}"]`, $manageUsersAvailableUsers).remove();
                                                            let responseJSON = JSON.parse(deleteUserRequest.response);
                                                            console.log(responseJSON.message);
                                                            $.Toast("Message", `${_userName} was successfully deleted`, "success", {
                                                                width: 420,
                                                                stack: true
                                                            });
                                                        }
                                                        else {
                                                            let errorMessage = deleteUserRequest.status + ": " + deleteUserRequest.response;
                                                            errorDialog(errorMessage);
                                                        }                                                   
                                                    }

                                                    deleteUserRequest.send(userDataJson);
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
                        },
                        {
                            text: "Create",
                            click: function () {
                                $.ajax({
                                    method: "GET",
                                    cache: false,
                                    url: "/ManageUsers/CreateUser/",
                                    beforeSend: () => {//show loading animation until response   
                                        $loadingAnimationContainer.css('visibility', 'visible');
                                    }
                                }).done((CreateUserHtml) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    let $createUserContainer = $('#createUserContainer')
                                    $createUserContainer
                                        .html(CreateUserHtml)
                                        .dialog({
                                            modal: true,
                                            title: "Create user",
                                            resizable: false,
                                            show: {
                                                effect: dialogEffect,
                                                duration: dialogDuration
                                            },
                                            hide: {
                                                effect: dialogEffect,
                                                duration: dialogDuration
                                            },
                                            width: 500,
                                            buttons: [
                                                {
                                                    text: "Create",
                                                    click: CreateUser()
                                                },
                                                {
                                                    text: "Cancel",
                                                    click: function () {
                                                        $(this).dialog('close');
                                                    }
                                                }

                                            ]
                                        });
                                }).fail((xhr, status, error) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    errorDialog(error);
                                });
                            }
                        },
                        {
                            text: "Cancel",
                            click: function () {
                                $(this).dialog('close');
                            }
                        }
                    ]
                });
            $manageUsersAvailableUsers = $('#manageUsersAvailableUsers');
            $manageUsersAvailableStatus = $('#manageUsersAvailableStatus');
            let currentUserStatusId = $('option:selected', $manageUsersAvailableUsers).attr('userStatusId');
            $(`option[accessId = "${currentUserStatusId}"]`, manageUsersAvailableStatus).prop('selected', true);

            //attach event handlers
            $manageUsersAvailableStatus.change(ManageUsersStatusChangedEventHandler);
            $manageUsersAvailableUsers.change(ManageUsersChangeSeletedUser);

        }).fail((xhr, status, error) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            try {
                errorDialog(xhr.responseJSON.message);
            }
            catch (e) {
              
                if (xhr.status == 403) {
                    console.log(xhr.status);
                    errorDialog("You do not have a permission to modify users");
                }
                else {
                    errorDialog(xhr.status + ": " + error);
                }                
            }            
        })
    });
});


