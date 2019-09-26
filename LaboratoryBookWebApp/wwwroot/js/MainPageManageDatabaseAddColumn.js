var $addColumnContainerOuter;
var columnNameIsValid = false;
var columnTypeIsValid = true;

function AddColumnButtonLogic() {
    $('#addColumnToDatabaseButton').off('click').click(async () => {
        //show loading animation
        $loadingAnimationContainer.css('visibility', 'visible');
        let getAddColumnHtml = await $.ajax({
            method: "GET",
            cache: false,
            url: "/ManageDatabase/AddColumn/"
        }).fail((xhr, status, error) => {
            errorDialog(error);
            $loadingAnimationContainer.css('visibility', 'hidden');
        });
        if (getAddColumnHtml.indexOf('Exception') > -1) {
            errorDialog(getAddColumnHtml);
            $loadingAnimationContainer.css('visibility', 'hidden');
            return;
        }

        $addColumnContainerOuter = $('#addColumnContainerOuter');
        $addColumnContainerOuter
            .css('visibility', 'visible')
            .html(getAddColumnHtml)
            .dialog({
                width: 420,
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
                buttons: [
                    {
                        text: "Add",
                        click: async function () {
                            if ((!columnNameIsValid) || (!columnTypeIsValid)) {
                                errorDialog("Check column name and type");
                                return;
                            }
                            $loadingAnimationContainer.css('visibility', 'visible');
                            _columnName = $('#addColumnColumnName', $addColumnContainerOuter).val();
                            _columnType = $('#addColumnColumnType', $addColumnContainerOuter).val();
                            _afterColumn = $('#addColumnAfterColumnSelect', $addColumnContainerOuter).val();
                            let addColumnResult = await $.ajax({
                                method: "PUT",
                                cache: false,
                                contentType:"application/json",
                                url: "/api/ManageDatabaseApi/AddColumnToDatabase/",
                                data: JSON.stringify({
                                    columnName: _columnName,
                                    columnType: _columnType,
                                    afterColumn: _afterColumn
                                })
                            }).fail((xhr, status, error) => {
                                errorDialog(error);
                                $loadingAnimationContainer.css('visibility', 'hidden');
                            });

                            if (addColumnResult.item1) {
                                let customColumnOption = '<option value ="' + _columnName +
                                    '" ColumnType = "' + _columnType + '">' + _columnName + '</option>';
                                $manageDatabaseColumnsSelect.append(customColumnOption);
                                DatabaseWasModified = true;
                                $.Toast("Message", `Column "${_columnName}" was successfully added`, "success", {
                                    width: 420,
                                    stack: true
                                });
                            }
                            else {
                                errorDialog(addColumnResult.item2);
                            }
                            $loadingAnimationContainer.css('visibility', 'hidden');
                            $(this).dialog('close');
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

        columnNameIsValid = false;
        columnTypeIsValid = true;

        $('#addColumnColumnName')//column name validation
            .off('keyup')
            .on('keyup', function (e) {
                let columnNamePattern = /^[a-zA-Z]{1,15}$/;
                let testResult = columnNamePattern.test($(this).val());
                if (testResult) {
                    $(this).removeClass('red-border-color');
                    columnNameIsValid = true;
                }
                else {
                    $(this).addClass('red-border-color');
                    columnNameIsValid = false;
                }
            });

        $('#addColumnColumnType')//column type validation
            .off('keyup')
            .on('keyup', function (e) {
                let columnTypePattern = /^(INT\(|VARCHAR\(|FLOAT\(|DOUBLE\()\d{1,2}\)$/i;
                let testResult = columnTypePattern.test($(this).val());
                if (testResult) {
                    $(this).removeClass('red-border-color');
                    columnTypeIsValid = true;
                }
                else {
                    $(this).addClass('red-border-color');
                    columnTypeIsValid = false;
                }
            });

        $loadingAnimationContainer.css('visibility', 'hidden');
    });
}
