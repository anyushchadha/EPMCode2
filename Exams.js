/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko,  Modernizr, alert, console, msgType, noReportData, openReportViewer, loadPatientSideNavigation, updatePageTitle, ApiClient, getDateDiff*/

var client = new ApiClient("PatientExams");
var pendingEhrExamsTable;

function buildPendingEhrExamsTable() {
    pendingEhrExamsTable = $('#pendingEhrExamsTable').alslDataTable({
        "aaSorting": [[1, "desc"]],
        "aoColumns": [
            {
                "sTitle": "",
                "mData": "ExamId",
                "sType": "string",
                "bVisible": false,
                "sWidth": "0%"
            },
            {
                "sTitle": "Exam Date",
                "mData": "ExamDate",
                "sType": "date",
                "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false
            },
            {
                "sTitle": "Doctor",
                "mData": "Doctor",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "Procedure Codes",
                "mData": "ProcedureCodes",
                "sType": "string",
                "sClass": "left col-lg-3 col-md-3 col-sm-3 col-xs-3",
                "bSortable": false
            },
            {
                "sTitle": "Diagnosis Codes",
                "mData": "DiagnosisCodes",
                "sType": "string",
                "sClass": "left col-lg-3 col-md-3 col-sm-3 col-xs-3",
                "bSortable": false
            },
            {
                "sTitle": "PQRS Codes",
                "mData": "PqrsCodes",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "",
                "mData": "ExamId",
                "sType": "string",
                "sClass": "center col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false,
                "mRender": function (data) {
                    return "<i class='btn icon-print no-margin' title='Print Exam' id='" + data + "' ></i>" + "<i class='btn icon-remove' title='Delete Exam' id='" + data + "' ></i>";
                }
            }
        ],
        "bAutoWidth": false,
        "bPaginate": false,
        "oLanguage": { "sEmptyTable": "There are no unused exams from Eyefinity EHR for this patient." },
        selectableRows: true,
        highlightRows: true
    });

    pendingEhrExamsTable.delegate("tbody td:not(:last-child)", "click", function () {
        if (pendingEhrExamsTable.fnPagingInfo().iTotal !== 0) {
            var aPos = pendingEhrExamsTable.fnGetPosition(this);
            var sData = pendingEhrExamsTable.fnGetData(aPos[0]);
            var redirectUrl = window.config.baseUrl + "Patient/NewExam?id=" + window.patientOrderExam.PatientId + "&oid=0" + "&examId=" + sData.ExamId + "&return=exam";
            window.location.href = redirectUrl;
        }
    });

    $('#pendingEhrExamsTable').css('margin-bottom', '60px');
}

var examsTable;
function buildExamsTable() {
    examsTable = $('#examsTable').alslDataTable({
        "aaSorting": [],
        "aoColumns": [
            {
                "sTitle": "",
                "mData": "ExamId",
                "sType": "string",
                "bVisible": false,
                "sWidth": "0%"
            },
            {
                "sTitle": "Order Date",
                "mData": "ExamDate",
                "sType": "date",
                "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false
            },
            {
                "sTitle": "Order Number",
                "mData": "OrderId",
                "sType": "integer",
                "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false
            },
            {
                "sTitle": "Doctor",
                "mData": "Doctor",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "Procedure Codes",
                "mData": "ProcedureCodes",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "Diagnosis Codes",
                "mData": "DiagnosisCodes",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "PQRS Codes",
                "mData": "PqrsCodes",
                "sType": "string",
                "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false
            },
            {
                "sTitle": "Source",
                "mData": "Source",
                "sType": "string",
                "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false
            },
            {
                "sTitle": "Invoiced",
                "mData": "IsInvoiced",
                "sType": "string",
                "sClass": "center col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false,
                "mRender": function (data) {
                    var markup = null;
                    if (data === true) {
                        markup = "<i class='icon-checkmark' id='" + data + "' title='Exam has been invoiced' ></i>";
                    } else if (data === false) {
                        markup = "<i class='icon-warning' id='" + data + "' title='Exam has NOT been invoiced' ></i>";
                    } else {
                        markup = "<i class='icon-question' id='" + data + "' title='Unknown if exam has been invoiced' ></i>";
                    }
                    return markup;
                }
            },
            {
                "sTitle": "",
                "mData": "OrderId",
                "sType": "string",
                "sClass": "center col-lg-1 col-md-1 col-sm-1 col-xs-1",
                "bSortable": false,
                "mRender": function (data) {
                    return "<i class='btn icon-print' title='Print Exam' id='" + data + "' ></i>";
                }
            }
        ],

        "bAutoWidth": false,
        "bPaginate": true,
        "iDisplayLength": 10,
        "oLanguage": { "sEmptyTable": "There are no exam orders for this patient." },
        selectableRows: true,
        highlightRows: true
    });

    examsTable.delegate("tbody td:not(:last-child)", "click", function () {
        if (examsTable.fnPagingInfo().iTotal !== 0) {
            var aPos = examsTable.fnGetPosition(this);
            var sData = examsTable.fnGetData(aPos[0]);
            var redirectUrl = window.config.baseUrl + "Patient/NewExam?id=" + window.patientOrderExam.PatientId + "&oid=" + sData.OrderId + "&examId=0&return=exam";
            window.location.href = redirectUrl;
        }
    });
}

function getData() {
    client
        .action("GetPatientExams")
        .get({
            "patientId": window.patientOrderExam.PatientId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                if (window.config.isEhrEnabled === "True") {
                    pendingEhrExamsTable.refreshDataTable(data.PendingExams);
                    pendingEhrExamsTable.fnDraw();
                    if (data.ContainsOlderExams) {
                        $("#msg_OldExams").removeClass("hidden");
                    } else {
                        $("#msg_OldExams").addClass("hidden");
                    }
                }
                examsTable.refreshDataTable(data.OrderExams);
            }
        });
}
//function verifyAccess() {
//    if (window.patient.Error !== undefined && window.patient.Error !== null && window.patient.Error.toString() !== '') {
//        $("#btnReturn").prop("disabled", true);
//        $(this).showSummaryMessage(msgType.SERVER_ERROR, window.patient.Error, true);
//    }
//}

$("#btnNewExam").click(function (e) {
    e.preventDefault();
    if (window.patientOrderExam.PatientId.toString() !== '' && window.patientOrderExam.PatientId.toString() !== '0') {
        var redirectUrl = window.config.baseUrl + "Patient/NewExam?id=" + window.patientOrderExam.PatientId + "&oid=0&examId=0&return=exam";
        window.location.href = redirectUrl;
    }
});

/* click handler for Delete Exam icon in grid */
$('#pendingEhrExamsTable').on("click", ".icon-remove", function (e) {
    e.preventDefault();
    var $this = $(this);
    var delExamId = $this.attr("id");

    $('#deleteExamModal').data('delExamId', delExamId).modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});

/* click handler for Delete Exam button */
$('#btnConfirmDeleteExam').click(function (e) {
    e.preventDefault();

    var delExamId = $('#deleteExamModal').data('delExamId');
    client
        .action("DeletePatientExam")
        .queryStringParams({ examId: delExamId })["delete"]()
        .done(function (data) {
            if (data === true) {
                getData();
                $(document).showSystemSuccess("Exam deleted.");
            }
        });
});


$("#pendingEhrExamsTable").on("click", ".icon-print", function (e) {
    e.preventDefault();
    var $this = $(this);
    var id = $this.attr("id");
    var url = window.config.baseUrl + 'PatientReports/PrintEhrExamReport?examId=' + id + '&officeNum=' + window.config.officeNumber;
    window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
});

$("#examsTable").on("click", ".icon-print", function (e) {
    e.preventDefault();
    var $this = $(this);
    var id = $this.attr("id");
    var url = window.config.baseUrl + 'PatientReports/PrintOrderExamReport?orderId=' + id + '&officeNum=' + window.config.officeNumber;
    window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
});

function buildTables() {
    if (window.config.isEhrEnabled === "True") {
        buildPendingEhrExamsTable();
        $("#titlePendingExams").removeClass("hidden");
    } else {
        $("#titlePendingExams").addClass("hidden");
    }
    buildExamsTable();
}

$(document).ready(function () {
    loadPatientSideNavigation(window.patientOrderExam.PatientId, "exams");
    updatePageTitle();
    $("#btnReturn").prop("disabled", false);
    buildTables();
    getData();
});