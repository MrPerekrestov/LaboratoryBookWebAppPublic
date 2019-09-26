
var hiddenColumns = localStorage.getItem("hiddencolumns" + LaboratoryBookName);
var hiddenColumIndexes = localStorage.getItem("hiddencolumnindexes" + LaboratoryBookName);
if ((hiddenColumns == "undefined") || (hiddenColumns == null)) {
    hiddenColumns = [];
}
document.addEventListener('DOMContentLoaded', function () {
   
    document
        .getElementById("selectColumnsButton")
        .addEventListener('click', () => {  //select columns click event handler;
            let selectColumnsXhr = new XMLHttpRequest();
            selectColumnsXhr.open("GET", "/LaboratoryBook/SelectColumns/", true);
            selectColumnsXhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            selectColumnsXhr.onloadstart = function () {//show loading animation
                $loadingAnimationContainer.css('visibility', 'visible');
            };

            selectColumnsXhr.onerror = function () {
                $loadingAnimationContainer.css('visibility', 'hidden');
                errorDialog("Delete user request syntax error")
            };

            selectColumnsXhr.onload = function () {//server sent the response
                $loadingAnimationContainer.css('visibility', 'hidden');

                if (selectColumnsXhr.status != 200) {//some error occured
                    let errorMessage = JSON.parse(selectColumnsXhr.response);
                    errorDialog(errorMessage.message);
                    return;
                }

                $('#selectColumnsContainer')
                    .html(selectColumnsXhr.response)
                    .dialog({
                        show: {
                            effect: dialogEffect,
                            duration: dialogDuration
                        },
                        hide: {
                            effect: dialogEffect,
                            duration: dialogDuration
                        },
                        title: 'Select columns',
                        width: 500,
                        modal: true,
                        resizable: false,
                        buttons: [
                            {
                                text: "Apply",
                                click: function () {
                                    let listOfHiddenColumns = [];
                                    document
                                        .querySelectorAll('#selectColumnsContainer input[type="checkbox"]')
                                        .forEach((checkbox, number) => {
                                            if (!checkbox.checked) {
                                                let columnName = checkbox.getAttribute('column-name');
                                                listOfHiddenColumns.push(columnName);
                                            }
                                        });

                                    let columns = $dataTable.columns().context[0].aoColumns;                                  

                                    let hiddenColumIndexes = [];
                                    columns.forEach((column, number) => {
                                        if ((listOfHiddenColumns.indexOf(column.sTitle) > -1) && (column.sTitle != "")) {
                                            hiddenColumIndexes.push(column.idx);
                                            $dataTable.column(column.idx).visible(false, false);
                                        }
                                        else {
                                            $dataTable.column(column.idx).visible(true, false);
                                        }
                                    });                           

                                    $dataTable.draw(false);

                                    localStorage.setItem("hiddencolumns" + LaboratoryBookName, listOfHiddenColumns);
                                    localStorage.setItem("hiddencolumnindexes" + LaboratoryBookName, hiddenColumIndexes);
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
                hiddenColumns = localStorage.getItem("hiddencolumns" + LaboratoryBookName) || [];
                document
                    .querySelectorAll('#selectColumnsContainer input[type="checkbox"]')
                    .forEach((checkbox, number) => {
                        let columnName = checkbox.getAttribute('column-name');
                        if (hiddenColumns.indexOf(columnName) > -1) {
                            checkbox.checked = false;
                        }
                        else {
                            checkbox.checked = true;
                        }
                    });

            }
            //send the request
            selectColumnsXhr.send();
        });

});



