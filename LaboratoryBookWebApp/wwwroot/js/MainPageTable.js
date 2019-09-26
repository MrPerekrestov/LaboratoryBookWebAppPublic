
var $progressBar = $("#progressbar").progressbar();
var progressbarValue = 0;
var $dataTable;

//initialize list of pre-defined lists
var Lists = {
    materials: {
        values: [],
        isLoaded: false
    },
    substrates: {
        values: [],
        isLoaded: false
    },
    operators: {
        values: [],
        isLoaded: false
    },
    regimes: {
        values: [],
        isLoaded: false
    },
    permissionIDs: {
        values: [],
        isLoaded: false
    },
}
var listsLoaded = false;

//initialize lists
getList('materials');
getList('substrates');
getList('operators');
getList('regimes');
getList('permissionIDs');

var $selectedTd;
var $selectedInput;
var defaultBorderColor;
var datumChangeFinished = true;

//get predefined list from the database
function getList(ListName) {
    $.ajax({
        cache: false,
        method: "GET",
        url: '/LaboratoryBook/Getlist',
        dataType: 'text',
        data: {
            listName: ListName
        }
    }).done((data) => {
        Lists[ListName].values = data.split(";");
        Lists[ListName].isLoaded = true;
        progressbarValue += 20;

        $("#progressbar").progressbar("option", "value", progressbarValue);

        let checkForAllLoaded = true;

        $.each(Lists, (key, val) => {
            if (!val.isLoaded) checkForAllLoaded = false;
        });
        
        listsLoaded = checkForAllLoaded;
        if (listsLoaded) {
            $('#progressbar-contaier').hide();
        }
    }).fail((xhr, ajaxOptions, thrownError) => {
        errorDialog(thrownError);
    })
}

//update database after changes in client
async function updateDbValue(DataId, DataType, DataValue) {
    try {
        let result = await
            $.ajax({
                cache: false,
                method: "PUT",
                contentType: "application/json",
                url: '/LaboratoryBook/UpdateValue',
                dataType: 'json',
                data: JSON.stringify({
                    dataId: DataId,
                    dataType: DataType,
                    dataValue: DataValue
                })
            });       
        return result;
    }
    catch (e) {        
        return {
            item1: false,
            item2: "You do not have a permission to make changes in laboratory book."
        };
    }
}

//change client table value after successfull update of the database
function UpdateTable(requestResult) {
    if (requestResult.item1) {
        $selectedTd.html($selectedInput.val() + '<input hidden spellcheck="false" type="text" value="" />');
        $selectedInput = null;
        $selectedTd = null;
    }
    else {
        $selectedInput.hide();
        $selectedTd.html($selectedTd.text() + '<input hidden spellcheck="false" type="text" value="" />');
        $selectedTd = null;
        //request result returned false=>show error dialog
        errorDialog(requestResult.item2);
    }
}

// document ready script
$(document).ready(function () {

    var table = $('.table-container-inner table');
    var tableContainer = $('.table-container');

    $(window).on("load", () => {
        //define toolbar-container minimum width
        let toolBarContainerLeftWidth = $('.toolbar-container-left').width();
        let toolBarContainerCenterWidth = $('.toolbar-container-center div').width();
        let toolBarContainerRightWidth = $('.toolbar-container-right').width();
        let sum = toolBarContainerRightWidth + toolBarContainerCenterWidth + toolBarContainerLeftWidth + 50;
        $('.toolbar-container').css("min-width", sum + "px");
    });

    //resize components 
    //$(window).resize(function () {
        //var table = $('.table-container-inner table');
        //var tableContainer = $('.table-container');

        //$('#testDiv').html(table.width() + ' ' + tableContainer.width());
        //if (table.width() > tableContainer.width()) {
        //    tableContainer.css("min-width", table.width() + "px");
        //    $('body').css("min-width", table.width() + "px");
        //    $('#samples_wrapper').css("min-width", table.width() + "px");
        //}
    //});

    //create jquery-ui datatable from html table
    let RowNumbers = $('#samples th').length;
    $dataTable = $('#samples').DataTable({
        select: 'single',
        "columnDefs": [
            { "orderable": false, "targets": RowNumbers - 1 }
        ]
    });
    $('#samples').show();

    hiddenColumns = localStorage.getItem("hiddencolumns" + LaboratoryBookName)||[];
    let columns = $dataTable.columns().context[0].aoColumns;
    columns.forEach((column, number) => {
        if ((hiddenColumns.indexOf(column.sTitle) > -1) && (column.sTitle!="")) {
            $dataTable.column(column.idx).visible(false, false);           
        }
        else {
            $dataTable.column(column.idx).visible(true, false);
        }
    });    

    $dataTable.columns.adjust().draw(false);
    //table cell click event when table is redrawn

    $dataTable.on('draw.dt', function () {
        $dataTable.$('td').off('click').click(function () {
            if (!datumChangeFinished) return;
            let id = $(this).attr('data-id');
            let type = $(this).attr('data-type');
            if ($selectedTd) {
                if ((id == $selectedTd.attr('data-id')) && (type == $selectedTd.attr('data-type'))) {
                    return;
                }
            }
            $selectedTd = $(this);

            if ($selectedInput) {
                $selectedInput.hide();
            }
            //define selected input
            if (Lists[type + 's']) {            //input data-type is in pre-defined list
                $selectedInput = $('select', this).show()
                    .width($(this).width())
                    .css({
                        'font-size': $(this).css('font-size'),
                        'font-family': $(this).css('font-family')
                    });
                if ($selectedInput.html() == "") {
                    var selectedItemFound = false;
                    $.each(Lists[type + 's'].values, (item, value) => {
                        let selected = (value == $(this).text().trim().split(' ')[0]) ? true : false;
                        if (selected) {
                            selectedItemFound = true;
                        }
                        let option = new Option(value, value, false, selected);
                        $selectedInput.append($(option));
                    });

                    if (!selectedItemFound) {
                        $selectedInput.prop("selectedIndex", -1);
                    }
                }
                //selected input chage event handler            
                $selectedInput.off('change').change(async () => {
                    let selectedOption = $('option:selected', $selectedInput);
                    // await for update result: item1 - true or false, result of the update action
                    //                          item2 - message from action.
                    var selectedInputTemp = $selectedInput;
                    var selectedTdTemp = $selectedTd;
                    var selectedOptionValue = selectedOption.val();
                    datumChangeFinished = false;
                    let updateResult = await updateDbValue(id, type, selectedOptionValue);

                    if (updateResult.item1) {
                        selectedTdTemp.html(selectedOptionValue + " <select hidden></select>");
                        $selectedInput = null;
                        $selectedTd = null;
                    }
                    else {
                        $selectedInput.hide();
                        $selectedTd.html($selectedTd.text().trim().split(' ')[0] + " <select hidden></select>");
                        $selectedTd = null;
                        $selectedInput = null;
                        errorDialog(updateResult.item2);
                    }
                    datumChangeFinished = true;
                });
            }
            else {//selected input data-type is not in pre-defened list => input type= text
                $selectedInput = $('input', this).show()
                    .width($(this).width())
                    .val($(this).text())
                    .css({
                        'font-size': $(this).css('font-size'),
                        'font-family': $(this).css('font-family')
                    })
                    .focus()
                    .on('keyup', async (e) => {
                        if (e.keyCode == 13) {
                            //id data-type is "date " => check the correspondence with "date" pattern
                            if ($selectedTd.attr('data-type') == 'date') {

                                let testResult = new RegExp('^20\\d{2}-([0]{1}\\d|[1]{1}[12]{1})-([0-2]{1}\\d|[3]{1}[01]{1})$', 'g').test($selectedInput.val());

                                if (testResult) {
                                    datumChangeFinished = false;
                                    let updateResult = await updateDbValue(id, type, $selectedInput.val());
                                    UpdateTable(updateResult);
                                }
                                else {
                                    defaultBorderColor = $selectedInput.css('border-color');
                                    $selectedInput.css({
                                        'border-color': 'red'
                                    });
                                }
                                datumChangeFinished = true;
                            }
                            //if data-type is "depositionTime" check the correspondence with time pattern
                            else if ($selectedTd.attr('data-type') == 'depositionTime') {
                                let testResult = new RegExp('^[0-5]{1}\\d:[0-5]{1}\\d:[0-5]\\d$', 'g').test($selectedInput.val());

                                if (testResult) {// if data in DB were changed   
                                    datumChangeFinished = false;
                                    let updateResult = await updateDbValue(id, type, $selectedInput.val());
                                    UpdateTable(updateResult);
                                }
                                else {//if data in DB were not changed
                                    defaultBorderColor = $selectedInput.css('border-color');
                                    $selectedInput.css({
                                        'border-color': 'red'
                                    });
                                }
                                datumChangeFinished = true;
                            }
                            else {//column does not have any pattern => just update withou checking 
                                datumChangeFinished = false;
                                let updateResult = await updateDbValue(id, type, $selectedInput.val());
                                UpdateTable(updateResult);
                                datumChangeFinished = true;
                            }
                        }
                    })//if text input losts focus => hide input
                    .on('focusout', () => {
                        $selectedInput.hide();
                        $selectedTd.html($selectedTd.text() + '<input hidden spellcheck="false" type="text" value="" />');
                        $selectedTd = null;
                    });
            }
        });//data table cell click event finish

        //delete button click event handler start
        $('button[delete-id]').click(function () {

            let $deleteRowButton = $(this);
            let deleteSampleId = $deleteRowButton.attr('delete-id'); 

            let samplePermissionId =$dataTable.$(`td[data-id="${deleteSampleId}"][data-type="permissionID"]`)
                .text()
                .trim();              
          
            $('#questionDialogMessage').html('Do you want to delete sample with id ' + $(this).attr('delete-id') + '?');
            let $deleteQuestionDialog = $('#questionDialog')
                .css('visibility', 'visible')              
                .dialog({                     
                    title: 'Row deleting',
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
                            click: function () {
                                if (!datumChangeFinished) return;
                                datumChangeFinished = false;                        
                                $.ajax({                                    
                                    cache: false,
                                    method: 'PUT',
                                    contentType:'application/json',
                                    dataType: 'json',
                                    data: JSON.stringify({
                                        SampleID: deleteSampleId,
                                        SamplePermissionId: samplePermissionId
                                    }),
                                    url: '/LaboratoryBook/DeleteRowFromDatabase',
                                    beforeSend: () => { //show loading animation until response   
                                        $loadingAnimationContainer.css('visibility', 'visible');
                                    }
                                    //deleted without ajax error
                                }).done((result) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');                      
                                    //deleted successfully
                                    if (result.item1) {                    
                                        $dataTable.row('tr[row-id = "' + deleteSampleId + '"]').remove().draw(false);
                                        $.Toast("Message", "Row was sucessfully deleted", "success", {
                                            width: 400
                                        });
                                    }//deleted with error
                                    else {
                                        errorDialog(result.item2);
                                    }
                                    datumChangeFinished = true;
                                    //ajax row deleting error
                                }).fail((xhr, ajaxOptions, thrownError) => {
                                    $loadingAnimationContainer.css('visibility', 'hidden');
                                    errorDialog(thrownError);
                                    datumChangeFinished = true;
                                });

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
        //delete button click event handler finish

    });//table draw event handler finish

    //draw datatable to apply events
    $dataTable.draw();

    //set elements dimesions
    $('#testDiv').html(table.width() + ' ' + tableContainer.width());
    if (table.width() > tableContainer.width()) {
        tableContainer.css("min-width", table.width() + "px");
        $('body').css("min-width", table.width() + "px");
        $('#samples_wrapper').css("min-width", table.width() + "px");
    }
});
