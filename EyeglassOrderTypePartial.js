/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko, Modernizr, console, msgType, loadPatientSideNavigation, ApiClient, alert, buildOrder, egOrderType, eyeglassOrderViewModel */

var orderTypeViewModel = null;
function OrderTypeViewModel() {
    var self = this;
    self.displayEgOrderType = ko.observableArray();
    self.egOrderType = ko.observableArray();
    self.egOrderType.push({ Id: egOrderType.COMPLETE, Name: "Complete Eyeglass" });
    self.egOrderType.push({ Id: egOrderType.LENSES_ONLY, Name: "Lenses Only" });
    self.egOrderType.push({ Id: egOrderType.FRAME_ONLY, Name: "Frame Only" });
    self.egOrderType.push({ Id: egOrderType.EXTRAS_ONLY, Name: "Extras Only" });

    var temp = self.egOrderType().slice(0);
    while (temp.length > 0) {
        self.displayEgOrderType.push(temp.splice(0, 2));
    }
}

var initChooseOrderTypePage = function () {
    window.setupPage(buildOrder.ORDERTYPE);
    $("#orderType").removeClass("hidden");
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order : Choose Order Type");

    if (orderTypeViewModel !== null) {
        //Any event is coming from change insurance right panel?
        if (!eyeglassOrderViewModel.selectedAuthorization().NonInsurance) {
            $("#orderType_4").addClass('disabled-allow-hover');
            $("#orderType_4.tt").popover({ container: "body", html: true, placement: "top" });
        } else {
            $("#orderType_4").removeClass('disabled-allow-hover');
            $("#orderType_4.tt").popover('destroy');
        }
        return;
    }

    //init the orderTypeViewModel, then apply the KO bindings to the egOrderType div only
    orderTypeViewModel = new OrderTypeViewModel();
    ko.applyBindings(orderTypeViewModel, $('#egOrderType')[0]);

    //Add tooltip to Extras Only Order if any insurance exists
    if (!eyeglassOrderViewModel.selectedAuthorization().NonInsurance) {
        $("#orderType_4").addClass('disabled-allow-hover');
        $("#orderType_4.tt").popover({ container: "body", html: true, placement: "top" });
    }

    $("div[id^='orderType_']").click(function (e) {
        e.preventDefault();
        switch (this.id) {
        case "orderType_1": //Complete Eyeglass Order
            $("#orderType").addClass("hidden");
            window.updateSelectedEyeglassOrderType(egOrderType.COMPLETE, "Complete");
            window.initChooseEyeglassRxPage();
            break;
        case "orderType_2": //Lenses Only Order
            $("#orderType").addClass("hidden");
            window.updateSelectedEyeglassOrderType(egOrderType.LENSES_ONLY, "Lenses Only");
            window.initChooseEyeglassRxPage();
            break;
        case "orderType_3": //Frame Only Order
            $("#orderType").addClass("hidden");
            window.updateSelectedEyeglassOrderType(egOrderType.FRAME_ONLY, "Frame Only");
            window.initChooseOrderFramePage();
            break;
        case "orderType_4": //Extras Only Order
	        if (!eyeglassOrderViewModel.selectedAuthorization().NonInsurance) {
                return;
	        }
	        $("#orderType").addClass("hidden");
	        window.updateSelectedEyeglassOrderType(egOrderType.EXTRAS_ONLY, "Extras Only");
	        if (eyeglassOrderViewModel.selectedAuthorization().NonInsurance) {
	            window.initChooseBuildOrderPage();
	        } else {
	            window.extrasFrom = "OrderType";
	            window.initChooseEyeglassExtrasPage();
	        }
            break;
        }
    });

    window.scrollTo(0, 0);
};
