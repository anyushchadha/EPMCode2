/*jslint browser: true, vars: true, plusplus: true, regexp: true */
/*global $, document, window, ko,  Modernizr, alert, console, msgType, loadPatientSideNavigation, ApiClient, InsPanelType, regex, convertDate, noReportData, ChargeItem, getTodayDate, initSummaryPage */
var client = new ApiClient("ContactLensOrder"),
    EligViewModel,
    RxViewModel,
    summaryViewModel,
    initChooseInsurancePage,
    initChooseRxPage,
    initSummaryPageFromTempStorage,
    initSummaryPageFromOrder,
    updateContactLensInsurancePanelFromCLOrder,
    updateContactLensInsurancePanelFromBESResponse,
    rxTable,
    rxTableInitialized = false,
    chooseInsuranceInitialized = false,
    summaryInitialized = false,
    selectedValues = {
        authorization: null,
        rx: null,
        insuranceCharges: null
    },
    rxData,
    validateTempOrderName,
    resetTempModal,
    priceVm,
    dataSource,
    providerName,
    clType,
    insEligId,
    addressList = null,
    redirect = false,
    chooseRxDirtyFlag = false,
    chooseInsuranceDirtyFlag = false,
    initLoad = true;

/* returns a details (collapsible) contact lens Rx row */
function fnFormatDetails(data) {
    if (data.ContactLens === undefined || data.ContactLens === null) {
        return "";
    }
    var leftQtyId = "leftQty_" + data.Id;
    var rightQtyId = "rightQty_" + data.Id;
    var sOut = "<div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "    <div class='row ClDetailRow'>" +
                "         <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "             <div class='col-lg-9 col-md-9 col-sm-9 col-xs-8'>" +
                "                   <h3>Right Lens</h3>" +
                "             </div>" +
                "             <div class='col-lg-2 col-md-2 col-sm-2 col-xs-2 control-label' style='margin-top:15px;'>" +
                "                   <span class='required'>*</span>Qty" +
                "             </div>" +
                "             <div class='col-lg-1 col-md-1 col-sm-1 col-xs-2' style='margin-top:15px; padding-right:0;'>" +
                "                   <input type='number' class='requiredField form-control integerFilter' id='" + rightQtyId + "' name='" + rightQtyId + "' data-text='" + data.ContactLens.RightQuantity + "' value='" + data.ContactLens.RightQuantity + "'/>" +
                "             </div>" +
                "         </div>" +
                "         <div class='col-lg-5 col-lg-offset-7 col-md-5 col-md-offset-7 col-sm-5 col-sm-offset-7 col-xs-5 col-xs-offset-7 fieldMessages'></div>" +
                "         <div class='clearfix'></div>";

    if (data.ContactLens.RightUlCondition !== null) {
        sOut += "        <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "           <div id='msg_UlCondition' class='alert alert-info messageCenterStyle' data-selectable='false' data-url-type='external' data-url=''>" +
                "               <span class='title'><i class='icon-info'></i><strong>Underlying Condition: " + data.ContactLens.RightUlCondition + "</strong></span>" +
                "           </div>" +
                "        </div>";
    }

    sOut += "         <div class='col-lg-5 col-md-5 col-sm-5 col-xs-12'>" +
                "             <div class='clDetailPanel panel'>" +
                "               <div class=''>" +
                "                  <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Supplier</div>" +
                "                  <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.RightSupplier || "--") + "</div>" +
                "                </div>" +
                "              <div class='clearfix'></div>" +
                "              <div class=''>" +
                "                  <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Mfr.</div>" +
                "                  <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.RightManufacturer || "--") + "</div>" +
                "              </div>" +
                "              <div class='clearfix'></div>" +
                "              <div class=''>" +
                "                   <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Style</div>" +
                "                   <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.RightStyle || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                   <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Color</div>" +
                "                   <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.RightColor || "--") + "</div>" +
                "               </div>" +
                "             </div>" +
                "         </div>" +
                "         <div class='col-lg-4 col-md-4 col-sm-4 col-xs-6'>" +
                "            <div class='clDetailPanel panel'>" +
                "               <div class=''>" +
                "                    <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Base Curve</div>" +
                "                    <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.RightBase || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                    <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Diameter</div>" +
                "                    <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.RightDiameter || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Sphere</div>" +
                "                  <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.RightSphere || "--") + "</div>" +
                "               </div>" +
                "           </div>" +
                "         </div>" +
                "         <div class='col-lg-3 col-md-3 col-sm-3 col-xs-6'>" +
                "             <div class='clDetailPanel panel'>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Cyl.</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.RightCylinder || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Axis</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.RightAxis || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Add</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.RightAdd || "--") + "</div>" +
                "               </div>" +
                "            </div>" +
                "       </div>" +
                "    </div>" +
                "    <div class='clearfix'></div>" +
                "    <div class='row ClDetailRow'>" +
                "         <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "             <div class='col-lg-9 col-md-9 col-sm-9 col-xs-8'>" +
                "                   <h3>Left Lens</h3>" +
                "             </div>" +
                "             <div class='col-lg-2 col-md-2 col-sm-2 col-xs-2 control-label' style='margin-top:15px;'>" +
                "                   <span class='required'>*</span>Qty" +
                "             </div>" +
                "             <div class='col-lg-1 col-md-1 col-sm-1 col-xs-2' style='margin-top:15px; padding-right:0;'>" +
                "                   <input type='number' class='requiredField form-control integerFilter' id='" + leftQtyId + "' name='" + leftQtyId + "' data-text='" + data.ContactLens.LeftQuantity + "' value='" + data.ContactLens.LeftQuantity + "'/>" +
                "             </div>" +
                "         </div>" +
                "         <div class='col-lg-5 col-lg-offset-7 col-md-5 col-md-offset-7 col-sm-5 col-sm-offset-7 col-xs-5 col-xs-offset-7 fieldMessages'></div>" +
                "         <div class='clearfix'></div>";

    if (data.ContactLens.LeftUlCondition !== null) {
        sOut += "       <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "           <div id='msg_UlCondition' class='alert alert-info messageCenterStyle' data-selectable='false' data-url-type='external' data-url=''>" +
                "               <span class='title'><i class='icon-info'></i><strong>Underlying Condition: " + data.ContactLens.LeftUlCondition + "</strong></span>" +
                "           </div>" +
                "       </div>";
    }

    sOut += "         <div class='col-lg-5 col-md-5 col-sm-5 col-xs-12'>" +
                "             <div class='clDetailPanel panel'>" +
                "              <div class=''>" +
                "                  <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Supplier</div>" +
                "                  <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.LeftSupplier || "--") + "</div>" +
                "              </div>" +
                "              <div class='clearfix'></div>" +
                "              <div class=''>" +
                "                  <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Mfr.</div>" +
                "                  <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.LeftManufacturer || "--") + "</div>" +
                "              </div>" +
                "              <div class='clearfix'></div>" +
                "              <div class=''>" +
                "                   <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Style</div>" +
                "                   <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.LeftStyle || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                   <div class='col-lg-3 col-md-3 col-sm-3 col-xs-3 control-label'>Color</div>" +
                "                   <div class='col-lg-9 col-md-9 col-sm-9 col-xs-9 form-control-static'>" + (data.ContactLens.LeftColor || "--") + "</div>" +
                "               </div>" +
                "             </div>" +
                "         </div>" +
                "         <div class='col-lg-4 col-md-4 col-sm-4 col-xs-6'>" +
                "            <div class='clDetailPanel panel'>" +
                "               <div class=''>" +
                "                    <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Base Curve</div>" +
                "                    <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.LeftBase || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                    <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Diameter</div>" +
                "                    <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.LeftDiameter || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-7 col-sm-6 col-xs-6 control-label'>Sphere</div>" +
                "                  <div class='col-lg-6 col-md-5 col-sm-6 col-xs-6 form-control-static'>" + (data.ContactLens.LeftSphere || "--") + "</div>" +
                "               </div>" +
                "           </div>" +
                "         </div>" +
                "         <div class='col-lg-3 col-md-3 col-sm-3 col-xs-6'>" +
                "             <div class='clDetailPanel panel'>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Cyl.</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.LeftCylinder || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Axis</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.LeftAxis || "--") + "</div>" +
                "               </div>" +
                "               <div class='clearfix'></div>" +
                "               <div class=''>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-5 col-xs-6 control-label'>Add</div>" +
                "                  <div class='col-lg-6 col-md-6 col-sm-7 col-xs-6 form-control-static'>" + (data.ContactLens.LeftAdd || "--") + "</div>" +
                "               </div>" +
                "            </div>" +
                "       </div>" +
                "    </div>" +
                "    <div class='clearfix'></div>" +
                "<div class='row ClDetailPriceRow'>" +
                "        <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "           <div id='msg_ALOI' class='alert alert-danger messageCenterStyle hidden' data-selectable='false' data-url-type='external' data-url=''>" +
                "              <span class='title'><i class='icon-spam'></i><strong>Contact Lens Not Available</strong></span>" +
                "              <span class='message'></span>" +
                "           </div>" +
                "        </div>" +
                "       <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "           <div id='msg_NonPricedItem' class='alert alert-warning messageCenterStyle hidden' data-selectable='true' data-url-type='external' data-url=''>" +
                "               <span class='title'><i class='icon-warning'></i><strong>Contact Lens Is Not Priced</strong></span>" +
                "               <span class='message'>You can continue the order, however you may be required to set a price before invoicing.  Or, <strong><u>Click Here</u></strong> to price now.</span>" +
                "           </div>" +
                "       </div>" +
                "       <div class='col-lg-12 col-md-12 col-sm-12 col-xs-12'>" +
                "           <button id='btnPrintRx' class='btn btn-default' data-rxid='" + data.Id + "'><i class='icon-print'></i>Print Rx</button>" +
                "           <button id='btnAddRxToOrder' class='btn btn-primary pull-right' data-rxid='" + data.Id + "'><i class='icon-plus'></i>Add To Order</button>" +
                "       </div>" +
                "    </div>" +
                "</div>";
    return sOut;
}

function compileCharges() {
    var items = [];
    ko.utils.arrayForEach(window.InsPanelViewModel.Items(), function (v) {
        var item = new ChargeItem();
        item.Title = v.Title();
        item.Retail = v.RetailPriceFormatted();
        item.PrimaryIns = v.PrimaryInsurancePriceFormatted();
        item.SecondaryIns = v.SecondaryInsurancePriceFormatted();
        item.PatientAmt = v.PatientPriceFormatted();
        items.push(item);
    });

    return items;
}

/* Gets the list of shipping addresses */
function getAddressList(shipTo) {
    if (shipTo === undefined || shipTo <= 0) {
        summaryViewModel.addressList(null);
        $("select#address").selectpicker("refresh");
        return;
    }
    client
        .action("GetAllAddressList")
        .get({
            shipToType: shipTo,
            patientId: window.patientOrderExam.PatientId
        })
        .done(function (data) {
            summaryViewModel.addressList(data);
            addressList = data;
            $("select#address").selectpicker("refresh");
        });
}

/* Summary View Model */
var ContactLensOrderViewModel = function (clData) {
    var self = this;

    self.isDirty = ko.observable(false);
    self.address = ko.observable(clData.ServiceLocationId);
    self.addressList = ko.observableArray(clData.AddressList);
    self.contactLens = ko.observable(clData.ContactLens);
    self.dispenseType = ko.observable(clData.DispenseType);
    self.dispenseTypeList = ko.observableArray(clData.DispenseTypeList);
    self.dispenseNote = ko.observable(clData.DispenseNote);
    self.doctor = ko.observable(clData.DoctorFullName);
    self.doctorId = ko.observable(clData.DoctorId);
    self.eligibilityId = ko.observable(clData.InsuranceEligibilityId);
    self.insuranceVM = ko.observable(window.InsPanelViewModel);
    self.isOutsideDoctor = ko.observable(clData.IsOutsideDoctor);
    self.labInstructions = ko.observable(clData.LabInstructions);
    self.officeNum = ko.observable(clData.OfficeNum);
    self.orderNumber = ko.observable(clData.OrderNumber);
    self.orderType = ko.observable(clData.OrderType);
    self.patientExamId = ko.observable(clData.PatientExamId);
    self.remakeOrder = ko.observable(clData.RemakeOrder);
    self.remakeTypeId = ko.observable(clData.RemakeTypeId);
    self.remake = ko.observable(clData.Remake);
    self.remakeReasons = ko.observableArray(clData.RemakeReasons);
    self.rxDate = ko.observable(clData.RxDate);
    self.shipTo = ko.observable(clData.ShipToType);
    self.shipToList = ko.observableArray(clData.ShipToList);
    self.staff = ko.observable(null);
    self.staff = ko.observable(clData.EmployeeId);
    self.staffList = ko.observableArray(clData.EmployeeList);
    self.suppliedBy = ko.observable(clData.SupplyType);
    self.suppliedByList = ko.observableArray(clData.SuppliedByList);
    self.toSupplyRightLens = ko.observable(clData.ToSupplyRightLens);
    self.toSupplyLeftLens = ko.observable(clData.ToSupplyLeftLens);

    addressList = clData.AddressList;

    /* handle UI styling for required fields */
    self.address.subscribe(function () {
        if (self.address() > 0) {
            $("#address").parents("div.bootstrap-select").removeClass("requiredField");
            $("#address").clearField();
        } else {
            $("#address").clearField();
            $("#address").parents("div.bootstrap-select").addClass("requiredField");
        }
    });

    self.dispenseType.subscribe(function () {
        if (self.dispenseType() > 0) {
            $("#dispensingStatus").parents("div.bootstrap-select").removeClass("requiredField");
            $("#dispensingStatus").clearField();
        } else {
            $("#dispensingStatus").clearField();
            $("#dispensingStatus").parents("div.bootstrap-select").addClass("requiredField");
        }
    });

    self.staff.subscribe(function () {
        if (self.staff() > 0) {
            $("#staff").parents("div.bootstrap-select").removeClass("requiredField");
            $("#staff").clearField();
        } else {
            $("#staff").clearField();
            $("#staff").parents("div.bootstrap-select").addClass("requiredField");
        }
    });

    self.suppliedBy.subscribe(function () {
        if (self.suppliedBy() > 0) {
            $("#suppliedBy").parents("div.bootstrap-select").removeClass("requiredField");
            $("#suppliedBy").clearField();
        } else {
            $("#suppliedBy").clearField();
            $("#suppliedBy").parents("div.bootstrap-select").addClass("requiredField");
        }
    });

    self.shipTo.subscribe(function () {
        getAddressList(self.shipTo());

        if (self.shipTo() > 0) {
            $("#shipTo").parents("div.bootstrap-select").removeClass("requiredField");
            $("#shipTo").clearField();
            $("#address").parents("div.bootstrap-select").removeClass("requiredField");
            $("#address").clearField();
        } else {
            $("#shipTo").clearField();
            $("#shipTo").parents("div.bootstrap-select").addClass("requiredField");
            $("#address").clearField();
            $("#address").parents("div.bootstrap-select").addClass("requiredField");
        }
    });

    // expand dispensing note textarea if we have notes
    if (self.dispenseNote() && self.dispenseNote().length > 0) {
        $("#linkAddDispensingNotes").click();
    }

    // expand lab instructions textarea if we have instructions
    if (self.labInstructions() && self.labInstructions().length > 0) {
        $("#linkAddInstructions").click();
    }
};

/* Set all fields on the Summary screen as Read Only (disabled)*/
function makeSumPageReadOnly() {
    $("#summary :input").prop("disabled", true);
    $("#summary :input").removeClass("requiredField");
    $("#remakeIns").addClass("hidden");

    $("#btnSummaryChangeInsurance, #btnSummaryChangeHardClRx, #btnCancelOrder, #btnContinueToPricing").removeClass("visible").addClass("hidden");
    $("#btnReturnToMaterialOrders, #btnSave").removeClass("hidden").addClass("visible");
    $("#btnPrintSummary, #btnReturnToMaterialOrders, #btnSave, #instructions").removeAttr("disabled");
    $("#instructions").focus();
}

/* Sets the Dirty Flag when any input changes on the Summary screen */
function setDirtyFlag() {
    $("#summary :input").change(function () {
        summaryViewModel.isDirty(true);
    });
    $("#summary select").change(function () {
        summaryViewModel.isDirty(true);
    });
}

/* Event handlers for the Summary Screen "Change" buttons */
function configChangeButtons() {
    setDirtyFlag();
    $("select").selectpicker("refresh");

    /* Click event handler for "Change Insurance" button */
    $("#btnSummaryChangeInsurance").click(function (e) {
        e.preventDefault();

        // if the order has applied insurance details, Goto pricing screen, otherwise Goto the Choose Insurance screen
        if (((window.patientOrderExam.OrderId !== null && window.patientOrderExam.OrderId !== undefined && window.patientOrderExam.OrderId > 0)
                || (summaryViewModel.orderNumber() !== null && summaryViewModel.orderNumber() !== undefined && summaryViewModel.orderNumber() > 0))
                && (summaryViewModel.remake() || (summaryViewModel.insuranceVM().PrimaryEligibility() && summaryViewModel.insuranceVM().IsEstimated() === false))) {
            $("#btnContinueToPricing").click();
        } else {
            if (dataSource !== "new" && chooseInsuranceInitialized !== true) {
                initChooseInsurancePage();
                $("#summary").addClass("hidden");
            } else {
                $("#chooseInsurance").removeClass("hidden");
                $("#summary").addClass("hidden");
            }
        }
    });

    /* Click event handler for "Change Rx" button */
    $("#btnSummaryChangeSoftClRx, #btnSummaryChangeHardClRx").click(function (e) {
        e.preventDefault();
        $("#summary").addClass("hidden");
        if (dataSource !== "new") {
            initChooseRxPage();
        }
        $("#chooseRx").removeClass("hidden");
    });
}

/* Validates the summary page */
function validateClSummary() {
    $("#clOrderSummaryForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            staff: {
                required: true
            },
            suppliedBy: {
                required: true
            },
            dispensingStatus: {
                required: true
            },
            shipTo: {
                required: true
            },
            address: {
                required: true
            }
        },
        messages: {
            staff: {
                required: "Select a Resource."
            },
            suppliedBy: {
                required: "Select a Supplied By."
            },
            dispensingStatus: {
                required: "Select a Dispensing Status"
            },
            shipTo: {
                required: "Select a ShipTo type."
            },
            address: {
                required: "Select an Address."
            }
        }
    });
}

/* Initializes the UI from Temporary Storage */
initSummaryPageFromTempStorage = function () {
    $("#summary").removeClass("hidden");
    validateClSummary();
    client
        .action("GetInProgressContactLensOrder")
        .get({
            patientId: window.patientOrderExam.PatientId,
            awsResourceId: window.patientOrderExam.AwsResourceId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                summaryInitialized = true;
                selectedValues.rx = data;

                if (data.PatientExamId === 0) {
                    alert("There is no Rx for this order. Please delete it from In-Progress Orders");
                    redirect = true;
                    var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=0";
                    window.location.href = redirectUrl;
                } else {
                    // show the soft or hard CL panel
                    if (data.OrderType.indexOf("S") !== -1) {
                        data.OrderType = "Soft CL";
                        clType = "Soft";
                        $("#softContactLensesPanel").removeClass("hidden");
                        $("#hardContactLensesPanel").addClass("hidden");
                    } else {
                        data.OrderType = "Hard CL";
                        clType = "Hard";
                        $("#hardContactLensesPanel").removeClass("hidden");
                        $("#softContactLensesPanel").addClass("hidden");
                    }

                    // initialize viewmodels
                    if (!window.InsPanelViewModel) {
                        window.initInsurancePanel("ContactLensOrder");
                    }
                    summaryViewModel = new ContactLensOrderViewModel(data);
                    ko.applyBindings(summaryViewModel, $('#summary')[0]);

                    if (data.InsurancePricing !== undefined && data.InsurancePricing !== null) {
                        updateContactLensInsurancePanelFromCLOrder(data, summaryViewModel.contactLens().LeftItemId, summaryViewModel.contactLens().RightItemId);
                    }
                    summaryViewModel.isDirty(false);
                }
            }

            configChangeButtons();
        });
};

/* Initializes the UI from a Saved Order */
initSummaryPageFromOrder = function () {
    $("#summary").removeClass("hidden");
    validateClSummary();
    if (summaryInitialized) {
        return;
    }
    client
        .action("GetContactLensOrderDetail")
        .get({
            patientId: window.patientOrderExam.PatientId,
            orderNumber: window.patientOrderExam.OrderId,
            orderType: ""
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                summaryInitialized = true;
                selectedValues.rx = data;

                // show the soft or hard CL panel
                if (data.OrderType === "S") {
                    data.OrderType = "Soft CL";
                    clType = "Soft";
                    $("#softContactLensesPanel").removeClass("hidden");
                    $("#hardContactLensesPanel").addClass("hidden");
                } else {
                    data.OrderType = "Hard CL";
                    clType = "Hard";
                    $("#hardContactLensesPanel").removeClass("hidden");
                    $("#softContactLensesPanel").addClass("hidden");
                }

                // initialize viewmodels
                if (!window.InsPanelViewModel) {
                    window.initInsurancePanel("ContactLensOrder");
                }
                summaryViewModel = new ContactLensOrderViewModel(data);
                ko.applyBindings(summaryViewModel, $('#summary')[0]);

                if (data.IsInvoiced) {
                    makeSumPageReadOnly();
                } else if (summaryViewModel.address() > 0) {
                    setTimeout(function () { $("#address").parents("div.bootstrap-select").removeClass("requiredField"); }, 100);
                }

                // build the Pricing Grid
                updateContactLensInsurancePanelFromCLOrder(data, summaryViewModel.contactLens().LeftItemId, summaryViewModel.contactLens().RightItemId);
                summaryViewModel.isDirty(false);
            }

            configChangeButtons();
        });
};

/* Creates an Order object from the ViewModel */
function getContactLensOrderDetailsFromViewModel() {
    var ct;
    var ot;
    if (clType.indexOf("Hard") !== -1) {
        ct = 1;
        ot = "H";
    } else {
        ct = 0;
        ot = "S";
    }

    var eligId = 0;
    if (selectedValues.authorization !== null) {
        eligId = selectedValues.authorization.InsuranceEligibilityId;
    } else if (summaryViewModel.orderNumber() !== null && summaryViewModel.orderNumber() > 0) {
        eligId = summaryViewModel.eligibilityId();
    }

    var itemCharges = compileCharges();
    var order = {
        PatientId: window.patientOrderExam.PatientId,
        AwsResourceId: window.patientOrderExam.AwsResourceId,
        EmployeeId: summaryViewModel.staff(),
        OfficeNum: summaryViewModel.officeNum(),
        DoctorId: summaryViewModel.doctorId(),
        DoctorFullName: summaryViewModel.doctor(),
        IsOutsideDoctor: summaryViewModel.isOutsideDoctor(),
        ContactLensType: ct,
        OrderType: ot,
        RxDate: summaryViewModel.rxDate(),
        Remake: summaryViewModel.remake(),
        ToSupplyRightLens: summaryViewModel.toSupplyRightLens(),
        ToSupplyLeftLens: summaryViewModel.toSupplyLeftLens(),
        RemakeOrder: summaryViewModel.remakeOrder(),
        RemakeTypeId: summaryViewModel.remakeTypeId(),
        SupplyType: summaryViewModel.suppliedBy(),
        LabInstructions: summaryViewModel.labInstructions(),
        DispenseType: summaryViewModel.dispenseType(),
        DispenseNote: summaryViewModel.dispenseNote(),
        ShipToType: summaryViewModel.shipTo(),
        ShipTo: summaryViewModel.address(),
        ServiceLocationId: summaryViewModel.address(),
        PatientExamId: summaryViewModel.patientExamId(),
        OrderNumber: summaryViewModel.orderNumber(),
        InsuranceEligibilityId: eligId,
        ContactLens: summaryViewModel.contactLens(),
        InsurancePricing: {
            IsEstimated: window.InsPanelViewModel.IsEstimated(),
            PrimaryInsurance: window.getPrimaryEligibility(),
            SecondaryInsurance: window.getSecondaryEligibility()
        },
        ItemCharges: itemCharges
    };
    return order;
}

function getBESEstimatesForSummaryPage(leftLensId, leftQty, rightLensId, rightQty, eligibilityId, doctorId) {
    if (leftQty !== undefined && leftQty !== "" && rightQty !== undefined && rightQty !== "") {
        client
            .action("GetBesResponseForContactLensOrder")
            .get({
                patientId: window.patientOrderExam.PatientId,
                leftLensId: leftLensId,
                leftLensQuantity: leftQty,
                rightLensId: rightLensId,
                rightLensQuantity: rightQty,
                eligibilityId: eligibilityId,
                doctorId: doctorId
            })
            .done(function (data) {
                selectedValues.insuranceCharges = data.besResponse;
                window.removeAllPricingLineItems();

                if (data) {
                    updateContactLensInsurancePanelFromBESResponse(data.besResponse, data.serverErrorMessage, leftLensId, rightLensId, rightQty, leftQty);
                }
            });
    }
}

/* Updates the Insurance Pricing Panel */
function updatePricingPanel(rowData, rightQty1, leftQty1) {
    window.removeAllPricingLineItems();
    if (rowData) {
        var name = '';
        var rightQty = parseInt(rightQty1, 10);
        var leftQty = parseInt(leftQty1, 10);
        if (rowData.ContactLens.LeftItemId === rowData.ContactLens.RightItemId) {
            name = "Lens (Both)";
            if (rightQty === 0 && leftQty > 0) {
                name = "Lens (L)";
            } else if (rightQty > 0 && leftQty === 0) {
                name = "Lens (R)";
            }
            window.updateOrAddPricingLineItem({
                itemId: rowData.ContactLens.LeftItemId,
                sortId: 10,
                title: name,
                retailPrice: (rowData.ContactLens.Price.RetailLeft * leftQty) + (rowData.ContactLens.Price.RetailRight * rightQty),
                patientPrice: (rowData.ContactLens.Price.RetailLeft * leftQty) + (rowData.ContactLens.Price.RetailRight * rightQty)
            });
        } else {
            if (rightQty > 0) {
                window.updateOrAddPricingLineItem({
                    itemId: rowData.ContactLens.RightItemId,
                    sortId: 10,
                    title: "Lens (R)",
                    retailPrice: rowData.ContactLens.Price.RetailRight * rightQty,
                    patientPrice: rowData.ContactLens.Price.RetailRight * rightQty
                });
            }
            if (leftQty > 0) {
                window.updateOrAddPricingLineItem({
                    itemId: rowData.ContactLens.LeftItemId,
                    sortId: 20,
                    title: "Lens (L)",
                    retailPrice: rowData.ContactLens.Price.RetailLeft * leftQty,
                    patientPrice: rowData.ContactLens.Price.RetailLeft * leftQty
                });
            }
        }
    }
}

function updateSuppliedByList(orderType) {
    if (orderType !== undefined && orderType !== null) {
        client
            .action("GetAllSuppliedByList")
            .get({
                orderType: orderType
            })
            .done(function (data) {
                summaryViewModel.suppliedByList(data);
                $("select#suppliedBy").selectpicker("refresh");
            });
    }
}

/* Updates the Summary page Rx */
function updateSummaryPageWithSelectedRx(selectedRx, eligId, leftQty, rightQty) {
    if (selectedRx !== undefined && selectedRx !== null) {
        summaryViewModel.patientExamId(selectedRx.Id);
        summaryViewModel.doctor(selectedRx.DoctorName);
        summaryViewModel.doctorId(selectedRx.DoctorId);
        summaryViewModel.isOutsideDoctor(selectedRx.IsOutsideDoctor);
        summaryViewModel.rxDate(selectedRx.ExamDate);
        providerName = selectedRx.DoctorName;
        clType = selectedRx.ClType;
        var oldClType = summaryViewModel.orderType();
        if (clType === "Soft") {
            summaryViewModel.orderType("Soft CL");
            $("#softContactLensesPanel").removeClass("hidden");
            $("#hardContactLensesPanel").addClass("hidden");
        } else {
            summaryViewModel.orderType("Hard CL");
            $("#softContactLensesPanel").addClass("hidden");
            $("#hardContactLensesPanel").removeClass("hidden");
        }

        if (oldClType !== undefined && oldClType !== null && oldClType !== summaryViewModel.orderType()) {
            updateSuppliedByList(summaryViewModel.orderType());
        }

        selectedRx.ContactLens.RightQuantity = rightQty;
        selectedRx.ContactLens.LeftQuantity = leftQty;
        if (rightQty > 0) {
            summaryViewModel.toSupplyRightLens(true);
        } else {
            summaryViewModel.toSupplyRightLens(false);
        }
        if (leftQty > 0) {
            summaryViewModel.toSupplyLeftLens(true);
        } else {
            summaryViewModel.toSupplyLeftLens(false);
        }
        summaryViewModel.contactLens(selectedRx.ContactLens);
        summaryViewModel.isDirty(true);
    }
    if (eligId !== undefined && eligId !== null) {
        summaryViewModel.eligibilityId(eligId);
        summaryViewModel.isDirty(true);
    }
}

function updateSummaryEstimatedCharges() {
    var data = selectedValues.rx;
    if (selectedValues.authorization.IsVsp) {
        var eligId = selectedValues.authorization.Eligibility.InsuranceEligibilityId;
        getBESEstimatesForSummaryPage(data.ContactLens.LeftItemId, data.ContactLens.LeftQuantity, data.ContactLens.RightItemId, data.ContactLens.RightQuantity, eligId, data.DoctorId);
    } else {
        window.updateServerError("");
        updatePricingPanel(data, data.ContactLens.RightQuantity, data.ContactLens.LeftQuantity);
    }
    if (!window.InsPanelViewModel) {
        window.initInsurancePanel("ContactLensOrder", selectedValues.authorization);
    } else {
        window.setPrimaryEligibility(selectedValues.authorization);
    }
    initSummaryPage(null, insEligId, 0, 0);
}

/* Initializes the Summary page */
function initSummaryPage(selectedRx, eligId, leftQty, rightQty) {

    // show the insurance info
    $("#summary").removeClass("hidden");

    validateClSummary();

    if (summaryInitialized) {
        updateSummaryPageWithSelectedRx(selectedRx, eligId, leftQty, rightQty);
        return;
    }

    client
        .action("GetContactLensOrderDetail")
        .get({
            patientId: window.patientOrderExam.PatientId,
            orderNumber: window.patientOrderExam.OrderId,
            orderType: selectedRx.ClType
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                summaryInitialized = true;

                //init the viewModel, then apply the KO bindings
                summaryViewModel = new ContactLensOrderViewModel(data);
                ko.applyBindings(summaryViewModel, $('#summary')[0]);

                // Render page in Read-Only for invoiced exams
                if (data.IsInvoiced) {
                    makeSumPageReadOnly();
                }

                updateSummaryPageWithSelectedRx(selectedRx, eligId, leftQty, rightQty);
                summaryViewModel.isDirty(false);

                // fire off the change event to update the UI styling for these inputs
                $("#suppliedBy, #dispensingStatus, #shipTo, #address").change();

                updateSummaryEstimatedCharges();
            }

            configChangeButtons();
        });
}

/* ALOI Validation */
function validateSelectedRx(examId, successCallbackFunction) {
    var valid = true,
        addToOrderBtn = "button#btnAddRxToOrder";
    client
        .action("ValidateContactLensOrderRx")
        .get({ patientExamId: examId })
        .done(function (cdata) {
            if (cdata !== undefined && cdata !== null) {
                valid = cdata.isValid;

                if (valid === false || valid === "false") {
                    $("#msg_ALOI .message").html(cdata.serverErrorMessage);
                    $("#msg_ALOI").removeClass("hidden");
                    $(addToOrderBtn).prop("disabled", true);
                } else {
                    $("#msg_ALOI").addClass("hidden");
                    $(addToOrderBtn).prop("disabled", false);

                    if (successCallbackFunction) {
                        successCallbackFunction();
                    }
                }
            }
        })
        .fail(function () {
            $("#msg_ALOI").addClass("hidden");
            $(addToOrderBtn).prop("disabled", false);
        });
}

/* Scrubs the details for UI.  Sometimes the rowData shows a Mfr or Style, etc. even if there is no lens because of an underlying condition*/
function scrubForUi(row) {
    if (row.ContactLens.LeftUlConditionId === "60" || row.ContactLens.LeftUlConditionId === "61" || row.ContactLens.LeftUlConditionId === "62"
            || row.ContactLens.LeftUlConditionId === "63") {
        row.ContactLens.LeftSupplier = "N/A";
        row.ContactLens.LeftManufacturer = "N/A";
        row.ContactLens.LeftStyle = "N/A";
        row.ContactLens.LeftSphere = "N/A";
        row.ContactLens.LeftSphereId = "N/A";
        row.ContactLens.LeftCylinder = "N/A";
        row.ContactLens.LeftCylinderId = "N/A";
        row.ContactLens.LeftAxis = "N/A";
        row.ContactLens.LeftAxisId = "N/A";
        row.ContactLens.LeftAdd = "N/A";
        row.ContactLens.LeftAddId = "N/A";
        row.ContactLens.LeftBase = "N/A";
        row.ContactLens.LeftDiameter = "N/A";
        row.ContactLens.LeftColor = "N/A";

        row.ContactLens.LeftPcRadius = "N/A";
        row.ContactLens.LeftPcWidth = "N/A";
        row.ContactLens.LeftBaseCurve2 = "N/A";
        row.ContactLens.LeftSphere2 = "N/A";
        row.ContactLens.LeftCylinder2 = "N/A";
        row.ContactLens.LeftAxis2 = "N/A";
        row.ContactLens.LeftRadius2 = "N/A";
        row.ContactLens.LeftWidth2 = "N/A";
        row.ContactLens.LeftPrism = "N/A";
        row.ContactLens.LeftCt = "N/A";
        row.ContactLens.LeftEt = "N/A";
        row.ContactLens.LeftOpticalZone = "N/A";
        row.ContactLens.LeftRadius3 = "N/A";
        row.ContactLens.LeftWidth3 = "N/A";
        row.ContactLens.LeftSegHeight = "N/A";
        row.ContactLens.LeftBlend = "N/A";
        row.ContactLens.LeftTint = "N/A";
    }

    if (row.ContactLens.RightUlConditionId === "60" || row.ContactLens.RightUlConditionId === "61" || row.ContactLens.RightUlConditionId === "62"
            || row.ContactLens.RightUlConditionId === "63") {
        row.ContactLens.RightSupplier = "N/A";
        row.ContactLens.RightManufacturer = "N/A";
        row.ContactLens.RightStyle = "N/A";
        row.ContactLens.RightSphere = "N/A";
        row.ContactLens.RightSphereId = "N/A";
        row.ContactLens.RightCylinder = "N/A";
        row.ContactLens.RightCylinderId = "N/A";
        row.ContactLens.RightAxis = "N/A";
        row.ContactLens.RightAxisId = "N/A";
        row.ContactLens.RightAdd = "N/A";
        row.ContactLens.RightAddId = "N/A";
        row.ContactLens.RightBase = "N/A";
        row.ContactLens.RightDiameter = "N/A";
        row.ContactLens.RightColor = "N/A";

        row.ContactLens.RightPcRadius = "N/A";
        row.ContactLens.RightPcWidth = "N/A";
        row.ContactLens.RightBaseCurve2 = "N/A";
        row.ContactLens.RightSphere2 = "N/A";
        row.ContactLens.RightCylinder2 = "N/A";
        row.ContactLens.RightAxis2 = "N/A";
        row.ContactLens.RightRadius2 = "N/A";
        row.ContactLens.RightWidth2 = "N/A";
        row.ContactLens.RightPrism = "N/A";
        row.ContactLens.RightCt = "N/A";
        row.ContactLens.RightEt = "N/A";
        row.ContactLens.RightOpticalZone = "N/A";
        row.ContactLens.RightRadius3 = "N/A";
        row.ContactLens.RightWidth3 = "N/A";
        row.ContactLens.RightSegHeight = "N/A";
        row.ContactLens.RightBlend = "N/A";
        row.ContactLens.RightTint = "N/A";
    }

    if (row.ContactLens.LeftUlConditionId === "64") {
        row.ContactLens.LeftCylinder = "N/A";
        row.ContactLens.LeftCylinderId = "N/A";
        row.ContactLens.LeftAxis = "N/A";
        row.ContactLens.LeftAxisId = "N/A";
        row.ContactLens.LeftAdd = "N/A";
        row.ContactLens.LeftAddId = "N/A";
    }

    if (row.ContactLens.RightUlConditionId === "64") {
        row.ContactLens.RightCylinder = "N/A";
        row.ContactLens.RightCylinderId = "N/A";
        row.ContactLens.RightAxis = "N/A";
        row.ContactLens.RightAxisId = "N/A";
        row.ContactLens.RightAdd = "N/A";
        row.ContactLens.RightAddId = "N/A";
    }
    return row;
}

/* Resets the on-demand pricing modal shown on the Choose Rx page */
var resetPricingModal = function () {
    priceVm = null;
    $("#leftCLPricingPanel").addClass("hidden");
    $("#rightCLPricingPanel").addClass("hidden");
    $("#retailPriceLeft").val("");
    $("#retailPriceRight").val("");
    $("#clNameLeft").html("");
    $("#clNameRight").html("");
    $("label[for='clNameRight'").html("Right Contact Lens");
};

/* Validation for the right price input on the On-Demand pricing modal*/
var validatePriceRight = function () {
    $("#saveClPriceRight").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            retailPriceRight: {
                required: false,
                maxlength: 8,
                nonZeroMoney: true,
                Regex: window.regex.MONEY
            }
        },
        messages: {
            retailPriceRight: {
                required: "",
                maxlength: "Price can not be larger than $9,999.99",
                nonZeroMoney: "Please enter a valid, non-zero currency amount",
                Regex: "Please enter a valid currency amount"
            }
        }
    });
};

/* Validation for the left price input on the On-Demand pricing modal*/
var validatePriceLeft = function () {
    $("#saveClPriceLeft").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            retailPriceLeft: {
                required: false,
                maxlength: 8,
                nonZeroMoney: true
            }
        },
        messages: {
            retailPriceLeft: {
                required: "",
                maxlength: "Price can not be larger than $9,999.99",
                nonZeroMoney: "Please enter a valid, non-zero currency amount"
            }
        }
    });
};

/* Shows the On-Demand pricing modal */
function showPricingDiag(rowData) {
    resetPricingModal();
    var title = "Contact Lens Price ";
    $("#clPricing .modal-title").html(title);
    $("#clPricing").data("id", 0).modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });

    client
        .action("GetContactLensPrice")
        .get({
            patientId: window.patientOrderExam.PatientId,
            itemIdLeft: rowData.ContactLens.LeftItemId,
            itemIdRight: rowData.ContactLens.RightItemId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                priceVm = data;
                $("#retailPriceLeft").val(data.RetailDisplayLeft);
                $("#retailPriceRight").val(data.RetailDisplayRight);
                $("#clNameLeft").html(data.NameLeft);
                $("#clNameRight").html(data.NameRight);

                if (data.ItemIdLeft === data.ItemIdRight) {
                    // same L/R items
                    validatePriceRight();
                    $("#leftCLPricingPanel").addClass("hidden");
                    $("#rightCLPricingPanel").removeClass("hidden");
                    $("label[for='clNameRight'").html("Left &amp; Right Contact Lens");
                    $("#clNameRight").html(data.NameRight);

                    if (data.AllowZeroPriceLeft === true && data.AllowZeroPriceRight === true) {
                        $("#retailPriceRight").rules("add", { nonZeroMoney: false });
                    } else {
                        $("#retailPriceRight").rules("add", { nonZeroMoney: true });
                    }

                } else {
                    // different L/R items
                    // show right
                    if (data.ItemIdRight !== 0 && (data.RetailRight === null || data.RetailRight === 0)) {
                        validatePriceRight();
                        $("#rightCLPricingPanel").removeClass("hidden");
                        if (data.AllowZeroPriceRight === true) {
                            $("#retailPriceRight").rules("add", { nonZeroMoney: false });
                        } else {
                            $("#retailPriceRight").rules("add", { nonZeroMoney: true });
                        }
                    } else {
                        $("#rightCLPricingPanel").addClass("hidden");
                    }

                    // show left
                    if (data.ItemIdLeft !== 0 && (data.RetailLeft === null || data.RetailLeft === 0)) {
                        validatePriceLeft();
                        $("#leftCLPricingPanel").removeClass("hidden");
                        if (data.AllowZeroPriceLeft === true) {
                            $("#retailPriceLeft").rules("add", { nonZeroMoney: false });
                        } else {
                            $("#retailPriceLeft").rules("add", { nonZeroMoney: true });
                        }
                    } else {
                        $("#leftCLPricingPanel").addClass("hidden");
                    }
                }
            }
        });
}

/* Builds the Choose Rx table */
function buildRxTable() {
    rxTable = $('#rxTable').alslDataTable({
        "aaSorting": [],
        "aoColumns": [
            {
                "sTitle": "",
                "mData": "Id",
                "sType": "string",
                "bVisible": false,
                "sWidth": "0%"
            },
            {
                "sTitle": "Rx Type",
                "mData": "RxType",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-2 nowrap",
                "mRender": function (data) {
                    return "<i class='btn icon-spinner-arrow-right' title='View Details'></i>" + data;
                },
                "bSortable": false
            },
            {
                "sTitle": "Type",
                "mData": "ClType",
                "sType": "string",
                "sClass": "left col-lg-1 col-md-1 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "Description",
                "mData": "ClDescription",
                "sType": "string",
                "sClass": "left col-lg-2 col-md-4 col-sm-3 col-xs-3",
                "bSortable": false
            },
            {
                "sTitle": "Provider",
                "mData": "DoctorName",
                "sType": "string",
                "sClass": "left col-lg-2 hidden-md hidden-sm hidden-xs",
                "bSortable": false
            },
            {
                "sTitle": "Rx Date",
                "mData": "ExamDate",
                "sType": "string",
                "sClass": "left col-lg-1 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false
            },
            {
                "sTitle": "Expiration",
                "mData": "ExpirationDate",
                "sType": "string",
                "sClass": "left col-lg-1 col-md-2 col-sm-2 col-xs-2",
                "bSortable": false,
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
                "mData": "ContactLens",
                "bVisible": false
            }
        ],
        "bAutoWidth": false,
        "bPaginate": false,
        "oLanguage": { "sEmptyTable": "There are no valid Contact Lens Rx's for this patient." },
        selectableRows: true,
        highlightRows: true
    });

    // Add event listener for opening and closing details
    /*jslint nomen: true*/
    $("#rxTable").delegate("tbody td", "click", function () {
        if (rxTable.fnPagingInfo().iTotal !== 0) {
            if (this.parentNode._DT_RowIndex !== undefined) {
                $('i.icon-spinner-arrow-down', this.closest("tr")).attr('class', 'btn icon-spinner-arrow-right');
                var allRows = $(this).parents('table').find('tr.odd, tr.even');
                var nTr = $(this).parents('tr')[0];
                var aPos = rxTable.fnGetPosition(this);
                var rx = rxTable.fnGetData(aPos[0]);

                $("#divCalcInsurance, #calculations, #infoText").addClass("hidden");

                if (rxTable.fnIsOpen(nTr)) {
                    /* if the row was already open, close it and reset the pricing panel to just a blank 'Total' row */
                    rxTable.fnClose(nTr);
                    $('td', nTr.closest("tr")).removeClass('highlighted');

                    // reset the pricing panel 
                    updatePricingPanel(null, 0, 0);
                    selectedValues.rx = null;
                } else {
                    /* if the row was closed, close all the rest of the rows, and opent this one, and set the pricing panel */
                    $.each(allRows, function (idx, item) {
                        rxTable.fnClose(item);
                        $('i.icon-spinner-arrow-down', item.closest("tr")).attr('class', 'btn icon-spinner-arrow-right');
                        $('td', item.closest("tr")).removeClass('highlighted');
                    });

                    // highlight this row, change the icon, and open it
                    $('i.icon-spinner-arrow-right', this.closest("tr")).attr('class', 'btn icon-spinner-arrow-down');
                    $('td', this.closest("tr")).addClass('highlighted');

                    client
                        .action("GetContactLensVm")
                        .get({ examId: rx.Id })
                        .done(function (data) {
                            rx.ContactLens = data.contactLensVm;
                            var rowData = scrubForUi(rx);

                            var leftQtyId = "#leftQty_" + rowData.Id;
                            var rightQtyId = "#rightQty_" + rowData.Id;

                            rxTable.fnOpen(nTr, fnFormatDetails(rowData), 'details');

                            // show the 'Non-Price Item' message if applicable
                            if ((!rowData.ContactLens.LeftHasPrice && rowData.ContactLens.LeftUlCondition === null) ||
                                    (!rowData.ContactLens.RightHasPrice && rowData.ContactLens.RightUlCondition === null)) {
                                $("#msg_NonPricedItem").removeClass("hidden");
                            } else {
                                $("#msg_NonPricedItem").addClass("hidden");
                            }

                            //underlying condition: Plano
                            var req = parseInt(rowData.ContactLens.LeftUlConditionId, 10) === 64 ? 'true' : 'false';
                            $(leftQtyId).attr('data-value', req);
                            req = parseInt(rowData.ContactLens.RightUlConditionId, 10) === 64 ? 'true' : 'false';
                            $(rightQtyId).attr('data-value', req);
                            if (parseInt($(rightQtyId).val(), 10) < 0) {
                                $(rightQtyId).val(0);
                            }
                            if (parseInt($(leftQtyId).val(), 10) < 0) {
                                $(leftQtyId).val(0);
                            }
                            if (parseInt($(rightQtyId).val(), 10) > 0 || parseInt($(leftQtyId).val(), 10) > 0) {
                                $(rightQtyId).removeClass("requiredField");
                                $(leftQtyId).removeClass("requiredField");
                            }
                            if (parseInt($(rightQtyId).val(), 10) < 0) {
                                $(rightQtyId).val(0);
                            }
                            if (parseInt($(leftQtyId).val(), 10) < 0) {
                                $(leftQtyId).val(0);
                            }

                            /* Quantity Validation Rules */
                            $(rightQtyId).rules("add", {
                                required: true,
                                digits: true,
                                min: function () {
                                    var retVal = 0;
                                    if ($(leftQtyId).val().length <= 0 || parseInt($(leftQtyId).val(), 10) <= 0 || $(rightQtyId).attr('data-value') === 'true') {
                                        retVal = 1;
                                    }
                                    return retVal;
                                },
                                max: 99,
                                messages: {
                                    required: "Must be greater than 0",
                                    min: "Right or Left must be greater than 0",
                                    max: "Right must be less than 100",
                                    digits: "Only numeric characters allowed"
                                }
                            });

                            $(leftQtyId).rules("add", {
                                required: true,
                                digits: true,
                                min: function () {
                                    var retVal = 0;
                                    if ($(rightQtyId).val().length <= 0 || parseInt($(rightQtyId).val(), 10) <= 0 || $(leftQtyId).attr('data-value') === 'true') {
                                        retVal = 1;
                                    }
                                    return retVal;
                                },
                                max: 99,
                                messages: {
                                    required: "Must be greater than 0",
                                    min: "Right or Left must be greater than 0",
                                    max: "Left must be less than 100",
                                    digits: "Only numeric characters allowed"
                                }
                            });

                            /* Quantity Validation Rules - Underlying Conditions for No Lens, Not Recorded */
                            if (parseInt(rowData.ContactLens.RightUlConditionId, 10) === 61 || parseInt(rowData.ContactLens.RightUlConditionId, 10) === 62) {
                                $(rightQtyId).rules("add", {
                                    required: false,
                                    min: 0,
                                    max: 0,
                                    messages: {
                                        min: "Right must be 0",
                                        max: "Right must be 0"
                                    }
                                });
                                $(rightQtyId).removeClass("requiredField");
                                $(rightQtyId).prop("disabled", "disabled");

                                $(leftQtyId).rules("add", {
                                    min: 1,
                                    messages: {
                                        min: "Left must be greater than 0"
                                    }
                                });
                            }

                            if (parseInt(rowData.ContactLens.LeftUlConditionId, 10) === 61 || parseInt(rowData.ContactLens.LeftUlConditionId, 10) === 62) {
                                $(leftQtyId).rules("add", {
                                    required: false,
                                    min: 0,
                                    max: 0,
                                    messages: {
                                        min: "Left must be 0",
                                        max: "Left must be 0"
                                    }
                                });
                                $(leftQtyId).removeClass("requiredField");
                                $(leftQtyId).prop("disabled", "disabled");

                                $(rightQtyId).rules("add", {
                                    min: 1,
                                    messages: {
                                        min: "Right must be greater than 0"
                                    }
                                });
                            }

                            $(rightQtyId).change(function () {
                                if ($(this).val().length === 0 || parseFloat($(this).val()) <= 0) {
                                    $(this).val("0");
                                }

                                $(rightQtyId).valid();
                                if ($(leftQtyId).attr('data-value') === 'false') {
                                    $(leftQtyId).valid();
                                }

                                if (parseInt($(rightQtyId).val(), 10) > 0 || parseInt($(leftQtyId).val(), 10) > 0) {
                                    $(rightQtyId).removeClass("requiredField");
                                    if ($(leftQtyId).attr('data-value') === 'false') {
                                        $(leftQtyId).removeClass("requiredField");
                                    }
                                }
                                if (!selectedValues.authorization.IsVsp) {
                                    updatePricingPanel(selectedValues.rx, $(this).val(), $(leftQtyId).val());
                                }
                            });

                            $(leftQtyId).change(function () {
                                if ($(this).val().length === 0 || parseFloat($(this).val()) <= 0) {
                                    $(this).val("0");
                                }

                                if ($(rightQtyId).attr('data-value') === 'false') {
                                    $(rightQtyId).valid();
                                }
                                $(leftQtyId).valid();

                                if (parseInt($(rightQtyId).val(), 10) > 0 || parseInt($(leftQtyId).val(), 10) > 0) {
                                    if ($(rightQtyId).attr('data-value') === 'false') {
                                        $(rightQtyId).removeClass("requiredField");
                                    }
                                    $(leftQtyId).removeClass("requiredField");
                                }
                                if (!selectedValues.authorization.IsVsp) {
                                    updatePricingPanel(selectedValues.rx, $(rightQtyId).val(), $(this).val());
                                }
                            });

                            $(rightQtyId).focus(function () { $(this).select(); });
                            $(leftQtyId).focus(function () { $(this).select(); });

                            // Update pricing automatically if we don't need BES (for VSP)
                            window.InsPanelViewModel.IsEstimated(true);
                            if (selectedValues.authorization) {
                                if (selectedValues.authorization.IsVsp !== true) {
                                    $("#calculations, #infoText").removeClass("hidden");
                                    updatePricingPanel(rowData, $(rightQtyId).val(), $(leftQtyId).val());

                                    $(leftQtyId).change(function () {
                                        updatePricingPanel(rowData, $(rightQtyId).val(), $(leftQtyId).val());
                                    });

                                    $(rightQtyId).change(function () {
                                        updatePricingPanel(rowData, $(rightQtyId).val(), $(leftQtyId).val());
                                    });
                                } else {
                                    // Reset the pricing panel 
                                    $("#divCalcInsurance").attr("data-text", "");
                                    $("#divCalcInsurance, #infoText").removeClass("hidden");
                                    updatePricingPanel(null, 0, 0);
                                }
                            }

                            /* Click Event handler for Calc Ins Estimates button (BES) */
                            $("#btnCalcInsurance").click(function (e) {
                                e.preventDefault();

                                // BES
                                if (selectedValues.authorization !== null && selectedValues.authorization !== undefined && selectedValues.authorization.IsVsp === true) {
                                    var leftLensId = rowData.ContactLens.LeftItemId,
                                        rightLensId = rowData.ContactLens.RightItemId,
                                        eligibilityId = insEligId,
                                        doctorId = 1864,
                                        leftQty = $(leftQtyId).val(),
                                        rightQty = $(rightQtyId).val();
                                    if ((eligibilityId === undefined || eligibilityId === null || eligibilityId === 0) && summaryInitialized) {
                                        if (window.InsPanelViewModel.PrimaryEligibility()) {
                                            eligibilityId = window.InsPanelViewModel.PrimaryEligibility().InsEligibilityId();
                                        }
                                    }
                                    if (leftQty !== undefined && leftQty !== "" && rightQty !== undefined && rightQty !== "") {
                                        client
                                            .action("GetBesResponseForContactLensOrder")
                                            .get({
                                                patientId: window.patientOrderExam.PatientId,
                                                leftLensId: leftLensId,
                                                leftLensQuantity: leftQty,
                                                rightLensId: rightLensId,
                                                rightLensQuantity: rightQty,
                                                eligibilityId: eligibilityId,
                                                doctorId: doctorId
                                            })
                                            .done(function (data) {
                                                $("#divCalcInsurance").attr("data-text", leftQty + "," + rightQty);
                                                selectedValues.insuranceCharges = data.besResponse;
                                                window.removeAllPricingLineItems();

                                                if (data) {
                                                    if (data.serverErrorMessage === '') {
                                                        $("#divCalcInsurance").addClass("hidden");
                                                        $("#calculations, #infoText").removeClass("hidden");
                                                    }
                                                    updateContactLensInsurancePanelFromBESResponse(data.besResponse, data.serverErrorMessage, rowData.ContactLens.LeftItemId, rowData.ContactLens.RightItemId, rightQty, leftQty);
                                                }
                                            });
                                    }
                                }
                            });

                            selectedValues.rx = rowData;

                            /* Click Event handler for all Print Rx buttons that have been dynamically generated */
                            $("button[id^='btnPrintRx']").click(function (e) {
                                e.preventDefault();
                                var rxId = $(this).attr("data-rxid");
                                var url = window.config.baseUrl + 'PatientReports/PrintRxReport?examId=' + rxId + '&officeNum=' + window.config.officeNumber;
                                window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
                            });

                            /* Click Event handler for all + Order buttons that have been dynamically generated */
                            $("button[id^='btnAddRxToOrder']").click(function (e) {
                                e.preventDefault();

                                // JS Validation
                                if ($("#chooseClRxForm").valid()) {
                                    // ALOI ABB Validation
                                    validateSelectedRx(rowData.Id, function () {
                                        //display result for summary page
                                        if (!$("#divCalcInsurance").hasClass("hidden")) {
                                            $("#btnCalcInsurance").click();
                                        }

                                        $("#chooseRx").addClass("hidden");
                                        // init the Summary Page
                                        initSummaryPage(rowData, insEligId, $(leftQtyId).val(), $(rightQtyId).val());
                                    });
                                }
                            });

                            $("#msg_NonPricedItem").click(function (e) {
                                e.preventDefault();
                                showPricingDiag(rowData);
                            });
                            $(".integerFilter").keydown(function (e) {
                                return $(this).integerFilter(e);
                            });
                            $(".integerFilter").keyup(function (e) {
                                var val2, val1 = $(this).val();
                                var v2, v1 = $(this).attr("data-text");
                                var id = $(this).attr("id");
                                if (id.indexOf("leftQty") >= 0) {
                                    id = id.replace('left', 'right');
                                } else {
                                    id = id.replace('right', 'left');
                                }
                                v2 = $('#' + id).attr("data-text");
                                val2 = $('#' + id).val();
                                chooseRxDirtyFlag = (val1 !== v1 || val2 !== v2) ? true : false;

                                //visible update VSP button?
                                if (selectedValues.authorization && selectedValues.authorization.IsVsp) {
                                    var values = $("#divCalcInsurance").attr("data-text");
                                    if (values !== "") {
                                        var val = values.split(',');
                                        if (val[0] !== val1 || val[1] !== val2) {
                                            $("#divCalcInsurance, #infoText").removeClass("hidden");
                                            $("#calculations").addClass("hidden");
                                        } else {
                                            $("#divCalcInsurance").addClass("hidden");
                                            $("#calculations, #infoText").removeClass("hidden");
                                        }
                                    }
                                }
                            });

                        }); //client
                }
            } //if (this.parentNode._DT_RowIndex !== undefined)
        }
        /*jslint nomen: false*/
    });
}

/* Checks for Incomplete Rxs and shows message if any found */
function checkForIncompleteClRx() {
    client
        .action("GetIncompleteRxByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (iData) {
            if (iData && iData === true) {
                $("div#msg_IncompleteClRxs").removeClass("hidden");
            }
        });
}

/* Gets the Rx Data */
function loadRxData() {
    client
        .action("GetAllContactLensRxByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (cdata) {
            if (cdata !== undefined && cdata !== null) {
                rxData = cdata;
                rxTable.refreshDataTable(rxData);
                rxTable.fnDraw();
            }

            // check for incomplete Rxs
            checkForIncompleteClRx();
        })
        .fail(function () {
            alert("GetAllContactLensRxByPatientId failed.");
        });
}

/* Initialize the Choose Rx page (and Right-hand Insurance panel) */
initChooseRxPage = function () {
    $("#chooseClRxForm").alslValidate({
        onfocusout: false,
        onclick: false
    });

    //build the rx table if it hasn't been built yet
    if (!rxTableInitialized) {
        rxTableInitialized = true;
        buildRxTable();

        if (rxData === undefined || rxData === null) {
            loadRxData();
        } else {
            rxTable.refreshDataTable(rxData);
            rxTable.fnDraw();
        }
    }

    //show the insurance info (either init for the first time or just set the primary auth)
    if (!window.InsPanelViewModel) {
        window.initInsurancePanel("ContactLensOrder");
    }
    if (selectedValues.authorization !== null) {
        window.setPrimaryEligibility(selectedValues.authorization);
    }

    if (summaryInitialized) {
        $("#btnRxChangeInsurance").addClass("hidden");
    } else {
        $("#btnRxChangeInsurance").removeClass("hidden");
    }
    // show the page and reset selected row
    $("#chooseRx").removeClass("hidden");

    /* reset the pricing panel */
    if (selectedValues.rx) {
        if (selectedValues.authorization) {
            if (selectedValues.authorization.IsVsp) {
                if (initLoad) {
                    initLoad = false;
                    $("#divCalcInsurance").addClass("hidden");                  //hide button
                    if (window.InsPanelViewModel.BesError() === '') {
                        $("#calculations, #infoText").removeClass("hidden");    //display grid
                    }
                }
            } else {
                $("#calculations, #infoText").removeClass("hidden");        //display grid
                $("#divCalcInsurance").addClass("hidden");                  //hide button
                var leftQtyId = "#leftQty_" + selectedValues.rx.Id;
                var rightQtyId = "#rightQty_" + selectedValues.rx.Id;
                updatePricingPanel(selectedValues.rx, parseInt($(rightQtyId).val(), 10), parseInt($(leftQtyId).val(), 10));
            }
        }
    } else {
        $("#divCalcInsurance").addClass("hidden");
    }

    /* Event handler for "Change Ins." button */
    $("#btnRxChangeInsurance").click(function (e) {
        e.preventDefault();
        if (dataSource !== "new") {
            initChooseInsurancePage();
        }
        $("#chooseRx").addClass("hidden");
        $("#chooseInsurance").removeClass("hidden");
    });
};

/* Choose Insurance page  View-Model */
var EligibilitiesViewModel = function (data) {
    var self = this;
    self.insurances = ko.observableArray(data.insurances);
    self.clEligibilities = ko.observableArray();
    self.displayClEligibilities = ko.observableArray();

    ko.utils.arrayForEach(self.insurances(), function (ins) {
        ko.utils.arrayForEach(ins.Eligibilities.Eligibilities, function (elig) {
            if (elig.IsClElig) {
                elig.AuthDate = convertDate(elig.AuthDate)[0];
                elig.AuthExpireDate = convertDate(elig.AuthExpireDate)[0];
                if (elig.AuthNumber === null || elig.AuthNumber.length <= 0) {
                    elig.AuthNumber = "N/A";
                }
                self.clEligibilities.push({
                    NonInsurance: false,
                    IsVsp: ins.IsVsp,
                    CarrierFull: ins.CarrierDisplay,
                    CarrierShort: ins.CarrierDisplay.split(" / ")[0],
                    CarrierId: ins.Id,
                    PlanId: ins.PlanId,
                    InsuranceEligibilityId: elig.InsuranceEligibilityId,
                    Eligibility: elig
                });
            }
        });
    });

    // add the Non-Insurance item manually
    self.clEligibilities.push({
        NonInsurance: true,
        IsVsp: false,
        CarrierFull: "Non-Insurance / Private Pay",
        CarrierShort: "Non-Insurance",
        CarrierId: 0,
        PlanId: 0,
        InsuranceEligibilityId: 0,
        Eligibility: {
            AuthNumber: 0
        }
    });

    //parse it into 2s to display better on the UI
    var tempClEligibilities = self.clEligibilities().slice(0);
    while (tempClEligibilities.length > 0) {
        self.displayClEligibilities.push(tempClEligibilities.splice(0, 2));
    }
};

/* Initializes the Choose Insurance page */
initChooseInsurancePage = function () {
    if (chooseInsuranceInitialized === true) {
        return;
    }
    client
        .action("GetAllContactLensEligibilitiesByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (data) {
            chooseInsuranceInitialized = true;
            //init the EligViewModel, then apply the KO bindings to the eligibilities div only
            EligViewModel = new EligibilitiesViewModel(data);
            ko.applyBindings(EligViewModel, $('#clEligibilities')[0]);

            // show no Auths message if no auths
            if (EligViewModel.clEligibilities().length <= 1) {
                $("#msg_noAuthsWarning").removeClass("hidden");
            }

            // show the page once everything is done
            $("#chooseInsurance").removeClass("hidden");

            /* Click Event handler for all + Order buttons that have been dynamically generated*/
            $("button[id^='btnStartOrder']").click(function (e) {
                e.preventDefault();
                insEligId = ($(this).data("insurance-eligibility-id"));
                ko.utils.arrayForEach(EligViewModel.clEligibilities(), function (elig) {
                    if (elig.InsuranceEligibilityId === insEligId) {
                        selectedValues.authorization = elig;
                    }
                });

                // hide the Choose Insurance page
                $("#chooseInsurance").addClass("hidden");
                chooseInsuranceDirtyFlag = true;

                // user changes insurance in the summary page (order already has an Rx selected)
                if (summaryInitialized) {
                    updateSummaryEstimatedCharges();
                } else {
                    // init the Choose Rx Page
                    initChooseRxPage();
                }
            });
        });
};

/* Converts a DB Pricing Object to JS ViewModel Eligibility object */
function getEligibilityForInsurancePanel(insuranceVm) {
    if (insuranceVm === undefined || insuranceVm === null) {
        return null;
    }

    var elig = {
        CarrierFull: insuranceVm.CarrierFull,
        InsuranceEligibilityId: insuranceVm.EligId,
        NonInsurance: false,
        IsVsp: insuranceVm.IsVsp,
        IsEstimated: insuranceVm.IsEstimated,
        Eligibility: {
            AuthNumber: insuranceVm.AuthNumber,
            AuthDate: insuranceVm.AuthDate,
            AuthExpireDate: insuranceVm.AuthExpireDate,
            IsExamElig: insuranceVm.IsExamElig,
            IsFrameElig: insuranceVm.IsFrameElig,
            IsLensElig: insuranceVm.IsLensElig,
            IsClElig: insuranceVm.IsClElig,
            IsClFitElig: insuranceVm.IsClFittingElig
        }
    };

    return elig;
}

updateContactLensInsurancePanelFromBESResponse = function (itemCharges, errorMessage, leftItemId, rightItemId, rightQtyIn, leftQtyIn) {
    if (errorMessage !== undefined && errorMessage !== null && errorMessage !== "") {
        window.updateServerError("Insurance estimated charges are unavailable.");
    } else {
        window.updateServerError("");
    }
    if (itemCharges !== undefined && itemCharges !== null && itemCharges.length > 0) {
        var patientCopay = 0;
        itemCharges.forEach(function (itemCharge) {
            // CL Pricing
            var name = '';
            var rightQty = parseInt(rightQtyIn, 10);
            var leftQty = parseInt(leftQtyIn, 10);
            if (leftItemId === rightItemId && itemCharge.ItemId === rightItemId) {
                name = "Lens (Both)";
                if (rightQty === 0 && leftQty > 0) {
                    name = "Lens (L)";
                } else if (rightQty > 0 && leftQty === 0) {
                    name = "Lens (R)";
                }
                window.updateOrAddPricingLineItem({
                    itemId: itemCharge.ItemId,
                    sortId: 10,
                    title: name,
                    retailPrice: itemCharge.RetailPrice,
                    primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                    secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                    patientPrice: itemCharge.PatientDisplayAmount
                });
            } else {
                if (itemCharge.ItemId === rightItemId && rightQty > 0) {
                    window.updateOrAddPricingLineItem({
                        itemId: itemCharge.ItemId,
                        sortId: 10,
                        title: "Lens (R)",
                        retailPrice: itemCharge.RetailPrice,
                        primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                        secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                        patientPrice: itemCharge.PatientDisplayAmount
                    });
                }
                if (itemCharge.ItemId === leftItemId && leftQty > 0) {
                    window.updateOrAddPricingLineItem({
                        itemId: itemCharge.ItemId,
                        sortId: 20,
                        title: "Lens (L)",
                        retailPrice: itemCharge.RetailPrice,
                        primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                        secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                        patientPrice: itemCharge.PatientDisplayAmount
                    });
                }
            }
            // keep a running total for all Copay charges
            patientCopay += itemCharge.PatientCopay;
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
};

/* Builds the Insurance Panel from a CL Order (DB) */
updateContactLensInsurancePanelFromCLOrder = function (data, leftItemId, rightItemId) {
    if (data !== undefined && data !== null) {

        // set the primary eligibility if we have one
        if (data.InsurancePricing && data.InsurancePricing.PrimaryInsurance !== undefined && data.InsurancePricing.PrimaryInsurance !== null) {
            window.setPrimaryEligibility(getEligibilityForInsurancePanel(data.InsurancePricing.PrimaryInsurance));
            selectedValues.authorization = getEligibilityForInsurancePanel(data.InsurancePricing.PrimaryInsurance);
        }

        // set the secondary eligibility if we have one
        if (data.InsurancePricing && data.InsurancePricing.SecondaryInsurance !== undefined && data.InsurancePricing.SecondaryInsurance !== null) {
            window.setSecondaryEligibility(getEligibilityForInsurancePanel(data.InsurancePricing.SecondaryInsurance));
        }

        // if we have InsurancePricing object, tell the viewmodel whether these are estimated charges, or actual priced charges
        if (data.InsurancePricing) {
            window.InsPanelViewModel.IsEstimated(data.InsurancePricing.IsEstimated);
        }

        // if there's no insurancePricing, we need to create a default object to bind to viewModel (Non-Insurance)
        if (!data.InsurancePricing || (!data.InsurancePricing.PrimaryInsurance && !data.InsurancePricing.SecondaryInsurance)) {
            var nonInsData = {
                CarrierFull: "Non-Insurance / Private Pay",
                CarrierShort: "Non-Insurance",
                NonInsurance: true,
                IsVsp: false
            };
            window.setPrimaryEligibility(nonInsData);
            selectedValues.authorization = nonInsData;
        }

        // build the pricing grid from Insurance Item Charges if we have them
        var name = "Lens (Both)";
        var rightQty = 0;
        var leftQty = 0;
        if (data.InsurancePricing && data.InsurancePricing.ItemCharges !== undefined && data.InsurancePricing.ItemCharges !== null && data.InsurancePricing.ItemCharges.length > 0) {
            var patientCopay = 0;
            window.updateServerError("");
            rightQty = parseInt(data.ContactLens.RightQuantity, 10);
            leftQty = parseInt(data.ContactLens.LeftQuantity, 10);
            data.InsurancePricing.ItemCharges.forEach(function (itemCharge) {
                // CL Pricing
                if (leftItemId === rightItemId && itemCharge.ItemId === rightItemId) {
                    name = "Lens (Both)";
                    if (rightQty === 0 && leftQty > 0) {
                        name = "Lens (L)";
                    } else if (rightQty > 0 && leftQty === 0) {
                        name = "Lens (R)";
                    }
                    window.updateOrAddPricingLineItem({
                        itemId: itemCharge.ItemId,
                        sortId: 10,
                        title: name,
                        retailPrice: itemCharge.RetailPrice,
                        primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                        secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                        patientPrice: itemCharge.PatientDisplayAmount
                    });
                } else {
                    if (itemCharge.ItemId === rightItemId && rightQty > 0) {
                        window.updateOrAddPricingLineItem({
                            itemId: itemCharge.ItemId,
                            sortId: 10,
                            title: "Lens (R)",
                            retailPrice: itemCharge.RetailPrice,
                            primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                            secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                            patientPrice: itemCharge.PatientDisplayAmount
                        });
                    }
                    if (itemCharge.ItemId === leftItemId && leftQty > 0) {
                        window.updateOrAddPricingLineItem({
                            itemId: itemCharge.ItemId,
                            sortId: 20,
                            title: "Lens (L)",
                            retailPrice: itemCharge.RetailPrice,
                            primaryInsurancePrice: itemCharge.InsuranceAllowancePrimary,
                            secondaryInsurancePrice: itemCharge.InsuranceAllowanceSecondary,
                            patientPrice: itemCharge.PatientDisplayAmount
                        });
                    }
                }
                // keep a running total for all Copay charges
                patientCopay += itemCharge.PatientCopay;
            });

            // Copay
            window.updateOrAddPricingLineItem({
                itemId: 0,
                sortId: 100,
                title: "Copay",
                primaryInsurancePrice: "N/A",
                patientPrice: patientCopay
            });
        } else if (data.InsurancePricing && data.InsurancePricing.IsEstimated && data.InsurancePricing.PrimaryInsurance !== undefined && data.InsurancePricing.PrimaryInsurance !== null && data.InsurancePricing.PrimaryInsurance.IsVsp) {
            // Make the BES call
            getBESEstimatesForSummaryPage(data.ContactLens.LeftItemId, data.ContactLens.LeftQuantity, data.ContactLens.RightItemId, data.ContactLens.RightQuantity, data.InsurancePricing.PrimaryInsurance.EligId, data.DoctorId);
        } else if (data.ContactLens && data.ContactLens.Price) {
            // build the pricing grid from raw CL data if we don't have insurance charges    
            rightQty = parseInt(data.ContactLens.RightQuantity, 10);
            leftQty = parseInt(data.ContactLens.LeftQuantity, 10);
            window.updateServerError("");
            if (data.ContactLens.LeftItemId === data.ContactLens.RightItemId) {
                if (rightQty === 0 && leftQty > 0) {
                    name = "Lens (L)";
                } else if (rightQty > 0 && leftQty === 0) {
                    name = "Lens (R)";
                }
                window.updateOrAddPricingLineItem({
                    itemId: data.ContactLens.LeftItemId,
                    sortId: 10,
                    title: name,
                    retailPrice: (data.ContactLens.Price.RetailLeft * data.ContactLens.LeftQuantity) + (data.ContactLens.Price.RetailLeft * data.ContactLens.RightQuantity),
                    patientPrice: (data.ContactLens.Price.RetailLeft * data.ContactLens.LeftQuantity) + (data.ContactLens.Price.RetailLeft * data.ContactLens.RightQuantity)
                });
            } else {
                if (rightQty > 0) {
                    window.updateOrAddPricingLineItem({
                        itemId: data.ContactLens.RightItemId,
                        sortId: 10,
                        title: "Lens (R)",
                        retailPrice: data.ContactLens.Price.RetailRight * data.ContactLens.RightQuantity,
                        patientPrice: data.ContactLens.Price.RetailRight * data.ContactLens.RightQuantity
                    });
                }
                if (leftQty > 0) {
                    window.updateOrAddPricingLineItem({
                        itemId: data.ContactLens.LeftItemId,
                        sortId: 20,
                        title: "Lens (L)",
                        retailPrice: data.ContactLens.Price.RetailLeft * data.ContactLens.LeftQuantity,
                        patientPrice: data.ContactLens.Price.RetailLeft * data.ContactLens.LeftQuantity
                    });
                }
            }
        }
    }
};

resetTempModal = function () {
    $("#resourceId").clearField();
    $("#resourceId").val("");
    $("#resourceId").addClass("requiredField");
    $(".summaryMessages").clearMsgBlock();
    $("#resourceId").focus();
};

function showInProgressDialog() {
    resetTempModal();
    $("#duplicateMsg").addClass("hidden");
    var title = "Save Draft";
    $("#tempStorageSave .modal-title").html(title);
    $("#tempStorageSave").data("id", 0).modal({
        keyboard: false,
        backdrop: "static",
        show: true
    });
    $("#tempStorageSave").on('shown.bs.modal', function () {
        $(this).find('#resourceId').focus();
    });
}

function saveToTempStorage(data) {
    data.PatientId = window.patientOrderExam.PatientId;
    data.InsEligibilityId = insEligId;
    client
        .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
        .action("SaveInProgressContactLensOrder")
        .put(data)
        .done(function () {
            summaryViewModel.isDirty(false);
            chooseRxDirtyFlag = false;
            chooseInsuranceDirtyFlag = false;
            redirect = true;
            var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=" + "0";
            window.location.href = redirectUrl;
        });
}

function getIpSummaryAndSave(data) {
    client
        .action("GetInProgressOrderSummary")
        .get({
            patientId: window.patientOrderExam.PatientId,
            awsResourceId: window.patientOrderExam.AwsResourceId
        })
        .done(function (summary) {
            if (summary === undefined || summary === null) {
                alert("error: no summary row");
                return;
            }

            providerName = summary.ResourceDisplay;
            saveToTempStorage(data);
        });
}

var validateMoney = function (element) {
    var item = element.replace(/[^\d\.]/g, '');
    if (item !== '') {
        var el = parseFloat(item).toFixed(2);
        if (el.length > 7) {
            return false;
        }
    }
    return true;
};

var formatMoney = function (element) {
    var item = element.replace(/[^\d\.]/g, '');
    var s = "$";
    if (item !== '') {
        var el = parseFloat(item).toFixed(2);
        if (el.length < 8) {
            return s + el;
        }
    }
    return element;
};

var moneyToFloat = function (money) {
    var item = money.replace(/[^\d\.]/g, '');
    if (item !== "") {
        return parseFloat(item).toFixed(2);
    }
    return 0;
};

$.validator.addMethod("nonZeroMoney", function (value, element) {
    if (element.value === "") {
        return false;
    }

    if (!new RegExp(regex.MONEY).test(element.value)) {
        return false;
    }

    if (parseFloat(value.replace(/[^\.\d]/g, "")) <= 0) {
        return false;
    }

    return true;
});

/* Validates the user-defined name when saving to temporary storage (WIP)*/
validateTempOrderName = function () {
    $("#duplicateMsg").removeClass("visible");
    $("#duplicateMsg").addClass("hidden");
    $("#saveTempOrder").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            resourceId: {
                required: true,
                maxlength: 30,
                Regex: "^[a-zA-Z 0-9_]*$",
                notString: "Enter your Order Name here..."
            }
        },
        messages: {
            resourceId: {
                required: "You must give a unique name to this order...",
                maxlength: "Order Name can not be more than 30 characters long.",
                Regex: "Only alphanumeric characters allowed in the name.",
                notString: "Enter your Order Name here..."
            }
        }
    });
};

/* Click Event Handler for the "No Auths Found" message on the Choose Insurance page */
$("div#msg_noAuthsWarning").click(function () {
    var redirectUrl = window.config.baseUrl + "Patient/InsuranceEligibility?id=" + window.patientOrderExam.PatientId;
    window.location.href = redirectUrl;
});

/* Click Event Handler for the "Incomplete Rxs Found" message on the Choose Rx page */
$("div#msg_IncompleteClRxs").click(function () {
    var redirectUrl = window.config.baseUrl + "Patient/Rx?id=" + window.patientOrderExam.PatientId;
    window.location.href = redirectUrl;
});

/* Click Event Handler for the "Save For Later" button on the Summary screen */
$("#btnSaveForLater").click(function (e) {
    e.preventDefault();
    if (dataSource === "new") {
        showInProgressDialog();
    } else if (dataSource === "tempStorage") {
        var data = getContactLensOrderDetailsFromViewModel();
        data.AwsResourceId = window.patientOrderExam.AwsResourceId;
        getIpSummaryAndSave(data);

    } else {
        alert("error: Order exists so temporary storage should have been disabled");
    }
});

/* Click event handler for "Continue to Pricing" button */
$("#btnContinueToPricing").click(function (e) {
    e.preventDefault();
    var i, redirectUrl;
    if (summaryInitialized && !summaryViewModel.isDirty() && window.patientOrderExam.OrderId > 0) {
        redirect = true;
        redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oid=" + window.patientOrderExam.OrderId;
        window.location.href = redirectUrl;
    } else {
        if ($("#clOrderSummaryForm").valid()) {
            var order = getContactLensOrderDetailsFromViewModel();

            //verify whether the selected address (addess1 or address2) contains more than 30 characters 
            for (i = 0; i < addressList.length; i++) {
                if (order.ShipTo === addressList[i].Key) {
                    if (addressList[i].KeyStr !== "") {
                        var msg = "";
                        if (order.ShipToType === 1 || order.ShipToType === 2) {
                            msg = "This office has an address with more than 30 characters in either the Address 1 field or the Address 2 field of the Office Information screen in Admin. In order " +
                                  "to proceed with this order, you will need to have your system administrator modify this office’s address and ensure that each address field has no more than 30 characters.";
                        } else {
                            msg = "This patient has an address with more than 30 characters in either the Address 1 field or Address 2 field of the Profile screen. In order " +
                                  "to proceed with this order you will need to edit the patient profile and ensure each address field has no more than 30 characters.";
                        }

                        var options = {
                                data: null,
                                title: "Address Validation",
                                message: msg,
                                buttons : ["", "OK"],
                                callback : null
                            };
                        $.messageDialog(options);

                        return;
                    }

                    break;
                }
            }

            client
                .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
                .action("SaveContactLensOrder")
                .put(order)
                .done(function (savedOrder) {
                    summaryViewModel.isDirty(false);
                    chooseRxDirtyFlag = false;
                    chooseInsuranceDirtyFlag = false;
                    redirect = true;
                    redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oid=" + savedOrder;
                    window.location.href = redirectUrl;
                });
        } else {
            // scroll to the first invalid element
            $("html, body").animate({
                scrollTop: $(".error").offset().top - 50
            }, 500);
        }
    }
});

/* OnBlur Event Handler Right Price On-Demand Pricing modal */
$("#retailPriceRight").on("blur", function () {
    var element = $("#retailPriceRight").val();
    var a = validateMoney(element);
    if (a) {
        $("#retailPriceRight").val(formatMoney(element));
    } else {
        $("#retailPriceRight").val(element);
    }
});

/* OnBlur Event Handler Left Price On-Demand Pricing modal */
$("#retailPriceLeft").on("blur", function () {
    var element = $("#retailPriceLeft").val();
    var a = validateMoney(element);
    if (a) {
        $("#retailPriceLeft").val(formatMoney(element));
    } else {
        $("#retailPriceLeft").val(element);
    }
});

/* Click Event Handler for Add Instructions link */
$("#linkAddInstructions").click(function (e) {
    e.preventDefault();

    $("#instructionsGroupAdd").addClass("hidden");
    $("#instructionsGroupFields").removeClass("hidden");
});

/* Click Event Handler for Add Dispensing Notes link */
$("#linkAddDispensingNotes").click(function (e) {
    e.preventDefault();

    $("#dispensingNotesGroupAdd").addClass("hidden");
    $("#dispensingNotesGroupFields").removeClass("hidden");
});

/* Click Event Handler Save Price button on On-Demand pricing modal */
$("#btnSavePrice").click(function (e) {
    e.preventDefault();

    var valid = true;
    if ($("#rightCLPricingPanel").is(':visible')) {
        if (!$("#saveClPriceRight").valid()) {
            valid = false;
        } else {
            priceVm.RetailDisplayRight = $("#retailPriceRight").val();
            priceVm.RetailRight = moneyToFloat(priceVm.RetailDisplayRight);
            if (selectedValues.rx.ContactLens.RightItemId === selectedValues.rx.ContactLens.LeftItemId) {
                priceVm.RetailDisplayLeft = priceVm.RetailDisplayRight;
                priceVm.RetailLeft = priceVm.RetailRight;
            }
        }
    }

    if ($("#leftCLPricingPanel").is(':visible')) {
        if (!$("#saveClPriceLeft").valid()) {
            valid = false;
        } else {
            priceVm.RetailDisplayLeft = $("#retailPriceLeft").val();
            priceVm.RetailLeft = moneyToFloat(priceVm.RetailDisplayLeft);
        }
    }

    if (valid) {
        client
            .action("SaveContactLensPrice")
            .put(priceVm)
            .done(function () {
                var contactLens = selectedValues.rx.ContactLens;
                $(document).showSystemSuccess("Contact Lens Price(s) are saved");
                $("#clPricing").modal("hide");
                $("#msg_NonPricedItem").addClass("hidden");
                if (contactLens !== undefined && contactLens !== null) {
                    var leftQtyId = "#leftQty_" + selectedValues.rx.Id;
                    var rightQtyId = "#rightQty_" + selectedValues.rx.Id;
                    if (priceVm.RetailLeft > 0) {
                        contactLens.LeftHasPrice = true;
                        contactLens.Price.RetailLeft = priceVm.RetailLeft;
                    }
                    if (priceVm.RetailRight > 0) {
                        contactLens.RightHasPrice = true;
                        contactLens.Price.RetailRight = priceVm.RetailRight;
                    }
                    selectedValues.rx.ContactLens = contactLens;
                    updatePricingPanel(selectedValues.rx, $(rightQtyId).val(), $(leftQtyId).val());
                }
            });
    }
});

/* On-KeyUp Event Handler for WIP order names */
$("#resourceId").on("keyup", function () {
    $("#duplicateMsg").removeClass("visible");
    $("#duplicateMsg").addClass("hidden");
});

/* Click Event Handdler for Save WIP button */
$("#btnSaveTemp").click(function (e) {
    e.preventDefault();
    var r = $("#resourceId").val().trim();
    validateTempOrderName();
    if (!$("#saveTempOrder").valid()) {
        return;
    }

    client
        .action("GetInProgressOrderSummary")
        .get({
            patientId: window.patientOrderExam.PatientId,
            awsResourceId: r
        })
        .done(function (summary) {
            if (summary === null) {
                var data = getContactLensOrderDetailsFromViewModel();
                data.AwsResourceId = r.trim();
                saveToTempStorage(data);
            } else {
                $("#duplicateMsg").removeClass("hidden");
                $("#duplicateMsg").addClass("visible");
            }
        });
});

$("#resourceId").keypress(function (e) {
    if (e.which === 13) {
        e.preventDefault();
        $("#btnSaveTemp").click();
        // hide the keyboard in iOS when the form is submitted 
        $(':focus').blur();
    }
});

$("#btnCancelClOrder").click(function (e) {
    e.preventDefault();
    $("#clOrderCancelModal").modal("hide");
    var redirectUrl = window.config.baseUrl;
    if (dataSource === "new") {
        redirectUrl += "Patient/ContactLensOrder?id=" + window.patientOrderExam.PatientId + "&resourceId=0&oId=0&update=yes";
    } else {
        redirectUrl += "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=0";
    }
    redirect = true;
    window.location.href = redirectUrl;
});

$("#btnCancelOrder").click(function (e) {
    e.preventDefault();
    $('#clOrderCancelModal').modal({
        keyboard: false,
        backdrop: 'static',
        show : true
    });
});

$("#btnReturnToMaterialOrders").click(function (e) {
    e.preventDefault();
    var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=0";
    window.location.href = redirectUrl;
});

$("#btnSave").click(function (e) {
    e.preventDefault();
    var order = getContactLensOrderDetailsFromViewModel();
    client
        .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
        .action("SaveContactLensOrder")
        .put(order)
        .done(function () {
            summaryViewModel.isDirty(false);
            $(document).showSystemSuccess("Contact Lens Order successfully saved.");
        });
});

$("#btnPrintSummary").click(function (e) {
    e.preventDefault();
    var oDetail = getContactLensOrderDetailsFromViewModel();

    $.ajax({
        url: window.config.baseUrl + "PatientReports/PrintContactLensOrderSummary",
        data: JSON.stringify(oDetail),
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

    //window.open(url, "_blank", "height=800,width=1020,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
});

window.onbeforeunload = function () {
    if (redirect) {
        return;
    }
    if (chooseInsuranceDirtyFlag || chooseRxDirtyFlag || (summaryViewModel && summaryViewModel.isDirty())) {
        return "You are trying to navigate to a new page, but you have not saved the data on this page.";
    }
};

$(document).ready(function () {
    $("select").alslSelectPicker();
    loadPatientSideNavigation(window.patientOrderExam.PatientId, "clOrder");
    window.updatePageTitle();
    var resourceId = window.patientOrderExam.AwsResourceId;
    var orderId = window.patientOrderExam.OrderId;

    if ((resourceId === null || resourceId === undefined || resourceId === "0") && (orderId === 0 || orderId === null)) {
        // no resourceid no orderid so its a new cl order
        dataSource = "new";
        initChooseInsurancePage();
        $("#btnSaveForLater").removeClass("hidden").addClass("visible");
    } else if ((resourceId === null || resourceId === undefined || resourceId === "0") && (orderId !== 0 && orderId !== null)) {
        // there is orderid and NO resourceid so load summary from order
        dataSource = "order";
        initSummaryPageFromOrder();
        $("#btnSaveForLater").removeClass("visible").addClass("hidden");
    } else if ((resourceId !== null && resourceId !== undefined && resourceId !== "0") && (orderId === 0 || orderId === null)) {
        // there is NO orderid and resourceid is present so load summary from Temp Storage
        dataSource = "tempStorage";
        initSummaryPageFromTempStorage();
        $("#btnSaveForLater").removeClass("hidden").addClass("visible");
    } else {
        // we should never get here cause we should never have a orderid and resourceId together
        alert("Error! Resource Id and Order Id both present. Taking the Order Id.");
        dataSource = "order";
        initSummaryPageFromOrder();
        $("#btnSaveForLater").removeClass("visible").addClass("hidden");
    }
});