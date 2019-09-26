$(document).ready(function () {

    function errorDialog(message) {
        $('#ErrorMessage').html(message);
        $('#ErrorDialog').css("display", "block");
        $('#ErrorDialog').dialog({
            width: "500px",
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
            buttons: [
                {
                    text: "OK",
                    click: function () {

                        $(this).dialog("close");
                    }
                }
            ]
        });
    }
    $("#createLaboratoryBookContainer").toggle();
    $('#selectBook-button').css("width", "140px");
    $("#select-container").animate(
        {
            top: '50%',
            opacity: '1'
        });
    $('#btnConnect').click(() => {
        let selectedItem = $('#selectBook').val();
        window.location.replace("/LaboratoryBook/MainPage/" + selectedItem);
    });
    $('#btnNew').click(() => {
        $("#createLaboratoryBookContainer").toggle("fast");
    });

    $('#btnDelete').click(() => {
        
        let laboratoryBookName = $('#selectBook').val();
        $('#questionDialogMessage').html(`Do you want to delete laboratory book <b>${laboratoryBookName}</b>?`);
        let $deleteQuestionDialog = $('#questionDialog')
            .css('display', 'block')
            .dialog({
                title: 'Book deleting',
                width: "500px",
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
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                method: "DELETE",
                                contentType: "application/json",
                                url: "/Login/DeleteLaboratoryBook/",
                                beforeSend: function () {
                                    $('#loadingAnimationContainer').css('display', 'block');
                                },
                                data: JSON.stringify({
                                    LaboratoryBookName: laboratoryBookName
                                })
                            }).done((result) => {
                                $(`#selectBook option[value="${laboratoryBookName}"`).remove();
                                $.Toast("Message", `Laboratory book <b>${laboratoryBookName}</b> was successfully deleted`, "success", {
                                    width: 420,
                                    stack: true
                                });
                                $(this).dialog("close");
                                $('#loadingAnimationContainer').css('display', 'none');
                            }).fail((xhr, status, error) => {                           
                                errorDialog(xhr.responseJSON.errorMessage);
                                $('#loadingAnimationContainer').css('display', 'none');
                                $(this).dialog("close");
                            });                              
                        }
                    },
                    {
                        text: "No",
                        click: function () {
                            $(this).dialog("close");
                        }
                    }]
            });


    });

    document
        .getElementById('btnCreate')
        .addEventListener('click', () => {
            //get new laboratory book name
            let laboratoryBookName = document
                .getElementById('newLaboratoryBookNameInput')
                .value;
            //send the request to create a new laboratory book
            let createBookXhr = new XMLHttpRequest();
            createBookXhr.open("POST", "/Login/CreateLaboratoryBook", true);
            createBookXhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
            createBookXhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            createBookXhr.onloadstart = function () {
                $('#loadingAnimationContainer').css('display', 'block');
            }
            createBookXhr.onerror = function () {        
                $('#loadingAnimationContainer').css('display', 'none');
                errorDialog(this.status + ": " + this.statusText);
            }
            createBookXhr.onload = function () {
                console.log(createBookXhr)
                if (createBookXhr.status == 200) {
                    let newOption = new Option(laboratoryBookName, laboratoryBookName, false, true);
                    $('#selectBook').append($(newOption));
                    $.Toast("Message", `Laboratory book <b>${laboratoryBookName}</b> was successfully created`, "success", {
                        width: 420,
                        stack: true
                    });
                    $('#loadingAnimationContainer').css('display', 'none');
                    $("#createLaboratoryBookContainer").toggle("fast");
                }
                else {
                    $('#loadingAnimationContainer').css('display', 'none');
                    errorDialog("Laboratory book was not created");
                }
               
            }

            //send the reqeust
            createBookXhr.send(JSON.stringify({
                LaboratoryBookName: laboratoryBookName
            }));

        });

});