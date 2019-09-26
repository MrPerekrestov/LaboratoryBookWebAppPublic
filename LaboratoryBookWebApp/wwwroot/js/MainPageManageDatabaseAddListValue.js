var selectedList;
var $addListValueContainerOuter;

 function AddListValueButtonLogic() {
    $('#manageDatabaseAddListValueButton')
        .off('click')
        .click(async () => {

            //get html of add list window
            $loadingAnimationContainer.css('visibility', 'visible');
            let addListValueHtml = await $.ajax({
                method: "GET",
                cache: false,
                url: "/ManageDatabase/AddListValue/"
            }).fail((xhr, status, error) => {
                errorDialog(error);
                $loadingAnimationContainer.css('visibility', 'hidden');
            });

            //check if format is html
            if (addListValueHtml.indexOf('html') > -1) {
                $loadingAnimationContainer.css('visibility', 'hidden');
                errorDialog("Cannot get page 'Add list value'");
                return;
            }

            //create add list dialog window
            $addListValueContainerOuter = $('#addListValueContainerOuter');
            $addListValueContainerOuter
                .css('visibility', 'visible')
                .html(addListValueHtml)
                .dialog({
                    cache: false,
                    title: 'Add list value (' + $('option:selected', $manageDatabaseLists).val() + ')',
                    modal: true,
                    show: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    hide: {
                        effect: dialogEffect,
                        duration: dialogDuration
                    },
                    resizable: false,
                    width: 400,
                    buttons: [
                        {
                            text: "Add",
                            click: async function () {//add list value logic
                                let _listName = $manageDatabaseLists.val();
                                let _listValue = $("#addListValueInput").val();
                                let listValuePattern = /^[a-zA-Z0-9\(\)\[\]]{1,32}$/;
                                if (!listValuePattern.test(_listValue)) {
                                    errorDialog("List name should include only alphanumeric symbols and ()[]. The maximum length is 32 symbols");
                                    return;
                                }
                                $.ajax({
                                    method: "PUT",
                                    cache: false,
                                    contentType: "application/json",
                                    data: JSON.stringify({
                                        listName: _listName,
                                        listValue: _listValue
                                    }),
                                    url: "/api/ManageDatabaseApi/AddListValueToDatabase/",
                                    beforeSend: () => {
                                        $loadingAnimationContainer.css('visibility', 'visible');
                                    }
                                }).done((addListValueResponse) => {
                                    if (addListValueResponse.item1) {
                                        manageDatabaseLists[_listName].push(_listValue);
                                        SetManageDatabaseListValues(_listName);
                                        $.Toast("Message", `List value "${_listValue}" was successfully added to list ${_listName}s`, "success", {
                                            width: 420,
                                            stack: true
                                        });
                                        DatabaseWasModified = true;
                                    }
                                    else {
                                        errorDialog(addListValueResponse.item2);
                                    }
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    $(this).dialog('close');
                                }).fail((xhr, status, error) => {
                                    errorDialog(error);
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    $(this).dialog('close');
                                });

                            }
                        },
                        {
                            text: "Close",
                            click: function () {
                                $(this).dialog('close');
                            }
                        }
                    ]
                });
            //add listvalue format check
                 
            $("#addListValueInput")
                .off('keyup')  
                .on('keyup', function () {                   
                    let listValuePattern = /^[a-zA-Z0-9\(\)\[\]]{1,32}$/;
                    if (listValuePattern.test($(this).val())) {
                        $(this).removeClass('red-border-color');
                    }
                    else {
                        $(this).addClass('red-border-color');
                    }
            });           
            $loadingAnimationContainer.css('visibility', 'hidden');
        });
}
