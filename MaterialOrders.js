/*jslint nomen:true, plusplus:true, regexp: true, sloppy: true */
/*global $, document, window, alert, alslContactLenses, loadPatientSideNavigation, ApiClient, viewModel, unclosedDaysCount, msgType, unclosedDaysList, noReportData*/
(function () {
    function showUnclosedDayWarningMessage(warningMsg) {
        $("#msg_dailyClose").removeClass("hidden");
        $("#msg_dailyClose").html('<i class="icon-spam"></i>' + warningMsg);
    }
    function hideUnclosedDayWarningMessage() {
        $("#msg_dailyClose").addClass("hidden");
        $("#msg_dailyClose").html('');
    }
    function getSystemDate() {
        var today = new Date(), year = today.getFullYear(), month = today.getMonth() + 1, day = today.getDate();
        return month + "/" + day + "/" + year;
    }
    function updateOrdersPageTitle() {
        new ApiClient("Patient")
            .action("GetPatientById")
            .id(window.patient.PatientId)
            .get()
            .done(function (data) {
                var title = data.FirstName + " " + data.LastName;
                $('#orderScreenHeading').html(title);
                $('#menu_PatientName').html(title);
                document.title = "EPM: " + data.FirstName + " : " + window.documentTitle;
            })
            .fail(function () {
                $('#iFrameScheduler').attr('src', window.errorPage);
            })
            .always(function (xhr) {
                xhr.handled = true;
            });
    }
    function getPreviousUnclosedBusinessDaysInfo() {
        var warningMsg,
            postingDate,
            oid = window.patient.OrderId,
            client = new ApiClient("DailyClosing");
        client.action("GetPreviousUnclosedBusinessDaysInfo")
              .get({ "officeNumber": window.config.officeNumber })
              .done(function (data) {
                var ordersUrl = $('#iFrameScheduler').attr("data-url"),
                    iFrameUrl = $('#iFrameScheduler').attr("data-url");
                if (oid !== undefined && oid !== 0) {
                    ordersUrl = iFrameUrl.replace("Patient/PatientOrders", "OrderPricing") + "&OrderId=" + oid;
                }

                if (data.ActualUnclosedDaysCount > 1) {
                    ordersUrl = ordersUrl + "&DailyClosed=False";
                } else {
                    ordersUrl = ordersUrl + "&DailyClosed=True";
                }

                $('#iFrameScheduler').attr("src", ordersUrl);
                $('#iFrameScheduler').show();

                if (data.IsPaperPractice) {
                    return;
                }

                switch (data.ActualUnclosedDaysCount) {
                case 0:
                    $("#msg_dailyClose").addClass("hidden");
                    break;
                case 1:
                    //postingDate = Date.parse(data.PostingDate);
                    postingDate = Date.parse(data.UnclosedDaysList[0] || getSystemDate());
                    warningMsg = 'Daily Closing has not been performed for the last business day. New transactions ' +
                        'will post to ' + postingDate.toString('dddd, MMMM dd, yyyy') + ".";
                    if (window.config.dayCloseRole !== 'True') {
                        warningMsg += " Ask an Administrator to perform the Daily Closing before posting transactions.";
                    }
                    showUnclosedDayWarningMessage(warningMsg);
                    break;
                default:
                    warningMsg = "Multiple unclosed days are preventing further transactions.";
                    if (window.config.dayCloseRole !== 'True') {
                        warningMsg += "<br>Contact an Administrator to do a Daily Closing.";
                    } else {
                        warningMsg += " Close one or more days to post new transactions.";
                    }
                    showUnclosedDayWarningMessage(warningMsg);
                    break;
                }
                if (data.IsTodayClosed) {
                    //warningMsg = "The Daily Closing has already been performed for " + new Date().toString('dddd, MMMM dd, yyyy') + ". Undo Daily Closing to post new transactions.";
                    warningMsg = "The Daily Closing has already been performed for today. Reopen Daily Closing to post new transactions.";
                    showUnclosedDayWarningMessage(warningMsg);
                }
            })
            .fail(function (xhr) {
                if (xhr.status === 403) {
                    $(document).showSystemFailure("You do not have security permission to access this functionality/information.<br/><br/> "
                        + "Please contact your Office Manager or Office Administrator if you believe this is an error.");
                    $('#iFrameScheduler').attr('src', window.errorPage);
                } else {
                    var message = xhr.responseText.replace(/\"/g, "");
                    $(document).showSummaryMessage(msgType.SERVER_ERROR, message, true);
                }
            });
    }

    var wipOrdersTable, hasData = false;

    function buildTable() {
        wipOrdersTable = $('#wipOrdersTable').alslDataTable({
            "aaSorting": [],
            "aoColumns": [
                {
                    "sTitle": "Order Name",
                    "mData": "AwsResourceId",
                    "sType": "string",
                    "sClass": "left col-lg-3 col-md-3 col-sm-3 col-xs-3",
                    "bSortable": false
                },
                {
                    "sTitle": "",
                    "mData": "AwsResourceUri",
                    "sType": "string",
                    "bVisible": false
                },
                {
                    "sTitle": "",
                    "mData": "CompanyId",
                    "sType": "string",
                    "bVisible": false
                },
                {
                    "sTitle": "Date Created",
                    "mData": "CreatedDateTime",
                    "sType": "date",
                    "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                    "bSortable": false
                },
                {
                    "sTitle": "Expires On",
                    "mData": "ExpiresOn",
                    "sType": "date",
                    "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                    "bSortable": false
                },
                {
                    "sTitle": "Type",
                    "mData": "OrderType",
                    "sType": "string",
                    "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1",
                    "bSortable": false
                },
                {
                    "sTitle": "Insurance/Plan",
                    "mData": "InsuranceDisplay",
                    "sType": "string",
                    "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                    "bSortable": false
                },
                {
                    "sTitle": "Auth #",
                    "mData": "Auth",
                    "sType": "string",
                    "sClass": "left col-lg-1 col-md-1 col-sm-2 col-xs-1",
                    "bSortable": false
                },
                {
                    "sTitle": "Doctor",
                    "mData": "ResourceDisplay",
                    "sType": "string",
                    "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2",
                    "bSortable": false
                },
                {
                    "sTitle": "",
                    "mData": "AwsResourceId",
                    "sType": "string",
                    "sClass": "center col-lg-1 col-md-1 col-sm-1 col-xs-1",
                    "bSortable": false,
                    "mRender": function (data) {
                        return "<i class='btn icon-print no-margin' title='Print Draft Order' id='" + data + "' ></i>" + "<i class='btn icon-remove' title='Delete Draft Order' id='" + data + "' ></i>";
                    }
                }
            ],
            "bAutoWidth": false,
            "bPaginate": true,
            //"sPaginationType": "full_numbers",
            //"iDisplayLength": 3,
            "oLanguage": { "sEmptyTable": "There are no Draft Orders for this patient." },
            selectableRows: true,
            highlightRows: true
        });

        wipOrdersTable.delegate("tbody td:not(:last-child)", "click", function () {
            if (wipOrdersTable.fnPagingInfo().iTotal !== 0) {
                var aPos = wipOrdersTable.fnGetPosition(this),
                    sData = wipOrdersTable.fnGetData(aPos[0]);

                window.location.href = sData.OrderType === "Eyeglass" ? window.config.baseUrl + "Patient/EyeglassOrder?id=" + window.patient.PatientId + "&resourceId=" + sData.AwsResourceId + "&oId=0&update=yes" :
                        window.config.baseUrl + "Patient/ContactLensOrder?id=" + window.patient.PatientId + "&resourceId=" + sData.AwsResourceId + "&oId=0&update=yes";
            }
        });
    }

    /* click handler for Delete icon in grid */
    $('#wipOrdersTable').on("click", ".icon-remove", function (e) {
        e.preventDefault();
        var $this = $(this),
            delOrderId = $this.attr("id");

        $('#deleteOrderModal').data('delOrderId', delOrderId).modal({
            keyboard: false,
            backdrop: 'static',
            show: true
        });
    });

    /* click handler for Delete Order button */
    $('#btnConfirmDeleteOrder').click(function (e) {
        e.preventDefault();
        var delOrderId = $('#deleteOrderModal').data('delOrderId');
        new ApiClient("PatientOrder")
            .action("DeletePatientIpOrder")
            .queryStringParams({ resourceId: delOrderId, patientId: window.patient.PatientId })["delete"]()
            .done(function (data) {
                $(document).showSystemSuccess("Draft order deleted.");
                if (data !== undefined && data !== null) {
                    wipOrdersTable.refreshDataTable(data);
                }
            });
    });

    function printFromWip(resourceId) {
        var url, options;
        $.ajax({
            url: window.config.baseUrl + "PatientReports/PrintFromInProgressGrid?orderId=0&resourceId=" + resourceId + "&patientId=" + window.patient.PatientId,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                switch (data) {
                case "noData":
                    noReportData();
                    break;
                case "dataError":
                    $(this).showSummaryMessage(msgType.SERVER_ERROR, "There is a problem with the data for this report. Contact your system administrator.", true);
                    break;
                case "403":
                    options = {
                        data: null,
                        title: "No Data",
                        message: "You do not have security permission to access this area.",
                        buttons: ["", "OK"],
                        callback: null
                    };
                    $.messageDialog(options);
                    break;
                default:
                    if (data !== null && data !== undefined && data.toLowerCase().indexOf("loginform") !== -1) {
                        $(document).showSystemBlockerDialog("Session Timeout", "Your session has timed out.", function () {
                            window.location.href = window.config.baseUrl + "Login?" + $.param({
                                ReturnUrl: window.location.href
                            });
                        });
                    } else if (data !== null && data !== undefined && data.length > 0) {
                        url = window.config.baseUrl + "Reporting/PdfView?id=" + data;
                        window.open(url, "_blank", "height=800,width=1020,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
                    }
                    break;
                }
            },
            error: function (x, y, z) {
                alert("error: \n" + x + "\n" + y + "\n" + z);
            }
        });
    }

    $("#wipOrdersTable").on("click", ".icon-print", function (e) {
        e.preventDefault();
        var $this = $(this),
            id = $this.attr("id");
        printFromWip(id);
    });

    function setListener() {
        window.addEventListener('message', function (e) {
            var message = e.data;
            if (message === "hide ip Grid") {
                $("#inProgressOrders").removeClass("visible");
                $("#inProgressOrders").addClass("hidden");
            }

            if (message === "show ip Grid" && hasData === true) {
                $("#inProgressOrders").removeClass("hidden");
                $("#inProgressOrders").addClass("visible");
            }
        });
    }

    function loadInProgressTable() {
        new ApiClient("PatientOrder")
            .action("GetPatientWipOrdersList")
            .id(window.patient.PatientId)
            .get()
            .done(function (data) {
                buildTable();
                wipOrdersTable.refreshDataTable(data);
                if (data !== undefined && data !== null && data.length > 0) {
                    hasData = true;
                }

                setListener();
            })
            .fail(function () {
                $('#iFrameScheduler').attr('src', window.errorPage);
            })
            .always(function (xhr) {
                xhr.handled = true;
            });
    }

    /*click handler for Patient Demographics button */
    $("a[href]#orderScreenHeading").click(function (e) {
        e.preventDefault();
        var redirectUrl = window.config.baseUrl + $(this).attr("data-query");
        window.location.href = redirectUrl;
    });

    $(document).ready(function () {
        loadInProgressTable();
        updateOrdersPageTitle();
        getPreviousUnclosedBusinessDaysInfo();
    });
}());
