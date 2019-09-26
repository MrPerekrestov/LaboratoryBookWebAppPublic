
var addRowInputBordeColorDefault;
var addRowInputValidation = true;

var selectColumns = ["material",
    "substrate",
    "operator",
    "regime",
    "permissionID"];

function MakeAllColumnsVisible() {
    let columns = $dataTable.columns().context[0].aoColumns;
    columns.forEach((column, number) => {
        $dataTable.column(column.idx).visible(true, false);
    });
}

function SetProperColumnVisibility() {
    let columns = $dataTable.columns().context[0].aoColumns;
    hiddenColumns = localStorage.getItem("hiddencolumns" + LaboratoryBookName) || [];
    columns.forEach((column, number) => {
        if ((hiddenColumns.indexOf(column.sTitle) > -1) && (column.sTitle != "")) {
            $dataTable.column(column.idx).visible(false, false);
        }
        else {
            $dataTable.column(column.idx).visible(true, false);
        }
    });
}

$(document).ready(($) => {
    //add row event handler
    $('#addRowButton').click(() => {
        $.ajax({
            method: "GET",
            url: "/LaboratoryBook/AddRow",
            dataType: "html",
            cache: false,
            beforeSend: () => { //show loading anumation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            } //if finished successfully           
        }).done((data) => {
            var positionLeft;
            var positionTop;

            $loadingAnimationContainer.css('visibility', 'hidden');
            var $addRowContaier = $('#addRowContainer').css('display', 'block');

            $addRowContaier.html(data);
            $addRowContaier.dialog({
                modal: true,
                cache: false,
                resizable: false
            });

            let $addRowContainerInner = $('#addRowContainerInner');
            $addRowContaier.dialog('option', 'width', $addRowContainerInner.width() + 50);
            $addRowContaier.dialog("option", "height", $addRowContainerInner.height() + 150);
            $addRowContaier.dialog('close');

            $addRowContaier.dialog({
                modal: true,
                cache: false,
                resizable: false,
                show: {
                    effect: dialogEffect,
                    duration: dialogDuration
                },
                hide: {
                    effect: dialogEffect,
                    duration: dialogDuration
                },
                close: function (event, ui) {
                    $(this).empty().dialog('destroy');
                },
                open: function () {
                    $(this).parent().promise().done(function () {
                        let $addRowContainerInner = $('#addRowContainerInner');
                        $addRowContaier.dialog('option', 'width', $addRowContainerInner.width() + 50);
                        $addRowContaier.dialog("option", "height", $addRowContainerInner.height() + 150);
                    });
                },
                buttons: [
                    {
                        text: "Add",//add button click event handler
                        click: () => {
                            if (!addRowInputValidation) {
                                errorDialog("Check data format");
                                return;
                            }
                            //show loading animation
                            $loadingAnimationContainer.css('visibility', 'visible');
                            var dataForServer = new Object();
                            let dataForDataTable = [];
                            let tableCaptions = [];

                            //collect data from addrow window
                            $("input, select, textarea", "#addRowContainerInner").each(function () {
                                $item = $(this);
                                dataForDataTable.push($item.val());
                                tableCaptions.push($item.attr('name'));
                                if ($item.val()) {
                                    dataForServer[$item.attr('name')] = $item.val();
                                }
                            });

                            dataForDataTable.push('<button style="color: red" delete-id="' + dataForServer.sampleID + '">x</button>');
                            tableCaptions.push('');

                            let dataForServerString = JSON.stringify(dataForServer);

                            //send row data to server
                            $.ajax({
                                method: 'PUT',
                                cache: false,
                                contentType: "application/json",
                                url: '/LaboratoryBook/AddRowToDatabase',
                                data: JSON.stringify({
                                    rowData: dataForServerString
                                }),
                                dataType: 'json'
                            }).done((result) => {
                                if (result.item1 == true) {//if finished successfully

                                    MakeAllColumnsVisible();

                                    $dataTable.row.add(dataForDataTable).draw(false);

                                    let $addedRow = $dataTable.$('tr:not([row-id])');
                                    let $addedRowTableData = $('td', $addedRow);

                                    if ((dataForServer.sampleID != "undefined") && (dataForServer.sampleID != null)) {
                                        $addedRow.attr('row-id', dataForServer.sampleID);
                                    }
                                    else {
                                        console.log('Sample Id was not found')
                                    }

                                    $addedRowTableData.each(function (i, item) {
                                        $tableDatum = $(this);
                                        if ((dataForServer.sampleID != "undefined") && (dataForServer.sampleID != null)) {
                                            $tableDatum.attr('data-id', dataForServer.sampleID);
                                        }
                                        else {
                                            console.log('Sample Id was not found')
                                        }
                                        $tableDatum.attr('data-type', tableCaptions[i]);

                                        if (selectColumns.indexOf(tableCaptions[i]) > -1) {
                                            $tableDatum.append(' <select hidden></select>');
                                        }
                                        else if (tableCaptions[i] != '') {
                                            $tableDatum.append('<input hidden spellcheck="false" type="text" value="" />')
                                            $tableDatum.css("white-space", "nowrap");

                                        }
                                    });
                                    
                                    SetProperColumnVisibility();                                    

                                    $('#addRowContainer').dialog("close");
                                    $.Toast("Message", "Row was sucessfully added", "success", {
                                        width: 400,
                                        stack: true
                                    });
                                    $dataTable.draw(false);
                                }
                                else {//if finished with error
                                    errorDialog(result.item2);
                                }
                            }).fail(() => {
                                errorDialog("Row addition error");
                            });
                            //stop loading animation
                            $loadingAnimationContainer.css('visibility', 'hidden');
                        }
                    },
                    {
                        text: "Close",//close button click event handler
                        click: function () {
                            $(this).dialog("close");
                        }
                    }
                ]
            });

            $addRowContainerInner = $('#addRowContainerInner');
            $addRowContaier.dialog('option', 'width', $addRowContainerInner.width() + 50);
            $addRowContaier.dialog("option", "height", $addRowContainerInner.height() + 150);


            //add date format validation
            $('#addRowContainerInner input[name="date"]').keyup((e) => {
                $date = $('#addRowContainerInner input[name="date"]');
                let testResult = new RegExp('^20\\d{2}-([0]{1}\\d|[1]{1}[12]{1})-([0-2]{1}\\d|[3]{1}[01]{1})$', 'g').test($date.val());
                if (testResult) {
                    addRowInputValidation = true;
                    $date.removeClass('red-border-color');
                }
                else {
                    addRowInputValidation = false;
                    $date.addClass('red-border-color');
                }
            });
            //add deposition time format validation
            $('#addRowContainerInner input[name="depositionTime"]').keyup((e) => {
                $depositionTime = $('#addRowContainerInner input[name="depositionTime"]');
                let testResult = new RegExp('^[0-5]{1}\\d:[0-5]{1}\\d:[0-5]\\d$', 'g').test($depositionTime.val());
                if (testResult) {
                    addRowInputValidation = true;
                    $depositionTime.removeClass('red-border-color');
                }
                else {
                    addRowInputValidation = false;
                    $depositionTime.addClass('red-border-color');
                }
            });
            //description max length validation
            $('textarea[name = "description"]').keyup((e) => {
                var $addRowTextArea = $('textarea[name = "description"]');
                if ($addRowTextArea.val().length < 65) {
                    addRowInputValidation = true;
                    $addRowTextArea.removeClass('red-border-color');
                }
                else {
                    addRowInputValidation = false;
                    $addRowTextArea.addClass('red-border-color');
                }
            });
        }).fail(() => {     //if failed
            $loadingAnimationContainer.css('visibility', 'hidden');
            $('#ErrorMessage').html("You do not have a permission to add the row");
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
        });
    });

});