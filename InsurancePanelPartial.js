/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko,  Modernizr, console, msgType, loadPatientSideNavigation,ApiClient, initInsurancePanel, InsPanelType, noReportData, alert */
var InsPanelViewModel,
    InsPanelType = {
        CONTACT: 1,
        FRAME: 2
    },
    LINE_ITEM_TOTAL = 10000;
var detailsTable;

var buildDetailsTable = function () {
    detailsTable = $('#detailsTable').alslDataTable({
        "aoColumns": [{
            "sTitle": "Item",
            "mData": "ItemName",
            "sType": "string",
            "sWidth": "65%",
            "sClass": "left",
            "mRender": function (data, type, row) {
                return row.Index === 0 ? "<span>" + data + "</span>" : "<span><b>" + data + "</b></span>";
            }
        }, {
            "sTitle": "Qty",
            "mData": "Quantity",
            "sType": "integer",
            "sWidth": "5%",
            "sClass": "right",
            "mRender": function (data, type, row) {
                return data === 0 ? "<span></span>" : row.Index === 0 ? "<span>" + data + "</span>" : "<span><b>" + data + "</b></span>";
            }
        }, {
            "sTitle": "Retail",
            "mData": "RetailPrice",
            "sType": "string",
            "sWidth": "10%",
            "sClass": "right",
            "mRender": function (data, type, row) {
                var str, amount = 0;
                if (row.ItemName === "Copay") {
                    str = "<span><b>N/A</b></span>";
                } else if (row.ItemName === "Total") {
                    amount = parseFloat(data);
                    str = row.Index === 0 ? "<span>$" + amount.toFixed(2) + "</span>" : "<span><b>$" + amount.toFixed(2) + "</b></span>";
                } else {
                    amount = row.InsuranceAllowancePrimary + row.PatientDisplayAmount;
                    str = row.Index === 0 ? "<span>$" + amount.toFixed(2) + "</span>" : "<span><b>$" + amount.toFixed(2) + "</b></span>";
                }
                return str;
            }
        }, {
            "sTitle": "Primary Insurance",
            "mData": "InsuranceAllowancePrimary",
            "sType": "string",
            "sWidth": "10%",
            "sClass": "right",
            "mRender": function (data, type, row) {
                if (row.ItemName === "Copay") {
                    return "<span><b>N/A</b></span>";
                }
                return row.Index === 0 ? "<span>$" + data.toFixed(2) + "</span>" : "<span><b>$" + data.toFixed(2) + "</b></span>";
            }
        }, {
            "sTitle": "Secondary Insurance",
            "mData": "InsuranceAllowanceSecondary",
            "sType": "string",
            "sWidth": "10%",
            "sClass": "right",
            "bVisible": false,
            "mRender": function (data, type, row) {
                if (row.ItemName === "Copay") {
                    return "<span><b>N/A</b></span>";
                }
                return row.Index === 0 ? "<span>$" + data.toFixed(2) + "</span>" : "<span><b>$" + data.toFixed(2) + "</b></span>";
            }
        }, {
            "sTitle": "Patient",
            "mData": "PatientDisplayAmount",
            "sType": "string",
            "sWidth": "10%",
            "sClass": "right",
            "mRender": function (data, type, row) {
                if (row.ItemName === "Copay") {
                    return "<span><b>$" + row.PatientCopay.toFixed(2) + "</b></span>";
                }
                return row.Index === 0 ? "<span>$" + data.toFixed(2) + "</span>" : "<span><b>$" + data.toFixed(2) + "</b></span>";
            }
        }],
        "bAutoWidth": false,
        "bSort": false,
        "bPaginate": false,
        "bInfo": false
    });
};

var InsurancePanelViewModel = function () {
    var self = this;
    self.PrimaryEligibility = ko.observable();
    self.SecondaryEligibility = ko.observable();
    self.Items = ko.observableArray();
    self.DetailItems = ko.observableArray();
    self.IsEstimated = ko.observable(true);
    self.Dummy = ko.observable();
    self.InfoText = ko.computed(function () {
        var retVal = "";
        var v = self.Dummy();
        if (self.PrimaryEligibility() !== undefined && self.PrimaryEligibility() !== null) {
            if (self.PrimaryEligibility().IsVsp()) {
                retVal = "Calculations below are estimates only.";
            } else if (self.PrimaryEligibility().IsNonInsurance()) {
                retVal = "Calculations below reflect retail price.";
            } else {
                retVal = "Calculations below reflect retail price.";
            }
        } else {
            retVal = "Calculations below reflect retail price.";
        }
        return retVal;
    });

    self.BesError = ko.observable();
};

/* Eligibility Item */
var Eligibility = function (data) {
    this.CarrierFull = ko.observable(data.CarrierFull || "");
    this.CarrierShort = ko.observable(data.CarrierShort || "");
    this.CarrierId = ko.observable(data.CarrierId || "");
    this.InsEligibilityId = ko.observable(data.InsuranceEligibilityId || "");
    this.PlanId = ko.observable(data.PlanId || "");
    this.AuthNumber = ko.observable(data.Eligibility ? data.Eligibility.AuthNumber : data.AuthNumber);
    this.AuthDate = ko.observable(data.Eligibility ? data.Eligibility.AuthDate : data.AuthDate);
    this.AuthExpireDate = ko.observable(data.Eligibility ? data.Eligibility.AuthExpireDate : data.AuthExpireDate);
    this.IsExamElig = ko.observable(data.Eligibility ? data.Eligibility.IsExamElig : data.IsExamElig);
    this.IsFrameElig = ko.observable(data.Eligibility ? data.Eligibility.IsFrameElig : data.IsFrameElig);
    this.IsLensElig = ko.observable(data.Eligibility ? data.Eligibility.IsLensElig : data.IsLensElig);
    this.IsClElig = ko.observable(data.Eligibility ? data.Eligibility.IsClElig : data.IsClElig);
    this.IsClFitElig = ko.observable(data.Eligibility ? data.Eligibility.IsClFitElig : data.IsClFitElig);
    this.IsNonInsurance = ko.observable(data.NonInsurance);
    this.IsVsp = ko.observable(data.IsVsp);
};

/* Pricing Line Item */
var LineItem = function (data) {
    this.ItemId = ko.observable(data.itemId);
    this.SortId = ko.observable(data.sortId);
    this.Title = ko.observable(data.title);
    this.Quantity = ko.observable(data.quantity);
    this.RetailPrice = ko.observable(data.retailPrice || 0);
    this.RetailPriceFormatted = $.formatMoney(this.RetailPrice);
    this.PrimaryInsurancePrice = ko.observable(data.primaryInsurancePrice || 0);
    this.PrimaryInsurancePriceFormatted = $.formatMoney(this.PrimaryInsurancePrice);
    this.SecondaryInsurancePrice = ko.observable(data.secondaryInsurancePrice || 0);
    this.SecondaryInsurancePriceFormatted = $.formatMoney(this.SecondaryInsurancePrice);
    this.PatientPrice = ko.observable(data.patientPrice || 0);
    this.PatientPriceFormatted = $.formatMoney(this.PatientPrice);
    this.IsHeader = ko.observable(data.isHeader || false);
};

/* Updates the Totals Row */
function updateTotalsRow() {
    var retailPrice = 0,
        primaryInsurancePrice = 0,
        secondaryInsurancePrice = 0,
        patientPrice = 0,
        totalsRow = null;

    ko.utils.arrayForEach(InsPanelViewModel.Items(), function (item) {
        if (item.ItemId() !== LINE_ITEM_TOTAL) {
            retailPrice += parseFloat(item.RetailPrice());
            primaryInsurancePrice += item.PrimaryInsurancePrice() === "N/A" ? 0 : parseFloat(item.PrimaryInsurancePrice());
            secondaryInsurancePrice += parseFloat(item.SecondaryInsurancePrice());
            patientPrice += parseFloat(item.PatientPrice());
        } else {
            totalsRow = item;
        }
    });

    totalsRow.RetailPrice(retailPrice);
    totalsRow.PrimaryInsurancePrice(primaryInsurancePrice);
    totalsRow.SecondaryInsurancePrice(secondaryInsurancePrice);
    totalsRow.PatientPrice(patientPrice);
}

/* Sets the Primary Eligibility */
function setPrimaryEligibility(data) {
    InsPanelViewModel.PrimaryEligibility(new Eligibility(data));
}

/* Replaces the Secondary Eligibility */
function setSecondaryEligibility(data) {
    InsPanelViewModel.SecondaryEligibility(new Eligibility(data));
}

function removeSecondaryEligibility() {
    InsPanelViewModel.SecondaryEligibility(null);
}

function updateServerError(message) {
    if (message !== '' && $("#NotifyError").hasClass("hidden")) {
        $("#ErrorText").removeClass('hidden');
    }
    InsPanelViewModel.BesError(message);
}
function clearInsurancePricingPanelDetailItems() {
    InsPanelViewModel.DetailItems.removeAll();
}

$("#btnPrintEstCharges").click(function (e) {
    var data = InsPanelViewModel.DetailItems();
    var list = [];
    var eligItem = {
        Title: InsPanelViewModel.PrimaryEligibility().InsEligibilityId()
    };
    list.push(eligItem);
    data.forEach(function (itemCharge) {
        var item = {
            ItemId: itemCharge.ItemId(),
            SortId: itemCharge.SortId(),
            Title: itemCharge.Title(),
            Quantity: itemCharge.Quantity(),
            RetailPriceFormatted: itemCharge.RetailPriceFormatted(),
            PrimaryInsurancePriceFormatted: itemCharge.PrimaryInsurancePriceFormatted(),
            SecondaryInsurancePriceFormatted: itemCharge.SecondaryInsurancePriceFormatted(),
            PatientPriceFormatted: itemCharge.PatientPriceFormatted(),
            IsHeader: itemCharge.IsHeader()
        };
        list.push(item);
    });

    var id = window.patient.PatientId;

    $.ajax({
        url: window.config.baseUrl + "PatientReports/PrintEgOrderEstimatedCharges?id="  + id,
        data: JSON.stringify(list),
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
                var options = {
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
                    var url = window.config.baseUrl + "Reporting/PdfView?id=" + data;
                    window.open(url, "_blank", "height=800,width=1020,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
                }
                break;
            }
        },
        error: function (x, y, z) {
            alert("error: \n" + x + "\n" + y + "\n" + z);
        }
    });
});

function updateInsurancePricingDetails(data) {
    detailsTable.refreshDataTable(data);
    var secondaryInsuranceColumn = 4;
    if (InsPanelViewModel.SecondaryEligibility() !== undefined && InsPanelViewModel.SecondaryEligibility() !== null) {
        detailsTable.fnSetColumnVis(secondaryInsuranceColumn, true);
    } else {
        detailsTable.fnSetColumnVis(secondaryInsuranceColumn, false);
    }

    if (data.length >= 10) {
        $("#note").css({ 'margin-top': '-40px' });
    } else {
        $("#note").css({ 'margin-top': '0px' });
    }
    InsPanelViewModel.DetailItems.removeAll();
    if (data !== null) {
        var patientCopay = 0;
        data.forEach(function (itemCharge) {
            // EG Pricing
            window.addPricingLineItemToDetailItems({
                itemId: itemCharge.ItemId,
                sortId: 10,
                title: itemCharge.ItemName,
                retailPrice: itemCharge.ItemName === "Copay" ? "N/A" : itemCharge.RetailPrice,
                quantity: itemCharge.ItemName === "Copay" ? "N/A" : itemCharge.Quantity,
                primaryInsurancePrice: itemCharge.ItemName === "Copay" ? "N/A" : itemCharge.InsuranceAllowancePrimary,
                secondaryInsurancePrice: itemCharge.ItemName === "Copay" ? "N/A" : itemCharge.InsuranceAllowanceSecondary,
                patientPrice: itemCharge.ItemName === "Copay" ? itemCharge.PatientCopay : itemCharge.PatientDisplayAmount,
                isHeader: itemCharge.Index === 1 || itemCharge.ItemName === "Total" || itemCharge.ItemName === "Copay"
            });

            if (itemCharge.Index === 1) {
                window.updateOrAddPricingLineItem({
                    itemId: itemCharge.ItemId,
                    sortId: 10,
                    title: itemCharge.ItemName,
                    retailPrice: itemCharge.RetailPrice,
                    primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                    secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                    patientPrice: itemCharge.PatientDisplayAmount
                });
                // keep a running total for all Copay charges
                patientCopay += itemCharge.PatientCopay;
            }
        });
        // Copay
        window.updateOrAddPricingLineItem({
            itemId: 0,
            sortId: 100,
            title: "Copay",
            primaryInsurancePrice: "N/A",
            patientPrice: patientCopay
        });
    }
}

function getSecondaryEligibility() {
    if (InsPanelViewModel.SecondaryEligibility() !== undefined && InsPanelViewModel.SecondaryEligibility() !== null) {
        var secondary = {
            IsPrimaryInsurance: false,
            IsSecondaryInsurance: true,
            CarrierFull: window.InsPanelViewModel.SecondaryEligibility().CarrierFull(),
            AuthNumber: window.InsPanelViewModel.SecondaryEligibility().AuthNumber(),
            EligId: window.InsPanelViewModel.SecondaryEligibility().InsEligibilityId(),
            AuthDate: window.InsPanelViewModel.SecondaryEligibility().AuthDate(),
            AuthExpireDate: window.InsPanelViewModel.SecondaryEligibility().AuthExpireDate(),
            IsExamElig: window.InsPanelViewModel.SecondaryEligibility().IsExamElig(),
            IsFrameElig: window.InsPanelViewModel.SecondaryEligibility().IsFrameElig(),
            IsLensElig: window.InsPanelViewModel.SecondaryEligibility().IsLensElig(),
            IsClElig: window.InsPanelViewModel.SecondaryEligibility().IsClElig(),
            IsClFitElig: window.InsPanelViewModel.SecondaryEligibility().IsClFitElig(),
            IsVsp: window.InsPanelViewModel.SecondaryEligibility().IsVsp()
        };
        return secondary;
    }
    return null;
}

function getPrimaryEligibility() {
    if (InsPanelViewModel.PrimaryEligibility() !== undefined && InsPanelViewModel.PrimaryEligibility() !== null) {
        var primary = {
            IsPrimaryInsurance: true,
            IsSecondaryInsurance: false,
            CarrierFull: window.InsPanelViewModel.PrimaryEligibility().CarrierFull(),
            AuthNumber: window.InsPanelViewModel.PrimaryEligibility().AuthNumber(),
            EligId: window.InsPanelViewModel.PrimaryEligibility().InsEligibilityId(),
            AuthDate: window.InsPanelViewModel.PrimaryEligibility().AuthDate(),
            AuthExpireDate: window.InsPanelViewModel.PrimaryEligibility().AuthExpireDate(),
            IsExamElig: window.InsPanelViewModel.PrimaryEligibility().IsExamElig(),
            IsFrameElig: window.InsPanelViewModel.PrimaryEligibility().IsFrameElig(),
            IsLensElig: window.InsPanelViewModel.PrimaryEligibility().IsLensElig(),
            IsClElig: window.InsPanelViewModel.PrimaryEligibility().IsClElig(),
            IsClFitElig: window.InsPanelViewModel.PrimaryEligibility().IsClFitElig(),
            IsVsp: window.InsPanelViewModel.PrimaryEligibility().IsVsp()
        };
        return primary;
    }
    return null;
}

function addPricingLineItemToDetailItems(data) {
    InsPanelViewModel.DetailItems.push(new LineItem(data));
}

/* Adds a Pricing Line Item */
function addPricingLineItem(data) {
    InsPanelViewModel.Items.push(new LineItem(data));
    InsPanelViewModel.Items.sort(function (left, right) {
        return left.SortId() === right.SortId() ? 0 : (left.SortId() < right.SortId() ? -1 : 1);
    });
    updateTotalsRow();
}

/* Updates an existing Pricing Line Item or adds if not found*/
function updateOrAddPricingLineItem(data) {
    var lineItem = null;

    // match the itemId
    if (!isNaN(data.itemId)) {
        ko.utils.arrayForEach(InsPanelViewModel.Items(), function (item) {
            if (item.ItemId() === data.itemId) {
                lineItem = item;
            }
        });
    }

    // update the attributes if a match was found, add a new item if not found
    if (lineItem !== null && lineItem !== undefined) {
        if (data.title && data.title.length > 0) {
            lineItem.Title(data.title);
        }

        if (!isNaN(data.retailPrice)) {
            lineItem.RetailPrice(data.retailPrice);
        }

        if (!isNaN(data.primaryInsurancePrice)) {
            if (data.primaryInsurancePrice === "N/A") {
                lineItem.PrimaryInsurancePrice(0);
            } else {
                lineItem.PrimaryInsurancePrice(data.primaryInsurancePrice);
            }
        }

        if (!isNaN(data.secondaryInsurancePrice)) {
            lineItem.SecondaryInsurancePrice(data.secondaryInsurancePrice);
        }

        if (!isNaN(data.patientPrice)) {
            lineItem.PatientPrice(data.patientPrice);
        }
    } else {
        addPricingLineItem({
            itemId: data.itemId,
            sortId: data.sortId,
            title: data.title,
            retailPrice: data.retailPrice,
            primaryInsurancePrice: data.primaryInsurancePrice,
            secondaryInsurancePrice: data.secondaryInsurancePrice,
            patientPrice: data.patientPrice
        });
    }
    updateTotalsRow();
}

/* Removes a Pricing Line Item by its ID */
function removePricingLineItemById(itemId) {
    InsPanelViewModel.Items.remove(function (item) {
        return item.ItemId() === itemId;
    });
    updateTotalsRow();
}

/* Removes all Pricing Line Items */
function removeAllPricingLineItems() {
    InsPanelViewModel.Items.removeAll();
    addPricingLineItem({
        itemId: LINE_ITEM_TOTAL,
        sortId: LINE_ITEM_TOTAL,
        title: "Total"
    });
}

/* Initializes the Insurance Panel */
function initInsurancePanel(mode, elig) {
    InsPanelViewModel = new InsurancePanelViewModel();

    // add an initial insurance if there is one
    if (elig) {
        setPrimaryEligibility(elig);
    }

    // add a default line items 
    addPricingLineItem({
        itemId: LINE_ITEM_TOTAL,
        sortId: LINE_ITEM_TOTAL,
        title: "Total"
    });

    if (mode === "EyeglassOrder") {
        buildDetailsTable();
    }

    ko.applyBindings(InsPanelViewModel, $('#insurancePanel')[0]);
}

function changeInsurancePanel(mode) {
    InsPanelViewModel.Dummy.notifySubscribers();
}

function ChargeItem(title, retail, primaryIns, secondaryIns, patientAmt) {
    this.Title = title;
    this.Retail = retail;
    this.PrimaryIns = primaryIns;
    this.SecondaryIns = secondaryIns;
    this.PatientAmt = patientAmt;
}