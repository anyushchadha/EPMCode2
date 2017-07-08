/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko, Modernizr, console, msgType, loadPatientSideNavigation, ApiClient, alert, buildOrder, egOrderType,
    eyeglassOrderViewModel, goBackToPage, setBuildOrderTileComplete, setBuildOrderTileError, setBuildOrderTileReset */

var buildOrderViewModel = null;

function BuildOrderViewModel() {
    var self = this;
    var lensTitle = "Add Lenses";
    var lensDisabled = false;
    var extrasDisabled = false;
    var name = (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY ||
        eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) ? "Add Patient's Supplied Frame" : "Add Frame";

    var selAuth = eyeglassOrderViewModel.selectedAuthorization();
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.FRAME_ONLY) {
        lensDisabled = true;
        if (selAuth.IsVsp) {
            extrasDisabled = true;
        }
    } else if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
        lensDisabled = selAuth.NonInsurance ? false : true;
        if (selAuth.NonInsurance) {
            lensTitle = "Add Lens Treatments";
        }
    }

    self.displayEgBuildOrder = ko.observableArray();
    self.egBuildOrder = ko.observableArray();
    self.egBuildOrder.push({ Id: 0, Name: name, Disabled: false });
    self.egBuildOrder.push({ Id: 1, Name: lensTitle, Disabled: lensDisabled });
    self.egBuildOrder.push({ Id: 2, Name: "Add Extras", Disabled: extrasDisabled });
    self.egBuildOrder.push({ Id: 3, Name: "Add Lab", Disabled: false });

    var temp = self.egBuildOrder().slice(0);
    while (temp.length > 0) {
        self.displayEgBuildOrder.push(temp.splice(0, 2));
    }
}

var getLensTitle = function () {
    var lensTitle = "Add Lenses";
    var lensAddedTitle = "Lenses Added";
    var lenses = eyeglassOrderViewModel.selectedLens();
    var selAuth = eyeglassOrderViewModel.selectedAuthorization();

    if (eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.EXTRAS_ONLY) {
        lensAddedTitle = "";

        // Right Lens Title
        if (lenses && lenses.RightLens && lenses.RightLens.TypeDescription) {
            lensAddedTitle += "(R) " + lenses.RightLens.TypeDescription + "<br>";
        }

        // Left Lens Title
        if (lenses && lenses.LeftLens && lenses.LeftLens.TypeDescription) {
            lensAddedTitle += "(L) " + lenses.LeftLens.TypeDescription;
        }

    } else if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY && selAuth.NonInsurance) {
        if (lenses) {
            lensAddedTitle = "Lens Treatments Added";
        } else {
            lensTitle = "Add Lens Treatments";
        }
    }

    return lenses === null || lensAddedTitle.length === 0 ? lensTitle : lensAddedTitle;
};

var koInitSetup = function () {
    //init the buildOrderViewModel, then apply the KO bindings to the egBuildOrder div only
    buildOrderViewModel = new BuildOrderViewModel();
    ko.applyBindings(buildOrderViewModel, $('#egBuildOrder')[0]);

    $("div[id^='buildOrder_']").click(function (e) {
        e.preventDefault();
        $("#buildOrder").addClass("hidden");
        switch (this.id) {
        case "buildOrder_0": //Add Frame
            $(this).attr("disabled", "disabled");
            window.initChooseOrderFramePage();
            break;
        case "buildOrder_1": //Add Lenses
            window.initChooseOrderLensesPage();
            break;
        case "buildOrder_2": //Add Extras
            window.extrasFrom = "BuildOrder";
            window.initChooseEyeglassExtrasPage();
            break;
        case "buildOrder_3": //Add Lab
            window.initChooseEyeglassLabPage();
            break;
        }
    });
};

var markBuildOrderTile = function (mark, info, panelIndex, item) {
    if (mark && (item && item.IsValid !== false)) {
        $("#space_" + panelIndex).addClass("hidden");
        $("#info_" + panelIndex).html(info);
        setBuildOrderTileComplete(panelIndex);
    } else if (mark && (item && item.IsValid === false)) {
        $("#space_" + panelIndex).removeClass("hidden");
        $("#info_" + panelIndex).html(info);
        setBuildOrderTileError(panelIndex);
    } else {
        $("#space_" + panelIndex).removeClass("hidden");
        $("#info_" + panelIndex).html(info);
        setBuildOrderTileReset(panelIndex);
    }

    $("#buildOrder").removeClass("hidden");
};

var setupPanelHeader = function (panelIndexes) {
    if (panelIndexes === undefined) {
        return;
    }
    panelIndexes += '';
    var mark = false;
    var info = "";
    var i, arr = panelIndexes.split(',');
    for (i = 0; i < arr.length; i++) {
        var panelIndex = parseInt(arr[i], 10);
        switch (panelIndex) {
        case buildOrder.FRAME:
            var frame = eyeglassOrderViewModel.selectedFrame();
            var name = frame === null || frame === undefined ? "Add Frame" : frame.Model;
            if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
                name = frame === null ? "Add Patient's Supplied Frame" : "Patient's Frame Added";
            }
            info = name;
            mark = frame === null ? false : true;
            markBuildOrderTile(mark, info, panelIndex, frame);
            break;

        case buildOrder.LENSES:
            var lenses = eyeglassOrderViewModel.selectedLens();
            info = getLensTitle();
            mark = lenses === null ? false : true;
            markBuildOrderTile(mark, info, panelIndex, lenses);
            break;

        case buildOrder.EXTRAS:
            var extras = eyeglassOrderViewModel.selectedExtras();
            info = extras === null || extras === undefined || extras.length === 0 ? "Add Extras" : "(" + extras.length + ") Extras Added";
            mark = extras === null || extras === undefined || extras.length === 0 ? false : true;
            if (info === "Add Extras") {
                markBuildOrderTile(mark, info, panelIndex, null);
            } else {
                markBuildOrderTile(mark, info, panelIndex, extras[0]);
            }
            break;

        case buildOrder.LAB:
            var lab = eyeglassOrderViewModel.selectedLab();
            info = lab === null || lab === undefined ? "Add Lab" : lab.Name;
            mark = lab === null || lab === undefined ? false : true;
            markBuildOrderTile(mark, info, panelIndex, lab);
            break;

        default:
            break;
        }
    }
};

var updateLayoutBuildOrderPage = function () {
    var auth = eyeglassOrderViewModel.selectedAuthorization();
    switch (eyeglassOrderViewModel.eyeglassOrderType()) {
    case egOrderType.FRAME_ONLY:
        setTimeout(function () {
            //Lens always disabled
            if (auth.IsVsp === true) {
                $("#buildOrder_2").addClass('disabled-extras-panel');            //Extras disabled
                //is 'Lab Supplied' chosen from 'Frame Source' drop down list?
                var frame = eyeglassOrderViewModel.selectedFrame();
                if (frame !== null && frame !== undefined) {
                    if (frame.FrameSourceID === 3) {                             //Lab Supplied
                        $("#buildOrder_3").removeClass('disabled-extras-panel'); //Lab enabled
                        $("#buildOrder_2").addClass('disabled-extras-panel');    //Extras disabled
                    } else {
                        $("#buildOrder_2, #buildOrder_3").addClass('disabled-extras-panel'); //Extras, Lab disabled
                        var lab = eyeglassOrderViewModel.selectedLab();
                        if (lab) {
                            window.clearLabCheckmarks();
                            setupPanelHeader(buildOrder.LAB);
                        }
                    }
                    var extras = eyeglassOrderViewModel.selectedExtras();
                    if (extras && extras.length > 0) {
                        window.clearExtrasCheckmarks();
                    }
                }
            } else {
                $("#buildOrder_2, #buildOrder_3").removeClass('disabled-extras-panel'); //Extras, Lab enabled
            }
        }, 300);
        break;

    case egOrderType.EXTRAS_ONLY:
        var info = getLensTitle();
        $("#info_" + buildOrder.LENSES).html(info);
        if (auth.NonInsurance) {
            $("#buildOrder_1").removeClass('disabled-extras-panel'); //Lens enabled
        } else {
            $("#buildOrder_1").addClass('disabled-extras-panel'); //Lens disabled
        }
        break;
    }
};

var initChooseBuildOrderPage = function (panelIndex) {
    if (buildOrderViewModel === null) {
        koInitSetup();
    }

    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Build Order");
    $("#orderName").val("");
    $("#egInsPanel").removeClass("hidden").addClass("hidden-sm hidden-xs");
    window.setupPage(buildOrder.BUILDORDER);
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    }

    goBackToPage.fromAddToOrder = '';
    updateLayoutBuildOrderPage();

    setupPanelHeader(buildOrder.FRAME + ',' + buildOrder.LENSES + ',' + buildOrder.EXTRAS + ',' + buildOrder.LAB);
    if (panelIndex === undefined) {
        //possible user changes insurance (VSP, non-VSP, no insurance) then we need to update layout build order page
        $("#buildOrder").removeClass("hidden");
    }

    //Any warning message?
    if (buildOrder.warningMessage !== '') {
        var selectedExtras = null;
        var msg = "";
        var i, v = buildOrder.warningMessage.split(',');
        for (i = 0; i < v.length; i++) {
            var val = parseInt(v[i], 10);
            if (val === buildOrder.LENSES) {
                msg += msg === '' ? 'Lenses' : ', Lenses';
                window.clearLenses();
            } else if (val === buildOrder.EXTRAS) {
                msg += msg === '' ? 'Extras' : ', Extras';
                selectedExtras = eyeglassOrderViewModel.selectedExtras();
                window.clearExtrasCheckmarks();
                window.updateFrameExtras(); //add Oversized Frame back
            } else {
                msg += msg === '' ? 'Lab' : ', Lab';
                window.clearLabCheckmarks();
            }
        }

        var msg2 = v.length === 1 ? "it" : "them";
        msg += v.length === 1 ? " selection was" : " selections were";
        msg = "Your " + msg + " automatically changed. Review " + msg2 + " before finalizing your order.";
        setupPanelHeader(buildOrder.warningMessage);
        buildOrder.warningMessage = '';
        $("#warningMsgTitle").html(msg);
        setTimeout(function () {
            if (selectedExtras !== null) {
                window.keepVspEligible(selectedExtras);
            }
            $("#msgWarning").removeClass("hidden");
        }, 500);
    } else {
        $("#msgWarning").addClass("hidden");
        if (eyeglassOrderViewModel.isCopyOrder() === true) {
            eyeglassOrderViewModel.isCopyOrder(false);
            var auth = eyeglassOrderViewModel.selectedAuthorization();
            if (eyeglassOrderViewModel.copyOrderIsVsp()) {
                var lab = eyeglassOrderViewModel.selectedLab();
                if (lab !== null) {
                    if (auth.IsVsp) {   //choose VSP
                        window.addLab(lab);
                        $("#btnContinue").click();  //redirect to summary page
                    } else {    //choose non-VSP
                        window.clearLabCheckmarks();
                        window.clearLabError();
                        setBuildOrderTileReset(buildOrder.LAB);
                        markBuildOrderTile(false, "Add Lab", buildOrder.LAB, null);
                    }
                }
            } else {
                if (!auth.IsVsp) {  //choose non-VSP
                    $("#btnContinue").click();  //redirect to summary page
                }
            }
        }
    }

    eyeglassOrderViewModel.isCopyOrder(false);
    window.scrollTo(0, 0);
};

//initially coming from pending order or in-progress order, this page should be initialized first
var updatePanelBuildOrderPage = function () {
    if (buildOrderViewModel === null) {
        koInitSetup();
    }

    var frame = eyeglassOrderViewModel.selectedFrame();
    var name = frame === null ? "Add Frame" : frame.Model;
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
        name = frame === null ? "Add Patient's Supplied Frame" : "Patient's Frame Added";
    }
    var info = name;
    var mark = frame === null ? false : true;
    if (mark) {
        $("#space_" + buildOrder.FRAME).addClass("hidden");
        $("#info_" + buildOrder.FRAME).html(info);
        if (eyeglassOrderViewModel.selectedFrame().IsValid) {
            setBuildOrderTileComplete(buildOrder.FRAME);
        }
    }

    var lenses = eyeglassOrderViewModel.selectedLens();
    info = getLensTitle();
    mark = lenses === null ? false : true;
    if (mark) {
        $("#space_" + buildOrder.LENSES).addClass("hidden");
        $("#info_" + buildOrder.LENSES).html(info);
        if (eyeglassOrderViewModel.selectedLens().IsValid) {
            setBuildOrderTileComplete(buildOrder.LENSES);
        }
    }

    var extras = eyeglassOrderViewModel.selectedExtras();
    info = extras === null || extras.length === 0 ? "Add Extras" : "(" + extras.length + ") Extras Added";
    mark = extras === null || extras.length === 0 ? false : true;
    if (mark) {
        $("#space_" + buildOrder.EXTRAS).addClass("hidden");
        $("#info_" + buildOrder.EXTRAS).html(info);
        setBuildOrderTileComplete(buildOrder.EXTRAS);
    }

    var lab = eyeglassOrderViewModel.selectedLab();
    info = lab === null ? "Add Lab" : lab.Name;
    mark = lab === null ? false : true;
    if (mark) {
        $("#space_" + buildOrder.LAB).addClass("hidden");
        $("#info_" + buildOrder.LAB).html(info);
        setBuildOrderTileComplete(buildOrder.LAB);
    }

    var rx = eyeglassOrderViewModel.selectedRx();
    if (rx !== null && !rx.IsValid) {
        $("#egSelectedRxPanel #rxPanel").addClass("error");
        $("#egSelectedRxPanel .errorMsgTxt").html(rx.ValidationMessage);
        $("#egSelectedRxPanel .errorMsg").removeClass("hidden");
    }

    var auth = eyeglassOrderViewModel.selectedAuthorization();
    if (auth !== null && !auth.IsValid) {
        $("#egSelectedInsurancePanel .errorMsgTxt").html(auth.ValidationMessage);
        $("#egSelectedInsurancePanel #insurancePanel").addClass("error");
        $("#egSelectedInsurancePanel .errorMsg").removeClass("hidden");
    }

    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    } else {
        $("#btnSaveForLater").removeClass("hidden");
    }
};