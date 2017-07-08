/*jslint browser: true, vars: true, plusplus: true, regexp: true */
/*global $, document, window, ko, Modernizr, alert, console, msgType, updatePageTitle, loadPatientSideNavigation, ApiClient, InsPanelType, regex,
    convertDate, noReportData, ChargeItem, getTodayDate, saveToTempStorageEg, validateBuildOrder, validateRequiredModule, validateExtras, validateRx, validateAuth, rx */
var buildOrder = { FRAME: 0, LENSES: 1, EXTRAS: 2, LAB: 3, INSURANCE: 4, SUMMARY: 5, BUILDORDER: 6, ORDERTYPE: 7, RX: 8, warningMessage: '' };
var egOrderType = { COMPLETE: 1, LENSES_ONLY: 2, FRAME_ONLY: 3, EXTRAS_ONLY: 4 };
var rxConversionType = { READING: 1, DISTANCE: 2 };
var buildOrderTitle = ["Add Frame", "Add Lenses", "Add Extras", "Add Lab"];
var client = new ApiClient("EyeglassOrder"),
    eyeglassOrderViewModel,
    getBESEstimatesForEyeglassOrderSummaryPage,
    EligViewModel,
    rxViewModel,
    egDataSource,
    providerName,
    chooseInsuranceInitialized = false,
    updateEyeglassInsurancePanelFromEgOrder,
    insEligId,
    initChooseInsurancePage,
    selectedEligibility = null,
    egSummaryInitialized = false,
    urlRedirect = false,
    extrasLoaded,
    labsInitialized,
    extrasFrom;

var goBackToPage = {
    fromInsurance: '',
    fromAddToOrder: ''
};
var customFrame = {
    id: null,
    redirect: false,
    fromAdmin: false
};
var egInsurancePanel = {
    updateEstimatedCharges: false,
    updateLens: false,
    updateFrame: false,
    updateExtras: false
};

function getEntityIds() {
    var i;
    var extras = eyeglassOrderViewModel.selectedExtras();
    var frame = eyeglassOrderViewModel.selectedFrame();
    var lens = eyeglassOrderViewModel.selectedLens();
    var hasValue = false;

    var entityIds = {
        FrameId: 0,
        LeftLensTypeId: 0,
        LeftMaterialId: 0,
        LeftStyleId: 0,
        LeftColorId: 0,
        LeftCoatingId: 0,
        RightLensTypeId: 0,
        RightMaterialId: 0,
        RightStyleId: 0,
        RightColorId: 0,
        RightCoatingId: 0,
        TintId: 0,
        EdgeId: 0,
        CoatingsIds: '',
        ExtrasIds: ''
    };

    if (extras !== undefined && extras !== null) {
        hasValue = true;
        for (i = 0; i < extras.length; i++) {
            entityIds.ExtrasIds += entityIds.ExtrasIds === '' ? extras[i].ItemId : ',' + extras[i].ItemId;
        }
    }

    if (frame !== undefined && frame !== null) {
        //lenses only or extras only order (Patient Own Frame)
        if (eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.LENSES_ONLY && eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.EXTRAS_ONLY) {
            hasValue = true;
            entityIds.FrameId = frame.Id;
        }
    }

    if (lens !== undefined && lens !== null) {
        hasValue = true;
        entityIds.LeftLensTypeId = lens.LeftLens.TypeId;
        entityIds.LeftMaterialId = lens.LeftLens.MaterialTypeId;
        entityIds.LeftStyleId = lens.LeftLens.StyleId;
        entityIds.LeftColorId = lens.LeftLens.ColorId;
        entityIds.LeftCoatingId = lens.LeftLens.MfgCoatingId;
        entityIds.RightLensTypeId = lens.RightLens.TypeId;
        entityIds.RightMaterialId = lens.RightLens.MaterialTypeId;
        entityIds.RightStyleId = lens.RightLens.StyleId;
        entityIds.RightColorId = lens.RightLens.ColorId;
        entityIds.RightCoatingId = lens.RightLens.MfgCoatingId;
        entityIds.TintId = lens.TintColorId;
        entityIds.EdgeId = lens.LensEdgeTypeId;
        entityIds.CoatingsIds = lens.AddCoatingsId;
    }

    return {
        Ids: entityIds,
        hasValue: hasValue
    };
}

function updatePricingPanel() {
    window.InsPanelViewModel.IsEstimated(true);
    window.removeAllPricingLineItems();
    var entityIds = getEntityIds().Ids;

    client
        .action("GetPriceByItemId")
        .get({ entityIds: entityIds })
        .done(function (data) {
            if (entityIds.LeftLensTypeId > 0 || entityIds.RightLensTypeId > 0) {
                window.updateOrAddPricingLineItem({
                    itemId: 1,
                    sortId: 10,
                    title: "Lens",
                    retailPrice: data.LensesPrice.toFixed(2),
                    patientPrice: data.LensesPrice.toFixed(2)
                });
            }
            if (entityIds.FrameId !== 0) {
                window.updateOrAddPricingLineItem({
                    itemId: 2,
                    sortId: 10,
                    title: "Frame",
                    retailPrice: data.FramePrice.toFixed(2),
                    patientPrice: data.FramePrice.toFixed(2)
                });
            }
            if (entityIds.TintId > 0 || entityIds.EdgeId > 0 || entityIds.CoatingsIds !== '' || entityIds.ExtrasIds !== '') {
                window.updateOrAddPricingLineItem({
                    itemId: 3,
                    sortId: 10,
                    title: "Extras",
                    retailPrice: data.ExtrasPrice.toFixed(2),
                    patientPrice: data.ExtrasPrice.toFixed(2)
                });
            }
        });
}
function getNonInsurancePricingForSummaryInsurancePanel() {
    window.InsPanelViewModel.IsEstimated(true);
    window.removeAllPricingLineItems();
    window.InsPanelViewModel.DetailItems.removeAll();
    var entityIds = getEntityIds().Ids;

    client
        .action("GetNonInsurancePriceByItemIds")
        .get({ entityIds: entityIds })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                var auth = eyeglassOrderViewModel.selectedAuthorization();
                if (auth && auth.IsValid) {
                    $("#calculations, #detailedBreakdown").removeClass("hidden");
                }
                window.removeSecondaryEligibility();
                window.updateInsurancePricingDetails(data);
            }
            egInsurancePanel.updateEstimatedCharges = false;
            egInsurancePanel.updateLens = false;
            egInsurancePanel.updateFrame = false;
            egInsurancePanel.updateExtras = false;
        });
}
function getEligibilityForInsurancePanel(insuranceVm) {
    if (insuranceVm === undefined || insuranceVm === null) {
        return null;
    }

    var elig = {
        CarrierFull: insuranceVm.CarrierFull,
        InsuranceEligibilityId: insuranceVm.EligId,
        NonInsurance: insuranceVm.InsuranceEligibilityId === 0 ? true : false,
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
        },
        IsValid: insuranceVm.IsValid,
        ValidationMessage: insuranceVm.ValidationMessage
    };

    return elig;
}

function setupPage(page) {
    $("#btnFrameToBuildOrder, #btnLensesToBuildOrder, #btnExtrasToBuildOrder, #btnLabsToBuildOrder").addClass("hidden");
    $("#btnAddFrameToOrder, #btnAddLensesToOrder, #btnAddExtrasToOrder, #btnContinueToPricing").addClass("hidden");
    $("#btnContinue, #btnPrintOrderSummary").addClass("hidden");
    switch (page) {
    case buildOrder.FRAME:
        $("#btnFrameToBuildOrder").removeClass("hidden");
        $("#btnAddFrameToOrder").removeClass("hidden");
        $("#btnSaveForLater").addClass("hidden");
        break;
    case buildOrder.LENSES:
        $("#btnLensesToBuildOrder").removeClass("hidden");
        $("#btnAddLensesToOrder").removeClass("hidden");
        break;
    case buildOrder.EXTRAS:
        $("#btnExtrasToBuildOrder").removeClass("hidden");
        $("#btnAddExtrasToOrder").removeClass("hidden");
        break;
    case buildOrder.LAB:
        $("#btnLabsToBuildOrder").removeClass("hidden");
        break;
    case buildOrder.INSURANCE:
        break;
    case buildOrder.SUMMARY:
        $("#btnCancelOrder, #btnPrintOrderSummary, #btnContinueToPricing, #btnSaveForLater").removeClass("hidden");
        break;
    case buildOrder.BUILDORDER:
        $("#btnCancelOrder, #btnSaveForLater, #btnContinue").removeClass("hidden");
        break;
    case buildOrder.ORDERTYPE:
        $("#btnCancelOrder").removeClass("hidden");
        break;
    default:
        $("#btnContinue, #btnCancelOrder").removeClass("hidden");
        break;
    }
}

/* Builds the Insurance Panel from an Eyeglass Order (DB) */
updateEyeglassInsurancePanelFromEgOrder = function (data) {
    if (data !== undefined && data !== null) {
        // set the primary eligibility if we have one
        if (data.InsurancePricing && data.InsurancePricing.PrimaryInsurance !== undefined && data.InsurancePricing.PrimaryInsurance !== null) {
            window.setPrimaryEligibility(window.getEligibilityForInsurancePanel(data.InsurancePricing.PrimaryInsurance));
            eyeglassOrderViewModel.selectedAuthorization(window.getEligibilityForInsurancePanel(data.InsurancePricing.PrimaryInsurance));
        }

        // set the secondary eligibility if we have one
        if (data.InsurancePricing && data.InsurancePricing.SecondaryInsurance !== undefined && data.InsurancePricing.SecondaryInsurance !== null) {
            window.setSecondaryEligibility(window.getEligibilityForInsurancePanel(data.InsurancePricing.SecondaryInsurance));
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
                IsVsp: false,
                IsValid: true
            };
            window.setPrimaryEligibility(nonInsData);
            eyeglassOrderViewModel.selectedAuthorization(nonInsData);
        }

        // build the pricing grid from Insurance Item Charges if we have them
        if (data.InsurancePricing && data.InsurancePricing.ItemCharges !== undefined && data.InsurancePricing.ItemCharges !== null && data.InsurancePricing.ItemCharges.length > 0) {
            window.updateInsurancePricingDetails(data.InsurancePricing.ItemCharges);
            $("#calculations, #infoText, #detailedBreakdown").removeClass("hidden");
        } else if (data.InsurancePricing && data.InsurancePricing.IsEstimated && data.InsurancePricing.PrimaryInsurance !== undefined && data.InsurancePricing.PrimaryInsurance !== null && data.InsurancePricing.PrimaryInsurance.IsVsp) {
            // Make the BES call
            getBESEstimatesForEyeglassOrderSummaryPage();
        } else if (data) {
            // build the pricing grid from raw EG data if we don't have insurance charges
            getNonInsurancePricingForSummaryInsurancePanel();
        }
    }
};

function updateInsurancePanel(eligibility) {
    if (!window.InsPanelViewModel) {
        window.initInsurancePanel("EyeglassOrder");
    }
    window.setPrimaryEligibility(eligibility);
    eyeglassOrderViewModel.insuranceVM(window.InsPanelViewModel);
    window.changeInsurancePanel("EyeglassOrder");
}

//update 'Estimated Charges' table, turn on/off 'Update VSP Estimates' button
var onChangePricingPanel = function (callBes) {
    if (egInsurancePanel.updateEstimatedCharges || egInsurancePanel.updateLens || egInsurancePanel.updateFrame || egInsurancePanel.updateExtras) {
        window.updateServerError("");
        var selAuth = eyeglassOrderViewModel.selectedAuthorization();
        if (selAuth && (selAuth.NonInsurance || !selAuth.IsVsp)) {
            $("#divCalcInsurance").addClass("hidden");
            if (selAuth.IsValid) {
                $("#calculations, #infoText").removeClass("hidden"); //display grid/detailed breakdown link
                getNonInsurancePricingForSummaryInsurancePanel();
            }
        } else {
            var enableButton = true;
            if (window.customFrame.fromAdmin) {
                enableButton = getEntityIds().hasValue;
            }
            //if (enableButton && selAuth.IsValid) {
            if (enableButton) {
                $("#divCalcInsurance, #infoText").removeClass("hidden"); //display button, hide grid/detailed breakdown link
                $("#calculations, #detailedBreakdown").addClass("hidden");
                if (callBes !== undefined && callBes !== null && callBes === true) {
                    getBESEstimatesForEyeglassOrderSummaryPage();
                }
            }
        }
        egInsurancePanel.updateEstimatedCharges = false;
    } else {
        var auth = eyeglassOrderViewModel.selectedAuthorization();
        if (auth && auth.IsValid && $("#divCalcInsurance").hasClass("hidden")) {
            $("#calculations, #infoText, #detailedBreakdown").removeClass("hidden");
        }
    }
};

/* Gets the list of shipping addresses */
function getAddressList(shipTo) {
    if (shipTo === undefined || shipTo <= 0) {
        eyeglassOrderViewModel.addressList(null);
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
            eyeglassOrderViewModel.addressList(data);
            $("select#address").selectpicker("refresh");
        });
}
var EyeglassOrderViewModel = function () {
    var self = this;
    self.isDirty = ko.observable(false);
    self.orderIsValid = ko.observable(false);
    self.selectedRx = ko.observable(null);
    self.selectedLens = ko.observable(null);
    self.selectedAuthorization = ko.observable();
    self.selectedExtras = ko.observableArray(null);
    self.selectedExtrasDisplay = ko.observableArray();
    self.selectedLab = ko.observable(null);
    self.selectedFrame = ko.observable(null);
    self.patientExamId = ko.observable();
    // Shipping information
    self.shipTo = ko.observable();
    self.shipToList = ko.observableArray();
    self.address = ko.observable();
    self.addressList = ko.observableArray();
    // Order information
    self.orderNumber = ko.observable();
    self.orderType = ko.observable();
    self.eyeglassOrderType = ko.observable();
    self.eyeglassOrderTypeDescription = ko.observable();
    self.officeNum = ko.observable();
    self.staff = ko.observable();
    self.staffList = ko.observableArray();
    self.doctor = ko.observable();
    self.doctorId = ko.observable();
    self.isOutsideDoctor = ko.observable();
    self.remakeOrder = ko.observable();
    self.remakeTypeId = ko.observable();
    self.remake = ko.observable();
    self.remakeReasons = ko.observableArray();
    self.dispenseType = ko.observable();
    self.dispenseTypeList = ko.observableArray();
    self.dispenseNote = ko.observable();
    // Lab information
    self.labName = ko.observable();
    self.labAddress = ko.observable();
    self.labInstructions = ko.observable();
    self.toMakeFrame = ko.observable();
    self.toMakeRightLens = ko.observable();
    self.toMakeLeftLens = ko.observable();
    self.toMakeExtras = ko.observable();
    // Insurance information
    self.eligibilityId = ko.observable();
    self.insuranceVM = ko.observable();
    self.insuranceCharges = ko.observable();
    self.isInvoiced = ko.observable();
    self.isOrderStatusLabOnHold = ko.observable();
    // rx information
    self.rxDate = ko.observable();
    self.extrasForFrameAndLens = ko.observable();
    // copy order
    self.isCopyOrder = ko.observable(false);
    self.copyOrderIsVsp = ko.observable(false);

    self.selectedAuthorization.subscribe(function () {
        if (self.selectedAuthorization() !== null) {
            updateInsurancePanel(self.selectedAuthorization());
            window.setExtrasVisibility();
            window.setLabsVisibility();
        }
    });
    // expand dispensing note textarea if we have notes
    self.dispenseNote.subscribe(function () {
        if (self.dispenseNote() && self.dispenseNote().length > 0) {
            $("#linkAddDispensingNotes").click();
        }
    });
    // expand lab instructions textarea if we have instructions
    self.labInstructions.subscribe(function () {
        if (self.labInstructions() && self.labInstructions().length > 0) {
            self.isDirty(true);
            $("#linkAddInstructions").click();
        }
    });
    self.selectedRx.subscribe(function () {
        if (self.selectedRx() !== null) {
            rxViewModel.rx(self.selectedRx());
            self.patientExamId(self.selectedRx().Id);
            self.doctor(self.selectedRx().DoctorName);
            self.doctorId(self.selectedRx().DoctorId);
            self.isOutsideDoctor(self.selectedRx().IsOutsideDoctor);
            if (self.isOutsideDoctor()) {
                self.doctor(self.selectedRx().DoctorName + " (Outside Provider)");
            }
            window.loadExtrasAndUpdateLens();
        }
    });
    self.selectedLab.subscribe(function () {
        if (self.selectedLab() !== null) {
            self.labName(self.selectedLab().Name);
            self.labAddress(self.selectedLab().Address);
        }
    });
    self.eyeglassOrderType.subscribe(function () {
        switch (self.eyeglassOrderType()) {
        case egOrderType.COMPLETE: //Complete Eyeglass Order
            self.toMakeRightLens(true);
            self.toMakeLeftLens(true);
            self.toMakeExtras(true);
            self.toMakeFrame(true);
            $("#btnRxChangeInsurance").removeClass("hidden");
            break;
        case egOrderType.LENSES_ONLY: //Lenses Only Order
            self.toMakeRightLens(true);
            self.toMakeLeftLens(true);
            self.toMakeExtras(true);
            self.toMakeFrame(false);
            $("#btnRxChangeInsurance").removeClass("hidden");
            break;
        case egOrderType.FRAME_ONLY: //Frame Only Order
            self.toMakeRightLens(false);
            self.toMakeLeftLens(false);
            self.toMakeExtras(true);
            self.toMakeFrame(true);
            $("#btnRxChangeInsurance").removeClass("hidden");
            break;
        case egOrderType.EXTRAS_ONLY: //Extras Only Order
            self.toMakeExtras(true);
            self.toMakeRightLens(false);
            self.toMakeLeftLens(false);
            self.toMakeFrame(false);
            $("#btnRxChangeInsurance").addClass("hidden");
            break;
        }
    });
    self.selectedExtras.subscribe(function () {
        if (self.selectedExtras() === null) {
            self.selectedExtrasDisplay("");
        } else {
            var extrasDisplay = "";
            $.each(self.selectedExtras(), function (index, extra) {
                if (extrasDisplay.length > 0) {
                    extrasDisplay += ", ";
                }
                extrasDisplay += extra.Name;
            });
            self.selectedExtrasDisplay(extrasDisplay);
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
            self.isDirty(true);
        }
    });
    self.dispenseType.subscribe(function () {
        if (self.dispenseType() > 0) {
            $("#dispensingStatus").parents("div.bootstrap-select").removeClass("requiredField");
            $("#dispensingStatus").clearField();
        } else {
            $("#dispensingStatus").clearField();
            $("#dispensingStatus").parents("div.bootstrap-select").addClass("requiredField");
            self.isDirty(true);
        }
    });
    self.staff.subscribe(function () {
        if (self.staff() > 0) {
            $("#staff").parents("div.bootstrap-select").removeClass("requiredField");
            $("#staff").clearField();
        } else {
            $("#staff").clearField();
            $("#staff").parents("div.bootstrap-select").addClass("requiredField");
            self.isDirty(true);
        }
    });

    self.selectedFrame.subscribe(function () {
        window.loadExtrasAndUpdateFrames();
    });

    self.selectedLens.subscribe(function () {
        window.loadExtrasAndUpdateLens();
    });

    self.importModel = function (data) {
        if (data !== undefined && data !== null) {
            self.staff(data.EmployeeId);
            self.staffList(data.EmployeeList);
            $("select#staff").selectpicker("refresh");
            self.shipToList(data.ShipToList);
            $("select#shipTo").selectpicker("refresh");
            self.dispenseTypeList(data.DispenseTypeList);
            $("select#dispensingStatus").selectpicker("refresh");

            self.shipTo(data.ShipToType);
            self.address(data.ServiceLocationId);
            self.staff(data.EmployeeId);
            self.dispenseType(data.DispenseType);
            $("#suppliedBy, #dispensingStatus, #shipTo, #address, #staff").change();

            if (data.OrderNumber > 0 || data.CopyOrderNumber || (data.AwsResourceId !== null && data.AwsResourceId.length > 0)) {
                self.orderType(data.OrderType);
                self.eyeglassOrderType(data.EyeglassOrderType);
                self.eyeglassOrderTypeDescription(data.EyeglassOrderTypeDescription);
                if (data.EyeglassOrderType === egOrderType.FRAME_ONLY || data.EyeglassOrderType === egOrderType.EXTRAS_ONLY) {
                    $("#egSelectedRxPanel").addClass("hidden");
                }
                $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + data.EyeglassOrderTypeDescription + ") : Summary");
                self.orderNumber(data.OrderNumber);
                self.patientExamId(data.PatientExamId);
                self.addressList(data.AddressList);

                self.eligibilityId(data.InsuranceEligibilityId);
                self.selectedRx(data.EyeglassRx);
                self.selectedFrame(data.Frame);

                if (data.InsurancePricing !== null) {
                    self.selectedAuthorization(data.InsurancePricing.PrimaryInsurance);
                }
                window.setExtrasVisibility();
                self.selectedExtras(data.Extras);
                window.setLabsVisibility();
                self.selectedLab(data.Lab);
                self.selectedLens(data.Lenses);
                self.selectedExtras(data.Extras);
                self.dispenseNote(data.DispenseNote);
                self.insuranceVM(window.InsPanelViewModel);
                self.labInstructions(data.LabInstructions);
                self.officeNum(data.OfficeNum);
                self.rxDate(data.RxDate);
                self.toMakeRightLens(data.ToMakeRightLens);
                self.toMakeLeftLens(data.ToMakeLeftLens);
                self.toMakeExtras(data.ToMakeExtras);
                self.toMakeFrame(data.ToMakeFrame);
                self.isInvoiced(data.IsInvoiced);
                self.isOrderStatusLabOnHold(data.IsOrderStatusLabOnHold);
                self.orderIsValid(data.OrderIsValid);
                self.remakeOrder(data.RemakeOrder);
                self.remake(data.Remake);
                self.remakeReasons(data.RemakeReasons);
                $("select#remakeReasons").selectpicker("refresh");
                self.remakeTypeId(data.RemakeTypeId);
                $("#remakeReasons").change();
                self.copyOrderIsVsp(data.IsVsp);
                self.isDirty(false);
            }

            if ((egSummaryInitialized && eyeglassOrderViewModel.remake()) || (eyeglassOrderViewModel.eyeglassOrderType() === window.egOrderType.EXTRAS_ONLY) || (data.InsurancePricing !== null && data.InsurancePricing.IsEstimated === false)) {
                $("#btnRxChangeInsurance").addClass("hidden");
            } else {
                $("#btnRxChangeInsurance").removeClass("hidden");
            }
        }
    };
};

var switchPanelPosition = function () {
    var div, contains;
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.FRAME_ONLY) {
        contains = $.contains($('#labInfoPanelContainer').get(0), $('#summaryFramePanel').get(0));
        if (!contains) {
            div = $('#summaryFramePanel').detach();
            $('#labInfoPanelContainer').append(div);
        }
    } else if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY) {
        contains = $.contains($('#summaryEgRxPanelContainer').get(0), $('#summaryLensPanel').get(0));
        if (!contains) {
            $("#fillerPatientOwnFrame").addClass("hidden");
            div = $('#summaryLensPanel').detach();
            $('#summaryEgRxPanelContainer').append(div);
            div = $('#extrasInfoPanel').detach();
            $('#summaryEgRxPanelContainer').append(div);
        }
    }
};

function updateSelectedEyeglassOrderType(type, description) {
    eyeglassOrderViewModel.eyeglassOrderType(type);
    eyeglassOrderViewModel.eyeglassOrderTypeDescription(description);
    if (type === egOrderType.FRAME_ONLY || type === egOrderType.EXTRAS_ONLY) {
        $("#egSelectedRxPanel").addClass("hidden");
    }
}

function updateEyeglassInsurancePanelFromBESResponse(itemCharges, errorMessage) {
    if (errorMessage !== undefined && errorMessage !== null && errorMessage !== "") {
        window.updateServerError("Insurance estimated charges are unavailable.");
        window.clearInsurancePricingPanelDetailItems();
    } else {
        window.updateServerError("");
    }
    if (itemCharges) {
        if (itemCharges.length > 0) {
            window.removeSecondaryEligibility();
        }
        window.updateInsurancePricingDetails(itemCharges);
    }
}

/* Set all fields on the Summary screen as Read Only (disabled)*/
function makeSumPageReadOnly(isOrderStatusLabOnHold) {
    $("#summary :input").prop("disabled", true);
    $("#summary :input").removeClass("requiredField");
    $("#instructions, #dispenseNote").removeAttr("disabled");
    //$("#remakeIns").addClass("hidden");
    $("#btnSummaryChangeLab, #btnSummaryChangeFrame, #btnSummaryChangeRx, #btnSummaryChangeLens, #btnSummaryChangeInsurance, #btnSummaryChangeExtras, #btnCancelOrder, #btnGoToMaterialOrders, #btnSaveForLater, #btnContinueToPricing").removeClass("visible").addClass("hidden");
    $("#btnSave").removeClass("hidden").addClass("visible");
    $("#btnGoToMaterialOrders").removeClass("hidden").addClass("visible");
    $("#btnGoToMaterialOrders").removeAttr("disabled");
    $("#btnCancelOrder").addClass("hidden");
    if (isOrderStatusLabOnHold) {
        $("#btnSummaryChangeLab, #btnSummaryChangeRx, #btnSummaryChangeLens").removeClass("hidden").addClass("visible");
        $("#btnSummaryChangeLab, #btnSummaryChangeRx, #btnSummaryChangeLens").removeAttr("disabled");
    }
    $("#instructions").focus();
}
function getCopiedEyeglassOrderDetails() {
    client
        .action("GetCopyEyeglassOrderDetail")
        .get({
            patientId: window.patientOrderExam.PatientId,
            copyOrderNumber: window.patientOrderExam.CopyOrderId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                // initialize viewmodels
                if (!window.InsPanelViewModel) {
                    window.initInsurancePanel("EyeglassOrder");
                }

                // import the view model and refresh all select boxes  w/current data
                eyeglassOrderViewModel.importModel(data);
                $("select").selectpicker("refresh");

                // fire off the change event to update the UI styling for these inputs
                $("#dispensingStatus, #shipTo, #address").change();

                updateEyeglassInsurancePanelFromEgOrder(data);
                egInsurancePanel.updateEstimatedCharges = true;
                switchPanelPosition();
                onChangePricingPanel(false);

                window.hideAll();
                window.setupPage(buildOrder.SUMMARY);
                $("#summary").removeClass("hidden");
                $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
                $("#btnSummaryChangeInsurance").click();
            }
        });
}
function getEyeglassOrderDetails() {
    client
        .action("GetEyeglassOrderDetail")
        .get({
            patientId: window.patientOrderExam.PatientId,
            orderNumber: window.patientOrderExam.OrderId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                // initialize viewmodels
                if (!window.InsPanelViewModel) {
                    window.initInsurancePanel("EyeglassOrder");
                }
                eyeglassOrderViewModel.importModel(data);
                $("select").selectpicker("refresh");
                // Render page in Read-Only for invoiced exams
                if (data.IsInvoiced) {
                    makeSumPageReadOnly(data.IsOrderStatusLabOnHold);
                    $("#btnContinueToPricing").addClass("hidden");
                } else {
                    $("#btnContinueToPricing").removeClass("hidden");
                }
                // fire off the change event to update the UI styling for these inputs
                $("#dispensingStatus, #shipTo, #address").change();

                if (data.OrderNumber > 0) {
                    updateEyeglassInsurancePanelFromEgOrder(data);
                } else {
                    egInsurancePanel.updateEstimatedCharges = true;
                    onChangePricingPanel(true);
                }
                setTimeout(function () {
                    window.updatePanelBuildOrderPage();
                    switchPanelPosition();
                }, 300);
            }
        });
}

function getIsVsp() {
    if ((eyeglassOrderViewModel.insuranceVM() !== undefined && eyeglassOrderViewModel.insuranceVM() !== null && eyeglassOrderViewModel.insuranceVM().PrimaryEligibility.IsVsp === true)
            || (eyeglassOrderViewModel.selectedAuthorization() !== undefined && eyeglassOrderViewModel.selectedAuthorization() !== null && eyeglassOrderViewModel.selectedAuthorization().IsVsp === true)) {
        return true;
    }
    return false;
}

function buildReportLines() {
    var data = window.InsPanelViewModel.DetailItems();
    var list = [];

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

    return list;
}

function getEyeglassOrderDetailsFromViewModel() {

    var labId = 0;
    if (eyeglassOrderViewModel.selectedLab() !== null) {
        labId = eyeglassOrderViewModel.selectedLab().Id;
    }
    var order = {
        PatientId: window.patientOrderExam.PatientId,
        AwsResourceId: window.patientOrderExam.AwsResourceId,
        OrderNumber: eyeglassOrderViewModel.orderNumber(),
        OfficeNum: eyeglassOrderViewModel.officeNum(),
        EmployeeId: eyeglassOrderViewModel.staff(),
        DoctorId: eyeglassOrderViewModel.doctorId(),
        DoctorFullName: eyeglassOrderViewModel.doctor(),
        DispenseType: eyeglassOrderViewModel.dispenseType(),
        DispenseNote: eyeglassOrderViewModel.dispenseNote(),
        OrderType: eyeglassOrderViewModel.orderType(),
        EyeglassOrderType: eyeglassOrderViewModel.eyeglassOrderType(),
        EyeglassOrderTypeDescription: eyeglassOrderViewModel.eyeglassOrderTypeDescription(),
        InsuranceEligibilityId: eyeglassOrderViewModel.eligibilityId(),
        InsurancePricing: {
            IsEstimated: window.InsPanelViewModel.IsEstimated(),
            PrimaryInsurance: window.getPrimaryEligibility(),
            SecondaryInsurance: window.getSecondaryEligibility()
        },
        LabId: labId,
        LabInstructions: eyeglassOrderViewModel.labInstructions(),
        ShipToType: eyeglassOrderViewModel.shipTo(),
        ShipTo: eyeglassOrderViewModel.address(),
        ServiceLocationId: eyeglassOrderViewModel.address(),
        RemakeOrder: eyeglassOrderViewModel.remakeOrder(),
        RemakeTypeId: eyeglassOrderViewModel.remakeTypeId(),
        Remake: eyeglassOrderViewModel.remake(),
        RxDate: eyeglassOrderViewModel.rxDate(),
        Extras: eyeglassOrderViewModel.selectedExtras(),
        Frame: eyeglassOrderViewModel.selectedFrame(),
        Lenses: eyeglassOrderViewModel.selectedLens(),
        EyeglassRx: eyeglassOrderViewModel.selectedRx(),
        PatientExamId: eyeglassOrderViewModel.patientExamId(),
        Lab: eyeglassOrderViewModel.selectedLab(),
        ToMakeFrame: eyeglassOrderViewModel.toMakeFrame(),
        ToMakeLeftLens: eyeglassOrderViewModel.toMakeLeftLens(),
        ToMakeRightLens: eyeglassOrderViewModel.toMakeRightLens(),
        ToMakeExtras: eyeglassOrderViewModel.toMakeExtras(),
        OrderIsValid: eyeglassOrderViewModel.orderIsValid(),
        ReportLines: buildReportLines()
    };
    return order;
}

$("#btnSave").click(function (e) {
    e.preventDefault();
    if ($("#egOrderSummaryForm").valid()) {
        var order = getEyeglassOrderDetailsFromViewModel();
        client
            .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
            .action("SaveEyeglassOrder")
            .put(order)
            .done(function () {
                eyeglassOrderViewModel.isDirty(false);
                $(document).showSystemSuccess("Eyeglass Order successfully saved.");
            });
    } else {
        // scroll to the first invalid element
        $("html, body").animate({
            scrollTop: 0
        }, 500);
    }
});
function showRemakesWarningModal(title, message) {
    if (!title || title.length === 0 || !message || message.length === 0) {
        return;
    }

    $('#egOrderRemakesMessageModal .modal-title').html(title);
    $('#egOrderRemakesMessageModal #egOrderRemakesMessage').html(message);
    $("#egOrderRemakesMessageModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
}
/* Click event handler for "Continue to Pricing" button */
$("#btnContinueToPricing").click(function (e) {
    e.preventDefault();
    var redirectUrl;
    if (egSummaryInitialized && !eyeglassOrderViewModel.isDirty() && !eyeglassOrderViewModel.remake() && window.patientOrderExam.OrderId > 0) {
        urlRedirect = true;
        redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oid=" + window.patientOrderExam.OrderId;
        window.location.href = redirectUrl;
    } else {
        if ($("#egOrderSummaryForm").valid()) {
            var order = getEyeglassOrderDetailsFromViewModel();
            client
                .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
                .action("SaveEyeglassOrder")
                .put(order)
                .done(function (data) {
                    var message;
                    eyeglassOrderViewModel.isDirty(false);
                    if (data.remakeMessageType === 1) {
                        $("#btnEgOrderRemakesMessageOk").attr('data-value', data.savedOrderNumber);
                        message = "This remake order does not reflect a change in product. Original pricing will copy over.";
                        showRemakesWarningModal("Remakes Pricing", message);
                    } else if (data.remakeMessageType === 2) {
                        $("#btnEgOrderRemakesMessageOk").attr('data-value', data.savedOrderNumber);
                        message = "This remake order reflects a change in product. Original pricing will not copy over. Price order with insurance and/or discounts that were on the original order.";
                        showRemakesWarningModal("Remakes Pricing", message);
                    } else {
                        urlRedirect = true;
                        redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oid=" + data.savedOrderNumber;
                        window.location.href = redirectUrl;
                    }
                });
        } else {
            // scroll to the first invalid element
            $("html, body").animate({
                scrollTop: 0
            }, 500);
        }
    }
});
$("#btnEgOrderRemakesMessageOk").click(function () {
    $("#egOrderRemakesMessageModal").modal("toggle");
    var savedOrderNumber = $("#btnEgOrderRemakesMessageOk").attr("data-value");
    urlRedirect = true;
    var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oid=" + savedOrderNumber;
    window.location.href = redirectUrl;
});
$("#btnGoToMaterialOrders").click(function (e) {
    e.preventDefault();
    urlRedirect = true;
    var redirectUrl = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=" + "0";
    window.location.href = redirectUrl;
});

/* Validates the summary page */
function validateEyeglassOrderSummary() {
    $("#egOrderSummaryForm").alslValidate({
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
            },
            instructions: {
                Regex: "^[a-zA-Z 0-9_.\"]*$"
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
                required: "Select a Ship To type."
            },
            address: {
                required: "Select an Address."
            },
            instructions: {
                Regex: "The special characters you entered are not allowed. Enter valid Lab Instructions."
            }
        }
    });
}
var initEyeglassSummaryPageFromCopyOrder = function () {
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Summary");

    // reset the page (hide all divs)
    window.hideAll();

    if (!egSummaryInitialized) {
        egSummaryInitialized = true;
        validateEyeglassOrderSummary();
        if (window.patientOrderExam.OrderId === 0 && (window.patientOrderExam.CopyOrderId !== undefined && window.patientOrderExam.CopyOrderId !== null && window.patientOrderExam.CopyOrderId > 0)) {
            getCopiedEyeglassOrderDetails();
        }
    }

    // show the page
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    } else {
        $("#btnSaveForLater").removeClass("hidden");
    }

    if (customFrame.fromAdmin) {
        setTimeout(function () {
            $("#btnSummaryChangeFrame").click();
        }, 500);
    }
    onChangePricingPanel(true);
};

var initEyeglassSummaryPage = function () {
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Summary");
    window.hideAll();
    window.setupPage(buildOrder.SUMMARY);
    if (!egSummaryInitialized) {
        egSummaryInitialized = true;
        validateEyeglassOrderSummary();
        getEyeglassOrderDetails();
    }

    // show the page
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    } else {
        $("#btnSaveForLater").removeClass("hidden");
    }

    $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
    $("#summary").removeClass("hidden");

    //switch frame div position if any
    switchPanelPosition();

    if (customFrame.fromAdmin) {
        setTimeout(function () {
            $("#btnSummaryChangeFrame").click();
        }, 500);
    }
};

var initEgSummaryPageFromTempStorage = function () {
    if (!egSummaryInitialized) {
        egSummaryInitialized = true;
        validateEyeglassOrderSummary();
    }

    client
        .action("GetInProgressEyeglassOrder")
        .get({
            patientId: window.patientOrderExam.PatientId,
            awsResourceId: window.patientOrderExam.AwsResourceId
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                if (!window.InsPanelViewModel) {
                    window.initInsurancePanel("EyeglassOrder");
                }

                eyeglassOrderViewModel.importModel(data);
                var auth = eyeglassOrderViewModel.selectedAuthorization();
                if (auth !== null && auth !== undefined) {
                    auth.InsuranceEligibilityId = auth.EligId;
                }

                updateEyeglassInsurancePanelFromEgOrder(data);
                window.initExtrasPageFromTempStorage(data);

                setTimeout(function () {
                    window.updatePanelBuildOrderPage();
                    switchPanelPosition();
                }, 300);

                eyeglassOrderViewModel.orderIsValid(validateBuildOrder());
                egInsurancePanel.updateEstimatedCharges = true;
                onChangePricingPanel(false);

                if (customFrame.fromAdmin) {
                    setTimeout(function () {
                        $("#btnSummaryChangeFrame").click();
                    }, 500);
                } else if (eyeglassOrderViewModel.orderIsValid()) {
                    $("#summary").removeClass("hidden");
                    $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
                    window.setupPage(buildOrder.SUMMARY);
                } else {
                    window.initChooseBuildOrderPage();
                }
            }
        });
};

function initSelectedAuthFromTempStorage(data) {
    eyeglassOrderViewModel.selectedAuthorization(data.InsurancePricing.PrimaryInsurance);
}

function updateSelectedAuthorization(eligibility, change) {
    var updateEstimatedCharges = false;
    if (eligibility.InsuranceEligibilityId !== eyeglassOrderViewModel.eligibilityId()) {
        if (eyeglassOrderViewModel.selectedFrame() !== null || eyeglassOrderViewModel.selectedExtras() !== null || eyeglassOrderViewModel.selectedLens() !== null) {
            updateEstimatedCharges = true;
            egInsurancePanel.updateEstimatedCharges = true;
        }
    }

    eyeglassOrderViewModel.selectedAuthorization(eligibility);
    eyeglassOrderViewModel.selectedAuthorization().IsValid = true;
    eyeglassOrderViewModel.selectedAuthorization().ValidationMessage = "";
    eyeglassOrderViewModel.eligibilityId(eligibility.InsuranceEligibilityId);
    eyeglassOrderViewModel.isDirty(true);

    // hide the Choose Insurance page
    $("#chooseInsurance").addClass("hidden");
    $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");

    //return where it belongs to
    if (goBackToPage.fromInsurance === '') {
        window.initChooseOrderTypePage();
    } else {
        switch (goBackToPage.fromInsurance) {
        case "buildOrder":
            window.initChooseBuildOrderPage();
            break;
        case "chooseRx":
            window.initChooseEyeglassRxPage();
            break;
        case "chooseLenses":
            window.initChooseOrderLensesPage();
            break;
        case "chooseFrame":
            window.initChooseOrderFramePage();
            break;
        case "chooseExtras":
            window.initChooseEyeglassExtrasPage();
            break;
        case "chooseLab":
            window.initChooseEyeglassLabPage();
            break;
        case "summary":
            if (change === '') {
                $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
                $("#summary").removeClass("hidden");
                $("#btnContinueToPricing").removeClass("hidden");
            } else {
                var i, v = change.split(',');
                for (i = 0; i < v.length; i++) {
                    if (v[i] === buildOrder.LENSES || v[i] === buildOrder.EXTRAS) {
                        updateEstimatedCharges = true;
                    }
                }
                buildOrder.warningMessage = change;
                window.initChooseBuildOrderPage();
            }
            break;
        }

        goBackToPage.fromInsurance = '';
        if (updateEstimatedCharges) {
            setTimeout(function () {
                egInsurancePanel.updateEstimatedCharges = true;
                onChangePricingPanel();
            }, 300);
        }
    }
}

function changeSelectedAuthorization(eligibility) {
    selectedEligibility = null;
    $("#btnChangeSelection").attr('data-value', '');
    var value = "";
    var equivalent = false;
    var info = '<ul>';
    var auth = eyeglassOrderViewModel.selectedAuthorization();
    if (auth) {
        if (auth.IsVsp && !eligibility.IsVsp && eyeglassOrderViewModel.selectedLab()) {     //change VSP to Non VSP
            //equivalent = window.isEquivalentNonVspLab(eyeglassOrderViewModel.selectedLab());
            selectedEligibility = eligibility;
            info += '<li>Labs</li></ul>';
            value = "3";
        } else if (!auth.IsVsp && eligibility.IsVsp) {      //change Non VSP to VSP
            if (eyeglassOrderViewModel.selectedLens()) {
                selectedEligibility = eligibility;
                info += '<li>Lenses</li>';
                value = "1";
            }
            if (eyeglassOrderViewModel.selectedExtras()) {
                var extrasWarning = false;
                ko.utils.arrayForEach(eyeglassOrderViewModel.selectedExtras(), function (v) {
                    if (v.IsVspEligible === false) {
                        extrasWarning = true;
                    }
                });
                if (extrasWarning) {
                    selectedEligibility = eligibility;
                    info += '<li>Extras</li>';
                    value += value === '' ? '2' : ',2';
                }
            }
            if (eyeglassOrderViewModel.selectedLab()) {
                //equivalent = window.isEquivalentVspLab(eyeglassOrderViewModel.selectedLab());
                selectedEligibility = eligibility;
                info += '<li>Lab</li>';
                value += value === '' ? '3' : ',3';
            }
            info += '</ul>';

            //copy order
            if (eyeglassOrderViewModel.isCopyOrder()) {
                if (eyeglassOrderViewModel.copyOrderIsVsp()) {
                    selectedEligibility = null;
                }
            }
        }
    }

    if (selectedEligibility === null) {
        updateSelectedAuthorization(eligibility, '');
        eyeglassOrderViewModel.selectedAuthorization().IsValid = true;
    } else {
        var msg = "You have selected to change the insurance for this order, which may alter your selections in the following areas:<br/><br/>";
        msg += info;
        $("#msgDialog").html(msg);
        $("#btnChangeSelection").attr('data-value', value);
        $('#changeSelectionModal').modal({
            keyboard: false,
            backdrop: 'static',
            show: true
        });
    }
}

/* function to show or hide general error message 
 * Param: module - optionally pass the module to validate against.  Function will look at all other modules and if all others are valid, or invalid, will the toggle the msg accordingly
 */
function toggleGeneralError(show, module) {
    if (show) {
        $("#egBuildOrderMsgPanel #msgWarning").addClass("hidden");
        $("#egBuildOrderMsgPanel #msgError").removeClass("hidden");
    } else if (module !== undefined && module !== null) {
        var frameIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedFrame(), buildOrder.FRAME, false);
        var lensesIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLens(), buildOrder.LENSES, false);
        var extrasIsValid = validateExtras(false);
        var labIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLab(), buildOrder.LAB, false);
        var rxIsValid = validateRx(false);
        var authIsValid = validateAuth(false);
        var clearMessage = frameIsValid && lensesIsValid && extrasIsValid && labIsValid && rxIsValid && authIsValid;

        if (clearMessage) {
            $("#egBuildOrderMsgPanel #msgError").addClass("hidden");
        } else {
            $("#egBuildOrderMsgPanel #msgError").removeClass("hidden");
        }
    } else {
        $("#egBuildOrderMsgPanel #msgError").addClass("hidden");
    }
}

/* function to set a Build Order tile to an "error" state */
function setBuildOrderTileError(module) {
    var tile = "#buildOrder_" + module;
    var icon = "#buildOrder_" + module + " #statusIcon_" + module;
    $(tile).removeClass("complete").addClass("error");
    $(icon).removeClass("icon-checkmark-circle hidden").addClass("icon-notification");
    toggleGeneralError(true);
}

/* function to set a Build Order tile to a "completed" state */
function setBuildOrderTileComplete(module) {
    var tile = "#buildOrder_" + module;
    var icon = "#buildOrder_" + module + " #statusIcon_" + module;
    $(tile).removeClass("error").addClass("complete");
    $(icon).removeClass("icon-notification hidden").addClass("icon-checkmark-circle");
    toggleGeneralError(false, module);
}

/* function to set a Build Order tile to a "reset/default" state */
function setBuildOrderTileReset(module) {
    var tile = "#buildOrder_" + module;
    var icon = "#buildOrder_" + module + " #statusIcon_" + module;
    $(tile).removeClass("error complete");
    $(icon).removeClass("icon-notification").addClass("icon-checkmark-circle hidden");
    toggleGeneralError(false, module);
}

/* function to set an Rx/Ins tile to an "error" state */
function setRxInsTileError(module) {
    $(module).find(".panel").addClass("error");
    $(module).find(".errorMsgTxt").html(rx.ValidationMessage);
    $(module).find(".errorMsg").removeClass("hidden");
    toggleGeneralError(true);
}

/* function to set an Rx/In tile to a "reset/default" state */
function setRxInsTileReset(module) {
    $(module).find(".panel").removeClass("error");
    $(module).find(".errorMsgTxt").html("");
    $(module).find(".errorMsg").addClass("hidden");
    toggleGeneralError(false, module);
}

function updateSelectedRx(rx) {
    eyeglassOrderViewModel.selectedRx(rx);
    setRxInsTileReset("#egSelectedRxPanel");
    eyeglassOrderViewModel.isDirty(true);
    if (eyeglassOrderViewModel !== null && egSummaryInitialized && goBackToPage.fromAddToOrder !== 'buildOrder') {
        window.initEyeglassSummaryPage();
    } else {
        window.initChooseBuildOrderPage();
    }
}

function updateSelectedEntity(panelIndex, entity) {
    switch (panelIndex) {
    case buildOrder.FRAME:
        var frame = eyeglassOrderViewModel.selectedFrame();
        if (frame === null || (frame && frame.Id !== entity.Id)) {
            if (frame === null && entity !== null) {
                egInsurancePanel.updateFrame = true;
            } else if (eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.LENSES_ONLY && eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.EXTRAS_ONLY) {
                egInsurancePanel.updateFrame = true;
            }
        }

        eyeglassOrderViewModel.selectedFrame(entity);
        eyeglassOrderViewModel.isDirty(true);
        window.updateLayoutBuildOrderPage();
        break;

    case buildOrder.LENSES:
        var lens = eyeglassOrderViewModel.selectedLens();
        if (lens !== null && entity !== null) {
            var leftLens = lens.LeftLens;
            var rightLens = lens.RightLens;
            var entLeftLens = entity.LeftLens;
            var entRightLens = entity.RightLens;

            if (leftLens && entLeftLens) {
                if (leftLens.MaterialTypeId !== entLeftLens.MaterialTypeId || leftLens.StyleId !== entLeftLens.StyleId ||
                        leftLens.ColorId !== entLeftLens.ColorId || leftLens.TypeId !== entLeftLens.TypeId || leftLens.MfgCoatingId !== entLeftLens.MfgCoatingId) {
                    egInsurancePanel.updateLens = true;
                }
            } else if ((leftLens === null && entLeftLens !== null) || (leftLens !== null && entLeftLens === null)) {
                egInsurancePanel.updateLens = true;
            }

            if (rightLens && entRightLens) {
                if (rightLens.MaterialTypeId !== entRightLens.MaterialTypeId || rightLens.StyleId !== entRightLens.StyleId ||
                        rightLens.ColorId !== entRightLens.ColorId || rightLens.TypeId !== entRightLens.TypeId || rightLens.MfgCoatingId !== entRightLens.MfgCoatingId) {
                    egInsurancePanel.updateLens = true;
                }
            } else if ((rightLens === null && entRightLens !== null) || (rightLens !== null && entRightLens === null)) {
                egInsurancePanel.updateLens = true;
            }

            if (lens.TintTypeId !== entity.TintTypeId || lens.TintColorId !== entity.TintColorId || lens.LensEdgeTypeId !== entity.LensEdgeTypeId || lens.AddCoatingsId !== entity.AddCoatingsId) {
                egInsurancePanel.updateLens = true;
            }

        } else {
            egInsurancePanel.updateLens = true;
        }

        eyeglassOrderViewModel.selectedLens(entity);
        eyeglassOrderViewModel.isDirty(true);
        break;

    case buildOrder.LAB:
        eyeglassOrderViewModel.selectedLab(entity);
        eyeglassOrderViewModel.isDirty(true);
        break;

    case buildOrder.EXTRAS:
        var i, j, found;
        var extras = eyeglassOrderViewModel.selectedExtras();
        if (extras !== null) {
            if (entity === null || entity.length !== extras.length) {
                egInsurancePanel.updateExtras = true;
            } else {
                for (i = 0; i < entity.length; i++) {
                    found = false;
                    for (j = 0; j < extras.length && !found; j++) {
                        if (entity[i].ItemId === extras[j].ItemId) {
                            found = true;
                        }
                    }
                    if (!found) {
                        egInsurancePanel.updateExtras = true;
                        break;
                    }
                }
            }
        } else if (entity !== null) {
            egInsurancePanel.updateExtras = true;
        }

        eyeglassOrderViewModel.selectedExtras(entity);
        eyeglassOrderViewModel.isDirty(true);
        break;
    }

    onChangePricingPanel();
}

function hideAll() {
    $("#orderType").addClass("hidden");
    $("#buildOrder").addClass("hidden");
    $("#chooseExtras").addClass("hidden");
    $("#chooseLab").addClass("hidden");
    $("#chooseLenses").addClass("hidden");
    $("#chooseFrame").addClass("hidden");
    $("#chooseRx").addClass("hidden");
    $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
    $("#summary").addClass("hidden");
    $("#btnCancelOrder").addClass("hidden");
    $("#btnGoToMaterialOrders").addClass("hidden");
    $("#btnSaveForLater").addClass("hidden");
    $("#btnContinue").addClass("hidden");
}

var displayPage = function (panelIndex, msg) {
    if (goBackToPage.fromAddToOrder === 'summary') {
        goBackToPage.fromAddToOrder = '';
        window.hideAll();
        window.initEyeglassSummaryPage();
    } else {
        window.initChooseBuildOrderPage(panelIndex);
    }

    if (msg !== "") {
        $(document).showSystemSuccess(msg);
    }
};

// clear temp save dialog ui
var resetTempEgOrderModal = function () {
    $("#resourceId").clearField();
    $("#resourceId").val("");
    $("#resourceId").addClass("requiredField");
    $(".summaryMessages").clearMsgBlock();
    $("#resourceId").focus();
};

function clearLensLists(data) {
    if (data.Lenses) {
        data.Lenses.LensTypes = null;
        data.Lenses.LensMaterial = null;
        data.Lenses.LensStyles = null;
        data.Lenses.LensColors = null;
        data.Lenses.LensMfgCoatings = null;
        data.Lenses.LensTintTypes = null;
        data.Lenses.LensTintColors = null;
        data.Lenses.LensEdgeTypes = null;
        data.Lenses.LensAddCoatings = null;
        if (data.Lenses.RightLens !== null && data.Lenses.RightLens !== undefined) {
            data.Lenses.RightLens.LensMaterial = null;
            data.Lenses.RightLens.LensStyles = null;
            data.Lenses.RightLens.LensColors = null;
            data.Lenses.RightLens.LensMfgCoatings = null;
        }

        if (data.Lenses.LeftLens !== null && data.Lenses.LeftLens !== undefined) {
            data.Lenses.LeftLens.LensMaterial = null;
            data.Lenses.LeftLens.LensStyles = null;
            data.Lenses.LeftLens.LensColors = null;
            data.Lenses.LeftLens.LensMfgCoatings = null;
        }
    }
}

function showInProgressDialog() {
    resetTempEgOrderModal();
    $("#duplicateMsg").addClass("hidden");
    var title = "Save Draft";
    $("#tempStorageEgSaveModal .modal-title").html(title);
    $("#tempStorageEgSaveModal").data("id", 0).modal({
        keyboard: false,
        backdrop: "static",
        show: true
    });
    $("#tempStorageEgSaveModal").on('shown.bs.modal', function () {
        $(this).find('#resourceId').focus();
    });
}

/* Validates the user-defined name when saving to temporary storage (WIP)*/
var validateTempEgOrderName = function () {
    $("#duplicateMsg").removeClass("visible");
    $("#duplicateMsg").addClass("hidden");
    $("#saveTempEgOrder").alslValidate({
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

/* Validates that a module has data and is valid */
function validateRequiredModule(module, buildOrderTile, required) {
    var valid = true;
    if ((required && !module) || (module && !module.IsValid)) {
        setBuildOrderTileError(buildOrderTile);
        valid = false;
    }
    return valid;
}

/* Validates that all extras are valid.  if not - sets build order tile error*/
function validateExtras(required) {
    var valid = true;
    if ((required && !eyeglassOrderViewModel.selectedExtras()) || (required && eyeglassOrderViewModel.selectedExtras().length === 0)) {
        setBuildOrderTileError(buildOrder.EXTRAS);
        valid = false;
    } else if (eyeglassOrderViewModel.selectedExtras()) {
        $.each(eyeglassOrderViewModel.selectedExtras(), function (idx, val) {
            if (!val.IsValid) {
                setBuildOrderTileError(buildOrder.EXTRAS);
                valid = false;
            }
        });
    }
    return valid;
}

function isIof(name) {
    return (name && name.toLowerCase().includes("iof"));
}
/* Validates that all extras are valid.  if not - sets build order tile error*/
function validateLabLensCompatibility() {
    var valid = true;
    var isVspInsurance = eyeglassOrderViewModel.selectedAuthorization() !== undefined && eyeglassOrderViewModel.selectedAuthorization() !== null && eyeglassOrderViewModel.selectedAuthorization().IsVsp;
    if (eyeglassOrderViewModel.selectedLens() && eyeglassOrderViewModel.selectedLab() && eyeglassOrderViewModel.selectedLens().IsIof) {
        if (isVspInsurance) {
            if (!isIof(eyeglassOrderViewModel.selectedLab().Name)) {
                setBuildOrderTileError(buildOrder.LAB);
                eyeglassOrderViewModel.selectedLab().ValidationMessage = "The selected lab does not support IOF lens. Please select an IOF lab.";
                eyeglassOrderViewModel.selectedLab().IsValid = false;
                valid = false;
            }
        } else {
            if (eyeglassOrderViewModel.selectedLab().IsSystemLab) {
                setBuildOrderTileError(buildOrder.LAB);
                eyeglassOrderViewModel.selectedLab().ValidationMessage = "eLabs do not support IOF lens. Please select a different lab.";
                eyeglassOrderViewModel.selectedLab().IsValid = false;
                valid = false;
            }
        }
    }
    return valid;
}

/* Validates the rx */
function validateRx(required) {
    var valid = true;
    if (required && !eyeglassOrderViewModel.selectedRx()) {
        eyeglassOrderViewModel.selectedRx().ValidationResultMessage = "Choose an Rx.";
        setRxInsTileError("#egSelectedRxPanel");
        valid = false;
    } else if (eyeglassOrderViewModel.selectedRx() && !eyeglassOrderViewModel.selectedRx().IsValid) {
        setRxInsTileError("#egSelectedRxPanel");
        valid = false;
    }
    return valid;
}

/* Validates the auth */
function validateAuth(required) {
    var valid = true;
    if (required && !eyeglassOrderViewModel.selectedAuthorization()) {
        eyeglassOrderViewModel.selectedAuthorization().ValidationResultMessage = "Choose an Insurance.";
        setRxInsTileError("#egSelectedInsurancePanel");
        $("#egInsPanel #egSelectedInsurancePanel #ErrorText").addClass("hidden");
        valid = false;
    } else if (eyeglassOrderViewModel.selectedAuthorization() && !eyeglassOrderViewModel.selectedAuthorization().IsValid) {
        setRxInsTileError("#egSelectedInsurancePanel");
        $("#egInsPanel #egSelectedInsurancePanel #ErrorText").addClass("hidden");
        valid = false;
    }
    return valid;
}

/* validates the Build Order page at a high-level, module level */
function validateBuildOrder() {
    var frameIsValid = false,
        lensesIsValid = false,
        extrasIsValid = false,
        labIsValid = false,
        rxIsValid = false,
        authIsValid = false;

    toggleGeneralError(false);
    switch (eyeglassOrderViewModel.eyeglassOrderType()) {

    case egOrderType.COMPLETE:
        frameIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedFrame(), buildOrder.FRAME, true);
        lensesIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLens(), buildOrder.LENSES, true);
        extrasIsValid = validateExtras(false);
        labIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLab(), buildOrder.LAB, true) && validateLabLensCompatibility();
        rxIsValid = validateRx(true);
        authIsValid = validateAuth(false);
        break;

    case egOrderType.EXTRAS_ONLY:
        frameIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedFrame(), buildOrder.FRAME, true);
        lensesIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLens(), buildOrder.LENSES, eyeglassOrderViewModel.selectedExtras() === null || eyeglassOrderViewModel.selectedExtras().length <= 0);
        extrasIsValid = validateExtras(eyeglassOrderViewModel.selectedLens() === null || !eyeglassOrderViewModel.selectedLens().IsValid);
        labIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLab(), buildOrder.LAB, true);
        rxIsValid = validateRx(false);
        authIsValid = validateAuth(false);
        break;

    case egOrderType.FRAME_ONLY:
        frameIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedFrame(), buildOrder.FRAME, true);
        lensesIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLens(), buildOrder.LENSES, false);
        extrasIsValid = validateExtras(false);
        labIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLab(), buildOrder.LAB, eyeglassOrderViewModel.selectedFrame() ? eyeglassOrderViewModel.selectedFrame().FrameSourceID === 3 : false);
        rxIsValid = validateRx(false);
        authIsValid = validateAuth(false);
        break;

    case egOrderType.LENSES_ONLY:
        frameIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedFrame(), buildOrder.FRAME, true);
        lensesIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLens(), buildOrder.LENSES, true);
        extrasIsValid = validateExtras(false);
        labIsValid = validateRequiredModule(eyeglassOrderViewModel.selectedLab(), buildOrder.LAB, true) && validateLabLensCompatibility();
        rxIsValid = validateRx(true);
        authIsValid = validateAuth(false);
        break;
    }

    // determines error states
    var orderIsValid = frameIsValid && lensesIsValid && extrasIsValid && labIsValid && rxIsValid && authIsValid;

    return orderIsValid;
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
                alert("error: no summary row in temp storage");
                return;
            }

            providerName = summary.ResourceDisplay;
            saveToTempStorageEg(data);
            $(document).showSystemSuccess("Eyeglass Order successfully saved.");
        });
}

function getDataForTempStorage() {
    var data = getEyeglassOrderDetailsFromViewModel();
    if (window.vmFrame !== null && window.vmFrame.isDirty()) {
        data.Frame = window.vmFrame.toModel("SaveRetailPrice");
    }

    if (window.orderExtrasViewModel !== null) {
        data.Extras = window.getSelectedExtrasFromUi();
    }

    if (window.vmLenses !== null && window.vmLenses.isDirty()) {
        data.Lenses = window.getSelectedLensesFromViewModel();
    }

    return data;
}
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
/* Click event handler for "Change Rx" button */
$("#btnChangeRx, #btnSummaryChangeRx, #quicklink_change_rx").click(function (e) {
    e.preventDefault();

    //remember where the panel is coming from
    goBackToPage.fromAddToOrder = 'buildOrder';

    window.initChooseEyeglassRxPage();
    $("#buildOrder, #chooseLab, #chooseExtras, #chooseLenses, #chooseFrame, #summary, #orderType").each(function () {
        if (!($(this).hasClass("hidden"))) {
            $(this).addClass("hidden");
        }
    });
    if ($("#egInsPanel").hasClass("hidden")) {
        $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
    }
});

/* Event handler for "Change Ins." button */
$("#btnRxChangeInsurance, #quicklink_change_insurance").click(function (e) {
    e.preventDefault();

    //remember where the panel is coming from
    goBackToPage.fromInsurance = '';
    var i, arr = ["buildOrder", "chooseRx", "chooseLenses", "chooseFrame", "chooseExtras", "chooseLab"];
    for (i = 0; i < arr.length && goBackToPage.fromInsurance === ''; i++) {
        if (!$('#' + arr[i]).hasClass("hidden")) {
            goBackToPage.fromInsurance = arr[i];
        }
    }

    // reset Auth tile
    eyeglassOrderViewModel.selectedAuthorization().IsValid = true;
    eyeglassOrderViewModel.selectedAuthorization().ValidationMessage = "";
    setRxInsTileReset("#egSelectedInsurancePanel");

    initChooseInsurancePage();

    $("#buildOrder, #chooseLab, #chooseExtras, #chooseLenses, #chooseFrame, #chooseRx, #summary, #orderType, #egInsPanel").each(function () {
        if (!($(this).hasClass("hidden"))) {
            $(this).addClass("hidden");
            if ($(this).is("#egInsPanel")) {
                $(this).removeClass("hidden-sm hidden-xs");
            }
        }
    });

    $("#btnContinue").addClass("hidden");
    $("#btnContinueToPricing").addClass("hidden");
});

$("#btnSummaryChangeFrame, #btnSummaryChangeLens, #btnSummaryChangeLab, #btnSummaryChangeExtras, #btnSummaryChangeInsurance").click(function (e) {
    e.preventDefault();
    switch (this.id) {
    case "btnSummaryChangeFrame":
        goBackToPage.fromAddToOrder = 'buildOrder';
        $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
        $("#buildOrder, #chooseLenses, #chooseExtras, #chooseRx, #chooseLab, #summary, #orderType").each(function () {
            $(this).addClass("hidden");
        });
        window.initChooseOrderFramePage();
        break;

    case "btnSummaryChangeLens":
        goBackToPage.fromAddToOrder = 'buildOrder';
        $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
        if (eyeglassOrderViewModel.isInvoiced() && eyeglassOrderViewModel.isOrderStatusLabOnHold()) {
            window.makeLensPageEditableForInvoicedLabOnHoldOrder();
        }
        $("#buildOrder, #chooseLab, #chooseExtras, #chooseFrame, #chooseRx, #summary, #orderType").each(function () {
            if (!($(this).hasClass("hidden"))) {
                $(this).addClass("hidden");
            }
        });
        window.initChooseOrderLensesPage();
        break;

    case "btnSummaryChangeLab":
        goBackToPage.fromAddToOrder = 'buildOrder';
        $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
        $("#buildOrder, #chooseLenses, #chooseExtras, #chooseFrame, #chooseRx, #summary, #orderType").each(function () {
            if (!($(this).hasClass("hidden"))) {
                $(this).addClass("hidden");
            }
        });
        window.initChooseEyeglassLabPage();
        break;

    case "btnSummaryChangeExtras":
        goBackToPage.fromAddToOrder = 'buildOrder';
        $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
        $("#buildOrder, #chooseLab, #chooseLenses, #chooseFrame, #chooseRx, #summary, #orderType").each(function () {
            if (!($(this).hasClass("hidden"))) {
                $(this).addClass("hidden");
            }
        });
        window.initChooseEyeglassExtrasPage();
        break;

    case "btnSummaryChangeInsurance":
        if (((window.patientOrderExam.OrderId !== undefined && window.patientOrderExam.OrderId !== null && window.patientOrderExam.OrderId > 0)
                || (eyeglassOrderViewModel.orderNumber() !== undefined && eyeglassOrderViewModel.orderNumber() !== null && eyeglassOrderViewModel.orderNumber() > 0))
                && (eyeglassOrderViewModel.remake() || (eyeglassOrderViewModel.insuranceVM().PrimaryEligibility() && eyeglassOrderViewModel.insuranceVM().IsEstimated() === false))) {
            $("#btnContinueToPricing").click();
        } else {
            goBackToPage.fromInsurance = "buildOrder";
            $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
            $("#buildOrder, #chooseLab, #chooseExtras, #chooseLenses, #chooseFrame, #chooseRx, #summary, #orderType, #egInsPanel").each(function () {
                if (!($(this).hasClass("hidden"))) {
                    $(this).addClass("hidden");
                    if ($(this).is("#egInsPanel")) {
                        $(this).removeClass("hidden-sm hidden-xs");
                    }
                }
            });
            initChooseInsurancePage();
            //prepare lab setup for copy order
            if (eyeglassOrderViewModel.isCopyOrder() && window.orderLabsViewModel === null) {
                window.initChooseEyeglassLabPage();
                $("#chooseLab").addClass("hidden");
            }
        }
        break;
    }

    $("#btnContinue").addClass("hidden");
    $("#btnContinueToPricing").addClass("hidden");
});

// selection change modal dialog
$("#btnChangeSelection").click(function () {
    $('#changeSelectionModal').modal("toggle");
    var change = $("#btnChangeSelection").attr('data-value');
    buildOrder.warningMessage = change;
    eyeglassOrderViewModel.selectedAuthorization().IsValid = true;
    if (selectedEligibility !== null) {     //Change Rx ALSL-6236
        updateSelectedAuthorization(selectedEligibility, change);
    }
    setTimeout(function () {
        $("#chooseExtras, #chooseRx, #chooseFrame, #selectedFrame, #chooseLab, #chooseLenses").addClass("hidden");
        egInsurancePanel.updateEstimatedCharges = true;
        window.initChooseBuildOrderPage();
    }, 300);
});

// global cancel button click
$("#btnCancelOrder").click(function (e) {
    $('#egOrderCancelModal').modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});

// print order summary button click
$("#btnPrintOrderSummary").click(function (e) {
    e.preventDefault();
    var order = getEyeglassOrderDetailsFromViewModel();
    $.ajax({
        url: window.config.baseUrl + "PatientReports/PrintEgOrderSummary?id=" + "resource",
        data: JSON.stringify(order),
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

// global save-for-later button click
$("#btnSaveForLater").click(function (e) {
    e.preventDefault();
    if (window.egDataSource === "new") {
        showInProgressDialog();
    } else if (window.egDataSource === "tempStorage") {
        var data = getDataForTempStorage();
        if (customFrame.id === null) {
            data.AwsResourceId = window.patientOrderExam.AwsResourceId;
            window.getIpSummaryAndSave(data);
        } else {
            var resourceId = $.urlParam(window.location.href, "resourceId", true);
            var arr = resourceId.split('z');
            if (arr.length === 5 && data.PatientId.toString() === arr[0]) {
                window.showInProgressDialog();
            } else {
                data.AwsResourceId = window.patientOrderExam.AwsResourceId;
                window.getIpSummaryAndSave(data);
            }
        }
    } else {
        alert("error: Order exists so temporary storage should have been disabled");
    }
});
$("#resourceId").keypress(function (e) {
    if (e.which === 13) {
        e.preventDefault();
        $("#btnSaveTempEgOrder").click();
        // hide the keyboard in iOS when the form is submitted 
        $(':focus').blur();
    }
});
/* Click Event Handler for Save WIP modal button */
$("#btnSaveTempEgOrder").click(function (e) {
    e.preventDefault();
    var r = $("#resourceId").val().trim();
    validateTempEgOrderName();
    if (!$("#saveTempEgOrder").valid()) {
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
                var data = getDataForTempStorage();
                data.AwsResourceId = r.trim();
                saveToTempStorageEg(data);
                if (!window.customFrame.redirect) {
                    $(document).showSystemSuccess("Eyeglass Order successfully saved.");
                }
            } else {
                $("#duplicateMsg").removeClass("hidden");
                $("#duplicateMsg").addClass("visible");
            }
        });
});

// global continue-to-summary button click
$("#btnContinue").click(function (e) {
    if (validateBuildOrder()) {
        var selAuth = eyeglassOrderViewModel.selectedAuthorization();
        if (selAuth && selAuth.IsVsp && window.InsPanelViewModel.BesError() === '') {
            if (!$("#divCalcInsurance").hasClass("hidden")) {
                egInsurancePanel.updateEstimatedCharges = true; //set it true to call Bess
            }
        }
        eyeglassOrderViewModel.orderIsValid(true);
        hideAll();
        $("#chooseInsurance").addClass("hidden");
        window.initEyeglassSummaryPage();
        eyeglassOrderViewModel.isDirty(true);
    }
});

$("#btnCancelEgOrder").click(function (e) {
    e.preventDefault();
    $("#egOrderCancelModal").modal("hide");
    var redirectUrl = window.config.baseUrl;
    if (egDataSource === "new") {
        redirectUrl += "Patient/EyeglassOrder?id=" + window.patientOrderExam.PatientId + "&resourceId=0&oId=0";
    } else {
        redirectUrl += "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=0";
    }
    urlRedirect = true;
    window.location.href = redirectUrl;
});

$("#detailedBreakdown").click(function (e) {
    $('#detailedBreakdownModal').modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});

function getBESEstimatesForEyeglassOrderSummaryPage() {
    var extras = eyeglassOrderViewModel.selectedExtras();
    var frame = eyeglassOrderViewModel.selectedFrame();
    var lens = eyeglassOrderViewModel.selectedLens();
    var rightLensId = 0, leftLensId = 0;
    if (lens !== undefined && lens !== null) {
        if (lens.RightLens !== null) {
            rightLensId = lens.RightLens.Id;
        }
        if (lens.LeftLens !== null) {
            leftLensId = lens.LeftLens.Id;
        }
    }
    var lab = eyeglassOrderViewModel.selectedLab();
    var auth = eyeglassOrderViewModel.selectedAuthorization();
    if (auth && auth.IsVsp === true && (frame !== null || extras !== null || lens !== null)) {
        var entityIds = getEntityIds().Ids;
        var doctorId = eyeglassOrderViewModel.doctorId() || 0;
        var examId = eyeglassOrderViewModel.patientExamId() || 0;
        var eligId = auth.InsuranceEligibilityId;

        var labId = 0;
        if (lab !== undefined && lab !== null) {
            labId = lab.ItemId;
        }
        client
            .action("GetBesResponseForEyeglassOrder")
            .get({
                patientId: window.patientOrderExam.PatientId,
                eligibilityId: eligId,
                doctorId: doctorId,
                labNum: labId,
                patientExamId: examId,
                rightLensId: rightLensId,
                leftLensId: leftLensId,
                entityIds: entityIds
            })
            .done(function (data) {
                eyeglassOrderViewModel.insuranceCharges(data.besResponse);
                window.removeAllPricingLineItems();
                updateEyeglassInsurancePanelFromBESResponse(data.besResponse, data.serverErrorMessage);
                if (auth && auth.IsValid && data.serverErrorMessage === '') {
                    $("#divCalcInsurance").addClass("hidden");
                    $("#calculations, #infoText, #detailedBreakdown").removeClass("hidden");

                    egInsurancePanel.updateLens = false;
                    egInsurancePanel.updateFrame = false;
                    egInsurancePanel.updateExtras = false;
                    egInsurancePanel.updateEstimatedCharges = false;
                }
            });
    }
}
 /* Click Event handler for Calc Ins Estimates button (BES) */
$("#btnCalcInsurance").click(function (e) {
    e.preventDefault();
    var extras = eyeglassOrderViewModel.selectedExtras();
    var frame = eyeglassOrderViewModel.selectedFrame();
    var lens = eyeglassOrderViewModel.selectedLens();
    var rightLensId = 0, leftLensId = 0;
    if (lens !== undefined && lens !== null) {
        if (lens.RightLens !== null) {
            rightLensId = lens.RightLens.Id;
        }
        if (lens.LeftLens !== null) {
            leftLensId = lens.LeftLens.Id;
        }
    }
    var lab = eyeglassOrderViewModel.selectedLab();
    var auth = eyeglassOrderViewModel.selectedAuthorization();
    if (auth && auth.IsVsp === true && (frame !== null || extras !== null || lens !== null)) {
        var entityIds = getEntityIds().Ids;
        var doctorId = eyeglassOrderViewModel.doctorId() || 0;
        var examId = eyeglassOrderViewModel.patientExamId() || 0;
        var labId = "";
        if (lab !== undefined && lab !== null) {
            labId = lab.Key;
        }
        client
            .action("GetBesResponseForEyeglassOrder")
            .get({
                patientId: window.patientOrderExam.PatientId,
                eligibilityId: auth.InsuranceEligibilityId,
                doctorId: doctorId,
                labNum: labId,
                patientExamId: examId,
                rightLensId: rightLensId,
                leftLensId: leftLensId,
                entityIds: entityIds
            })
            .done(function (data) {
                eyeglassOrderViewModel.insuranceCharges(data.besResponse);
                window.removeAllPricingLineItems();
                updateEyeglassInsurancePanelFromBESResponse(data.besResponse, data.serverErrorMessage);

                var auth = eyeglassOrderViewModel.selectedAuthorization();
                if (auth && auth.IsValid && data.serverErrorMessage === '') {
                    $("#divCalcInsurance").addClass("hidden");
                    $("#calculations, #infoText, #detailedBreakdown").removeClass("hidden");

                    egInsurancePanel.updateLens = false;
                    egInsurancePanel.updateFrame = false;
                    egInsurancePanel.updateExtras = false;
                }
            });
    }
});

var RxViewModel = function (data) {
    var self = this;
    self.rx = ko.observable(data);
};

function saveToTempStorageEg(data) {
    data.PatientId = window.patientOrderExam.PatientId;
    data.InsEligibilityId = 0;
    clearLensLists(data);
    client
        .queryStringParams({ orderNumber: window.patientOrderExam.OrderId })
        .action("SaveInProgressEyeglassOrder")
        .put(data)
        .done(function () {
            urlRedirect = true;
            if (customFrame.redirect) {
                var patientId = $.urlParam(window.location.href, "id", true);
                window.location.href = window.config.baseUrl + "Admin/ProductsServices/AddCustomFrame?id=0&pId=" + patientId + "&resourceId=" + data.AwsResourceId;
            } else {
                window.location.href = window.config.baseUrl + "Patient/MaterialOrders?id=" + window.patientOrderExam.PatientId + "&oId=" + "0";
            }
        });
}

/* Choose Insurance page  View-Model */
var EligibilitiesViewModel = function (data) {
    var self = this;
    self.insurances = ko.observableArray(data.insurances);
    self.egEligibilities = ko.observableArray();
    self.displayEgEligibilities = ko.observableArray();

    ko.utils.arrayForEach(self.insurances(), function (ins) {
        ko.utils.arrayForEach(ins.Eligibilities.Eligibilities, function (elig) {
            if (elig.IsFrameElig || elig.IsLensElig) {
                elig.AuthDate = convertDate(elig.AuthDate)[0];
                elig.AuthExpireDate = convertDate(elig.AuthExpireDate)[0];
                if (elig.AuthNumber === null || elig.AuthNumber.length <= 0) {
                    elig.AuthNumber = "N/A";
                }
                var carrierSubscriber = ins.CarrierDisplay;
                if (ins.Subscriber !== null) {
                    if (ins.Subscriber.DisplayName !== null) {
                        carrierSubscriber += " / Subscriber: " + ins.Subscriber.DisplayName;
                    }
                }
                self.egEligibilities.push({
                    NonInsurance: false,
                    IsVsp: ins.IsVsp,
                    CarrierFull: carrierSubscriber,
                    CarrierShort: ins.CarrierDisplay.split(" / ")[0],
                    CarrierId: ins.Id,
                    PlanId: ins.PlanId,
                    InsuranceEligibilityId : elig.InsuranceEligibilityId,
                    Eligibility: elig
                });
            }
        });
    });

    // add the Non-Insurance item manually
    self.egEligibilities.push({
        NonInsurance : true,
        IsVsp : false,
        CarrierFull : "Non-Insurance / Private Pay",
        CarrierShort : "Non-Insurance",
        CarrierId: 0,
        PlanId: 0,
        InsuranceEligibilityId : 0,
        Eligibility: {
            AuthNumber : 0
        }
    });

    //parse it into 2s to display better on the UI
    var tempEgEligibilities = self.egEligibilities().slice(0);
    while (tempEgEligibilities.length > 0) {
        self.displayEgEligibilities.push(tempEgEligibilities.splice(0, 2));
    }
};


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

/* Click Event Handler for the "No Auths Found" message on the Choose Insurance page */
$("div#msg_noAuthsWarning").click(function () {
    var redirectUrl = window.config.baseUrl + "Patient/InsuranceEligibility?id=" + window.patientOrderExam.PatientId;
    window.location.href = redirectUrl;
});

/* Initializes the Choose Insurance page */
initChooseInsurancePage = function () {
    if (window.patient.FirstName !== '') {
        $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order");
    }
    window.setupPage(buildOrder.INSURANCE);
    if (chooseInsuranceInitialized === true) {
        $("#chooseInsurance").removeClass("hidden");
        $("#egInsPanel").addClass("hidden").removeClass("hidden-sm hidden-xs");
        return;
    }

    client
        .action("GetValidEyeglassEligibilitiesByPatientId")
        .get({ patientId: window.patientOrderExam.PatientId })
        .done(function (data) {
            chooseInsuranceInitialized = true;
            //init the EligViewModel, then apply the KO bindings to the eligibilities div only
            EligViewModel = new EligibilitiesViewModel(data);
            ko.applyBindings(EligViewModel, $('#egEligibilities')[0]);

            // show no Auths message if no auths
            if (EligViewModel.egEligibilities().length <= 1) {
                $("#msg_noAuthsWarning").removeClass("hidden");
            }

            if (eyeglassOrderViewModel.isCopyOrder() === true) {
                $("#msgInsuranceWarning").removeClass("hidden");
            }

            // show the page once everything is done
            $("#chooseInsurance").removeClass("hidden");

            /* Click Event handler for all + Order buttons that have been dynamically generated*/
            $("button[id^='btnStartOrder']").click(function (e) {
                e.preventDefault();
                insEligId = $(this).data("insurance-eligibility-id");
                ko.utils.arrayForEach(EligViewModel.egEligibilities(), function (elig) {
                    if (elig.InsuranceEligibilityId === insEligId) {
                        changeSelectedAuthorization(elig);
                    }
                });
            });
        });
};

var validateEyeglassOrder = function () {
    $("#saveTempOrder").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            resourceId: { required: true }
        },
        messages: {
            resourceId: { required: "Enter the order name." }
        }
    });
};

$(document).ready(function () {
    loadPatientSideNavigation(window.patientOrderExam.PatientId, "egOrder");
    updatePageTitle();
    window.extrasLoaded = false;
    window.labsInitialized = false;
    var resourceId = window.patientOrderExam.AwsResourceId;
    var orderId = window.patientOrderExam.OrderId;
    var copyOrderId = window.patientOrderExam.CopyOrderId;
    customFrame.id = $.urlParam(window.location.href, "frame", true);
    if (customFrame.id !== null) {
        customFrame.fromAdmin = true;
    }

    validateEyeglassOrder();
    rxViewModel = new RxViewModel();
    ko.applyBindings(rxViewModel, $("#egSelectedRxPanel")[0]);
    eyeglassOrderViewModel = new EyeglassOrderViewModel();
    ko.applyBindings(eyeglassOrderViewModel, $("#summary")[0]);
    //getAllFrameAndLensExtras();
    if ((resourceId === null || resourceId === undefined || resourceId === "0") && (orderId === 0 || orderId === null)) {
        egDataSource = "new";
        if (copyOrderId !== null && copyOrderId !== undefined && copyOrderId > 0) {
            eyeglassOrderViewModel.isCopyOrder(true);
            window.initEyeglassSummaryPageFromCopyOrder();
        } else {
            // no resourceid no orderid so its a new eyeglass order
            initChooseInsurancePage();
        }
    } else if ((resourceId === null || resourceId === undefined || resourceId === "0") && (orderId !== 0 && orderId !== null)) {
        // there is orderid and NO resourceid so load summary from order
        egDataSource = "order";
        window.initEyeglassSummaryPage();
    } else if ((resourceId !== null && resourceId !== undefined && resourceId !== "0") && (orderId === 0 || orderId === null)) {
        // there is NO orderid and resourceid is present so load summary from Temp Storage
        egDataSource = "tempStorage";
        window.initEgSummaryPageFromTempStorage();
    } else {
        // we should never get here cause we should never have a orderid and resourceId together
        alert("Error! Resource Id and Order Id both present. Taking the Order Id.");
        egDataSource = "order";
        window.initEyeglassSummaryPage();
        $("#btnSaveForLater").addClass("hidden");
    }

    setTimeout(function () {
        eyeglassOrderViewModel.isDirty(false);
    }, 300);
});

window.onbeforeunload = function () {
    //urlRedirect: redirect to material orders or admin custom frame
    if (urlRedirect) {
        return;
    }
    if (eyeglassOrderViewModel.isDirty() || window.vmFrame.isDirty() || window.orderExtrasViewModel.isDirty() || window.vmLenses.isDirty()) {
        return "You are trying to navigate to a new page, but you have not saved the data on this page.";
    }
    return;
};