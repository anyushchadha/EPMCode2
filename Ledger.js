/*jslint vars:true, plusplus:true */
/*global ApiClient, window, $, regex, document, ko,
updatePageTitle, setTimeout, buildPatientLedgerTable, verifyAccess, loadPatientLedgerTable, msgType*/
var client = new ApiClient("PatientLedger");
var patientLedgerTable;
function buildPatientLedgerTable() {
    patientLedgerTable = $('#patientLedgerTable').alslDataTable({
        "aaSorting": [],
        "aoColumns": [
            {
                "sTitle": "Date",
                "mData": "PostDate",
                "sType": "date",
                "sClass": "left",
                "sWidth": "8%",
                "bSortable": false
            },
            {
                "sTitle": "Description",
                "mData": "Description",
                "sType": "string",
                "sClass": "left",
                "sWidth": "24%",
                "bSortable": false
            },
            {
                "sTitle": "Transaction Type",
                "mData": "TransactionType",
                "sType": "string",
                "sClass": "left",
                "sWidth": "17%",
                "bSortable": false
            },
            {
                "sTitle": "Reference",
                "mData": "Reference",
                "sType": "string",
                "sClass": "left",
                "sWidth": "18%",
                "bSortable": false
            },
            {
                "sTitle": "Retail Amount",
                "mData": "RetailAmountDisplay",
                "sType": "string",
                "sClass": "right",
                "sWidth": "8%",
                "bSortable": false
            },
            {
                "sTitle": "Patient Amount",
                "mData": "PatientAmountDisplay",
                "sType": "string",
                "sWidth": "8%",
                "sClass": "right",
                "bSortable": false
            },
            {
                "sTitle": "Insurance Amount",
                "mData": "InsuranceAmountDisplay",
                "sType": "string",
                "sClass": "right",
                "sWidth": "8%",
                "bSortable": false
            },
            {
                "sTitle": "Amount Due",
                "mData": "AmountDueDisplay",
                "sType": "string",
                "sClass": "right",
                "sWidth": "8%",
                "bSortable": false
            }
        ],
        "bAutoWidth": false,
        "bPaginate": false,
        "oLanguage": {
            "sEmptyTable": "Select an order to view the ledger details."
        },
        "fnCreatedCell": function (nTd, sData, oData, iRow, iCol) {
            if (iRow > 0) {
                if (sData.substring(0, 1) !== '') {
                    $(nTd).style.color('red');
                }
            }
        },

        "fnInitComplete": function (oSettings) {
            oSettings.oLanguage.sEmptyTable = "No ledger details found.";
        },
        "fnRowCallback": function (nRow, aData, iDisplayIndex) {
            if (aData.Description !== undefined && aData.Description !== null) {
                if (aData.Description === "Totals" || aData.Description === "Balance") {
                    $('td', nRow).css('background-color', '#bfbfbf');  //dark gray
                    $('td:eq(1)', nRow).css('font-weight', 'bold');
                }
            }

            if (aData.Description !== undefined && aData.Description !== null) {
                if (aData.Description.length > 0 && aData.Description !== "Totals" && aData.Description !== "Balance" && aData.TransactionType === null) {
                    $('td', nRow).css('background-color', '#e6e6e6'); //light gray
                    if (aData.Description.length > 0) {
                        $('td', nRow).css('font-weight', 'bold');
                    }
                }
            }

            if (aData.PatientAmountDisplay !== undefined && aData.PatientAmountDisplay.substring(0, 2) === ('($')) {
                $('td:eq(5)', nRow).css('color', '#008000');
            }

            if (aData.InsuranceAmountDisplay !== undefined && aData.InsuranceAmountDisplay.substring(0, 2) === ('($')) {
                $('td:eq(6)', nRow).css('color', '#008000');
            }

            if (aData.Description === "Balance" && aData.PatientAmountDisplay !== undefined && aData.PatientAmountDisplay.length > 0) {
                if (aData.PatientAmountDisplay.substring(0, 2) === ('($')) {
                    $('td:eq(5)', nRow).css('color', '#008000');
                } else if (aData.PatientAmount > 0) {
                    $('td:eq(5)', nRow).css('color', '#ff0000');
                }
            }

            if (aData.Description === "Balance" && aData.InsuranceAmountDisplay !== undefined && aData.InsuranceAmountDisplay.length > 0) {
                if (aData.InsuranceAmountDisplay.substring(0, 2) === ('($')) {
                    $('td:eq(6)', nRow).css('color', '#008000');
                } else if (aData.InsuranceAmount > 0) {
                    $('td:eq(6)', nRow).css('color', '#ff0000');
                }
            }

            if (aData.Description === "Balance" && aData.AmountDueDisplay !== undefined && aData.AmountDueDisplay.length > 0) {
                if (aData.AmountDueDisplay.substring(0, 2) === ('($')) {
                    $('td:eq(7)', nRow).css('color', '#008000');
                } else if (aData.AmountDue > 0) {
                    $('td:eq(7)', nRow).css('color', '#ff0000');
                }
            }
        },
        selectableRows: false,
        highlightRows: false,

        "iDisplayLength": "All",
        "sDom": '<"top">t<"bottom"><"clear">'
    });
}
// builds the patient search grid 
function loadPatientLedgerTable() {
    client
        .action("GetPatientLedger")
        .get({
            "patientId": window.patient.PatientId,
            "orderId": window.patient.OrderId
        })
        .done(function (data) {
            if (data !== undefined && data !== null && data.LedgerOrderDetails !== null) {
                if (data.LedgerOrderDetails[0] !== null && data.LedgerOrderDetails[0].OrderLineItems !== null) {
                    patientLedgerTable.refreshDataTable(data.LedgerOrderDetails[0].OrderLineItems);
                }
            }
        });
}
function verifyAccess() {
    if (window.patient.Error !== undefined && window.patient.Error !== null && window.patient.Error.toString() !== '') {
        $("#btnReturn").prop("disabled", true);
        $(this).showSummaryMessage(msgType.SERVER_ERROR, window.patient.Error, true);
    }
}
$("#btnReturn").click(function (e) {
    e.preventDefault();
    if (window.patient.PatientId.toString() !== '' && window.patient.PatientId.toString() !== '0') {
        var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patient.PatientId + "&oid=0";
        window.location.href = redirectUrl;
    }
});

/*click handler for Patient Demographics button */
$("a[href]#ledgerScreenHeading").click(function (e) {
    e.preventDefault();
    var redirectUrl = window.config.baseUrl + $(this).attr("data-query");
    window.location.href = redirectUrl;
});

$("#btnPrintAtGlance").click(function (e) {
    e.preventDefault();
    $.printModal($("#LedgerScreen"));
});

$(document).ready(function () {
    $("#btnReturn").prop("disabled", false);
    buildPatientLedgerTable();
    verifyAccess();
    loadPatientLedgerTable();
});