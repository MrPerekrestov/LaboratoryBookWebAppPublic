var $manageDatabaseContainer;
var $manageDatabaseUsersSelect;
var $manageDatabasePermissionSelect;
var $manageDatabaseColumnsSelect;
var $manageDatabaseColumnName;
var $manageDatabaseColumnType;
var $manageDatabaseLists;
var $manageDatabaseListValues;
var $manageDatabaseListValue;

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
    $manageDatabasePermissionSelect.off('change').change(function () {
        let changedPermissionId = $('option:selected', $manageDatabasePermissionSelect).val();
        $('option:selected', $manageDatabaseUsersSelect).attr('permissionid', changedPermissionId);
        console.log($manageDatabasePermissionSelect.val());
    });
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
   
    if (typeof $manageDatabaseLists != 'undefined') {
        //lists select change event handler
        $manageDatabaseLists.off('change').change(function () {
            let listValue = $(this).val();
            SetManageDatabaseListValues(listValue);
        });
    }   

}
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
            $manageDatabaseContainer = $('#manageDatabaseContainer').html(data);

            //get users info
            var getUserInfo = $.ajax({
                method: "GET",
                cache: false,
                url: "/LaboratoryBook/GetUsersInfoForManageDatabase/"
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
                    $.ajax({
                        method: "GET",
                        cache: false,
                        url: "/LaboratoryBook/GetPermissionsForManageDatabase/"
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
                        errorDialog(error);
                    });
                }
                else {
                    errorDialog(result.item2)
                }

            }).fail((xhr, status, error) => {
                errorDialog(error);
            });
            //finish get user info

            //append editable columns to select
            var getColumnsInfo = $.ajax({
                method: "GET",
                cache: false,
                url: "/LaboratoryBook/GetColumnsForManageDatabase/"
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
                errorDialog(error);
            });

            let getMaterials = manageDatabaseGetList('materials');
            let getRegimes = manageDatabaseGetList('regimes');
            let getSubstrates = manageDatabaseGetList('substrates');

            Promise.all([getUserInfo, getColumnsInfo, getMaterials, getRegimes, getSubstrates]).then(() => {
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

                //create dialog with close button
                $manageDatabaseContainer = $('#manageDatabaseContainer')
                    .css('visibility', 'visible')
                    .dialog({
                        width: 470,
                        modal: true,
                        resizable: false,
                        buttons: [{
                            text: "Close",
                            click: function () {
                                $(this).dialog("close");
                            }
                        }]
                    });
            });
        }).fail((xhr, satus, error) => {
            //finish loading animation
            $loadingAnimationContainer.css('visibility', 'hidden');
            errorDialog(error);
        })
    });
});