/*jslint browser: true, vars: true, plusplus: true */
/*global $, msgType, loadPatientSideNavigation, updatePageTitle, ApiClient */

var timer = 0, bError = false;
var documentTable;
var client = new ApiClient("Document");
var buildDocumentTable = function () {
    documentTable = $('#documentTable').alslDataTable({
        "aaSorting": [[0, "desc"]],
        "aoColumns": [
            {
                "sTitle": "Date",
                "mData": "Date",
                "sWidth": "10%",
                "sType": "date",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Entered By",
                "mData": "EnteredBy",
                "sType": "string",
                "sWidth": "17%",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Type",
                "mData": "DocumentType",
                "sType": "string",
                "sWidth": "19%",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Name",
                "mData": "DocumentName",
                "sType": "string",
                "sWidth": "17%",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Comments",
                "mData": "Comments",
                "sType": "string",
                "sWidth": "25%",
                "sClass": "left",
                "bSortable": true,
                "mRender": function (data) {
                    if (data !== undefined && data !== null) {
                        data = data.replace(/\n/g, '<br/>');
                    }
                    return data;
                }
            },
            {
                "sTitle": "Actions",
                "mData": "Id",
                "sType": "integer",
                "sWidth": "12%",
                "sClass": "center",
                "bSortable": false,
                "mRender": function (data, type, row) {
                    return '<a class="pick" href="" id="' + data + "|" + row.DocumentFormat + '">' +
                        '<i class="btn icon-file-pdf" title="Export as a PDF"  id="' + data + "|" + row.DocumentFormat + "|VIEW" + '"></i>' +
                        '<a class="delete" href="" id="' + data + '">' +
                        '<i class="btn icon-remove" title="Delete Document" id="' + data + '"></i>';
                }
            }
        ],
        "oLanguage": {
            "sEmptyTable": "No documents found."
        }
    });
};

function loadDocumentTable() {
    documentTable.fnClearTable();
    client
        .action("GetAllDocuments")
        .get({
            "patientId": window.patient.PatientId
        })
        .done(function (data) {
            documentTable.refreshDataTable(data);
        });
}

function loadDocumentType() {
    var i;
    client
        .action("GetDocumentType")
        .get()
        .done(function (data) {
            for (i = 0; i < data.length; i++) {
                $("select#uploadDocumentType").append($("<option />", { "text": data[i].Description, "value": data[i].Description }));
            }
            $("select#uploadDocumentType").selectpicker('refresh');
        });
}

function validateInput(name) {
    var val = $(name).val().length;
    var bValid = false;
    switch (name) {
    case "#uploadDocumentName":
        if (val === 0) {
            $(name).showFieldMessage(msgType.ERROR, "Enter the Document Name.");
        } else if (val > 30) {
            $(name).showFieldMessage(msgType.ERROR, ["Document Name cannot exceed 30 characters.", "Current name has " + val.toString() + " characters."], true);
        } else {
            $(name).clearField();
            $(name).removeClass("requiredField");
            bValid = true;
        }
        break;
    case "#uploadDocumentComments":
        if (val > 255) {
            $(name).showFieldMessage(msgType.ERROR, ["Document Comments cannot exceed 255 characters.", "Current comments has " + val.toString() + " characters."], true);
        } else {
            $(name).clearField();
            bValid = true;
        }
        break;
    }
    return bValid;
}

var validateFile = function (file) {
    //IE won't work for validation then let CS code validate the file
    if (navigator.appVersion.indexOf("MSIE") > -1) {
        return '';
    }
    var message = "Invalid file. Supported file types are png, jpg, gif, tif and pdf.";
    var supportedImageTypes = ["image/gif", "image/jpeg", "image/tiff", "image/png", "application/pdf"];
    var supportedExtensions = [".gif", ".jpg", ".jpeg", ".tif", ".tiff", ".png", ".pdf"];
    var extension = file.name.substr(file.name.lastIndexOf(".")).toLowerCase();
    var valid = false;
    if (file.type) {
        var contentType = file.type.toLowerCase();
        if (supportedImageTypes.indexOf(contentType) >= 0) {
            valid = true;
        }
    }
    if (!valid) {
        if (supportedExtensions.indexOf(extension) >= 0) {
            valid = true;
        }
    }
    if (valid) {
        message = '';
        if (file.size > 4096000) {
            message = "Invalid file. The size of the file should not exceed 4 MB.";
        }
    }
    return message;
};

//Prevent Enter Key from closing Dialog
$("#uploadDocumentName").keydown(function (e) {
    return e.keyCode !== 13;
});

$("#uploadDocumentName, #uploadDocumentComments").keyup(function (e) {
    //tab or shift tab key
    if (e.keyCode === 9 || e.keyCode === 16) {
        return;
    }
    validateInput("#" + this.name);
});

var addUploadEventHandlers = function addUploadEventHandlers() {
    $("#documentTable").on("click", ".pick", function (e) {
        e.preventDefault();
        var $this = $(this);
        var ids = $this.attr("id");
        var splitStr = ids.split("|");
        var id = splitStr[0];
        var documentFormat = splitStr[1];
        var action = splitStr[2];
        var url = window.config.baseUrl + "Patient/GetPdfDocument?id=" + id + "&patientId=" + window.patient.PatientId;
        window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
    });

    $("#documentTable").on("click", ".delete", function (e) {
        e.preventDefault();
        var $this = $(this);
        var documentId = $this.attr("id");
        $('#deleteDocumentModal').data('id', documentId).modal({
            keyboard: false,
            backdrop: 'static',
            show: true
        });
    });

    //  define click handler for Delete Document button    
    $('#btnDeleteDocument').click(function () {
        var documentId = $('#deleteDocumentModal').data('id');
        client
            .action("Delete")
            .queryStringParams({ "id": documentId, "patientId": window.patient.PatientId })["delete"]()
            .done(function () {
                loadDocumentTable();
                $(document).showSystemSuccess("Document deleted.");
                $('#btnSaveDocument').attr('disabled', true);
                $('#deleteDocumentModal').modal('hide');
            })
            .fail(function () {
                $('#btnSaveDocument').attr('disabled', true);
                $('#deleteDocumentModal').modal('hide');
            });
    });

    // define click handler for Upload Document button ======================================================================================   
    $('#btnUpload input').fileupload({
        dataType: 'json',
        headers: { 'RequestVerificationToken': $("#antiForgeryToken").val() },
        done: function (e, data) {
            if (data.result !== undefined) {
                if (data.result.errorMsg === '') {
                    $('#uploadDocument').val(data.result.filePath);
                    $('#uploadFileName').html(data.result.fileName);
                    $("#uploadFileName").attr("title", data.result.title);
                    $("#uploadFileName").clearField();
                    $('#btnSaveDocument').attr('disabled', false);
                } else {
                    $("#uploadFileName").html("No file is selected.");
                    $("#uploadFileName").showFieldMessage(msgType.ERROR, data.result.errorMsg, true);
                    $('#btnSaveDocument').attr('disabled', true);
                    bError = true;
                }
            }
        },
        start: function () {
            timer = $("#progressTimer").alslProgressStart({ "text": "" });
        },
        stop: function () {
            timer = $("#progressTimer").alslProgressStop(timer);
            //last step to put up the error message if it is not done yet by the 'done' function 
            if (!bError && $('#uploadDocument').val() === "") {
                $("#uploadFileName").html("No file is selected.");
                $("#uploadFileName").showFieldMessage(msgType.ERROR, "Invalid file. The size of the file should not exceed 4 MB.", true);
            }
        }
    }).on('fileuploadsubmit', function (e, data) {
        bError = false;
        timer = $("#progressTimer").alslProgressStop(timer);
        $('#uploadDocument').val("");
        $("#uploadFileName").attr("title", "");
        $("#uploadFileName").clearField();
        var message = validateFile(data.files[0]);
        if (message !== '') {
            $("#uploadFileName").html("No file is selected.");
            $("#uploadFileName").showFieldMessage(msgType.ERROR, message, true);
            e.preventDefault();
        }
    });

    $("#btnCancel").click(function (e) {
        $('#btnSaveDocument').attr('disabled', true);
    });

    $("#btnUploadDocument").click(function (e) {
        e.preventDefault();
        $("#uploadDocumentName").val("");
        $("#uploadDocumentName").removeClass("error").addClass("requiredField");
        $("#uploadDocumentName").clearField();
        $("#uploadFileName").attr("title", "");
        $("#uploadFileName").clearField();
        $("#uploadFileName").html("No file is selected.");
        $("#uploadDocumentComments").clearField();
        $("#uploadDocumentComments").val("");
        $("#uploadDocument").val("");
        $(".summaryMessages").clearMsgBlock();
        $('#uploadDocumentModal').modal({
            keyboard: false,
            backdrop: 'static',
            show: true
        });
        setTimeout(function () {
            $("button[data-id='uploadDocumentType']").focus();
        }, 300);
    });

    $('#btnSaveDocument').click(function () {
        if (!$("#uploadDocumentModal").is(':visible')) {
            return; //double click too fast
        }

        $(this).attr('disabled', true);
        var bValid = true;

        $(".summaryMessages").clearMsgBlock();
        $("#uploadDocumentName").clearField();
        $("#uploadDocumentComments").clearField();
        $("#uploadFileName").clearField();

        var val = $("#uploadDocumentName").val().trim();
        $("#uploadDocumentName").val(val);
        if (!validateInput("#uploadDocumentName")) {
            bValid = false;
        }

        val = $("#uploadDocumentComments").val().trim();
        $("#uploadDocumentComments").val(val);
        if (!validateInput("#uploadDocumentComments")) {
            bValid = false;
        }

        if ($("#uploadFileName").html() === "No file is selected.") {
            $("#uploadFileName").showFieldMessage(msgType.ERROR, "Select a file to upload.");
            bValid = false;
        }

        if (bValid) {
            var $this = $(this);
            var requestData = {
                UserId: window.config.userId,
                PatientId: window.patient.PatientId,
                DocumentName: $("#uploadDocumentName").val().trim(),
                DocumentType: $("#uploadDocumentType").val(),
                Comments: $("#uploadDocumentComments").val().trim(),
                FilePath: $("#uploadDocument").val()
            };
            $("#uploadDocument").val('');
            client
                .action("Upload")
                .post(requestData)
                .done(function (data, xhr, e) {
                    if (xhr === 'success') {
                        $this.attr('disabled', false);
                        $('#btnSaveDocument').attr('disabled', true);
                        $("#uploadDocumentModal").modal("hide");
                        loadDocumentTable();
                        $(document).showSystemSuccess(data);
                    } else {
                        window.parent.jQuery(document).showSystemFailure(xhr.responseText.replace(/\"/g, ''));
                        $(".summaryMessages").clearMsgBlock();
                        $("#uploadFileName").html("No file is selected.");
                        $('#btnSaveDocument').attr('disabled', true);
                        bValid = false;
                    }
                })
                .fail(function (xhr) {
                    if (xhr.statusText === 'Bad Request') {
                        window.parent.jQuery(document).showSystemFailure(xhr.responseText.replace(/\"/g, ''));
                        $(".summaryMessages").clearMsgBlock();
                        $("#uploadFileName").html("No file is selected.");
                        $('#btnSaveDocument').attr('disabled', true);
                        bValid = false;
                    }
                });
        } else {
            $(this).attr('disabled', false);
            $('#btnSaveDocument').attr('disabled', true);
        }
    });

};

$(document).ready(function () {
    // load the side nav
    loadPatientSideNavigation(window.patient.PatientId, "documents");
    updatePageTitle();
    //initialize variables
    timer = 0;
    buildDocumentTable();
    loadDocumentTable();
    loadDocumentType();
    addUploadEventHandlers();
    $('#documentTable').removeAttr('style');
    $('#btnSaveDocument').attr('disabled', true);
    $('#btnTransferDocument').attr('disabled', true);
    $('#uploadFileName').attr('disabled', true);
});