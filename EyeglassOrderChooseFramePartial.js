/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko, Modernizr, console, msgType, loadPatientSideNavigation, ApiClient, alert, regex, buildOrder, egOrderType, eyeglassOrderViewModel, getTodayTime */

var frameTable, initFramePage = true;
var clientFrame = null;
var vmFrame = null, frameSelected = null;

var FrameViewModel = function (frame) {
    var self = this;
    self.isDirty = ko.observable(frame !== null);
    self.frameModel = ko.observable(frame.Model);
    self.manufacturerDesc = ko.observable(frame.ManufacturerDesc);
    self.collectionDesc = ko.observable(frame.CollectionDesc);
    self.upcDisplay = ko.observable(frame.UPCDisplay);
    self.itemNumber = ko.observable(frame.Number);
    self.customerFrameName = ko.observable(frame.Name);
    self.color = ko.observable(frame.Color);
    self.customerFrameColor = ko.observable(frame.Color);
    self.wholesalePrice = ko.observable(frame.ListPrice !== null ? frame.ListPrice.toFixed(2) : "0.00");
    self.retailPriceDisplay = ko.observable("$" + frame.Retail.toFixed(2));
    self.retailPrice = ko.observable(frame.Retail.toFixed(2)); //modal dialog
    self.isMedicaid = ko.observable(frame.IsMedicaid ? "Yes" : "No");
    self.eye = ko.observable(frame.Eye || "");
    self.bridge = ko.observable(frame.Bridge || "");
    self.temple = ko.observable(frame.Temple || "");
    self.aMeasure = ko.observable(frame.AMeasure || "");
    self.bMeasure = ko.observable(frame.BMeasure || "");
    self.edMeasure = ko.observable(frame.EdMeasure || "");
    self.onHandQty = ko.observable(frame.OnHandQty);
    self.isSafety = ko.observable(frame.IsSafety);
    self.frameWrap = ko.observable(frame.FrameWrap || "");
    self.pantoscopicTilt = ko.observable(frame.PantoscopicTilt || "");
    self.vertex = ko.observable(frame.Vertex || "");

    self.edgeTypeList = ko.observableArray(frame.FrameEdgeTypeList);
    self.selectedEdgeType = ko.observable(frame.FrameEdgeTypeID);
    self.shapeList = ko.observableArray(frame.ShapeList === null ? [] : frame.ShapeList);
    self.shapeList.unshift({ "Key": 0, "Description": "Select" });
    self.selectedShape = ko.observable(frame.ShapeID);
    self.materialList = ko.observableArray(frame.MaterialList === null ? [] : frame.MaterialList);
    self.materialList.unshift({ "Key": 0, "Description": "Select" });
    self.selectedMaterial = ko.observable(frame.MaterialID);

    self.frameSourceList = ko.observableArray(frame.FrameSourceList === null ? [] : frame.FrameSourceList);
    self.frameSourceList.unshift({ "Key": 0, "Description": "Select" });
    self.selectedFrameSource = ko.observable(frame.FrameSourceID);
    self.selectedFrameSource.subscribe(function (newValue) {
        $("#frameSource").clearField();
        if (newValue === 2) {   //store enclosed
            $("#divOnhandQty").removeClass("hidden");
            if (frame.OnHandQty < 0 && frame.AllowNegativeOnHandQty === false) {
                $("#negativeQty").removeClass("hidden");
            }
        } else {
            $("#divOnhandQty").addClass("hidden");
            $("#negativeQty").addClass("hidden");
        }
        $("#frameSource").parents("div.bootstrap-select").removeClass('requiredField');
    });

    self.importModel = function (model) {
        vmFrame.isDirty(true);
        vmFrame.frameModel(model.Model);
        vmFrame.manufacturerDesc(model.ManufacturerDesc);
        vmFrame.collectionDesc(model.CollectionDesc);
        vmFrame.upcDisplay(model.UPCDisplay);
        vmFrame.itemNumber(model.Number);
        vmFrame.customerFrameName(model.Name);
        vmFrame.customerFrameColor(model.Color);
        vmFrame.color(model.Color);
        vmFrame.wholesalePrice(model.ListPrice !== null ? model.ListPrice.toFixed(2) : "0.00");
        vmFrame.retailPriceDisplay("$" + model.Retail.toFixed(2));
        vmFrame.retailPrice(model.Retail.toFixed(2));
        vmFrame.isMedicaid(model.IsMedicaid ? "Yes" : "No");
        vmFrame.eye(model.Eye || "");
        vmFrame.bridge(model.Bridge || "");
        vmFrame.temple(model.Temple || "");
        vmFrame.aMeasure(model.AMeasure || "");
        vmFrame.bMeasure(model.BMeasure || "");
        vmFrame.edMeasure(model.EdMeasure || "");
        vmFrame.onHandQty(model.OnHandQty);
        vmFrame.isSafety(model.IsSafety);
        vmFrame.frameWrap(model.FrameWrap || "");
        vmFrame.pantoscopicTilt(model.PantoscopicTilt || "");
        vmFrame.vertex(model.Vertex || "");
        vmFrame.selectedFrameSource(model.FrameSourceID);
        vmFrame.selectedEdgeType(model.FrameEdgeTypeID);
        vmFrame.selectedShape(model.ShapeID);
        vmFrame.selectedMaterial(model.MaterialID);
    };

    self.toModel = function (mode) {
        var self = this;
        var model = frameSelected;
        if (mode === "SaveRetailPrice") {
            self.retailPriceDisplay("$" + parseFloat(self.retailPrice()).toFixed(2));
            model.Retail = parseFloat(self.retailPrice()).toFixed(2);
        }
        model.IsValid = false;
        model.IsSafety = self.isSafety();
        model.Eye = self.eye();
        model.Bridge = self.bridge();
        if (model.DblMeasure === null || model.DblMeasure === 0) {
            model.DblMeasure = model.Bridge;
        }
        model.Temple = self.temple();
        model.AMeasure = self.aMeasure();
        model.BMeasure = self.bMeasure();
        model.EdMeasure = self.edMeasure();
        model.FrameWrap = self.frameWrap();
        if (self.frameWrap() !== undefined && self.frameWrap() !== null && self.frameWrap() !== '') {
            model.FrameWrap = parseFloat(self.frameWrap()).toFixed(2);
        }
        model.PantoscopicTilt = self.pantoscopicTilt();
        if (self.pantoscopicTilt() !== undefined && self.pantoscopicTilt() !== null && self.pantoscopicTilt() !== '') {
            model.PantoscopicTilt = parseFloat(self.pantoscopicTilt()).toFixed(2);
        }
        model.Vertex = self.vertex();
        if (self.vertex() !== undefined && self.vertex() !== null && self.vertex() !== '') {
            model.Vertex = parseFloat(self.vertex()).toFixed(2);
        }
        model.FrameSourceID = self.selectedFrameSource();
        model.FrameEdgeTypeID = self.selectedEdgeType();
        model.FrameShapeID = self.selectedShape();
        model.FrameMaterialID = self.selectedMaterial();

        model.FrameSourceDesc = $("#frameSource option:selected").text();
        model.FrameEdgeTypeDesc = $("#edgeType option:selected").text();
        model.ShapeDesc = model.ShapeID === 0 ? '' : $("#shape option:selected").text();
        model.MaterialDesc = model.MaterialID === 0 ? '' : $("#material option:selected").text();
        model.FrameDesc = "";

        //lenses only or extras only order (Patient Own Frame)
        if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
            model.Name = self.customerFrameName();
            model.Color = self.customerFrameColor();
            model.FrameDesc = eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY ? "POF" : "POF_LENS";
        } else if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.FRAME_ONLY) {
            model.FrameDesc = "NO_LENS";
        }

        return model;
    };
};

var initSelectedFrame = function (frame) {
    if (vmFrame === null) {
        vmFrame = new FrameViewModel(frame);
        ko.applyBindings(vmFrame, $("#selectedFrame")[0]);
        $("#frameSource").refreshSelectPicker();
        vmFrame.selectedFrameSource.valueHasMutated();
    } else {
        vmFrame.importModel(frame);
        $("#frameSource").clearField();
        $("#frameSource").recreateSelectPicker();
    }

    if (window.egDataSource !== "order") {
        $("#btnSaveForLater").removeClass("hidden");
    }

    if (frame.Image !== null) {
        $("#egImage img").attr("src", "data:image/png;base64," + frame.Image);
        $("#egNoImage").addClass("hidden");
        $("#egImage").removeClass("hidden");
    } else {
        $("#egImage img").attr("src", "");
        $("#egImage").addClass("hidden");
        $("#egNoImage").removeClass("hidden");
    }

    switch (eyeglassOrderViewModel.eyeglassOrderType()) {
    case egOrderType.COMPLETE:
        $("#shape, #material, #edgeType, #isSafety").attr("disabled", "disabled");
        $("#frameWrap, #pantoscopicTilt, #vertex").removeAttr("disabled", "disabled");
        break;
    case egOrderType.LENSES_ONLY:
        $("#shape, #material").attr("disabled", "disabled");
        $("#edgeType, #isSafety, #frameWrap, #pantoscopicTilt, #vertex").removeAttr("disabled");
        break;
    case egOrderType.FRAME_ONLY:
        $("#shape, #material, #edgeType, #isSafety, #frameWrap, #pantoscopicTilt, #vertex").attr("disabled", "disabled");
        break;
    case egOrderType.EXTRAS_ONLY:
        $("#shape, #material, #frameWrap, #pantoscopicTilt, #vertex").attr("disabled", "disabled");
        $("#edgeType, #isSafety").removeAttr("disabled");
        break;
    }

    $("#shape, #material, #edgeType").refreshSelectPicker();

    //Verify on hand negative quantity if frame source is 'store enclosed' 
    if (frame.OnHandQty < 0 && frame.AllowNegativeOnHandQty === false && frame.FrameSourceID === 2) {
        $("#negativeQty").removeClass("hidden");
    } else {
        $("#negativeQty").addClass("hidden");
    }
    //add 'requiredField' if none selected otherwise remove it 
    if (frame.FrameSourceID === 0) {
        $("#frameSource").parents("div.bootstrap-select").addClass('requiredField');
    } else {
        $("#frameSource").parents("div.bootstrap-select").removeClass('requiredField');
    }

    frame.FrameSourceDesc = $("#frameSource option:selected").text();
    frame.FrameEdgeTypeDesc = $("#edgeType option:selected").text();
    frame.ShapeDesc = frame.ShapeID === 0 ? '' : $("#shape option:selected").text();
    frame.MaterialDesc = frame.MaterialID === 0 ? '' : $("#material option:selected").text();

    frameSelected = frame;
};

var frameClearFields = function () {
    var i, arr = ["#eye", "#bridge", "#temple", "#aMeasure", "#bMeasure", "#edMeasure", "#customerFrameName", "customerFrameColor",
        "#frameWrap", "#pantoscopicTilt", "#vertex"];
    for (i = 0; i < arr.length; i++) {
        $(arr[i]).clearField();
        if (i <= 7) {
            if ($(arr[i]).val() === "") {
                $(arr[i]).addClass("requiredField");
            }
        }
    }
};

var selectFrameId = function (id, upc, productCode) {
    clientFrame
        .action("GetEgFrameByFrame")
        .get({
            orderType: eyeglassOrderViewModel.eyeglassOrderType(),
            frameId: id,
            officeNumber: window.config.officeNumber,
            upcCode: upc,
            productCode: productCode
        })
        .done(function (data) {
            initSelectedFrame(data);
            frameClearFields();
        });
};

function showFrameError() {
    if (window.eyeglassOrderViewModel.selectedFrame()) {
        $("#selectedFrame #egFrameMsgPanel #msgError .message").html(window.eyeglassOrderViewModel.selectedFrame().ValidationMessage);
    }
    $("#selectedFrame #egFrameMsgPanel #msgError").removeClass("hidden");
}

function clearFrameError() {
    if (window.eyeglassOrderViewModel.selectedFrame() && !window.eyeglassOrderViewModel.selectedFrame().IsValid) {
        window.eyeglassOrderViewModel.selectedFrame().ValidationMessage = "";
    }
    $("#selectedFrame #egFrameMsgPanel #msgError .message").html("");
    $("#selectedFrame #egFrameMsgPanel #msgError").addClass("hidden");
}

var buildFrameTable = function () {
    frameTable = $('#frameTable').alslDataTable({
        "iDisplayLength": 10,
        "aaSorting": [[1, "asc"]],
        "aoColumns": [
            { "sTitle": "Manufacturer", "mData": "ManufacturerDesc", "sType": "string", "sClass": "left col-lg-2 col-md-2 col-sm-2 hidden-xs", "bSortable": true },
            { "sTitle": "Collection", "mData": "CollectionDesc", "sType": "string", "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-3", "bSortable": true },
            { "sTitle": "Model", "mData": "Model", "sType": "string", "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-3", "bSortable": true },
            { "sTitle": "Eye", "mData": "Eye", "sType": "string", "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1", "bSortable": true },
            { "sTitle": "Temple", "mData": "Temple", "sType": "string", "sClass": "left col-lg-1 col-md-1 col-sm-1 col-xs-1", "bSortable": true },
            { "sTitle": "Color", "mData": "Color", "sType": "string", "sClass": "left col-lg-2 col-md-2 col-sm-2 col-xs-4", "bSortable": true },
            { "sTitle": "UPC", "mData": "UPCDisplay", "sType": "string", "sClass": "left col-lg-2 col-md-2 col-sm-2 hidden-xs", "bSortable": true }],
        "bAutoWidth": false,
        "oLanguage": { "sEmptyTable": "Enter information in the search field above to search for frames." },
        "selectableRows": true
    });

    frameTable.delegate("tbody td", "click", function () {
        if (frameTable.fnPagingInfo().iTotal !== 0) {
            var aPos = frameTable.fnGetPosition(this);
            var aData = frameTable.fnGetData(aPos[0]);
            var upc = aData.UPCCode === null ? "" : aData.UPCCode.trim();
            var productCode = aData.JobsonProductCode === null ? "" : aData.JobsonProductCode.trim();

            // reset any Frame server validation messages
            clearFrameError();

            $("#addFrame").addClass("hidden");
            $("#selectedFrame, #btnAddFrameToOrder").removeClass("hidden");
            selectFrameId(aData.Id, upc, productCode);
            window.scrollTo(0, 0);
        }
    });
};

var validateFrame = function () {
    $.validator.addMethod("validateZeroPrice", function (value, element) {
        if ($("#framePriceModal").is(':visible') === true) {
            if (element.value !== '') {
                if ($("#retailPrice").val() !== '') {
                    var price = parseFloat($("#retailPrice").val());
                    if (price !== 0) {
                        return true;
                    }
                }
            }
            return false;
        }
        return true;
    });
    $.validator.addMethod("validateFrameSource", function (value, element) {
        var val = $('#frameSource').val();
        return val === "0" ? false : true;
    });
    $.validator.addMethod("validateText", function (value, element) {
        var valid = true;
        //lenses only or extras only order (Patient Own Frame)
        if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
            valid = /^[a-zA-Z0-9\. \-$]+$/.test(element.value);
        }
        return valid;
    });

    $("#selectedFrameForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            eye: { required: true, range: [1, 99], Regex: regex.NUM },
            bridge: { required: true, range: [1, 99], Regex: regex.NUM },
            temple: { required: true, range: [1, 999], Regex: regex.NUM },
            aMeasure: { required: true, range: [10, 80], Regex: regex.NUM },
            bMeasure: { required: true, range: [1, 99], Regex: regex.NUM },
            edMeasure: { required: true, range: [1, 99], Regex: regex.NUM },
            frameSource: { required: true, validateFrameSource: true },
            frameWrap: { range: [0.0, 30.0] },
            pantoscopicTilt: { range: [-5.0, 25.0] },
            vertex: { range: [5.0, 25.0] },
            retailPrice: { required: true, validateZeroPrice: true },
            customerFrameName: { required: true, validateText: true },
            customerFrameColor: { required: true, validateText: true }
        },
        messages: {
            eye: { required: "Enter the Eye size (1 to 99).", Range: "Enter the Eye size (1 to 99).", Regex: "Enter the Eye size (1 to 99)." },
            bridge: { required: "Enter the Bridge width (1 to 99).", Range: "Enter the Bridge width (1 to 99).", Regex: "Enter the Bridge width (1 to 99)." },
            temple: { required: "Enter the Temple length (1 to 999).", Range: "Enter the Temple length (1 to 999).", Regex: "Enter the Temple length (1 to 999)." },
            aMeasure: { required: "Enter the A measurement (10 to 80).", range: "Enter the A measurement (10 to 80).", Regex: "Enter the A measurement (10 to 80)." },
            bMeasure: { required: "Enter the B measurement (1 to 99).", range: "Enter the B measurement (1 to 99).", Regex: "Enter the B measurement (1 to 99)." },
            edMeasure: { required: "Enter the ED measurement (1 to 99).", range: "Enter the ED measurement (1 to 99).", Regex: "Enter the ED measurement (1 to 99)." },
            frameSource: { required: "Select the Frame Source.", validateFrameSource: "Select the Frame Source." },
            frameWrap: { range: "Enter the frame wrap (0.0 to 30.0)." },
            pantoscopicTilt: { range: "Enter the Pantoscopic tilt (-5.0 to 25.0)." },
            vertex: { range: "Enter the vertex (5.0 to 25.0)." },
            retailPrice: { required: "Enter a non-zero retail price.", validateZeroPrice: "Enter a non-zero retail price." },
            customerFrameName: { required: "Enter the Name.", validateText: "The special characters you entered are not allowed. Enter a valid Name." },
            customerFrameColor: { required: "Enter the Color.", validateText: "The special characters you entered are not allowed. Enter a valid Color." }
        }
    });
};

var initChooseOrderFramePage = function () {
    var name = (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) ? "Frame Details" : "Choose Frame";
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : " + name);
    if (clientFrame === null) {
        clientFrame = new ApiClient("Frame");
    }

    if (initFramePage) {
        initFramePage = false;
        buildFrameTable();
        validateFrame();
        $(".tt").popover({ container: 'body', html: true, placement: 'top' });
    }

    window.setupPage(buildOrder.FRAME);
    var frame = eyeglassOrderViewModel.selectedFrame();
    if (frame !== null) {
        $("#chooseFrame, #selectedFrame").removeClass("hidden");
        $("#addFrame").addClass("hidden");
    } else {
        $("#chooseFrame, #addFrame").removeClass("hidden");
        $("#selectedFrame, #btnAddFrameToOrder").addClass("hidden");
    }

    //lenses only or extras only order (Patient Own Frame)
    if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
        $("#addFrame, #divInfoFrame, #divShape, #divMaterial, #linkChangeFrame").addClass("hidden");
        $("#selectedFrame, #divCustomerFrame, #btnAddFrameToOrder").removeClass("hidden");
        $("#divFrameSource").css("height", "245px");
    }

    //come back from Admin custom frame
    if (window.customFrame.fromAdmin) {
        $("#addFrame").addClass("hidden");
        $("#selectedFrame, #btnAddFrameToOrder").removeClass("hidden");
        window.customFrame.fromAdmin = false;
        var upc = $.urlParam(window.location.href, "upc", true);
        selectFrameId(window.customFrame.id, upc, "");
        window.scrollTo(0, 0);
        return;
    }

    //reload selected frame
    if (frame !== null) {
        frameClearFields();
        initSelectedFrame(frame);
    } else {
        //lenses only or extras only (Patient Own Frame)
        if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
            frameClearFields();
            clientFrame
                .action("GetEgFrameBySearchCriteria")
                .get({ searchCriteria: "PatientOwnFrame", officeNumber: window.config.officeNumber })
                .done(function (data) {
                    initSelectedFrame(data);
                })
                .fail(function (xhr) {
                    var msg = JSON.parse(xhr.responseText);
                    alert(msg);
                });
        } else {
            //frame search
            $("#frameSearch").val("");
            frameTable.fnClearTable();
            frameTable.fnSettings().oLanguage.sEmptyTable = "Enter information in the search field above to search for frames.";
            frameTable.fnDraw();
            $("#frameSearch").focus();
        }
    }

    if (window.eyeglassOrderViewModel.selectedFrame() && window.eyeglassOrderViewModel.selectedFrame().IsValid === false) {
        showFrameError();
    }

    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    }
    window.scrollTo(0, 0);
};

$("#frameSearch").keydown(function (evt) {
    var charCode = evt.which || evt.keyCode;
    if (charCode === 13) {
        $("#btnFrameSearch").focus().click();
        evt.preventDefault();
    }
});

$("#eye, #bridge, #temple, #aMeasure, #bMeasure, #edMeasure").keydown(function (e) {
    return $(this).integerFilter(e);
});
$("#frameWrap, #pantoscopicTilt, #vertex, #retailPrice").keydown(function (e) {
    var allowNegative = this.id === "pantoscopicTilt" ? true : false;
    return $(this).decimalFilter(e, allowNegative);
});

$("#btnCustomFrame, #btnCustomFrame-xs, #btnFrameToBuildOrder, #linkChangeFrame, " +
    "#btnFrameSearch, #btnAddFrameToOrder, #btnSaveFramePrice, #btnCancelFramePrice").click(function (e) {
    e.preventDefault();
    var selectedFrame = null;
    switch (this.id) {
    case "btnCustomFrame":
    case "btnCustomFrame-xs":
        window.customFrame.redirect = true;
        var patientId = $.urlParam(window.location.href, "id", true);
        var id = patientId + 'z' + getTodayTime();
        if (window.egDataSource === "new") {
            $("#resourceId").val(id);
            $("#btnSaveTempEgOrder").click();
        } else {
            var resourceId = $.urlParam(window.location.href, "resourceId", true);
            var arr = resourceId.split('z');
            //user changes custom frame again after selecting the first one a while ago
            if (arr.length === 5 && patientId.toString() === arr[0]) {
                $("#resourceId").val(id);
                $("#btnSaveTempEgOrder").click();
            } else {
                var data = window.getEyeglassOrderDetailsFromViewModel();
                data.AwsResourceId = window.patientOrderExam.AwsResourceId;
                window.getIpSummaryAndSave(data);
            }
        }
        break;

    case "btnFrameToBuildOrder":
        $("#chooseFrame").addClass("hidden");
        window.initChooseBuildOrderPage(buildOrder.FRAME);
        break;

    case "linkChangeFrame":
        $("#selectedFrame, #btnAddFrameToOrder").addClass("hidden");
        $("#addFrame").removeClass("hidden");
        $("#frameSearch").focus();
        window.scrollTo(0, 0);
        break;

    case "btnFrameSearch":
        var searchData = $("#frameSearch").val().trim();
        if (searchData === "") {
            frameTable.fnClearTable();
            frameTable.fnSettings().oLanguage.sEmptyTable = "Enter information in the search field above to search for frames.";
            frameTable.fnDraw();
        } else {
            clientFrame
                .action("GetEgFrameBySearchCriteria")
                .get({ searchCriteria: searchData, officeNumber: window.config.officeNumber })
                .done(function (data) {
                    frameTable.refreshDataTable(data);
                    if (data.length === 0) {
                        $("#frameSearch").focus();
                    }
                })
                .fail(function (xhr) {
                    var msg = JSON.parse(xhr.responseText);
                    frameTable.fnClearTable();
                    frameTable.fnSettings().oLanguage.sEmptyTable = msg;
                    frameTable.fnDraw();
                    $("#frameSearch").focus();
                });
        }
        break;

    case "btnAddFrameToOrder":
        vmFrame.retailPrice("0.00");
        if ($("#selectedFrameForm").valid() && $("#negativeQty").hasClass("hidden")) {
            //lenses only or extras only order (Patient Own Frame)
            if (frameSelected.FrameDesc === null) {
                frameSelected.FrameDesc = '';
                if (eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.LENSES_ONLY || eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY) {
                    frameSelected.FrameDesc = eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY ? "POF" : "POF_LENS";
                }
            }
            if (frameSelected.Retail === 0 && frameSelected.FrameDesc.indexOf("POF") === -1) {
                $("#framePriceModal").modal({
                    keyboard: false,
                    backdrop: 'static',
                    show: true
                });
                setTimeout(function () {
                    $("#retailPrice").focus();
                }, 200);
            } else {
                selectedFrame = vmFrame.toModel("CancelRetailPrice");
                $("#chooseFrame").addClass("hidden");
                vmFrame.isDirty(false);
                selectedFrame.IsValid = true;

                //need to update in case the selected frame is inactive
                if (selectedFrame.Stat === "Inactive") {
                    //if POF's retail price = 0, don't care just put $1 for making the frame active
                    if (selectedFrame.Retail === 0) {
                        selectedFrame.Retail = 1;
                    }
                    clientFrame
                        .action("UpdateEgFrameRetailPrice")
                        .put(selectedFrame, { "officeNumber": window.config.officeNumber, "retailPrice": selectedFrame.Retail })
                        .done(function () {
                            //do nothing
                        });
                }

                clearFrameError();

                window.updateSelectedEntity(buildOrder.FRAME, selectedFrame);
                window.displayPage(buildOrder.FRAME, "Selected frame is added to the eyeglass order.");
            }
        }
        break;

    case "btnSaveFramePrice":
        if (!$("#selectedFrameForm").valid()) {
            return;
        }

        var retailPrice = parseFloat(vmFrame.retailPrice()).toFixed(2);
        clientFrame
            .action("UpdateEgFrameRetailPrice")
            .put(frameSelected, { "officeNumber": window.config.officeNumber, "retailPrice": retailPrice })
            .done(function (data) {
                selectedFrame = vmFrame.toModel("SaveRetailPrice");
                $("#framePriceModal").modal('toggle');
                $("#chooseFrame").addClass("hidden");
                vmFrame.isDirty(false);
                selectedFrame.IsValid = true;

                // reset any Frame server validation messages
                clearFrameError();

                window.updateSelectedEntity(buildOrder.FRAME, selectedFrame);
                window.displayPage(buildOrder.FRAME, "Selected frame is added to the eyeglass order.");
            })
            .fail(function (xhr) {
                $("#framePriceModal").modal('toggle');
                $(document).showSystemFailure(xhr.responseText.replace(/\"/g, ''));
            });
        break;

    case "btnCancelFramePrice":
        //May need to map the frame even though the user has canceled updating the price here.
        clientFrame
            .action("UpdateEgFrameRetailPrice")
            .put(selectedFrame, { "officeNumber": window.config.officeNumber, "retailPrice": "0" })
            .done(function (data) {})
            .fail(function (xhr) {});

        selectedFrame = vmFrame.toModel("CancelRetailPrice");
        $("#framePriceModal").modal('toggle');
        $("#chooseFrame").addClass("hidden");
        vmFrame.isDirty(false);
        selectedFrame.IsValid = true;
        // reset any Frame server validation messages
        clearFrameError();
        window.updateSelectedEntity(buildOrder.FRAME, selectedFrame);
        window.displayPage(buildOrder.FRAME, "Selected frame is added to the eyeglass order.");
        break;
    }
});