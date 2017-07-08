/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko,  Modernizr, console, msgType, loadPatientSideNavigation, ApiClient, getTodayDate, alert, eyeglassOrderViewModel, egOrderType,
    setBuildOrderTileComplete, setBuildOrderTileError, setBuildOrderTileReset, setExtraCheckMark, buildOrder, canAddCustomMeasurementsExtra */
var client = new ApiClient("EyeglassOrder");

var orderExtrasViewModel = null;
var allExtras = [];
var dataExtrasVsp = [];
var dataExtrasNonVsp = [];

function showExtrasError() {
    var allValid = true;

    if (window.eyeglassOrderViewModel.selectedExtras() !== null) {
        ko.utils.arrayForEach(window.eyeglassOrderViewModel.selectedExtras(), function (e) {
            if (!e.IsValid) {
                allValid = false;
                $("#egOrderExtras #egExtrasMsgPanel #msgError .message").html(e.ValidationMessage);
                $("#egOrderExtras #egExtrasMsgPanel #msgError").removeClass("hidden");
            }
        });
    }

    if (allValid) {
        $("#egOrderExtras #egExtrasMsgPanel #msgError .message").html("");
        $("#egOrderExtras #egExtrasMsgPanel #msgError").addClass("hidden");
    }
}

function OrderExtrasViewModel(data) {
    var self = this;
    self.isDirty = ko.observable(false);
    $.each(data, function (index, item) {
        allExtras.push(item);
        if (item.IsVspEligible === true) {
            dataExtrasVsp.push(item);
        } else {
            dataExtrasNonVsp.push(item);
        }
    });

    self.egOrderExtrasVsp = ko.observableArray(dataExtrasVsp);
    var temp1 = self.egOrderExtrasVsp().slice(0);
    self.displayEgOrderExtrasVsp = ko.observableArray();
    while (temp1.length > 0) {
        self.displayEgOrderExtrasVsp.push(temp1.splice(0, 4));
    }

    self.egOrderExtrasNonVsp = ko.observableArray(dataExtrasNonVsp);
    var temp2 = self.egOrderExtrasNonVsp().slice(0);
    self.displayEgOrderExtrasNonVsp = ko.observableArray();
    while (temp2.length > 0) {
        self.displayEgOrderExtrasNonVsp.push(temp2.splice(0, 4));
    }

    self.egOrderExtrasAll = ko.observableArray(allExtras);
    var temp3 = self.egOrderExtrasAll().slice(0);
    self.displayEgOrderExtrasAll = ko.observableArray();
    while (temp3.length > 0) {
        self.displayEgOrderExtrasAll.push(temp3.splice(0, 4));
    }
}

function getExtraItem(extraName) {
    var result = null;
    if (window.getIsVsp() === true) {
        if (dataExtrasVsp !== undefined && dataExtrasVsp !== null) {
            $.each(dataExtrasVsp, function (index, item) {
                if (item.ItemName === extraName) {
                    result = item;
                }
            });
        }
    } else {
        if (allExtras !== undefined && allExtras !== null) {
            $.each(allExtras, function (index, item) {
                if (item.ItemName === extraName) {
                    result = item;
                }
            });
        }
    }

    return result;
}

function setExtrasVisibility() {
    if (window.getIsVsp() === true) {
        $("#firstExtrasGroup").removeClass("hidden");
        $("#secondExtrasGroup").removeClass("hidden");
        $("#allExtrasGroup").addClass("hidden");
    } else {
        $("#firstExtrasGroup").addClass("hidden");
        $("#secondExtrasGroup").addClass("hidden");
        $("#allExtrasGroup").removeClass("hidden");
    }
}

function setExtrasDisabledEnabled() {
    if (orderExtrasViewModel !== null) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v) {
            var d = "#orderExtras_" + v.Id.toString();
            var i = "#extraLogoId_" + v.Id.toString();
            if (window.getIsVsp() === true) {
                if (v.IsVspEligible === true) {
                    $(d).removeClass("disabled-extras-panel");
                    $(i).removeClass("hidden");
                } else {
                    $(d).addClass("disabled-extras-panel");
                }
            } else {
                $(d).removeClass("disabled-extras-panel");
                $(i).addClass("hidden");
            }
        });

        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasNonVsp(), function (v) {
            var d = "#orderExtras_" + v.Id.toString();
            if (window.getIsVsp() === true) {
                if (v.IsVspEligible === true) {
                    $(d).removeClass("disabled-extras-panel");
                } else {
                    $(d).addClass("disabled-extras-panel");
                }
            } else {
                if (v.IsVspEligible === true) {
                    $(d).addClass("disabled-extras-panel");
                } else {
                    $(d).removeClass("disabled-extras-panel");
                }
            }
        });
    }
}

function updateExtrasBuildOrderTile() {
    if (window.eyeglassOrderViewModel.selectedExtras()) {
        var selExCnt = (window.eyeglassOrderViewModel.selectedExtras().length);
        if (selExCnt > 0) {
            $("#space_2").addClass("hidden");
            $("#info_2").html("(" + selExCnt + ") Extras Added");
            setBuildOrderTileComplete(buildOrder.EXTRAS);
        } else {
            $("#space_2").removeClass("hidden");
            $("#info_2").html("Add Extras");
            setBuildOrderTileReset(buildOrder.EXTRAS);
        }
    }
}

function searchInObjArray(id, list) {
    var i;
    for (i = 0; i < list.length; i++) {
        if (list[i].Id === id) {
            return list[i];
        }
    }
}


function addExtraToSelectedExtras(item) {
    var extras = [];
    if (window.eyeglassOrderViewModel.selectedExtras() === null) {
        extras.push(item);
    } else {
        ko.utils.arrayForEach(window.eyeglassOrderViewModel.selectedExtras(), function (v) {
            extras.push(v);
        });

        var match = searchInObjArray(item.Id, extras);
        if (!match) {
            extras.push(item);
        }
    }

    window.updateSelectedEntity(window.buildOrder.EXTRAS, extras);
    updateExtrasBuildOrderTile();
}

function removeExtraFromSelectedExtras(extraName) {
    var i, extras = [];
    if (window.eyeglassOrderViewModel.selectedExtras() !== null) {
        ko.utils.arrayForEach(window.eyeglassOrderViewModel.selectedExtras(), function (v) {
            extras.push(v);
        });
    }

    if (extras.length === 0) {
        return;
    }

    var removed = extras.filter(function (el) {
        return el.ItemName !== extraName;
    });

    window.updateSelectedEntity(window.buildOrder.EXTRAS, removed);
    updateExtrasBuildOrderTile();
}

function setIsVspChecked(id, val) {
    if (window.getIsVsp() === true) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v) {
            if (v.Id.toString() === id) {
                v.IsSelected = val;
            }
        });
    } else {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasAll(), function (v) {
            if (v.Id.toString() === id) {
                v.IsSelected = val;
            }
        });
    }
}

function clearLensExtras() {
    var rx = eyeglassOrderViewModel.selectedRx();
    var itemPrism = getExtraItem("Prism");
    if (rx !== undefined && rx !== null) {
        if (itemPrism !== null && itemPrism !== undefined) {
            setExtraCheckMark(itemPrism, false);
            removeExtraFromSelectedExtras(itemPrism.ItemName);
        }

        var itemSlaboff = getExtraItem("Slab Off");
        if (itemSlaboff !== null && itemSlaboff !== undefined) {
            setExtraCheckMark(itemSlaboff, false);
            removeExtraFromSelectedExtras(itemSlaboff.ItemName);
        }

        var itemVantage = getExtraItem("Vantage Polarized");
        if (itemVantage !== null && itemVantage !== undefined) {
            setExtraCheckMark(itemVantage, false);
            removeExtraFromSelectedExtras(itemVantage.ItemName);
        }
    }
}

function clearExtrasCheckmarks() {
    var eid = "";
    if (orderExtrasViewModel !== null) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v) {
            v.IsSelected = false;
            eid = "#extraId_" + v.Id.toString();
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
        });
    }

    if (orderExtrasViewModel !== null) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasNonVsp(), function (v) {
            v.IsSelected = false;
            eid = "#extraId_" + v.Id.toString();
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
        });
    }
    if (orderExtrasViewModel !== null) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasAll(), function (v) {
            v.IsSelected = false;
            eid = "#allExtraId_" + v.Id.toString();
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
        });
    }

    window.updateSelectedEntity(window.buildOrder.EXTRAS, null);
}

function clearIneligibleCheckmarks() {
    var eid = "";
    var did = "";
    if (orderExtrasViewModel !== null) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasNonVsp(), function (v) {
            v.IsSelected = false;
            eid = "#extraId_" + v.Id.toString();
            did = "#orderExtras_" + v.Id.toString();
            if ($(did).hasClass("disabled-extras-panel")) {
                $(eid).addClass("hidden");
                $(eid).parent().removeClass("selected");
            }
        });
    }
}

function setExtraCheckMark(item, check) {
    var eid = "";
    if (orderExtrasViewModel !== null) {
        if (window.getIsVsp() === true) {
            ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v) {
                if (item.Id === v.Id) {
                    v.IsSelected = check;
                    eid = "#extraId_" + v.Id.toString();
                    if (check === true) {
                        $(eid).removeClass("hidden");
                        $(eid).parent().addClass("selected");
                    } else {
                        $(eid).addClass("hidden");
                        $(eid).parent().removeClass("selected");
                    }
                }
            });
        } else {
            ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasAll(), function (v) {
                if (item.Id === v.Id) {
                    v.IsSelected = check;
                    eid = "#allExtraId_" + v.Id.toString();
                    if (check === true) {
                        $(eid).removeClass("hidden");
                        $(eid).parent().addClass("selected");
                    } else {
                        $(eid).addClass("hidden");
                        $(eid).parent().removeClass("selected");
                    }
                }
            });
        }
    }
}

function getSelectedExtrasFromUi() {
    var data = [];
    var eid = "";
    if (window.getIsVsp() === true) {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v) {
            eid = "#extraId_" + v.Id.toString();
            if (!$(eid).hasClass("hidden")) {
                data.push(v);
            }
        });

    } else {
        ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasAll(), function (v) {
            eid = "#allExtraId_" + v.Id.toString();
            if (!$(eid).hasClass("hidden")) {
                data.push(v);
            }
        });
    }

    return data;
}

function getSelectedExtrasFromVm() {
    var data = [];
    ko.utils.arrayForEach(window.eyeglassOrderViewModel.selectedExtras(), function (v) {
        if (v.IsSelected === true) {
            data.push(v);
        }
    });

    return data;
}

function setExtrasCheckMarks() {
    if (window.eyeglassOrderViewModel !== null && window.eyeglassOrderViewModel.selectedExtras() !== null) {
        ko.utils.arrayForEach(window.eyeglassOrderViewModel.selectedExtras(), function (v) {
            setExtraCheckMark(v, true);
        });
    }
}

$("#btnExtrasToBuildOrder").click(function (e) {
    e.preventDefault();
    $("#chooseExtras").addClass("hidden");
    if (window.extrasFrom === "OrderType") {
        window.initChooseOrderTypePage();
    } else {
        window.initChooseBuildOrderPage(window.buildOrder.EXTRAS);
    }
});

$("#btnAddExtrasToOrder").click(function (e) {
    e.preventDefault();
    var data = getSelectedExtrasFromUi();
    $("#chooseExtras").addClass("hidden");
    $("#btnContinue").removeClass("hidden");
    eyeglassOrderViewModel.isDirty(true);

    var msg = "";
    if (data.length > 0) {
        msg = "Selected Extras are added to the eyeglass order.";
    } else {
        data = null;
        if (window.eyeglassOrderViewModel.selectedExtras() === null) {
            msg = "No Extras were added to the order.";
        }
    }

    window.updateSelectedEntity(window.buildOrder.EXTRAS, data);
    window.displayPage(window.buildOrder.EXTRAS, msg);
});

function showExtrasPage() {
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Add Extras");
    $("#chooseExtras").removeClass("hidden");
    $("#btnSaveForLater").removeClass("hidden");
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    }

    $("#btnContinue").addClass("hidden");
    window.scrollTo(0, 0);
}

function setClickEvents() {
    $("div[id^='orderExtras_']").click(function (e) {
        var eid = "#extraId_" + this.id.toString();
        var vid = this.id.replace("orderExtras_", "");
        eid = eid.replace("orderExtras_", "");
        e.preventDefault();
        if ($(eid).hasClass("hidden")) {
            $(eid).removeClass("hidden");
            $(eid).parent().addClass("selected");
            setIsVspChecked(vid, true);
        } else {
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
            setIsVspChecked(vid, false);
        }

        orderExtrasViewModel.isDirty(true);
    });
    $("div[id^='allExtras_']").click(function (e) {
        var eid = "#allExtraId_" + this.id.toString();
        var vid = this.id.replace("allExtras_", "");
        eid = eid.replace("allExtras_", "");
        e.preventDefault();
        if ($(eid).hasClass("hidden")) {
            $(eid).removeClass("hidden");
            $(eid).parent().addClass("selected");
            setIsVspChecked(vid, true);
        } else {
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
            setIsVspChecked(vid, false);
        }

        orderExtrasViewModel.isDirty(true);
    });
    $("div[id^='orderExtras_']").hover(function (e) {
        var vid = this.id.replace("orderExtras_", "");
        $.each(allExtras, function (index, item) {
            if (item.Id.toString() === vid) {
                var lid = "#extraLogoId_" + item.Id.toString();
                if (!$(lid).hasClass("hidden")) {
                    var imgSrc = ($(lid).prop("src"));
                    if (e.type === "mouseenter" && !$(lid).parents("div.panelLink").hasClass("selected") && imgSrc.indexOf("_RGB_") !== -1) {
                        imgSrc = imgSrc.replace("_RGB_", "_W_");
                        $(lid).attr("src", imgSrc);
                    } else if (imgSrc.indexOf("_W_") !== -1) {
                        imgSrc = imgSrc.replace("_W_", "_RGB_");
                        $(lid).attr("src", imgSrc);
                    }
                }
            }
        });
    });
}

function handleVspCustomMeasurements() {
    var itemCustomMeasurements = getExtraItem("Custom Measurements");
    if (itemCustomMeasurements !== null && itemCustomMeasurements !== undefined) {
        var frame = eyeglassOrderViewModel.selectedFrame();
        if (frame !== null && frame !== undefined) {
            if (canAddCustomMeasurementsExtra(frame)) {
                if (window.getIsVsp() === true) {
                    $("#orderExtras_" + itemCustomMeasurements.Id).addClass("disabled-extras-panel");
                } else {
                    $("#orderExtras_" + itemCustomMeasurements.Id).removeClass("disabled-extras-panel");
                }
            }
        }
    }
}

function clearAutoLensExtras() {
    var itemP = getExtraItem("Prism");
    if (itemP !== null && itemP !== undefined) {
        setExtraCheckMark(itemP, false);
        removeExtraFromSelectedExtras(itemP.ItemName);
    }

    var itemS = getExtraItem("Slab Off");
    if (itemS !== null && itemS !== undefined) {
        setExtraCheckMark(itemS, false);
        removeExtraFromSelectedExtras(itemS.ItemName);
    }

    var itemV = getExtraItem("Vantage Polarized");
    if (itemV !== null && itemV !== undefined) {
        setExtraCheckMark(itemV, false);
        removeExtraFromSelectedExtras(itemV.ItemName);
    }
}

function getAllMappedExtras() {
    if (window.extrasLoaded === false) {
        window.extrasLoaded = true;
        client
            .action("GetEgExtrasByCompany")
            .get({
                patientId: window.patientOrderExam.PatientId,
                awsResourceId: window.patientOrderExam.AwsResourceId
            })
            .done(function (data) {
                if (data !== undefined && data !== null) {
                    orderExtrasViewModel = new OrderExtrasViewModel(data);
                    ko.applyBindings(orderExtrasViewModel, $("#egOrderExtras")[0]);
                    setClickEvents();
                    setExtrasCheckMarks();
                    setExtrasVisibility();
                    setExtrasDisabledEnabled();
                    handleVspCustomMeasurements();
                    orderExtrasViewModel.isDirty(false);
                    //clearAutoLensExtras();

                    // show any server error messaging
                    showExtrasError();
                }
            })
            .fail(function () {
                window.extrasLoaded = false;
            });
    } else {
        setExtrasCheckMarks();
        setExtrasVisibility();
        setExtrasDisabledEnabled();
        clearIneligibleCheckmarks();
        handleVspCustomMeasurements();

        // show any server error messaging
        showExtrasError();
    }
}

function keepVspEligible(selectedExtras) {
    ko.utils.arrayForEach(selectedExtras, function (v1) {
        if (v1.IsVspEligible === true) {
            ko.utils.arrayForEach(orderExtrasViewModel.egOrderExtrasVsp(), function (v2) {
                if (v1.ItemId === v2.ItemId) {
                    setExtraCheckMark(v2, true);
                    addExtraToSelectedExtras(v2);
                    return true;
                }
            });
        }
    });
}
function canAddCustomMeasurementsExtra(frame) {
    if (frame !== null && frame !== undefined) {
        if ((frame.FrameWrap !== null && frame.FrameWrap !== undefined && frame.FrameWrap !== 0 && frame.FrameWrap !== "")
                || (frame.PantoscopicTilt !== null && frame.PantoscopicTilt !== undefined && frame.PantoscopicTilt !== 0 && frame.PantoscopicTilt !== "")
                || (frame.Vertex !== null && frame.Vertex !== undefined && frame.Vertex !== 0 && frame.Vertex !== "")) {
            return true;
        }
    }
    return false;
}

function canRemoveCustomMeasurementExtra(frame) {
    if (frame !== null && frame !== undefined) {
        if ((frame.FrameWrap === undefined || frame.FrameWrap === null || frame.FrameWrap === 0 || frame.FrameWrap === "")
                && (frame.PantoscopicTilt === undefined || frame.PantoscopicTilt === null || frame.PantoscopicTilt === 0 || frame.PantoscopicTilt === "")
                && (frame.Vertex === undefined || frame.Vertex === null || frame.Vertex === 0 || frame.Vertex === "")) {
            return true;
        }
    }
    return false;
}

function updateFrameExtras() {
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.COMPLETE || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY) {
        if (window.eyeglassOrderViewModel !== null && window.eyeglassOrderViewModel.selectedFrame() !== null) {
            // Custom Measurements
            var itemCustomMeasurements = getExtraItem("Custom Measurements");
            var frame = eyeglassOrderViewModel.selectedFrame();
            if (frame !== null && frame !== undefined && itemCustomMeasurements !== null && itemCustomMeasurements !== undefined) {
                if (canAddCustomMeasurementsExtra(frame)) {
                    setExtraCheckMark(itemCustomMeasurements, true);
                    addExtraToSelectedExtras(itemCustomMeasurements);
                    if (window.getIsVsp() === true) {
                        $("#orderExtras_" + itemCustomMeasurements.Id).addClass("disabled-extras-panel");
                    } else {
                        $("#orderExtras_" + itemCustomMeasurements.Id).removeClass("disabled-extras-panel");
                    }
                }

                if (canRemoveCustomMeasurementExtra(frame)) {
                    setExtraCheckMark(itemCustomMeasurements, false);
                    removeExtraFromSelectedExtras(itemCustomMeasurements.ItemName);
                    $("#allExtras_" + itemCustomMeasurements.Id).removeClass("disabled-extras-panel");
                }
            }

            // Oversize
            var itemOversize = getExtraItem("Oversize");
            if (frame !== null && frame !== undefined && itemOversize !== null && itemOversize !== undefined && eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.FRAME_ONLY) {
                if (window.eyeglassOrderViewModel.selectedFrame().Eye > 60) {
                    setExtraCheckMark(itemOversize, true);
                    addExtraToSelectedExtras(itemOversize);
                } else {
                    setExtraCheckMark(itemOversize, false);
                    removeExtraFromSelectedExtras(itemOversize.ItemName);
                }
            }
        }
    }
}
//}

function updateLensExtras() {
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.COMPLETE || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY) {
        if (window.eyeglassOrderViewModel.selectedLens() !== null) {
            var lens = eyeglassOrderViewModel.selectedLens();
            if (lens !== undefined && lens !== null) {
                var rx = eyeglassOrderViewModel.selectedRx();
                var itemPrism = getExtraItem("Prism");

                // Prism

                if (rx !== undefined && rx !== null) {
                    if (itemPrism !== null && itemPrism !== undefined) {
                        if (rx.RightPrismTxt !== "" || rx.LeftPrismTxt !== "") {
                            setExtraCheckMark(itemPrism, true);
                            addExtraToSelectedExtras(itemPrism);
                        }

                        if (rx.RightPrismTxt === "" && rx.LeftPrismTxt === "") {
                            setExtraCheckMark(itemPrism, false);
                            removeExtraFromSelectedExtras(itemPrism.ItemName);
                        }
                    }

                    // Slab Off

                    var itemSlaboff = getExtraItem("Slab Off");
                    if (itemSlaboff !== null && itemSlaboff !== undefined) {
                        if (rx.RightIsSlabOff === true || rx.LeftIsSlabOff === true) {
                            setExtraCheckMark(itemSlaboff, true);
                            addExtraToSelectedExtras(itemSlaboff);
                        }

                        if (rx.RightIsSlabOff === false && rx.LeftIsSlabOff === false) {
                            setExtraCheckMark(itemSlaboff, false);
                            removeExtraFromSelectedExtras(itemSlaboff.ItemName);
                        }
                    }
                }

                // Vantage

                var itemVantage = getExtraItem("Vantage Polarized");
                if (itemVantage !== null && itemVantage !== undefined) {
                    var rc = false;
                    var lc = false;
                    if (lens.RightLens !== "" && lens.RightLens !== undefined && lens.RightLens !== null) {
                        client
                            .action("GetMfgCoatings")
                            .get({
                                lensTypeId: lens.RightLens.TypeId,
                                lensMaterialId: lens.RightLens.MaterialTypeId,
                                lensStyleId: lens.RightLens.StyleId,
                                lensColorId: lens.RightLens.ColorId,
                                isVsp: window.getIsVsp()
                            })
                            .done(function (data) {
                                if (data.additionalColor !== undefined && data.additionalColor.length !== 0) {
                                    rc = true;
                                    setExtraCheckMark(itemVantage, true);
                                    addExtraToSelectedExtras(itemVantage);
                                } else {
                                    if (lens.LeftLens !== "" && lens.LeftLens !== undefined && lens.LeftLens !== null) {
                                        client
                                            .action("GetMfgCoatings")
                                            .get({
                                                lensTypeId: lens.LeftLens.TypeId,
                                                lensMaterialId: lens.LeftLens.MaterialTypeId,
                                                lensStyleId: lens.LeftLens.StyleId,
                                                lensColorId: lens.LeftLens.ColorId,
                                                isVsp: window.getIsVsp()
                                            })
                                            .done(function (data) {
                                                if (data.additionalColor !== undefined && data.additionalColor.length !== 0) {
                                                    lc = true;
                                                    setExtraCheckMark(itemVantage, true);
                                                    addExtraToSelectedExtras(itemVantage);
                                                }

                                                if (rc === false && lc === false) {
                                                    setExtraCheckMark(itemVantage, false);
                                                    removeExtraFromSelectedExtras(itemVantage.ItemName);
                                                }
                                            });
                                    }
                                }
                            });
                    }
                }
            }
        } else {
            if (window.egDataSource === "new") {
                setTimeout(function () { clearAutoLensExtras(); }, 300);
            }
        }
    }
}

function initExtrasPageFromTempStorage(data) {
    getAllMappedExtras();
    window.updateSelectedEntity(window.buildOrder.EXTRAS, data.Extras);
    updateExtrasBuildOrderTile();
}

function loadExtrasAndUpdateFrames() {
    getAllMappedExtras();
    updateFrameExtras();
}

function loadExtrasAndUpdateLens() {
    getAllMappedExtras();
    updateLensExtras();
}

var initChooseEyeglassExtrasPage = function () {
    window.setupPage(window.buildOrder.EXTRAS);
    getAllMappedExtras();
    updateExtrasBuildOrderTile();
    showExtrasPage();
};