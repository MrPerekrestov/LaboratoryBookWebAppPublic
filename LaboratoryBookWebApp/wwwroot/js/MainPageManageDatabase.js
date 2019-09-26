var $manageDatabaseContainer;
var $manageDatabaseUsersSelect;
var $manageDatabasePermissionSelect;
var $manageDatabaseColumnsSelect;
var $manageDatabaseColumnName;
var $manageDatabaseColumnType;
var $manageDatabaseLists;
var $manageDatabaseListValues;
var $manageDatabaseListValue;
var DatabaseWasModified = false;


var manageDatabaseLists = {
    material: [],
    regime: [],
    substrate: []
}
function SetManageDatabaseListValues(listName) {
    if ($manageDatabaseListValues != undefined) {
        $manageDatabaseListValues.html('');
        $.each(manageDatabaseLists[listName], (i, listItem) => {
            var listItemOption = new Option(listItem, listItem, false, false);
            $manageDatabaseListValues.append($(listItemOption));
        });
    }
}

function PermissionSelectEventHandler() {
    $manageDatabasePermissionSelect.off('change').change(async function () {
        let changedPermissionId = $('option:selected', $manageDatabasePermissionSelect).val();
        let selectedUserName = $manageDatabaseUsersSelect.val();

        let permissionUpdateResult = await $.ajax({
            method: "PUT",
            cache: false,
            contentType:"application/json",
            url: "/api/ManageDatabaseApi/UpdateUserPermission/",
            data: JSON.stringify({
                permissionId: changedPermissionId,
                userName: selectedUserName
            }),
            beforeSend: () => { //show loading animation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).fail((xhr, status, error) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            errorDialog(error);
        });

        if (permissionUpdateResult.item1) {
            $('option:selected', $manageDatabaseUsersSelect).attr('permissionid', changedPermissionId);
            $loadingAnimationContainer.css('visibility', 'hidden');
            DatabaseWasModified = true;
        }
        else {
            $loadingAnimationContainer.css('visibility', 'hidden');
            errorDialog(permissionUpdateResult.item2);
        }
    });
}
function AttachEventHandlersToRemoveButtons() {

    if (typeof $('#manageDatabaseRomoveUserButton') != 'undefined') {
        //remove user button event handler
        $('#manageDatabaseRomoveUserButton').off('click').click(function () {
            let userName = $manageDatabaseUsersSelect.val();
            $('#questionDialogMessage').html('Do you want to remove user "' + userName + '" from laboratory book?');
            let $deleteQuestionDialog = $('#questionDialog')
                .css('visibility', 'visible')
                .dialog({
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    title: 'User removing',
                    width: "500px",
                    modal: true,
                    cache: false,
                    resizable: false,
                    buttons: [
                        {
                            text: "Yes",
                            click: async function () {//remove user from database
                                //show loading animation
                                $loadingAnimationContainer.css('visibility', 'visible');
                                let _userId = $('option:selected', $manageDatabaseUsersSelect).attr('userid');
                                let _userName = $('option:selected', $manageDatabaseUsersSelect).val();
                                let userRemoveResult = await $.ajax({
                                    method: "DELETE",
                                    cache: false,
                                    url: "/api/ManageDatabaseApi/RemoveUserFromDatabase/" + _userId
                                }).fail((xhr, status, error) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    errorDialog(error);
                                });

                                if (userRemoveResult.item1) {
                                    let $selectedUserInfoOption = $('option:selected', $manageDatabaseUsersSelect);
                                    $selectedUserInfoOption.remove();
                                    let permissionId = $('option:selected', $manageDatabaseUsersSelect).attr('permissionid');
                                    $manageDatabasePermissionSelect.val(permissionId);
                                    DatabaseWasModified = true;
                                    $.Toast("Message", `User "${_userName}" was successfully removed`, "success", {
                                        width: 420,
                                        stack: true
                                    });
                                }
                                else {
                                    errorDialog(userRemoveResult.item2)
                                }
                                $loadingAnimationContainer.css('visibility', 'hidden');
                                $(this).dialog("close");
                            }
                        },
                        {
                            text: "No",
                            click: function () {
                                $(this).dialog("close");
                            }
                        }
                    ]
                });
        });
    }

    if (typeof $('#removeColumnFromDatabaseButton') != 'undefined') {
        //remove column button event handler
        $('#removeColumnFromDatabaseButton').off('click').click(function () {
            let $seletedColumn = $('option:selected', $manageDatabaseColumnsSelect);
            let _columnName = $seletedColumn.val();
            if ((_columnName == null) || (typeof _columnName == "undefined")) {
                errorDialog("Column name is empty string");
                return;
            }
            $('#questionDialogMessage').html('Do you want to remove column "' + _columnName + '" from laboratory book?');
            let $deleteQuestionDialog = $('#questionDialog')
                .css('visibility', 'visible')
                .dialog({
                    title: 'Column removing',
                    width: "500px",
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    modal: true,
                    cache: false,
                    resizable: false,
                    buttons: [
                        {
                            text: "Yes",
                            click: async function () {//remove user from database
                                //show loading animation
                                $loadingAnimationContainer.css('visibility', 'visible');

                                let columnRemoveResult = await $.ajax({
                                    method: "DELETE",
                                    cache: false,                                    
                                    url: "/api/ManageDatabaseApi/RemoveColumnFromDatabase/" + _columnName
                                }).fail((xhr, status, error) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    errorDialog(error);
                                });

                                if (columnRemoveResult.item1) {
                                    $.Toast("Message", `Column "${$seletedColumn.val()}" was successfully removed`, "success", {
                                        width: 420,
                                        stack: true
                                    });
                                    $seletedColumn.remove();
                                    let $manageDatabaseColumnSelectedOption = $('option:selected', $manageDatabaseColumnsSelect);
                                    $manageDatabaseColumnName.val($manageDatabaseColumnSelectedOption.val());
                                    $manageDatabaseColumnType.val($manageDatabaseColumnSelectedOption.attr('columntype'));
                                    
                                    DatabaseWasModified = true;
                                }
                                else {
                                    errorDialog(columnRemoveResult.item2)
                                }
                                $loadingAnimationContainer.css('visibility', 'hidden');
                                $(this).dialog("close");
                            }
                        },
                        {
                            text: "No",
                            click: function () {
                                $(this).dialog("close");
                            }
                        }
                    ]
                });
        });
    }
    if (typeof $('#removeListValueFromDatabaseButton') != 'undefined') {
        $('#removeListValueFromDatabaseButton').off('click').click(function () {
            let _selectedList = $manageDatabaseLists.val();
            let _selectedListValue = $manageDatabaseListValues.val();            
            if ((_selectedListValue == null)
                || (typeof _selectedListValue == "undefined")
                || (_selectedListValue.trim()=="")) {
                errorDialog("Selected list value is empty string");
                return;
            }
            $('#questionDialogMessage').html('Do you want to remove list value "' + _selectedListValue + '" from "' + _selectedList + '"');
            let $deleteQuestionDialog = $('#questionDialog')
                .css('visibility', 'visible')
                .dialog({
                    title: 'List value removing',
                    width: "500px",
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    modal: true,
                    cache: false,
                    resizable: false,
                    buttons: [
                        {
                            text: "Yes",
                            click: async function () {//remove user from database
                                //show loading animation
                                $loadingAnimationContainer.css('visibility', 'visible');

                                let columnRemoveResult = await $.ajax({
                                    method: "DELETE",
                                    cache: false,                                  
                                    url: "/api/ManageDatabaseApi/RemoveListValueFromDatabase/" +
                                        _selectedList + "/" + _selectedListValue + "/"
                                }).fail((xhr, status, error) => {
                                    errorDialog(error);
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    $(this).dialog('close');
                                });

                                if (columnRemoveResult.item1) {
                                    //delete list value from array
                                    let listValueIndex = manageDatabaseLists[_selectedList].indexOf(_selectedListValue);
                                    manageDatabaseLists[_selectedList].splice(listValueIndex, 1);     
                                    //delete list value from html
                                    $('option:selected', $manageDatabaseListValues).remove();
                                    $manageDatabaseListValue.val($manageDatabaseListValues.val());  

                                    $.Toast("Message", `List value "${_selectedListValue}" was successfully removed from list ${_selectedList}s`, "success", {
                                        width: 420,
                                        stack: true
                                    });
                                    DatabaseWasModified = true;
                                }
                                else {
                                    errorDialog(columnRemoveResult.item2)
                                }
                                $loadingAnimationContainer.css('visibility', 'hidden');
                                $(this).dialog("close");
                            }
                        },
                        {
                            text: "No",
                            click: function () {
                                $(this).dialog("close");
                            }
                        }
                    ]
                });

        });
    }

}
function AttachEventHandlersToManageDatabaseInputsAndLists() {
    //user info list change event handler
    if (typeof $manageDatabaseUsersSelect != 'undefined') {
        $manageDatabaseUsersSelect.off('change').change(function () {
            let permissionId = $('option:selected', $(this)).attr('permissionid');
            $manageDatabasePermissionSelect.val(permissionId);
        });
    }

    if (typeof $manageDatabasePermissionSelect != 'undefined') {
        //permission list event handler
        $manageDatabasePermissionSelect.off('change').change(
            PermissionSelectEventHandler());
    }
    if (typeof $manageDatabaseColumnsSelect != 'undefined') {
        //column list event handler
        $manageDatabaseColumnsSelect.off('change').change(function () {
            let $manageDatabaseColumnSelectedOption = $('option:selected', $(this));
            $manageDatabaseColumnName.val($manageDatabaseColumnSelectedOption.val());
            $manageDatabaseColumnType.val($manageDatabaseColumnSelectedOption.attr('columntype'));
        });
        // $manageDatabaseColumnName
    }

    if (typeof $manageDatabaseLists != 'undefined') {
        //lists select change event handler
        $manageDatabaseLists.off('change').change(function () {
            let listValue = $(this).val();
            SetManageDatabaseListValues(listValue);
        });
    }

    //manage database column name event handle on keyup
    if (typeof $manageDatabaseColumnName != 'undefined') {
        $manageDatabaseColumnName
            .off('keyup')
            .on('keyup', async function (e) {
                if ($(this).val() == $manageDatabaseColumnsSelect.val()) {
                    $(this).removeClass('red-border-color');
                    return;
                }
                if (e.keyCode == 13) {
                    let regexp = /^[a-zA-Z]{1,15}$/;
                    let regexpTestResult = regexp.test($(this).val());

                    if (regexpTestResult) {
                        let _oldColumnName = $manageDatabaseColumnsSelect.val();
                        let _newColumnName = $(this).val();
                        let _columnType = $manageDatabaseColumnType.val();
                        let columnUpdateResult = await $.ajax({
                            method: "PUT",
                            cache: false,
                            contentType:"application/json",
                            url: "/api/ManageDatabaseApi/UpdateColumn/",
                            data: JSON.stringify({
                                oldColumnName: _oldColumnName,
                                newColumnName: _newColumnName,
                                columnType: _columnType
                            }),
                            beforeSend: () => { //show loading animation until response   
                                $loadingAnimationContainer.css('visibility', 'visible');
                            }
                        }).fail((xhr, status, error) => {
                            errorDialog(error);
                            $loadingAnimationContainer.css('visibility', 'hidden');
                        });

                        let $selectedColumnInfo = $('option:selected', $manageDatabaseColumnsSelect);

                        if (columnUpdateResult.item1) {
                            $(this).removeClass('red-border-color');
                            $selectedColumnInfo.val($(this).val());
                            $selectedColumnInfo.html($(this).val());
                            $loadingAnimationContainer.css('visibility', 'hidden');
                            DatabaseWasModified = true;
                        }
                        else {
                            $(this).val($selectedColumnInfo.val());
                            errorDialog(columnUpdateResult.item2);
                            $loadingAnimationContainer.css('visibility', 'hidden');
                        }
                    }
                    else {
                        $(this).addClass('red-border-color');
                    }
                }
            });
    }

    //manage database column type event handle on keyup
    if (typeof $manageDatabaseColumnType != 'undefined') {
        $manageDatabaseColumnType.on('keyup', async function (e) {
            if (e.keyCode == 13) {
                let $selectedColumnInfo = $('option:selected', $manageDatabaseColumnsSelect);
                if ($(this).val() == $selectedColumnInfo.attr('columntype')) {
                    $(this).removeClass('red-border-color');
                    return;
                }
                let columnTypePattern = /^(INT\(|VARCHAR\(|FLOAT\(|DOUBLE\()\d{1,2}\)$/i;
                let regexpTestResult = columnTypePattern.test($(this).val());

                if (regexpTestResult) {
                    let _oldColumnName = $manageDatabaseColumnsSelect.val();
                    let _newColumnName = $manageDatabaseColumnsSelect.val();
                    let _columnType = $(this).val();
                    let columnUpdateResult = await $.ajax({
                        method: "GET",
                        cache: false,
                        url: "/api/ManageDatabaseApi/UpdateColumn/",
                        data: {
                            oldColumnName: _oldColumnName,
                            newColumnName: _newColumnName,
                            columnType: _columnType
                        },
                        beforeSend: () => { //show loading animation until response   
                            $loadingAnimationContainer.css('visibility', 'visible');
                        }
                    }).fail((xhr, status, error) => {
                        errorDialog(error);
                        $loadingAnimationContainer.css('visibility', 'hidden');
                    });

                    if (columnUpdateResult.item1) {
                        $(this).removeClass('red-border-color');
                        $selectedColumnInfo.attr('columntype', $(this).val());
                        $loadingAnimationContainer.css('visibility', 'hidden');
                        DatabaseWasModified = true;
                    }
                    else {
                        $(this).val($selectedColumnInfo.attr('columntype'));
                        errorDialog(columnUpdateResult.item2);
                        $loadingAnimationContainer.css('visibility', 'hidden');
                    }
                }
                else {
                    $(this).addClass('red-border-color');
                }
            }
        });
    }

    //manage database lists event handler on change
    if (typeof $manageDatabaseLists != 'undefined') {
        $manageDatabaseLists.off('change').change(function () {
            let listName = $('option:selected', $(this)).val();
            SetManageDatabaseListValues(listName);
            $manageDatabaseListValue.val($manageDatabaseListValues.val());
        });
    }

    //manage database list values event handler on change
    if (typeof $manageDatabaseListValues != 'undefined') {
        $manageDatabaseListValues.off('change').change(function () {
            $manageDatabaseListValue.val($(this).val());
        });
    }

    //manage database list value event handler on change
    if (typeof $manageDatabaseListValue != 'undefined') {
        $manageDatabaseListValue.off('keyup').on('keyup', async function (e) {
            if (e.keyCode == 13) {
                if ($(this).val() == $manageDatabaseListValues.val()) {
                    $(this).removeClass('red-border-color');
                    return;
                }

                let listValuePattern = /^[a-zA-Z0-9\(\)\[\]]{1,32}$/;
                if (!listValuePattern.test($(this).val())) {
                    $(this).addClass('red-border-color');
                    return;
                }

                let _listName = $manageDatabaseLists.val();
                let _oldListValue = $manageDatabaseListValues.val();
                let _newListValue = $(this).val();

                var listValueUpdateResult = await $.ajax({
                    method: "PUT",
                    cache: false,
                    contentType:"application/json",
                    url: "/api/ManageDatabaseApi/UpdateListValue/",
                    data: JSON.stringify({
                        listName: _listName,
                        oldListValue: _oldListValue,
                        newListValue: _newListValue
                    }),
                    beforeSend: () => { //show loading animation until response   
                        $loadingAnimationContainer.css('visibility', 'visible');
                    }
                }).fail((xhr, state, error) => {
                    $loadingAnimationContainer.css('visibility', 'hidden');
                    errorDialog(error);
                });

                if (listValueUpdateResult.item1) {
                    let oldValue = $manageDatabaseListValues.val();
                    let selectedList = $manageDatabaseLists.val();
                    let indexOfOldValue = manageDatabaseLists[_listName].indexOf(_oldListValue);
                    manageDatabaseLists[_listName][indexOfOldValue] = $(this).val();
                    $('option:selected', $manageDatabaseListValues)
                        .val($(this).val())
                        .html($(this).val());
                    $(this).removeClass('red-border-color');
                    $loadingAnimationContainer.css('visibility', 'hidden');
                    DatabaseWasModified = true;
                }
                else {
                    errorDialog(listValueUpdateResult.item2);
                    $loadingAnimationContainer.css('visibility', 'hidden');
                }
            }
        });
    }
}
//get database pre-defined lists (materials,substrates,regimes)
function manageDatabaseGetList(ListName) {
    return $.ajax({
        cache: false,
        method: "GET",
        url: '/LaboratoryBook/Getlist',
        dataType: 'text',
        data: {
            listName: ListName
        }
    }).done((data) => {
        manageDatabaseLists[ListName.substring(0, ListName.length - 1)] = data.split(";");
    }).fail((xhr, ajaxOptions, thrownError) => {
        $loadingAnimationContainer.css('visibility', 'hidden');
        errorDialog(thrownError);
    })
}
$(document).ready(() => {
    $('#manageDatabase').click(() => {
        $.ajax({
            url: "/LaboratoryBook/ManageDatabase",
            cache: false,
            method: "GET",
            beforeSend: () => { //show loading animation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).done((data) => {
            $manageDatabaseContainer = $('#manageDatabaseContainer')
                .css('visibility', 'hidden')
                .html(data)
                .hide();

            if ($manageDatabaseContainer.hasClass('ui-dialog-content')) {
                $manageDatabaseContainer.dialog('destroy');
            }
            //get users info
            var getUserPermission;
            var getUserInfo = $.ajax({
                method: "GET",
                cache: false,
                url: "/api/ManageDatabaseApi/GetUsersInfo"
            }).done(async (result) => {
                if (result.item1) {

                    //append users info values to select
                    $manageDatabaseUsersSelect = $('#manageDatabaseUsersSelect').html("");
                    $.each(result.item2, (i, userInfo) => {
                        let userInfoOption = '<option userId ="' + userInfo.userId +
                            '" name = "' + userInfo.name + '" permissionId = "' + userInfo.permissionId +
                            '">' + userInfo.name + '</option>';
                        $manageDatabaseUsersSelect.append(userInfoOption);
                    });

                    //append permissions id values to select
                    getUserPermission = $.ajax({
                        method: "GET",
                        cache: false,
                        url: "/api/ManageDatabaseApi/GetPermissions"
                    }).done((permissionList) => {
                        if (permissionList.item1) {
                            $manageDatabasePermissionSelect = $("#manageDatabasePermissionSelect");
                            let currentPermissionId = $('option:selected', $manageDatabaseUsersSelect).attr('permissionId');
                            $.each(permissionList.item2, (i, permissionId) => {
                                let isCurrentPermissionId = false;
                                if (permissionId == currentPermissionId) {
                                    isCurrentPermissionId = true;
                                }
                                let permissionOption = new Option(permissionId, permissionId, false, isCurrentPermissionId);
                                $manageDatabasePermissionSelect.append($(permissionOption));
                            });
                            //attach event handler beacause of late initialization
                            $manageDatabasePermissionSelect.off('change').change(PermissionSelectEventHandler());
                        }
                        else {

                            errorDialog(permissionList.item2);
                        }
                    }).fail((xhr, status, error) => {
                        $loadingAnimationContainer.css('visibility', 'hidden');
                        errorDialog(error);
                    });
                }
                else {
                    errorDialog(result.item2)
                }

            }).fail((xhr, status, error) => {
                $loadingAnimationContainer.css('visibility', 'hidden');
                errorDialog(error + status);
            });
            //finish get user info

            //append editable columns to select
            var getColumnsInfo = $.ajax({
                method: "GET",
                cache: false,
                url: "/api/ManageDatabaseApi/GetColumns"
            }).done((columns) => {
                if (columns.item1) {
                    //append columnsInfo to select
                    $manageDatabaseColumnsSelect = $('#manageDatabaseColumnsSelect').html('');
                    $.each(columns.item2, (i, customColumn) => {
                        let customColumnOption = '<option value ="' + customColumn.columnName +
                            '" ColumnType = "' + customColumn.columnType + '">' + customColumn.columnName + '</option>';
                        $manageDatabaseColumnsSelect.append(customColumnOption);
                    });
                    //set selected column name
                    $manageDatabaseColumnName = $('#manageDatabaseColumnName');
                    let selectedColumnName = $('option:selected', $manageDatabaseColumnsSelect).html();
                    $manageDatabaseColumnName.val(selectedColumnName);
                    //set selected column type
                    $manageDatabaseColumnType = $('#manageDatabaseColumnType');
                    let selectedColumnType = $('option:selected', $manageDatabaseColumnsSelect).attr('columnType');
                    $manageDatabaseColumnType.val(selectedColumnType);
                }
                else {
                    errorDialog(columns.item2);
                }
            }).fail((xhr, status, error) => {
                $loadingAnimationContainer.css('visibility', 'hidden');
                errorDialog(error);
            });

            let getMaterials = manageDatabaseGetList('materials');
            let getRegimes = manageDatabaseGetList('regimes');
            let getSubstrates = manageDatabaseGetList('substrates');

            Promise.all([getUserInfo, getColumnsInfo, getMaterials, getRegimes, getSubstrates, getUserPermission]).then(() => {
                //finish loading animation
                $loadingAnimationContainer.css('visibility', 'hidden');
                $manageDatabaseLists = $('#manageDatabaseLists');
                let selectedList = $('option:selected', $manageDatabaseLists).val();

                //set current selected list values
                $manageDatabaseListValues = $('#manageDatabaseListValues');
                SetManageDatabaseListValues(selectedList);

                //set current selected list values input value
                $manageDatabaseListValue = $('#manageDatabaseListValue');
                let selectedListSelectedValue = $('option:selected', $manageDatabaseListValues).val();
                $manageDatabaseListValue.val(selectedListSelectedValue);

                AttachEventHandlersToManageDatabaseInputsAndLists();
                AttachEventHandlersToRemoveButtons();

                //add buttons logic
                AddListValueButtonLogic();
                AddColumnButtonLogic();
                AddUserButtonLogic();

                DatabaseWasModified = false;

                //create dialog with close button
                $manageDatabaseContainer
                    .css('visibility', 'visible')
                    .dialog({
                        width: 560,
                        modal: true,

                        resizable: false,
                        cache: false,
                        show: {
                            effect: dialogEffect,
                            duration: dialogDuration
                        },
                        hide: {
                            effect: dialogEffect,
                            duration: dialogDuration
                        },
                        buttons: [{
                            text: "Close",
                            click: function () {
                                if (DatabaseWasModified) {
                                    $('#refreshTableButton').click();
                                }
                                $(this).html("");
                                $(this).dialog("close");
                            }
                        }]
                    });
            });
        }).fail((xhr, satus, error) => {
            //finish loading animation
            $loadingAnimationContainer.css('visibility', 'hidden');
            if (xhr.status == 403) {
                errorDialog("You do not have a permission to manage laboratory book.");
            }
            else {
                errorDialog(error);
            }
        });
    });
});