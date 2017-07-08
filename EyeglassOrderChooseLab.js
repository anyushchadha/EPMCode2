/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko,  Modernizr, console, msgType, loadPatientSideNavigation,ApiClient, getTodayDate, alert, setupPage, egOrderType */
var client = new ApiClient("EyeglassOrder");

var orderLabsViewModel = null;
var allLabs = null;

function OrderLabsViewModel(data) {
    var self = this;
    allLabs = data;
    var dataLabsVsp = [];
    var dataLabsNonVsp = [];
    $.each(data, function (index, item) {
        if (item.IsVspLab === true) {
            dataLabsVsp.push(item);
        } else {
            dataLabsNonVsp.push(item);
        }
    });

    self.egOrderLabsVsp = ko.observableArray(dataLabsVsp);
    var temp1 = self.egOrderLabsVsp().slice(0);
    self.displayEgOrderLabsVsp = ko.observableArray();
    while (temp1.length > 0) {
        self.displayEgOrderLabsVsp.push(temp1.splice(0, 4));
    }

    self.egOrderLabsNonVsp = ko.observableArray(dataLabsNonVsp);
    var temp2 = self.egOrderLabsNonVsp().slice(0);
    self.displayEgOrderLabsNonVsp = ko.observableArray();
    while (temp2.length > 0) {
        self.displayEgOrderLabsNonVsp.push(temp2.splice(0, 4));
    }
}

function setLabsVisibility() {
    if (window.getIsVsp() === true) {
        $("#firstLabsGroup").removeClass("hidden");
        $("#secondLabsGroup").removeClass("hidden");
    } else {
        $("#firstLabsGroup").addClass("hidden");
        $("#secondLabsGroup").removeClass("hidden");
    }
}

function logoVisibility() {
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsNonVsp(), function (v) {
        var d = "#logoId_" + v.Id.toString();
        var pid = "#orderLabs_" + v.Id.toString();
        if (v.IsSystemLab === true) {
            $(d).removeClass("hidden");
        } else {
            $(d).addClass("hidden");
        }
        if (window.getIsVsp() === true) {
            $(pid).addClass("disabled-extras-panel");
        } else {
            $(pid).removeClass("disabled-extras-panel");
        }
    });
}

function vspLabCheck() {
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsVsp(), function (v) {
        var d = "#orderLabs_" + v.Id.toString();
        if (window.getIsVsp() === true) {
            if (v.IsVspLab === true) {
                $(d).removeClass("disabled-extras-panel");
            } else {
                $(d).addClass("disabled-extras-panel");
            }
        } else {
            if (v.IsVspLab === true) {
                $(d).addClass("disabled-extras-panel");
            } else {
                $(d).removeClass("disabled-extras-panel");
            }
        }
    });

    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsNonVsp(), function (v) {
        var d = "#orderLabs_" + v.Id.toString();
        if (window.getIsVsp() === true) {
            if (v.IsVspLab === true) {
                $(d).removeClass("disabled-extras-panel");
            } else {
                $(d).addClass("disabled-extras-panel");
            }
        } else {
            if (v.IsVspLab === true) {
                $(d).addClass("disabled-extras-panel");
            } else {
                $(d).removeClass("disabled-extras-panel");
            }
        }
    });
}

//item is VSP Lab
function isEquivalentNonVspLab(item) {
    var found = false;
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsNonVsp(), function (v) {
        if (v.Name === item.Name) {
            found = true;
        }
    });
    return found;
}

//item is Non-VSP Lab
function isEquivalentVspLab(item) {
    var found = false;
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsVsp(), function (v) {
        if (v.Name === item.Name) {
            found = true;
        }
    });
    return found;
}

function setIsLabChecked(id, val) {
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsVsp(), function (v) {
        if (v.Id.toString() === id) {
            v.IsSelected = val;
        }
    });
    ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsNonVsp(), function (v) {
        if (v.Id.toString() === id) {
            v.IsSelected = val;
        }
    });
}

function addLab(item) {
    var vid = item.Id.toString();
    var eid = "#labId_" + item.Id.toString();
    $(eid).removeClass("hidden");
    $(eid).parent().addClass("selected");
    setIsLabChecked(vid, true);
    $.each(allLabs, function (index, itm) {
        if (itm.Id.toString() === vid) {
            $("#chooseLab").addClass("hidden");
            window.updateSelectedEntity(window.buildOrder.LAB, itm);
            window.displayPage(window.buildOrder.LAB, "");
        }
    });
}

function clearLabCheckmarks() {
    var eid = "";
    if (orderLabsViewModel !== null) {
        ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsVsp(), function (v) {
            v.IsSelected = false;
            eid = "#labId_" + v.Id.toString();
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
        });
    }

    if (orderLabsViewModel !== null) {
        ko.utils.arrayForEach(orderLabsViewModel.egOrderLabsNonVsp(), function (v) {
            v.IsSelected = false;
            eid = "#labId_" + v.Id.toString();
            $(eid).addClass("hidden");
            $(eid).parent().removeClass("selected");
        });
    }

    window.updateSelectedEntity(window.buildOrder.LAB, null);
}

function showCheckmark(id) {
    var eid = "#labId_" + id.toString();
    $(eid).removeClass("hidden");
    $(eid).parent().addClass("selected");
}

$("#btnLabsToBuildOrder").click(function (e) {
    e.preventDefault();
    $("#chooseLab").addClass("hidden");
    window.initChooseBuildOrderPage(window.buildOrder.LAB);
});

function initLabsPageFromTempStorage(data) {
    setLabsVisibility();
    window.eyeglassOrderViewModel.selectedLab(data.Lab);
}

function showExtrasOnlyWarning(eyeglassOrderType) {
    if (eyeglassOrderType !== undefined && eyeglassOrderType !== null && eyeglassOrderType === egOrderType.EXTRAS_ONLY) {
        $("#egOrderLabs #egLabMsgPanel #msgWarning").removeClass("hidden");
    } else {
        $("#egOrderLabs #egLabMsgPanel #msgWarning").addClass("hidden");
    }
}

function showLabError() {
    if (window.eyeglassOrderViewModel.selectedLab()) {
        $("#egOrderLabs #egLabMsgPanel #msgError .message").html(window.eyeglassOrderViewModel.selectedLab().ValidationMessage);
    }
    $("#egOrderLabs #egLabMsgPanel #msgError").removeClass("hidden");
}

function clearLabError() {
    if (window.eyeglassOrderViewModel.selectedLab() && !window.eyeglassOrderViewModel.selectedLab().IsValid) {
        window.eyeglassOrderViewModel.selectedLab().ValidationMessage = "";
        window.eyeglassOrderViewModel.selectedLab().IsValid = true;
    }
    $("#egOrderLabs #egLabMsgPanel #msgError .message").html("");
    $("#egOrderLabs #egLabMsgPanel #msgError").addClass("hidden");
}

var initChooseEyeglassLabPage = function () {
    setupPage(window.buildOrder.LAB);
    if (window.eyeglassOrderViewModel.isCopyOrder()) {
        $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order");
    } else {
        $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + window.eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Add Lab");
    }
    $("#chooseLab").removeClass("hidden");
    $("#btnSaveForLater").removeClass("hidden");
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    }

    if (window.eyeglassOrderViewModel.selectedLab() && window.eyeglassOrderViewModel.selectedLab().IsValid === false) {
        showLabError();
    }

    showExtrasOnlyWarning(window.eyeglassOrderViewModel.eyeglassOrderType());

    $("#btnContinue").addClass("hidden");
    $("#btnContinueToPricing").addClass("hidden");
    if (window.labsInitialized === false) {
        client
            .action("GetEgLabsByCompany")
            .get({
                patientId: window.patientOrderExam.PatientId,
                awsResourceId: window.patientOrderExam.AwsResourceId
            })
            .done(function (data) {
                if (data !== undefined && data !== null) {
                    window.labsInitialized = true;
                    orderLabsViewModel = new OrderLabsViewModel(data);
                    ko.applyBindings(orderLabsViewModel, $('#egOrderLabs')[0]);
                    setLabsVisibility();
                    logoVisibility();
                    if (window.eyeglassOrderViewModel !== null && window.eyeglassOrderViewModel.selectedLab() !== null && window.eyeglassOrderViewModel.selectedLab().IsValid !== false) {
                        showCheckmark(window.eyeglassOrderViewModel.selectedLab().Id);
                    }

                    if (!data || data.length <= 0) {
                        $("#egOrderLabs #egLabMsgPanel #msgError .message").html("No lab is available to process the order.  Go to the Labs (Administration) page to activate the labs.");
                        showLabError();
                    }

                    window.scrollTo(0, 0);

                    $("div[id^='orderLabs_']").click(function (e) {
                        var vid = this.id.replace("orderLabs_", "");
                        var eid = "#labId_" + this.id.toString();
                        eid = eid.replace("orderLabs_", "");
                        e.preventDefault();
                        window.eyeglassOrderViewModel.isDirty(true);

                        // reset any Lab server validation messages
                        clearLabError();

                        if ($(eid).hasClass("hidden")) {
                            clearLabCheckmarks();
                            $(eid).removeClass("hidden");
                            $(eid).parent().addClass("selected");
                            setIsLabChecked(vid, true);
                            $.each(allLabs, function (index, item) {
                                if (item.Id.toString() === vid) {
                                    $("#chooseLab").addClass("hidden");
                                    window.updateSelectedEntity(window.buildOrder.LAB, item);
                                    window.displayPage(window.buildOrder.LAB, "Selected lab is added to the eyeglass order.");
                                }
                            });
                        } else {
                            $(eid).addClass("hidden");
                            $(eid).parent().removeClass("selected");
                            setIsLabChecked(vid, false);
                            $("#chooseLab").addClass("hidden");
                            window.updateSelectedEntity(window.buildOrder.LAB, null);
                            window.displayPage(window.buildOrder.LAB, "Selected lab is removed from the eyeglass order.");
                            $("#summary").addClass("hidden");
                            $("#chooseLab").addClass("hidden");
                            window.initChooseBuildOrderPage(window.buildOrder.LAB);
                        }
                    });
                    $("div[id^='orderLabs_']").hover(function (e) {
                        var vid = this.id.replace("orderLabs_", "");
                        $.each(window.allLabs, function (index, item) {
                            if (item.Id.toString() === vid) {
                                var lid = "#logoId_" + item.Id.toString();
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
            });
    } else {
        logoVisibility();
    }
};
