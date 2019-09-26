
$(document).ready(() => {
    //refresh table button click event handler
    $('#refreshTableButton').click(() => {
        $.ajax({
            url: "/LaboratoryBook/RefreshTable",
            method: "GET",
            cache: false,
            beforeSend: () => { //show loading anumation until response   
                $loadingAnimationContainer.css('visibility', 'visible');
            }
        }).done((data) => {
            $loadingAnimationContainer.css('visibility', 'hidden');

            //reconstruct new data table
            $dataTable.destroy();
            $("#samples").html(data);
            let RowNumbers = $('#samples th').length;
            $dataTable = $('#samples').DataTable({
                select: 'single',
                "columnDefs": [
                    { "orderable": false, "targets": RowNumbers - 1 }
                ]
            });
            $dataTable.draw();

            //set elements dimesions

            //let table = $('.table-container-inner table');
            //let tableContainer = $('.table-container');

            //$('#testDiv').html(table.width() + ' ' + tableContainer.width());
            //if (table.width() > tableContainer.width()) {
            //    tableContainer.css("min-width", table.width() + "px");
            //    $('body').css("min-width", table.width() + "px");
            //    $('#samples_wrapper').css("min-width", table.width() + "px");
            //}

            //reload lists
            progressbarValue = 0;
            listsLoaded = false;
            $.each(Lists, function (key, value) {
                Lists[key].isLoaded = false;              
            })
            $progressBar.progressbar("option", "value", progressbarValue);
            $('#progressbar-contaier').show();           
            
            getList('materials');
            getList('substrates');
            getList('operators');
            getList('regimes');
            getList('permissionIDs');

            hiddenColumns = localStorage.getItem("hiddencolumns" + LaboratoryBookName);

            if ((hiddenColumns != "undefined") && (hiddenColumns != null)) {
                let columns = $dataTable.columns().context[0].aoColumns;
                columns.forEach((column, number) => {
                    if ((hiddenColumns.indexOf(column.sTitle) > -1) && (column.sTitle != "")) {
                        $dataTable.column(column.idx).visible(false, false);
                    }
                    else {
                        $dataTable.column(column.idx).visible(true, false);
                    }
                });
                $dataTable.columns.adjust().draw(false);
            }           
            
        }).fail((xhr, ajaxOptions, thrownError) => {
            $loadingAnimationContainer.css('visibility', 'hidden');
            errorDialog(thrownError);
        });
    });
    //refresh table button click event handler finish
});