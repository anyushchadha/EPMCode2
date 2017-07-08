/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko,  Modernizr, console, msgType, loadPatientSideNavigation,ApiClient, getTodayDate, alert, buildOrder, eyeglassOrderViewModel */
var rxTable, rxTableInitialized = false, client = new ApiClient("EyeglassOrder");
var newRx;
var convertRx = {
    id: 0,
    click: false
};
/* Builds the Choose Rx table */
function showChangeRxModal() {
    $("#ChangeRxWarningModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
}

function isVisionTypeChanging(sData) {
    var result = false;
    if (window.eyeglassOrderViewModel !== null && window.eyeglassOrderViewModel.selectedRx() !== null && (eyeglassOrderViewModel.selectedLens() || eyeglassOrderViewModel.selectedExtras())) {
        var currentVisionType = window.eyeglassOrderViewModel.selectedRx().VisionType;
        var newVisionType = sData.VisionType;
        if (currentVisionType !== newVisionType) {
            result = true;
        }
    }

    return result;
}

function buildRxTable() {
    rxTable = $("#rxTable").alslDataTable({
        "iDisplayLength": 5,
        "aLengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "All"]],
        "aoColumns": [
            {
                "sTitle": "",
                "mData": "Id",
                "sType": "string",
                "bVisible": false
            },
            {
                "sTitle": "Type",
                "mData": "Id",
                "sType": "string",
                "sWidth": "29%",
                "mRender": function (data, type, row) {
                    var html = row.RxType + "<br/>";
                    var ruCond = row.RightUlCondition === null ? '' : row.RightUlCondition + " (R)";
                    var luCond = row.LeftUlCondition === null ? '' : row.LeftUlCondition + " (L)";
                    if (ruCond !== '' || luCond !== '') {
                        html += "<br/><div style='color: #c09853;'>";
                        html += "<i class='icon-warning' style='margin-left:-1px'></i><span class='message'>Underlying Condition:</span><br/>";
                        if (ruCond !== '') {
                            html += "<span class='message' style='margin-left:25px'>" + ruCond + "</span><br/>";
                        }
                        if (luCond !== '') {
                            html += "<span class='message' style='margin-left:25px'>" + luCond + "</span>";
                        }
                        html += "</div>";
                    }
                    var i, pattern = ["multifocal", "multi-focal", "multi focal"];
                    var str = row.RxType.toLowerCase();
                    for (i = 0; i < pattern.length; i++) {
                        if (str.indexOf(pattern[i]) >= 0) {
                            html += "<br/><a href='#' id='" + data + "' class='convertRx'>Convert Rx</a>";
                            break;
                        }
                    }
                    return html;
                }
            },
            {
                "sTitle": "",
                "mData": "Id",
                "sWidth": "1%",
                "mRender": function (data, type, row) {
                    var right = "R";
                    var left = "L";
                    if (row.RightPrismTxt !== '') {
                        right += "<br/>&nbsp;";
                    }
                    if (row.LeftPrismTxt !== '') {
                        left += "<br/>&nbsp;";
                    }
                    return right + "<br/>" + left;
                }
            },
            {
                "sTitle": "Sph",
                "mData": "RightSphereTxt",
                "sType": "string",
                "sWidth": "2%",
                "sClass": "center",
                "mRender": function (data, type, row) {
                    var rSphere = row.RightSphereTxt === '' ? '--' : row.RightSphereTxt;
                    var lSphere = row.LeftSphereTxt === '' ? '--' : row.LeftSphereTxt;
                    if (row.RightPrismTxt !== '') {
                        rSphere += "<br/>&nbsp;";
                    }
                    if (row.LeftPrismTxt !== '') {
                        lSphere += "<br/>&nbsp;";
                    }
                    return rSphere + "<br/>" + lSphere;
                }
            },
            {
                "sTitle": "Cyl",
                "mData": "RightCylinderTxt",
                "sType": "string",
                "sWidth": "2%",
                "sClass": "center",
                "mRender": function (data, type, row) {
                    var rCyl = row.RightCylinderTxt === '' ? '--' : row.RightCylinderTxt;
                    var lCyl = row.LeftCylinderTxt === '' ? '--' : row.LeftCylinderTxt;
                    if (row.RightPrismTxt !== '') {
                        rCyl += "<br/>&nbsp;";
                    }
                    if (row.LeftPrismTxt !== '') {
                        lCyl += "<br/>&nbsp;";
                    }
                    return rCyl + "<br/>" + lCyl;
                }
            },
            {
                "sTitle": "Axis",
                "mData": "RightAxisTxt",
                "sType": "string",
                "sWidth": "2%",
                "sClass": "center",
                "mRender": function (data, type, row) {
                    var rAxis = row.RightAxisTxt === '' ? '--' : row.RightAxisTxt;
                    var lAxis = row.LeftAxisTxt === '' ? '--' : row.LeftAxisTxt;
                    if (row.RightPrismTxt !== '') {
                        rAxis += "<br/>&nbsp;";
                    }
                    if (row.LeftPrismTxt !== '') {
                        lAxis += "<br/>&nbsp;";
                    }
                    return rAxis + "<br/>" + lAxis;
                }
            },
            {
                "sTitle": "Add",
                "mData": "RightAddTxt",
                "sType": "string",
                "sWidth": "3%",
                "sClass": "center",
                "mRender": function (data, type, row) {
                    var rAdd = row.RightAddTxt === '' ? '--' : row.RightAddTxt;
                    var lAdd = row.LeftAddTxt === '' ? '--' : row.LeftAddTxt;
                    if (row.RightPrismTxt !== '') {
                        rAdd += "<br/>&nbsp;";
                    }
                    if (row.LeftPrismTxt !== '') {
                        lAdd += "<br/>&nbsp;";
                    }
                    return rAdd + "<br/>" + lAdd;
                }
            },
            {
                "sTitle": "Prism",
                "mData": "RightPrismTxt",
                "sType": "string",
                "sWidth": "17%",
                "mRender": function (data, type, row) {
                    var v, rPrism, lPrism;
                    if (row.RightPrismTxt === '') {
                        rPrism = "&nbsp;";
                    } else {
                        v = row.RightPrismTxt.split('/');
                        if (v.length > 1) {
                            rPrism = v[0] + "<br/>" + v[1];
                        } else {
                            rPrism = v[0] + "<br/>" + "&nbsp;";
                        }
                    }
                    if (row.LeftPrismTxt === '') {
                        lPrism = "&nbsp;";
                    } else {
                        v = row.LeftPrismTxt.split('/');
                        if (v.length > 1) {
                            lPrism = v[0] + "<br/>" + v[1];
                        } else {
                            lPrism = v[0] + "<br/>" + "&nbsp;";
                        }
                    }
                    return rPrism + "<br/>" + lPrism;
                }
            },
            {
                "sTitle": "Provider",
                "mData": "DoctorName",
                "sType": "string",
                "sWidth": "12%",
                "mRender": function (data, type, row) {
                    if (row.IsOutsideDoctor) {
                        return row.DoctorName + "<br/>" + "(Outside Provider)";
                    }
                    return row.DoctorName;
                }
            },
            {
                "sTitle": "Date",
                "mData": "ExamDate",
                "sType": "string",
                "sWidth": "3%"
            },
            {
                "sTitle": "Expires",
                "mData": "ExpirationDate",
                "sType": "string",
                "sWidth": "3%",
                "mRender": function (data, type, row) {
                    var html = data;
                    var today = getTodayDate(0);
                    if (Date.parse(row.ExpirationDate) < Date.parse(today)) {
                        html += "<i class='icon-warning color-12 no-vertical-margin' title='Expired Rx'></i>";
                    }
                    return html;
                }
            },
            {
                "mData": "Id",
                "sType": "integer",
                "sWidth": "1%",
                "sClass": "center",
                "mRender": function (data, type, row) {
                    return "<i class='btn icon-print' style='width:9px;margin-left:-1px;' title='Print Rx' id='" + data + "'></i>";
                }
            }
        ],
        "bSort": false,
        "bAutoWidth": false,
        "bPaginate": true,
        "oLanguage": { "sEmptyTable": "There are no valid Eyeglass Lens Rx's for this patient." },
        selectableRows: true,
        highlightRows: true
    });

    $('#rxTable').on("click", ".icon-print", function (e) {
        e.preventDefault();
        var $this = $(this);
        var id = $this.attr("id");
        var url = window.config.baseUrl + 'PatientReports/PrintRxReport?examId=' + id + '&officeNum=' + window.config.officeNumber;
        window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
        return false;
    });

    $('#rxTable').on("click", ".convertRx", function (e) {
        e.preventDefault();
        var $this = $(this);
        convertRx.click = true;
        convertRx.id = $this.attr("id");
        $("#rdReading").prop('checked', true);
        $("#convertRxModal").modal({
            keyboard: false,
            backdrop: 'static',
            show: true
        });
    });

    function verifyAndUpdateRx(rxData) {
        // if there is an already selected rx compare its vision type to new selections vision type 
        var isChanged = isVisionTypeChanging(rxData);
        if (isChanged === false) {
            $("#chooseRx").addClass("hidden");
            window.updateSelectedRx(rxData);
            window.loadExtrasAndUpdateLens();
        } else {
            var extras = window.eyeglassOrderViewModel.selectedExtras();
            var showModal = false;
            if (extras === null || extras === undefined || extras.length === 0) {
                $("#extrasLabel").addClass("hidden");
            } else {
                $("#extrasLabel").removeClass("hidden");
                showModal = true;
            }

            var lens = window.eyeglassOrderViewModel.selectedLens();
            if (lens === null || lens === undefined) {
                $("#lensesLabel").addClass("hidden");
            } else {
                $("#lensesLabel").removeClass("hidden");
                showModal = true;
            }

            if (showModal) {
                showChangeRxModal();
            } else {
                $("#chooseRx").addClass("hidden");
                window.updateSelectedRx(rxData);
                window.loadExtrasAndUpdateLens();
            }

            newRx = rxData;
        }
    }

    $("#btnConvertRx").click(function (e) {
        e.preventDefault();
        convertRx.click = false;
        var type = $("#rdReading").prop('checked') ? window.rxConversionType.READING : window.rxConversionType.DISTANCE;
        $("#convertRxModal").modal("toggle");
        client
            .action("GetConvertedEyeglassRxDetailsById")
            .get({ patientId: window.patientOrderExam.PatientId, examId: convertRx.id, conversionType: type })
            .done(function (convertedRxDetails) {
                if (convertedRxDetails !== undefined && convertedRxDetails !== null) {
                    verifyAndUpdateRx(convertedRxDetails);
                }
            })
            .fail(function () {
                alert("Patient Exam Not Found.");
            });
    });

    rxTable.on("click", "tbody td", function () {
        if (convertRx.click === true) {
            convertRx.click = false;
            return;
        }
        var aPos, sData;
        if (rxTable.fnPagingInfo().iTotal !== 0) {
            aPos = rxTable.fnGetPosition(this);
            sData = rxTable.fnGetData(aPos[0]);
            verifyAndUpdateRx(sData);
        }
    });
}

/* Click Event Handler for the "Incomplete Rxs Found" message on the Choose Rx page */
$("div#msg_IncompleteEgRxs").click(function () {
    var redirectUrl = window.config.baseUrl + "Patient/Rx?id=" + window.patientOrderExam.PatientId;
    window.location.href = redirectUrl;
});

/* click event for btnCancelRxChange */
$("#btnCancelRxChange").click(function (e) {
    e.preventDefault();
    $("#ChangeRxWarningModal").modal("hide");
});

/* click event for btnConfirmRxChange */
$("#btnConfirmRxChange").click(function (e) {
    e.preventDefault();
    $("#ChangeRxWarningModal").modal("hide");
    $("#chooseRx").addClass("hidden");
    window.clearLensExtras();
    window.clearLenses();
    window.updateSelectedEntity(window.buildOrder.LENSES, null);
    window.setupPanelHeader(window.buildOrder.LENSES);
    window.updateSelectedRx(newRx);
    $("#summary").addClass("hidden");
    window.initChooseBuildOrderPage();
});

/* Checks for Incomplete Rxs and shows message if any found */
function checkForIncompleteEyeglassRx() {
    client
        .action("GetIncompleteRxByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (iData) {
            if (iData && iData === true) {
                $("div#msg_IncompleteEgRxs").removeClass("hidden");
            }
        });
}

/* Gets the Rx Data */
function loadRxData() {
    client
        .action("GetAllEyeglassRxDetailsByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (rxData) {
            if (rxData !== undefined && rxData !== null) {
                rxTable.refreshDataTable(rxData);
                rxTable.fnDraw();
            }

            // check for incomplete Rxs
            checkForIncompleteEyeglassRx();
        })
        .fail(function () {
            alert("GetAllEyeglassRxDetailsByPatientId failed.");
        });
}

/* Initialize the Choose Rx page (and Right-hand Insurance panel) */
var initChooseEyeglassRxPage = function () {
    window.setupPage();
    $("#btnSaveForLater").addClass("hidden");
    $("#btnContinue").addClass("hidden");
    $("#btnContinueToPricing").addClass("hidden");
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Choose Rx");
    $("#chooseEgRxForm").alslValidate({
        onfocusout: false,
        onclick: false
    });

    //build the rx table if it hasn't been built yet
    if (!rxTableInitialized) {
        rxTableInitialized = true;
        buildRxTable();
        loadRxData();
    }
    // show the page
    $("#chooseRx").removeClass("hidden");
    window.scrollTo(0, 0);
};
