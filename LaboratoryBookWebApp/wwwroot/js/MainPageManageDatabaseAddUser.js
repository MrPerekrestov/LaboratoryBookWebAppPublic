var $addUserContainerOuter;

function AddUserButtonLogic() {
    $('#addUserToDatabaseButton').off('click').click(async () => {
        var addUserPartialHtml = await $.ajax({
            method: "GET",
            cache: false,
            url: "/ManageDatabase/AddUser",
            beforeSend: function () {//show loading anumation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).fail((xhr, status, error) => {
            errorDialog(error);
        });
        //finish loading anumation
        $loadingAnimationContainer.css('visibility', 'hidden');
        //the exception was thrown
        if (addUserPartialHtml.indexOf('Exception') > -1) {
            errorDialog(addUserPartialHtml);
            return;
        }
        //exception was not thrown
        $addUserContainerOuter = $('#addUserContainerOuter').html(addUserPartialHtml);
        $addUserContainerOuter.css('visibility', 'visible');
        $addUserContainerOuter.dialog({
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
                    text: "Add",
                    click: async function () {
                        //start loading animation
                        $loadingAnimationContainer.css('visibility', 'visible');

                        let _userId = $('#addUserUserSelect option:selected').attr('userid');
                        let _userName = $('#addUserUserSelect').val();
                        let _permissionId = $('#addUserPermissionSelect').val();

                        var addUserResult = await $.ajax({
                            method: "PUT",
                            cache: false,
                            contentType: 'application/json',
                            dataType:"json",
                            url: "/api/ManageDatabaseApi/AddUserToDatabase/",
                            data: JSON.stringify({
                                permissionId: _permissionId,
                                userId: _userId
                            })
                        }).fail((xhr, satus, error) => {
                            $loadingAnimationContainer.css('visibility', 'hidden');
                            errorDialog(error);
                        });

                        if (addUserResult.item1) {
                            let userInfoOption = '<option userId ="' + _userId +
                                '" name = "' + _userName + '" permissionId = "' + _permissionId +
                                '">' + _userName + '</option>';
                            $manageDatabaseUsersSelect.append(userInfoOption);
                            $.Toast("Message", `User "${_userName}" was successfully added`, "success", {
                                width: 420,
                                stack: true
                            });
                            DatabaseWasModified = true;
                        }
                        else {
                            errorDialog(addUserResult.item2)
                        }
                        $loadingAnimationContainer.css('visibility', 'hidden');
                        $(this).dialog("close");
                    }
                },
                {
                    text: "Close",
                    click: function () {

                        $(this).dialog("close");
                    }
                }
            ]
        });
    });
    // $('#addUserContainerOuter')
}

