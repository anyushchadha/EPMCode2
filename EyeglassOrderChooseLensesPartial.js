/*jslint browser: true, vars: true, plusplus: true */
/*global $, document, window, ko, Modernizr, console, msgType, loadPatientSideNavigation, ApiClient, alert, eyeglassOrderViewModel, egOrderType, setupPage,
 setBuildOrderTileError, setBuildOrderTileComplete, setBuildOrderTileReset, buildOrder, toggleGeneralError, leftLensType, leftLensMaterial, rightLensType, rightLensMaterial */

var client = new ApiClient("EyeglassOrder"),
    selectedLenses,
    vmLenses = null,
    isVspInsurance = false,
    materialIdRight = 0,
    styleIdRight = 0,
    colorIdRight = 0,
    coatingIdRight = 0,
    materialIdLeft = 0,
    styleIdLeft = 0,
    colorIdLeft = 0,
    coatingIdLeft = 0,
    tintColorId = 0,
    tintColorIdLeft = 0,
    tintTypeDescription = "",
    isMultiFocal = false,
    isLensLoading = false,
    isPageInitialized = false,
    lensMeasurements = null,
    //searchLensType = "",
    lensAttributeTable,
    searchLensSide;

var egLens = { RIGHT: 0, LEFT: 1, BOTH: 2 };
var messageAddColor = "You have selected a lens color that has an additional attribute of Polarization. " +
    "<br>The miscellaneous item ‘{0}’ will be automatically added to this lab order to ensure that the order, insurance claim and patient amount are correct.";
var messageAddCoating = "You have selected an AR coating that has an additional attribute of " +
    "‘{0}’.<br>The ‘{1}’ coating will be automatically added to this lab order to ensure that the order, insurance claim and patient amount are correct.";

function customizeLens() {
    $("#leftLensImage, #lensSelectionTitle").addClass("hidden");
    $("#leftLensSelections").removeClass("hidden");
    $("#lensSelectionRightTitle, #lensSelectionLeftTitle, #rightSlabOff, #rightBalanced, #labelBothLenses").removeClass("hidden");
    $("#spanRightUncut").text("Uncut (applies to both lenses)");
    $("#spanRightLensIof").text("In Office (applies to both lenses)");
    if (vmLenses.leftLensTypeId() === 0) {
        $("#leftLensType").addClass("requiredField");
    }
    $("#leftLensType, #leftLensMaterial, #leftLensStyle, #leftLensColor").change();
}

function resetPage() {
    $("#leftLensImage, #lensSelectionTitle").removeClass("hidden");
    $("#leftLensSelections").addClass("hidden");
    $("#lensSelectionRightTitle, #lensSelectionLeftTitle, #rightSlabOff, #rightBalanced, #labelBothLenses").addClass("hidden");
    $("#rightMaterial, #leftMaterial, #rightStyle, #leftStyle, #rightColor, #leftColor, " +
        "#rightManufacturer, #leftManufacturer, #rightManufacturerCoatingDiv, #leftManufacturerCoatingDiv").addClass("hidden");
    $("#leftMfgCoatingRequired").addClass("hidden");
    $("#rightMfgCoatingRequired").addClass("hidden");
    $("#tintColorRequired").addClass("hidden");
    $("#segHeightRequired").addClass("hidden");
    $("#measureRightSH").removeClass("requiredField");
    $("#measureLeftSH").removeClass("requiredField");
    $("#measureRightOCH, #measureRightSH, #measureLeftOCH, #measureLeftSH").attr("disabled", "disabled");
    $("#measureRightOCH").clearField();
    $("#measureRightSH").clearField();
    $("#measureLeftOCH").clearField();
    $("#measureLeftSH").clearField();
    $("#measureRightBC").clearField();
    $("#measureLeftBC").clearField();
    $("#measureRightThickness").clearField();
    $("#measureLeftThickness").clearField();
    $("#rightFarPupilDistance").clearField();
    $("#rightNearPupilDistance").clearField();
    $("#leftFarPd").clearField();
    $("#leftNearPd").clearField();
    $("#spanRightUncut").text("Uncut");
    $("#spanRightLensIof").text("In Office");
}

function setDirtyFlag() {
    $("#chooseLenses :input").change(function () {
        vmLenses.isDirty(true);
    });
    $("#chooseLenses select").change(function () {
        vmLenses.isDirty(true);
    });
}

function changeDropdownList() {
    if (vmLenses.rightLensTypeId()) {
        $("#rightLensType").change();
    }
    if (vmLenses.rightMaterialId()) {
        $("#rightLensMaterial").change();
    }
    if (vmLenses.rightStyleId()) {
        $("#rightLensStyle").change();
    }
    if (vmLenses.rightColorId()) {
        $("#rightLensColor").change();
    }
    if (vmLenses.rightCoatingId()) {
        $("#rightManufacturerCoating").change();
    }
    if (vmLenses.isCustom()) {
        if (vmLenses.leftLensTypeId()) {
            $("#leftLensType").change();
        }
        if (vmLenses.leftMaterialId()) {
            $("#leftLensMaterial").change();
        }
        if (vmLenses.leftStyleId()) {
            $("#leftLensStyle").change();
        }
        if (vmLenses.leftColorId()) {
            $("#leftLensColor").change();
        }
        if (vmLenses.leftCoatingId()) {
            $("#leftManufacturerCoating").change();
        }
    }
}

function resetTintColor() {
    $("#tintColorRequired").addClass("hidden");
    $("select#tintColor").clearField();
    $("select#tintColor").removeClass("requiredField").rules("remove", "selectBox");
    $("select#tintColor").parent().removeClass("requiredField");
    $("select#tintColor").selectpicker('refresh');
    tintColorId = 0;
}

function loadLens(lens, lensTypeId, materialId, styleId, colorId, coatingId) {
    if (!isLensLoading) {
        client
            .action("GetLensByAttributes")
            .get({ lensTypeId: lensTypeId, lensMaterialId: materialId, lensStyleId: styleId, lensColorId: colorId, lensMfgCoating: coatingId })
            .done(function (data) {
                if (data !== undefined && data !== null) {
                    if (lens === egLens.RIGHT || lens === egLens.BOTH) {
                        vmLenses.rightLens(data);
                        vmLenses.importLensInfo(egLens.RIGHT, data);
                    }

                    if (lens === egLens.LEFT || lens === egLens.BOTH) {
                        vmLenses.leftLens(data);
                        vmLenses.importLensInfo(egLens.LEFT, data);
                    }
                }
            });
    }
}

function convertValue(value, decimal, allowZero) {
    var returnValue = "";
    if (allowZero === undefined || allowZero === null) {
        allowZero = false;
    }
    if (value !== null && value !== undefined) {
        if (parseFloat(value) !== 0) {
            returnValue = value !== "" ? parseFloat(value).toFixed(decimal) : "";
        } else {
            if (allowZero) {
                returnValue = parseFloat(0).toFixed(decimal);
            }
        }
    }
    return returnValue;
}

function setMaterialList(eyeType, rollup, typeId) {
    var eye = "left";
    if (eyeType === egLens.RIGHT) {
        eye = "right";
    }

    if (typeId && typeId > 0) {
        $("select#" + eye + "LensMaterial").selectpicker('refresh');
        $("#" + eye + "Material").removeClass("hidden");
    } else {
        $("#" + eye + "Material").addClass("hidden");
    }

    if (rollup) {
        $("#" + eye + "Style, #" + eye + "Color, #" + eye + "Manufacturer").addClass("hidden");
        if (eye === "right") {
            styleIdRight = colorIdRight = coatingIdRight = 0;
            vmLenses.rightMaterialThickness("");
        } else {
            styleIdLeft = colorIdLeft = coatingIdLeft = 0;
            vmLenses.leftMaterialThickness("");
        }
    }
} // setMaterialList

function setStyleList(eyeType, rollup, materialId) {
    var eye = "left";
    if (eyeType === egLens.RIGHT) {
        eye = "right";
    }

    if (materialId && materialId > 0) {
        $("select#" + eye + "LensStyle").selectpicker('refresh');
        $("#" + eye + "Style").removeClass("hidden");
        $("#" + eye + "Iof").removeClass("hidden");
    } else {
        $("#" + eye + "Style").addClass("hidden");
        $("#" + eye + "Iof").addClass("hidden");
    }

    if (rollup) {
        $("#" + eye + "Color, #" + eye + "Manufacturer").addClass("hidden");
        if (eye === "right") {
            colorIdRight = coatingIdRight = 0;
            vmLenses.rightMaterialThickness("");
        } else {
            colorIdLeft = coatingIdLeft = 0;
            vmLenses.leftMaterialThickness("");
        }
    }
} // setStyleList

function setColorList(eyeType, rollup, styleId) {
    var eye = "left";
    if (eyeType === egLens.RIGHT) {
        eye = "right";
    }

    if (styleId && styleId > 0) {
        $("select#" + eye + "LensColor").selectpicker('refresh');
        $("#" + eye + "Color").removeClass("hidden");
    } else {
        $("#" + eye + "Color").addClass("hidden");
    }

    if (rollup) {
        $("#" + eye + "Manufacturer").addClass("hidden");
        if (eye === "right") {
            coatingIdRight = 0;
            vmLenses.rightMaterialThickness("");
        } else {
            coatingIdLeft = 0;
            vmLenses.leftMaterialThickness("");
        }
    }
} // setColorList

function setCoatingList(eyeType, noCoating) {
    var eye = "left";
    if (!vmLenses.rightColorId()) {
        return;
    }

    if (eyeType === egLens.RIGHT) {
        eye = "right";
    }

    if (noCoating) {
        $("#" + eye + "Manufacturer").removeClass("hidden");
        $("#no" + eye + "MfgCoating").removeClass("hidden");
        $("#" + eye + "ManufacturerCoatingDiv").addClass("hidden");
        $("#" + eye + "MfgCoatingRequired").addClass("hidden");
    } else {
        $("select#" + eye + "ManufacturerCoating").selectpicker('refresh');
        $("#" + eye + "Manufacturer").removeClass("hidden");
        $("#no" + eye + "MfgCoating").addClass("hidden");
        $("#" + eye + "ManufacturerCoatingDiv").removeClass("hidden");
        $("#" + eye + "MfgCoatingRequired").removeClass("hidden");
    }
} // setCoatingList

function showLensWarningModal(title, message) {
    if (!title || title.length === 0 || !message || message.length === 0) {
        return;
    }

    $('#egOrderLensMessageModal .modal-title').html(title);
    $('#egOrderLensMessageModal #egOrderLensMessage').html(message);
    $("#egOrderLensMessageModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
}

function clearAddCoatingsSelects() {
    var addCoating = document.getElementsByClassName("addCoatings");
    if (addCoating !== null && addCoating !== undefined) {
        var i = addCoating.length - 1;
        while (i >= 0) {
            if (addCoating[i].id && addCoating[i].id.length !== 0) {
                addCoating[i].remove();
            }
            i -= 1;
        }

        //// This is added dynamically so that the select refresh doesn't add the drop-down twice. DO NOT REMOVE COMMENT!     data-bind=\"attr: { id: 'addCoating_' + Id }\"
        var newCoat = "<div class='addCoatings' > " +
            "     <div class='form-group'> " +
            "         <div class='col-lg-5 col-md-5 col-sm-5 control-label'> " +
            "             <label data-bind=\"attr: { for: 'addCoating_' + Id}\">Additional Coating</label> " +
            "         </div> " +
            "         <div class='col-lg-7 col-md-7 col-sm-7'> " +
            "             <select id='addCoating' name='addCoating' data-bind=\"attr: { id: 'addCoating_' + Id, name: 'addCoating_' + Id }, options: AdditionalCoatings, value: AdditionalCoatingsId, optionsText: 'Description', optionsValue: 'Key' \" data-live-search='true' data-size='5' data-dropup-auto='false'></select> " +
            "         </div> " +
            "     </div> " +
            "</div> ";
        $(".additional-coatings").html(newCoat);
    }
} // clearAddCoatingsSelects

function setupMeasurementFields(eye) {
    switch (eye) {
    case egLens.LEFT:
        if (!vmLenses.leftLensIsSingleVision()) {
            vmLenses.leftOcHeight("");
            $("#measureLeftOCH").attr("disabled", "disabled");
            $("#measureLeftSH").removeAttr("disabled");
            //if (vmLenses.leftSegmentHeight() !== null && vmLenses.leftSegmentHeight() !== undefined && vmLenses.leftSegmentHeight().length === 0) {
            if (!vmLenses.leftSegmentHeight() || (!!vmLenses.leftSegmentHeight() && vmLenses.leftSegmentHeight().length === 0)) {
                $("#measureLeftSH").addClass("requiredField");
            }
            $("#segHeightRequired").removeClass("hidden");
            $("#measureLeftSH").clearField();
        } else {
            vmLenses.leftSegmentHeight("");
            $("#measureLeftOCH").removeAttr("disabled");
            $("#measureLeftSH").attr("disabled", "disabled").removeClass("requiredField");
            $("#segHeightRequired").addClass("hidden");
            $("#measureLeftSH").clearField();
        }
        break;
    default:
        if (!vmLenses.rightLensIsSingleVision()) {
            vmLenses.rightOcHeight("");
            $("#measureRightOCH").attr("disabled", "disabled");
            $("#measureRightSH").removeAttr("disabled");
            if (!vmLenses.rightSegmentHeight() || (!!vmLenses.rightSegmentHeight() && vmLenses.rightSegmentHeight().length === 0)) {
                $("#measureRightSH").addClass("requiredField");
            }
            $("#segHeightRequired").removeClass("hidden");
            $("#measureRightSH").clearField();
        } else {
            vmLenses.rightSegmentHeight("");
            $("#measureRightOCH").removeAttr("disabled");
            $("#measureRightSH").attr("disabled", "disabled").removeClass("requiredField");
            $("#segHeightRequired").addClass("hidden");
            $("#measureRightSH").clearField();
        }
        break;
    }
} // setupMeasurementFields

function getAdditionalCoatingItem(coatingId) {
    if (coatingId !== 0) {
        client
            .action("GetAdditionalCoatingItem")
            .get({ coatingId: coatingId })
            .done(function (data) {
                if (data && data.ID !== 0) {

                    // add the item...
                    var id = vmLenses.additionalCoatingsIndex(), i;
                    for (i = 0; i < vmLenses.displayAdditionalCoatings().length; i++) {
                        if (!vmLenses.displayAdditionalCoatings()[i].AdditionalCoatingsId) {
                            id = i;
                            break;
                        } else {
                            if (vmLenses.displayAdditionalCoatings()[i].AdditionalCoatingsId === data.ID) {
                                return;
                            }
                        }
                    }

                    // display message...
                    showLensWarningModal("Addtional Coating Required", messageAddCoating.replace("{0}", data.CompanyItemName).replace("{1}", data.CompanyItemName));
                    if (id === vmLenses.additionalCoatingsIndex()) {
                        vmLenses.addAdditionalCoatings(id, data.ID);
                    } else {
                        $("select#addCoating_" + id).val(data.ID).selectpicker('refresh');
                        vmLenses.displayAdditionalCoatings()[id].AdditionalCoatingsId = data.ID;
                    }
                }
            });
    }
} // getAdditionalCoatingItem

function getDefaultLookupValue(data, vmData) {
    if (!!data) {
        if (data.length > 0 && data.length <= 2) {
            if (!!vmData) {
                vmData(0);
            }
            return data[data.length - 1].Key;
        }
    }

    return 0;
}

function checkRxTypeMultiFocal() {
    if (eyeglassOrderViewModel.selectedRx() !== null) {
        if (eyeglassOrderViewModel.selectedRx().RxType !== null) {
            var isConvertedRx = eyeglassOrderViewModel.selectedRx().ConvertedToReading || eyeglassOrderViewModel.selectedRx().ConvertedToDistance;
            return (eyeglassOrderViewModel.selectedRx().RxType.toLowerCase().indexOf("multi") >= 0 &&
                eyeglassOrderViewModel.selectedRx().RxType.toLowerCase().indexOf("focal") >= 0 && !isConvertedRx);
        }
    }
    return false;
}

function getTintColors(tintGroup, tintColorId) {
    if (tintGroup !== undefined && tintGroup !== null && tintGroup.length > 0) {
        client
            .action("GetTintColors")
            .get({
                tintGroup: tintGroup,
                isVsp: isVspInsurance
            })
            .done(function (data) {
                if (data !== undefined && data !== null) {
                    vmLenses.lensTintColors(data);
                    tintColorId = tintColorId !== 0 ? tintColorId : getDefaultLookupValue(data);
                    vmLenses.lensTintColorId(tintColorId);
                    if (tintColorId === 0) {
                        $("select#tintColor").addClass("requiredField").rules("add", { selectBox: true, messages: { selectBox: "Select a tint color." } });
                        $("#tintColorRequired").removeClass('hidden');
                    }
                    $("select#tintColor").selectpicker('refresh');
                } else {
                    vmLenses.lensTintColors(null);
                    vmLenses.lensTintColorId(0);
                    $("select#tintColor").addClass("requiredField").rules("add", { selectBox: true, messages: { selectBox: "Select a tint color." } });
                    $("#tintColorRequired").removeClass('hidden');
                    //resetTintColor();
                }
            });
    }
} // getTintColors

function getPdMinValue() {
    if (vmLenses && vmLenses.distanceType() !== "2") {
        //return "20.0";  This is the real value for Binocular but needs to be changed to double the Monocular because it is stored as Mono and passed through backend as Mono until it gets changed.
        return "32.0";
    }
    return "16.0";
}

function getPdMaxValue() {
    if (vmLenses && vmLenses.distanceType() !== "2") {
        //return "90.0";  This is the real value for Binocular but needs to be changed to double the Monocular because it is stored as Mono and passed through backend as Mono until it gets changed.
        return "76.0";
    }
    return "38.0";
}

function setPdValidations() {
    var errorMessage = "Enter the distant PD. (" + getPdMinValue() + " to " + getPdMaxValue() + ").";
    switch (vmLenses.distanceType()) {
    case "1":
        $("#rightPdLabelDiv, #leftPdDiv, #leftFarPd, #leftNearPd, #leftPdMessages").addClass("hidden");
        $("#pdLabel2").addClass("col-lg-offset-3 col-md-offset-2 col-sm-offset-2 col-xs-offset-2");
        break;
    default:
        $("#rightPdLabelDiv, #leftPdDiv, #leftFarPd, #leftNearPd, #leftPdMessages").removeClass("hidden");
        $("#pdLabel2").removeClass("col-lg-offset-3 col-md-offset-2 col-sm-offset-2 col-xs-offset-2");
        break;
    }

    $("#rightFarPupilDistance").clearField();
    $("#rightNearPupilDistance").clearField();
    $("#leftFarPd").clearField();
    $("#leftNearPd").clearField();
    $("#rightFarPupilDistance").rules("add", { range: [getPdMinValue(), getPdMaxValue()], messages: { range: errorMessage } });
    $("#rightNearPupilDistance").rules("add", { range: [getPdMinValue(), getPdMaxValue()], messages: { range: errorMessage.replace("distant", "near") } });
} // setPdValidations

function showLensError() {
    if (window.eyeglassOrderViewModel.selectedLens()) {
        $("#chooseEgLensesForm #egLensMsgPanel #msgError .message").html(window.eyeglassOrderViewModel.selectedLens().ValidationMessage);
    }
    $("#chooseEgLensesForm #egLensMsgPanel #msgError").removeClass("hidden");
}

function clearLensError() {
    if (window.eyeglassOrderViewModel.selectedLens() && !window.eyeglassOrderViewModel.selectedLens().IsValid) {
        window.eyeglassOrderViewModel.selectedLens().ValidationMessage = "";
    }
    $("#chooseEgLensesForm #egLensMsgPanel #msgError .message").html("");
    $("#chooseEgLensesForm #egLensMsgPanel #msgError").addClass("hidden");
}


var buildLensAttributeTable = function () {
    lensAttributeTable = $('#lensAttributeTable').alslDataTable({
        "iDisplayLength": 10,
        "aaSorting": [],
        "aoColumns": [
            { "sTitle": "Type", "mData": "Type", "sType": "string", "sClass": "left col-lg-3 col-md-3 col-sm-3 col-xs-3", "bSortable": false },
            { "sTitle": "Description", "mData": "Description", "sType": "string", "sClass": "left col-lg-6 col-md-6 col-sm-6 col-xs-6", "bSortable": false },
            {
                "sTitle": '<label class="required">*</label>Retail Price',
                "mData": "Price",
                "sType": "string",
                "sClass": "left col-lg-3 col-md-3 col-sm-3 col-xs-3",
                "bSortable": false,
                "mRender": function (data) {
                    if (!!data) {
                        if (parseFloat(data) === 0) {
                            data = "0.00";
                        }
                    } else {
                        data = "";
                    }

                    return '<input type="numeric" id="retailPrice" name="retailPrice" class="retailPrice requiredField form-control" value="' + (data.isNaN ? "" : convertValue(data, 2, true)) + '" /><div id="retailPriceFieldMessage" class="fieldMessages"></div>';
                }
            }],
        "bAutoWidth": false,
        "oLanguage": { "sEmptyTable": "Select a lens from the Lens Description list above." },
        "selectableRows": true
    });
};

$("#lensAttributeTable").on("keydown", ".retailPrice", function (e) {
    return $(this).decimalFilter(e, false);
});

$("#lensAttributeTable").on("change", ".retailPrice", function (e) {
    e.preventDefault();
    var $this = $(this);
    var retailPrice = $this.val();
    var row = $this.closest("tr").get(0);
    var rownum = lensAttributeTable.fnGetPosition(row);
    var sData = lensAttributeTable.fnGetData(rownum);
    if (parseFloat(retailPrice) === 0) {
        sData.AllowZeroPrice = true;
        sData.Price = "0.00";
    } else {
        sData.Price = convertValue(retailPrice, 2);
    }

    vmLenses.eyeglassLensPricingItems()[rownum].Price = sData.Price;
    sData.IsDirtyFlag = true;
    //isDataChanged = true;
    lensAttributeTable.fnUpdate(sData, rownum);
});

function getLensStylesToMap(eye) {
    var lensTypeId, lensMaterialId, isIof;
    if (eye === egLens.LEFT) {
        var leftLensType = $("#leftLensType")[0];
        var leftLensMaterial = $("#leftLensMaterial")[0];
        lensTypeId = vmLenses.leftLensTypeId();
        lensMaterialId = vmLenses.leftMaterialId();
        isIof = (vmLenses.leftIsIof() !== undefined && vmLenses.leftIsIof() !== null && vmLenses.leftIsIof() === true);
        vmLenses.searchLensTypeDescription(leftLensType[leftLensType.selectedIndex].text);
        vmLenses.searchLensMaterialDescription(leftLensMaterial[leftLensMaterial.selectedIndex].text);
    } else {
        var rightLensType = $("#rightLensType")[0];
        var rightLensMaterial = $("#rightLensMaterial")[0];
        lensTypeId = vmLenses.rightLensTypeId();
        lensMaterialId = vmLenses.rightMaterialId();
        isIof = (vmLenses.rightIsIof() !== undefined && vmLenses.rightIsIof() !== null && vmLenses.rightIsIof() === true);
        vmLenses.searchLensTypeDescription(rightLensType[rightLensType.selectedIndex].text);
        vmLenses.searchLensMaterialDescription(rightLensMaterial[rightLensMaterial.selectedIndex].text);
    }

    client
        .action("GetAllLensStyles")
        .get({
            lensTypeId: lensTypeId,
            lensMaterialId: lensMaterialId,
            isVsp: isVspInsurance,
            isIof: isIof
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                vmLenses.searchLensStyleList(data);
                $("#searchLensStyle").selectpicker('refresh').change();
                $("#lensMainForm .requiredField").addClass("ignore");
            }
            $("#modalLensSearch").modal({
                keyboard : false,
                backdrop : 'static',
                show : true
            });
        });
}

function getLensStyles(lens) {
    if (!lens) {
        return;
    }

    var typeId = lens.TypeId,
        materialId = lens.MaterialTypeId,
        styleId = lens.StyleId,
        colorId = lens.ColorId,
        coatingId = lens.MfgCoatingId;
    var iofOnly = (vmLenses.rightIsIof() !== undefined && vmLenses.rightIsIof() !== null && vmLenses.rightIsIof() === true) || (vmLenses.leftIsIof() !== undefined && vmLenses.leftIsIof() !== null && vmLenses.leftIsIof() === true);

    client
        .action("GetLensStyles")
        .get({ lensTypeId: typeId, lensMaterialId: materialId, isVsp: isVspInsurance, iofOnly: iofOnly })
        .done(function (data) {
            if (!!data) {
                if (searchLensSide === egLens.RIGHT) {
                    $("select#rightLensStyle").clearField();
                    $("select#rightLensStyle").removeClass("requiredField").parent().removeClass("requiredField");
                    vmLenses.rightStyleId(0);
                    vmLenses.rightStyleList(data);
                    styleIdRight = styleId;
                    colorIdRight = colorId;
                    coatingIdRight = coatingId;
                    vmLenses.rightStyleId(styleIdRight);
                    vmLenses.rightColorId(colorId);
                    vmLenses.rightCoatingId(coatingId);
                    //$("#rightLensStyle").change();
                    setStyleList(egLens.RIGHT, false, materialId);
                }

                if (searchLensSide === egLens.LEFT || !vmLenses.isCustom()) {
                    $("select#leftLensStyle").clearField();
                    vmLenses.leftStyleId(0);
                    vmLenses.leftStyleList(data);
                    styleIdLeft = styleId;
                    colorIdLeft = colorId;
                    coatingIdLeft = coatingId;
                    vmLenses.leftStyleId(styleIdLeft);
                    //$("#leftLensStyle").change();
                    setStyleList(egLens.LEFT, false, materialId);
                }
            }
        });
}

function saveSelectedLensAndMap(lensid, lensAttributes) {
    //var lensClient = new ApiClient("EyeglassLens");
    client
        .action("SaveLensAttributePricingAndMapping")
        .queryStringParams({
            "lensId": lensid
        })
        .put(lensAttributes)
        .done(function (lens) {
            if (!!lens) {
                $(document).showSystemSuccess("Eyeglass Lens successfully saved.");
                getLensStyles(lens);
            }
            $("#searchLensStyle").addClass("requiredField");
            $("#lensAttributes").addClass("hidden");
            lensAttributeTable.fnClearTable();
            lensAttributeTable.fnSettings().oLanguage.sEmptyTable = "Select a lens from the Lens Description list above.";
            lensAttributeTable.fnDraw();
            $("#searchLensStyle")[0].selectedIndex = 0;
            $("#lensMainForm .ignore").removeClass("ignore");
            $("#lensMainForm .requiredField").clearField();
            $("#modalLensSearch").modal("hide");
        })
        .fail(function (xhr) {
            if (xhr.status === 400) {
                $(document).showSummaryMessage(msgType.ERROR, xhr.responseJSON.validationmessage);
                xhr.handled = true;
            }
        });
}

var EyeglassLensesViewModel = function () {
    var self = this;
    self.isDirty = ko.observable(false);
    self.isCustom = ko.observable(false);
    self.lensTypeFilterList = ko.observableArray();

    // drop-down lists
    self.rightMaterialList = ko.observableArray();
    self.rightStyleList = ko.observableArray();
    self.rightColorList = ko.observableArray();
    self.rightMfgCoatings = ko.observableArray();

    self.leftMaterialList = ko.observableArray();
    self.leftStyleList = ko.observableArray();
    self.leftColorList = ko.observableArray();
    self.leftMfgCoatings = ko.observableArray();

    self.additionalCoatings = ko.observableArray();
    self.lensTintTypes = ko.observableArray();
    self.lensTintColors = ko.observableArray();
    self.lensEdgeTypes = ko.observableArray();

    // right-eye attributes
    self.rightLens = ko.observable();
    self.rightLensId = ko.observable(0);
    self.rightLensTypeId = ko.observable(0);
    self.rightLensTypeItemNum = ko.observable();
    self.rightLensIsSingleVision = ko.observable();
    self.rightLensTypeDescription = ko.observable();
    self.rightMaterialId = ko.observable(0);
    self.rightMaterialDescription = ko.observable();
    self.rightMaterialThickness = ko.observable();
    self.rightStyleId = ko.observable(0);
    self.rightStyleDescription = ko.observable();
    self.rightColorId = ko.observable(0);
    self.rightColorDescription = ko.observable();
    self.rightCoatingId = ko.observable(0);
    self.rightCoatingDescription = ko.observable();
    self.rightFarPd = ko.observable();
    self.rightNearPd = ko.observable();
    self.rightBase = ko.observable();
    self.rightOcHeight = ko.observable();
    self.rightSegmentHeight = ko.observable();
    self.rightThickness = ko.observable();
    self.rightIsBalanced = ko.observable();
    self.rightIsSlabOff = ko.observable();
    self.rightIsUncut = ko.observable();
    self.rightIsIof = ko.observable(false);

    // left eye attributes
    self.leftLens = ko.observable();
    self.leftLensId = ko.observable(0);
    self.leftLensTypeId = ko.observable(0);
    self.leftLensTypeItemNum = ko.observable();
    self.leftLensIsSingleVision = ko.observable();
    self.leftLensTypeDescription = ko.observable();
    self.leftMaterialId = ko.observable(0);
    self.leftMaterialDescription = ko.observable();
    self.leftMaterialThickness = ko.observable();
    self.leftStyleId = ko.observable(0);
    self.leftStyleDescription = ko.observable();
    self.leftColorId = ko.observable(0);
    self.leftColorDescription = ko.observable();
    self.leftCoatingId = ko.observable(0);
    self.leftCoatingDescription = ko.observable();
    self.leftFarPd = ko.observable();
    self.leftNearPd = ko.observable();
    self.leftBase = ko.observable();
    self.leftOcHeight = ko.observable();
    self.leftSegmentHeight = ko.observable();
    self.leftThickness = ko.observable();
    self.leftIsBalanced = ko.observable();
    self.leftIsSlabOff = ko.observable();
    self.leftIsUncut = ko.observable();
    self.leftIsIof = ko.observable(false);

    // additional lens information
    self.distanceType = ko.observable();
    self.binocularFarPd = ko.observable();
    self.binocularNearPd = ko.observable();
    self.rightFarPdInput = ko.observable();
    self.rightNearPdInput = ko.observable();
    self.lensTintTypeId = ko.observable();
    self.lensTintTypeDescription = ko.observable();
    self.lensTintColorId = ko.observable();
    self.lensTintColorDescription = ko.observable();
    self.lensEdgeTypeId = ko.observable();
    self.lensEdgeTypeDescription = ko.observable();
    self.labInstructions = ko.observable();

    self.additionalCoatingsId = ko.observableArray();
    self.displayAdditionalCoatings = ko.observableArray();
    self.backsideAr = ko.observable();
    self.rx = ko.observable();

    //self.searchLensType = ko.observable(egLens.RIGHT);
    self.searchLensId = ko.observable();
    //self.eyeglassLensPricingItems = ko.viewModelArray(EyeglassLensPricingViewModel);
    self.eyeglassLensPricingItems = ko.observableArray();
    self.searchLensTypeDescription = ko.observable();
    self.searchLensMaterialDescription = ko.observable();
    self.searchLensStyleList = ko.observableArray();

    // right eye subscriptions
    self.rightLensTypeId.subscribe(function () {
        if (!isLensLoading) {
            if (self.rightLensTypeId() !== null && self.rightLensTypeId() > 0) {
                var rightLensType = $("#rightLensType")[0];
                self.rightLensTypeDescription(rightLensType[rightLensType.selectedIndex].text);
                client
                    .action("GetLensMaterials")
                    .get({ lensType: self.rightLensTypeId() })
                    .done(function (data) {
                        if (data !== undefined && data !== null) {
                            self.rightMaterialList(data);
                            materialIdRight = materialIdRight !== 0 ? materialIdRight : getDefaultLookupValue(data, self.rightMaterialId);
                            self.rightMaterialId(materialIdRight);
                            $("#rightLensMaterial").selectpicker('refresh');
                            setMaterialList(egLens.RIGHT, true, self.rightLensTypeId());
                            if (!self.isCustom()) {
                                self.leftLensTypeId(self.rightLensTypeId());
                                self.leftLensTypeDescription(rightLensType[rightLensType.selectedIndex].text);
                                $("select#leftLensType").selectpicker('refresh');
                                self.leftMaterialList(data);
                                self.leftMaterialId(materialIdRight);
                                setMaterialList(egLens.LEFT, true, self.leftLensTypeId());
                            }
                        }
                    });
            } else {
                materialIdRight = 0;
                self.rightMaterialId(0);
                setMaterialList(egLens.RIGHT, true, self.rightLensTypeId());
                if (!self.isCustom()) {
                    materialIdLeft = 0;
                    self.leftMaterialId(0);
                    setMaterialList(egLens.LEFT, true, self.leftLensTypeId());
                }
            }
        }
    });

    self.rightMaterialId.subscribe(function () {
        self.getRightLensStyles();
    });

    self.rightIsIof.subscribe(function (newValue) {
        if (!newValue) {
            self.rightStyleId(0);
            styleIdRight = 0;
        }
        if (self.leftIsIof() !== newValue) {
            self.leftIsIof(self.rightIsIof());
        }
        self.getRightLensStyles();
    });

    self.rightStyleId.subscribe(function () {
        if (!isLensLoading) {
            if (self.rightLensTypeId() !== null && self.rightLensTypeId() > 0 && self.rightMaterialId() !== null && self.rightMaterialId() > 0 &&
                    self.rightStyleId() !== null && self.rightStyleId() > 0) {
                client
                    .action("GetLensColors")
                    .get({ lensTypeId: self.rightLensTypeId(), lensMaterialId: self.rightMaterialId(), lensStyleId: self.rightStyleId(), isVsp: isVspInsurance })
                    .done(function (data) {
                        //$("#rightLensStyle").change();
                        if (data !== undefined && data !== null) {
                            $("select#rightLensStyle").removeClass("requiredField").parent().removeClass("requiredField");
                            self.rightColorList(data);
                            colorIdRight = colorIdRight !== 0 ? colorIdRight : getDefaultLookupValue(data, self.rightColorId);
                            self.rightColorId(colorIdRight);
                            setColorList(egLens.RIGHT, true, self.rightStyleId());

                            if (!self.isCustom()) {
                                self.leftStyleId(self.rightStyleId());
                                $("select#leftLensStyle").removeClass("requiredField").selectpicker('refresh');
                                self.leftColorList(data);
                                self.leftColorId(colorIdRight);
                                setColorList(egLens.LEFT, true, self.leftStyleId());
                            }
                        }
                    });
            } else {
                $("#rightLensStyle").addClass("requiredField");
                colorIdRight = 0;
                self.rightColorId(0);
                setColorList(egLens.RIGHT, true, self.rightStyleId());
                if (!self.isCustom()) {
                    $("#leftLensStyle").addClass("requiredField");
                    colorIdLeft = 0;
                    self.leftColorId(0);
                    setColorList(egLens.LEFT, true, self.leftStyleId());
                }
            }
        }
    });

    self.rightColorId.subscribe(function () {
        if (!isLensLoading) {
            if (self.rightLensTypeId() !== null && self.rightLensTypeId() > 0 && self.rightMaterialId() !== null && self.rightMaterialId() > 0 &&
                    self.rightStyleId() !== null && self.rightStyleId() > 0 && self.rightColorId() !== null && self.rightColorId() > 0) {
                client
                    .action("GetMfgCoatings")
                    .get({
                        lensTypeId: self.rightLensTypeId(),
                        lensMaterialId: self.rightMaterialId(),
                        lensStyleId: self.rightStyleId(),
                        lensColorId: self.rightColorId(),
                        isVsp: isVspInsurance
                    })
                    .done(function (data) {
                        if (data.additionalColor !== undefined && data.additionalColor.length !== 0) {
                            var itemColor = window.getExtraItem(data.additionalColor);
                            if (itemColor !== null && itemColor !== undefined) {
                                showLensWarningModal("Addtional Color Required", messageAddColor.replace("{0}", data.additionalColor));
                            }
                        }
                        $("select#rightLensColor").selectpicker('refresh');
                        $("#rightLensColor").parents("div.bootstrap-select").removeClass("requiredField");

                        if (data.lensMfgCoatings !== undefined && data.lensMfgCoatings !== null) {
                            self.rightMfgCoatings(data.lensMfgCoatings);
                            self.rightCoatingId(coatingIdRight);
                            setCoatingList(egLens.RIGHT, false);
                            if (!self.isCustom()) {
                                self.leftColorId(self.rightColorId());
                                $("select#leftLensColor").selectpicker('refresh').removeClass("requiredField");
                                self.leftMfgCoatings(data.lensMfgCoatings);
                                self.leftCoatingId(coatingIdRight);
                                setCoatingList(egLens.LEFT, false);
                            }
                        } else {
                            var lens = self.isCustom() ? egLens.RIGHT : egLens.BOTH;
                            self.rightMfgCoatings(null);
                            self.rightCoatingId(coatingIdRight);
                            setCoatingList(egLens.RIGHT, true);
                            if (!isLensLoading) {
                                loadLens(lens, self.rightLensTypeId(), self.rightMaterialId(), self.rightStyleId(), self.rightColorId(), 0);
                            }

                            if (!self.isCustom()) {
                                self.leftMfgCoatings(null);
                                self.leftColorId(self.rightColorId());
                                self.leftCoatingId(self.rightCoatingId());
                                $("select#leftLensColor").selectpicker('refresh').removeClass("requiredField");
                                setCoatingList(egLens.LEFT, true);
                            }
                        }
                    });
            } else {
                $("#rightLensColor").addClass("requiredField");
                coatingIdRight = 0;
                self.rightCoatingId(0);
                setCoatingList(egLens.RIGHT, true);
                if (!self.isCustom()) {
                    $("#leftLensColor").addClass("requiredField");
                    coatingIdLeft = 0;
                    self.leftCoatingId(0);
                    setCoatingList(egLens.LEFT, true);
                }
            }
        }
    });

    self.rightCoatingId.subscribe(function () {
        if (!isLensLoading) {
            if (self.rightCoatingId() !== undefined && self.rightCoatingId() !== null && self.rightCoatingId() > 0) {
                var lens = self.isCustom() ? egLens.RIGHT : egLens.BOTH;
                loadLens(lens, self.rightLensTypeId(), self.rightMaterialId(), self.rightStyleId(), self.rightColorId(), self.rightCoatingId());
                $("#rightManufacturerCoating").removeClass("requiredField");
                getAdditionalCoatingItem(self.rightCoatingId());
                if (!self.isCustom()) {
                    self.leftCoatingId(self.rightCoatingId());
                }
            }
        }
    });

    // left eye subscriptions
    self.leftLensTypeId.subscribe(function () {
        if (!isLensLoading && self.isCustom()) {
            if (self.leftLensTypeId() !== null && self.leftLensTypeId() > 0) {
                var leftLensType = $("#leftLensType")[0];
                self.leftLensTypeDescription(leftLensType[leftLensType.selectedIndex].text);
                client
                    .action("GetLensMaterials")
                    .get({ lensType: self.leftLensTypeId() })
                    .done(function (data) {
                        if (data !== undefined && data !== null) {
                            self.leftMaterialList(data);
                            materialIdLeft = materialIdLeft !== 0 ? materialIdLeft : getDefaultLookupValue(data, self.leftMaterialId);
                            self.leftMaterialId(materialIdLeft);
                            setMaterialList(egLens.LEFT, true, self.leftLensTypeId());
                        }
                    });
            } else {
                $("#leftLensMaterial").addClass("requiredField");
                materialIdLeft = 0;
                self.leftMaterialId(0);
                setMaterialList(egLens.LEFT, true, self.leftLensTypeId());
            }
        }
    });

    self.leftMaterialId.subscribe(function () {
        self.getLeftLensStyles();
    });

    self.leftIsIof.subscribe(function (newValue) {
        if (!newValue) {
            self.leftStyleId(0);
            styleIdLeft = 0;
        }
        if (self.rightIsIof() !== newValue) {
            self.rightIsIof(self.leftIsIof());
        }
        self.getLeftLensStyles();
    });

    self.leftStyleId.subscribe(function () {
        if (!isLensLoading && self.isCustom()) {
            if (self.leftLensTypeId() !== null && self.leftLensTypeId() > 0 && self.leftMaterialId() !== null &&
                    self.leftMaterialId() > 0 && self.leftStyleId() !== null && self.leftStyleId() > 0) {
                client
                    .action("GetLensColors")
                    .get({ lensTypeId: self.leftLensTypeId(), lensMaterialId: self.leftMaterialId(), lensStyleId: self.leftStyleId(), isVsp: isVspInsurance })
                    .done(function (data) {
                        //$("#leftLensStyle").change();
                        if (data !== undefined && data !== null) {
                            self.leftColorList(data);
                            colorIdLeft = colorIdLeft !== 0 ? colorIdLeft : getDefaultLookupValue(data, self.leftColorId);
                            self.leftColorId(colorIdLeft);
                            setColorList(egLens.LEFT, true, self.leftStyleId());
                            $("select#leftLensStyle").change();
                        }
                    });
            } else {
                $("#leftLensColor").addClass("requiredField");
                colorIdLeft = 0;
                self.leftColorId(0);
                setColorList(egLens.LEFT, true, self.leftStyleId());
            }
        }
    });

    self.leftColorId.subscribe(function () {
        if (!isLensLoading && self.isCustom()) {
            if (self.leftLensTypeId() !== null && self.leftLensTypeId() > 0 && self.leftMaterialId() !== null && self.leftMaterialId() > 0 &&
                    self.leftStyleId() !== null && self.leftStyleId() > 0 && self.leftColorId() !== null && self.leftColorId() > 0) {
                client
                    .action("GetMfgCoatings")
                    .get({
                        lensTypeId: self.leftLensTypeId(),
                        lensMaterialId: self.leftMaterialId(),
                        lensStyleId: self.leftStyleId(),
                        lensColorId: self.leftColorId,
                        isVsp: isVspInsurance
                    })
                    .done(function (data) {
                        if (data.additionalColor !== undefined && data.additionalColor.length !== 0) {
                            showLensWarningModal("Addtional Color Required", messageAddColor.replace("{0}", data.additionalColor));
                        }
                        $("select#leftLensColor").selectpicker('refresh').removeClass("requiredField");
                        $("#leftLensColor").parents("div.bootstrap-select").removeClass("requiredField");

                        if (data.lensMfgCoatings !== undefined && data.lensMfgCoatings !== null) {
                            self.leftMfgCoatings(data.lensMfgCoatings);
                            //coatingIdLeft = coatingIdLeft !== 0 ? coatingIdLeft : getDefaultLookupValue(data);
                            self.leftCoatingId(coatingIdLeft);
                            setCoatingList(egLens.LEFT, false);
                        } else {
                            self.leftMfgCoatings(null);
                            self.leftCoatingId(coatingIdLeft);
                            $("select#leftLensColor").selectpicker('refresh').removeClass("requiredField");
                            setCoatingList(egLens.LEFT, true);
                            if (!isLensLoading) {
                                loadLens(egLens.LEFT, self.leftLensTypeId(), self.leftMaterialId(), self.leftStyleId(), self.leftColorId(), 0);
                            }
                        }
                    });
            } else {
                $("#leftManufacturerCoating").addClass("requiredField");
                coatingIdLeft = 0;
                self.leftCoatingId(0);
                setCoatingList(egLens.LEFT, true);
            }
        }
    });

    self.leftCoatingId.subscribe(function () {
        if (!isLensLoading && self.isCustom()) {
            if (self.leftCoatingId() !== undefined && self.leftCoatingId() !== null && self.leftCoatingId() > 0) {
                loadLens(egLens.LEFT, self.leftLensTypeId(), self.leftMaterialId(), self.leftStyleId(), self.leftColorId(), self.leftCoatingId());
                $("#leftManufacturerCoating").removeClass("requiredField");
                if (self.isCustom()) {
                    getAdditionalCoatingItem(self.leftCoatingId());
                }
            }
        }
    });

    self.lensTintTypeId.subscribe(function () {
        if (self.lensTintTypeId() !== null && self.lensTintTypeId() > 0) {
            var tintGroup = $("#tintType option:selected").text();
            if (tintGroup === "Select") {
                tintGroup = tintTypeDescription;
            }
            self.lensTintTypeDescription(tintGroup);
            getTintColors(tintGroup, tintColorId);
        } else {
            self.lensTintTypeDescription("");
            self.lensTintColors(null);
            self.lensTintColorId(0);
            self.labInstructions("");
            resetTintColor();
        }
    });

    self.lensTintColorId.subscribe(function () {
        if (self.lensTintColorId() !== null && self.lensTintColorId() > 0) {
            self.lensTintColorDescription($("#tintColor option:selected").text());
        } else {
            self.lensTintColorDescription("");
        }
    });

    self.lensEdgeTypeId.subscribe(function () {
        if (self.lensEdgeTypeId() !== null && self.lensEdgeTypeId() > 0) {
            self.lensEdgeTypeDescription($("#lensEdging option:selected").text());
        } else {
            self.lensEdgeTypeDescription("");
        }
    });

    self.rightIsBalanced.subscribe(function () {
        if (self.rightIsBalanced() === true) {
            self.leftIsBalanced(false);
        }
    });

    self.leftIsBalanced.subscribe(function () {
        if (self.leftIsBalanced() === true) {
            self.rightIsBalanced(false);
        }
    });

    self.rightIsSlabOff.subscribe(function () {
        if (self.rightIsSlabOff() === true) {
            self.leftIsSlabOff(false);
        }
    });

    self.leftIsSlabOff.subscribe(function () {
        if (self.leftIsSlabOff() === true) {
            self.rightIsSlabOff(false);
        }
    });

    self.rightIsUncut.subscribe(function () {
        if (self.rightIsUncut) {
            self.leftIsUncut(self.rightIsUncut());
        }
    });

    self.leftIsUncut.subscribe(function () {
        if (self.leftIsUncut) {
            self.rightIsUncut(self.leftIsUncut());
        }
    });

    self.rightLensIsSingleVision.subscribe(function () {
        if (self.rightLensIsSingleVision() !== null) {
            setupMeasurementFields(egLens.RIGHT);
        }
    });

    self.leftLensIsSingleVision.subscribe(function () {
        if (self.leftLensIsSingleVision() !== null) {
            setupMeasurementFields(egLens.LEFT);
        }
    });

    self.isMonoPd = function () {
        return self.distanceType() === "2";
    };

    self.rightFarPdInput.subscribe(function () {
        self.rightFarPdInput(convertValue(self.rightFarPdInput(), 1));
        var rightNearPd = parseFloat(self.rightFarPdInput());
        if (isNaN(rightNearPd)) {
            self.rightNearPdInput("");
            self.rightFarPd("");
            if (self.rightLensId() === self.leftLensId()) {
                self.leftFarPd("");
                $("#leftFarPd").change();
            }
        } else {
            if (self.distanceType() === "2") {
                self.rightFarPd(convertValue(self.rightFarPdInput(), 1));
                if (!self.rightNearPdInput() || self.rightNearPdInput().length === 0) {
                    rightNearPd += -1.5;
                    self.rightNearPdInput(convertValue(rightNearPd, 1));
                }

                if (!self.leftFarPd() || (self.leftFarPd().length === 0 && self.rightLensId() && self.rightLensId() === self.leftLensId())) {
                    self.leftFarPd(convertValue(self.rightFarPdInput(), 1));
                    $("#leftFarPd").change();
                }
            } else {
                self.binocularFarPd(self.rightFarPdInput());
                if (!self.rightNearPdInput() || self.rightNearPdInput().length === 0) {
                    rightNearPd += -3;
                    self.rightNearPdInput(convertValue(rightNearPd, 1));
                }
            }
        }
        $("#rightNearPupilDistance").change();
    });

    self.leftFarPd.subscribe(function () {
        self.leftFarPd(convertValue(self.leftFarPd(), 1));
        var leftNearPd = parseFloat(self.leftFarPd());
        if (isNaN(leftNearPd)) {
            self.leftNearPd("");
            $("#leftNearPd").change();
        } else {
            if (!self.leftNearPd() || self.leftNearPd().length === 0) {
                leftNearPd = parseFloat(self.leftFarPd()) - 1.5;
                if (isNaN(leftNearPd)) {
                    self.leftNearPd("");
                } else {
                    self.leftNearPd(convertValue(leftNearPd, 1));
                }

                $("#leftNearPd").change();
            }
        }
    });

    self.leftNearPd.subscribe(function () {
        self.leftNearPd(convertValue(self.leftNearPd(), 1));
    });

    self.rightNearPdInput.subscribe(function () {
        self.rightNearPdInput(convertValue(self.rightNearPdInput(), 1));
        if (self.distanceType() === "2") {
            self.rightNearPd(self.rightNearPdInput());
        } else {
            self.binocularNearPd(self.rightNearPdInput());
        }
    });

    self.rightOcHeight.subscribe(function () {
        self.rightOcHeight(convertValue(self.rightOcHeight(), 0));
        if (self.rightLensId() === self.leftLensId()) {
            if (!self.leftOcHeight() || (!!self.leftOcHeight() && self.leftOcHeight() === "")) {
                self.leftOcHeight(convertValue(self.rightOcHeight(), 0));
            }
        }
    });

    self.rightSegmentHeight.subscribe(function () {
        self.rightSegmentHeight(convertValue(self.rightSegmentHeight(), 2));
        if (self.rightLensId() === self.leftLensId()) {
            if (!self.leftSegmentHeight() || (!!self.leftSegmentHeight() && self.leftSegmentHeight() === "")) {
                if (self.leftSegmentHeight() !== self.rightSegmentHeight()) {
                    self.leftSegmentHeight(convertValue(self.rightSegmentHeight(), 2));
                    $("#measureLeftSH").change();
                }
            }
        }
    });

    self.rightBase.subscribe(function () {
        self.rightBase(convertValue(self.rightBase(), 1));
        if (self.rightLensId() === self.leftLensId()) {
            if (!self.leftBase() || (!!self.leftBase() && self.leftBase() === "")) {
                self.leftBase(convertValue(self.rightBase(), 1));
            }
        }
    });

    self.RightOptions = ko.computed(function () {
        var rightOptions = "";
        if (self.rightIsSlabOff && self.rightIsSlabOff() === true) {
            rightOptions = "Slab-Off";
        }

        if (self.rightIsBalanced && self.rightIsBalanced() === true) {
            rightOptions += (rightOptions.length > 0 ? ", " : "") + "Balanced";
        }

        if (self.rightIsUncut && self.rightIsUncut() === true) {
            rightOptions += (rightOptions.length > 0 ? ", " : "") + "Uncut";
        }
        return rightOptions;
    });

    self.LeftOptions = ko.computed(function () {
        var leftOptions = "";
        if (self.leftIsSlabOff && self.leftIsSlabOff() === true) {
            leftOptions = "Slab-Off";
        }

        if (self.leftIsBalanced && self.leftIsBalanced() === true) {
            leftOptions += (leftOptions.length > 0 ? ", " : "") + "Balanced";
        }

        if (self.leftIsUncut && self.leftIsUncut() === true) {
            leftOptions += (leftOptions.length > 0 ? ", " : "") + "Uncut";
        }
        return leftOptions;
    });

    self.additionalCoatingsIndex = function () {
        var coatingCount = 0;
        if (self.displayAdditionalCoatings()) {
            coatingCount = self.displayAdditionalCoatings().length;
        }
        return coatingCount;
    };

    self.addAdditionalCoatings = function (id, value) {
        self.displayAdditionalCoatings.push({
            Id: id,
            Name: "addCoating_" + id,
            AdditionalCoatings: self.additionalCoatings,
            AdditionalCoatingsId: value
        });
        $("select#addCoating_" + id).alslSelectPicker();
        $("select#addCoating_" + id).selectpicker('refresh');
        $("select#addCoating_" + id).change(function () {
            if (this.value !== 0) {
                var addCoatingId = this.value;
                getAdditionalCoatingItem(addCoatingId);
            }
        });
        setDirtyFlag();
    };

    self.getRightLensStyles = function () {
        if (!isLensLoading) {
            if (self.rightLensTypeId() !== null && self.rightLensTypeId() > 0 && self.rightMaterialId() !== null && self.rightMaterialId() > 0) {
                $("#rightLensMaterial").removeClass("requiredField");
                var rightLensMaterial = $("#rightLensMaterial")[0];
                var iofOnly = self.rightIsIof() !== undefined && self.rightIsIof() !== null && self.rightIsIof() === true;
                self.rightMaterialDescription(rightLensMaterial[rightLensMaterial.selectedIndex].text);
                client
                    .action("GetLensStyles")
                    .get({ lensTypeId: self.rightLensTypeId(), lensMaterialId: self.rightMaterialId(), isVsp: isVspInsurance, iofOnly: iofOnly })
                    .done(function (data) {
                        if (data !== undefined && data !== null) {
                            $("select#rightLensStyle").clearField();
                            self.rightStyleList(data);
                            styleIdRight = styleIdRight !== 0 ? styleIdRight : getDefaultLookupValue(data, self.rightStyleId);
                            self.rightStyleId(styleIdRight);
                            setStyleList(egLens.RIGHT, true, self.rightMaterialId());

                            if (!self.isCustom()) {
                                self.leftMaterialId(self.rightMaterialId());
                                self.leftMaterialDescription(rightLensMaterial[rightLensMaterial.selectedIndex].text);
                                $("select#leftLensMaterial").removeClass("requiredField").selectpicker('refresh');
                                self.leftStyleList(data);
                                self.leftStyleId(styleIdRight);
                                setStyleList(egLens.LEFT, true, self.leftMaterialId());
                            }
                        }
                    });
            } else {
                $("#rightLensMaterial").addClass("requiredField");
                styleIdRight = 0;
                self.rightStyleId(0);
                setStyleList(egLens.RIGHT, true, self.rightMaterialId());
                if (!self.isCustom()) {
                    $("#leftLensMaterial").addClass("requiredField");
                    styleIdLeft = 0;
                    self.leftStyleId(0);
                    setStyleList(egLens.LEFT, true, self.leftMaterialId());
                }
            }
        }
    };

    self.getLeftLensStyles = function () {
        if (!isLensLoading && self.isCustom()) {
            if (self.leftLensTypeId() !== null && self.leftLensTypeId() > 0 && self.leftMaterialId() !== null && self.leftMaterialId() > 0) {
                $("#leftLensMaterial").removeClass("requiredField");
                var leftLensMaterial = $("#leftLensMaterial")[0];
                var iofOnly = self.leftIsIof() !== undefined && self.leftIsIof() !== null && self.leftIsIof() === true;
                self.leftMaterialDescription(leftLensMaterial[leftLensMaterial.selectedIndex].text);
                client
                    .action("GetLensStyles")
                    .get({ lensTypeId: self.leftLensTypeId(), lensMaterialId: self.leftMaterialId(), isVsp: isVspInsurance, iofOnly: iofOnly })
                    .done(function (data) {
                        if (data !== undefined && data !== null) {
                            $("select#leftLensStyle").clearField();
                            self.leftStyleList(data);
                            styleIdLeft = styleIdLeft !== 0 ? styleIdLeft : getDefaultLookupValue(data, self.leftStyleId);
                            self.leftStyleId(styleIdLeft);
                            setStyleList(egLens.LEFT, true, self.leftMaterialId());
                        }
                    });
            } else {
                $("#leftLensStyle").addClass("requiredField");
                styleIdLeft = 0;
                self.leftStyleId(0);
                setStyleList(egLens.LEFT, true, self.leftMaterialId());
            }
        }
    };

    self.importLensSelections = function (eye, lens) {
        if (lens) {
            switch (eye) {
            case egLens.LEFT: // left
                materialIdLeft = lens.MaterialTypeId;
                styleIdLeft = lens.StyleId;
                colorIdLeft = lens.ColorId;
                coatingIdLeft = lens.MfgCoatingId;
                self.leftIsSlabOff(lens.SlabOff);
                self.leftIsUncut(lens.Uncut);
                self.leftIsBalanced(lens.Balanced);
                self.leftLensTypeId(lens.TypeId);
                self.leftMaterialList(lens.LensMaterial);
                self.leftMaterialId(lens.MaterialTypeId);
                if (lens.TypeId && lens.TypeId !== 0) {
                    setMaterialList(egLens.LEFT, false, lens.TypeId);
                }
                self.leftStyleList(lens.LensStyles);
                self.leftStyleId(lens.StyleId);
                if (lens.MaterialTypeId && lens.MaterialTypeId !== 0) {
                    setStyleList(egLens.LEFT, false, lens.MaterialTypeId);
                }
                self.leftColorList(lens.LensColors);
                self.leftColorId(lens.ColorId);
                if (lens.StyleId && lens.StyleId !== 0) {
                    setColorList(egLens.LEFT, false, lens.StyleId);
                }
                self.leftMfgCoatings(lens.LensMfgCoatings);
                self.leftCoatingId(lens.MfgCoatingId);
                if (lens.ColorId && lens.ColorId !== 0) {
                    setCoatingList(egLens.LEFT, lens.LensMfgCoatings === null || lens.LensMfgCoatings.length === 0);
                }

                break;
            default: // right
                materialIdRight = lens.MaterialTypeId;
                styleIdRight = lens.StyleId;
                colorIdRight = lens.ColorId;
                coatingIdRight = lens.MfgCoatingId;
                self.rightStyleList(lens.LensStyles);
                self.rightIsSlabOff(lens.SlabOff);
                self.rightIsUncut(lens.Uncut);
                self.rightIsBalanced(lens.Balanced);
                self.rightLensTypeId(lens.TypeId);
                self.rightMaterialList(lens.LensMaterial);
                self.rightMaterialId(lens.MaterialTypeId);
                if (lens.TypeId && lens.TypeId !== 0) {
                    setMaterialList(egLens.RIGHT, false, lens.TypeId);
                }
                //$("#rightLensMaterial").change();
                self.rightStyleId(lens.StyleId);
                if (lens.MaterialTypeId && lens.MaterialTypeId !== 0) {
                    setStyleList(egLens.RIGHT, false, lens.MaterialTypeId);
                }
                self.rightColorList(lens.LensColors);
                self.rightColorId(lens.ColorId);
                if (lens.StyleId && lens.StyleId !== 0) {
                    setColorList(egLens.RIGHT, false, lens.StyleId);
                }
                self.rightMfgCoatings(lens.LensMfgCoatings);
                self.rightCoatingId(lens.MfgCoatingId);
                if (lens.ColorId && lens.ColorId !== 0) {
                    setCoatingList(egLens.RIGHT, lens.LensMfgCoatings === null || lens.LensMfgCoatings.length === 0);
                }
            }
        }
    };

    self.importLensInfo = function (eye, lens) {
        if (lens) {

            switch (eye) {
            case egLens.LEFT:
                self.leftLensId(lens.Id);
                if (lens.TypeItemNum) {
                    self.leftLensTypeItemNum(lens.TypeItemNum);
                    self.leftLensIsSingleVision(!checkRxTypeMultiFocal());
                }
                self.leftLensTypeDescription(lens.TypeDescription);
                self.leftMaterialDescription(lens.MaterialDescription);
                self.leftMaterialThickness(convertValue(lens.MaterialThickness, 1));
                self.leftStyleDescription(lens.StyleDescription);
                self.leftColorDescription(lens.ColorDescription);
                self.leftCoatingDescription(lens.MfgCoatingDescription);

                break;
            default: // right
                self.rightLensId(lens.Id);
                if (lens.TypeItemNum) {
                    self.rightLensTypeItemNum(lens.TypeItemNum);
                    self.rightLensIsSingleVision(!checkRxTypeMultiFocal());
                }
                self.rightLensTypeDescription(lens.TypeDescription);
                self.rightMaterialDescription(lens.MaterialDescription);
                self.rightMaterialThickness(convertValue(lens.MaterialThickness, 1));
                self.rightStyleDescription(lens.StyleDescription);
                self.rightColorDescription(lens.ColorDescription);
                self.rightCoatingDescription(lens.MfgCoatingDescription);
            }
        }
    };

    self.importPDMeasurements = function (lens) {
        self.distanceType(lens.PdType.toString());
        if (lens.PdType === "1") {
            lens.RightLens.FarPd = lens.RightLens.FarPd.toNumber() + lens.LeftLens.FarPd.toNumber();
            lens.RightLens.NearPd = lens.RightLens.NearPd.toNumber() + lens.LeftLens.NearPd.toNumber();
            lens.LeftLens.FarPd = 0;
            lens.LeftLens.NearPd = 0;
            setPdValidations();
        }
    };

    self.importLensMeasurements = function (eye, lens) {
        if (lens) {
            switch (eye) {
            case egLens.LEFT:
                self.leftSegmentHeight(convertValue(lens.SegHeight, 2));
                self.leftBase(convertValue(lens.BaseCurve, 1));
                self.leftOcHeight(convertValue(lens.OcHeight, 0));
                self.leftLensIsSingleVision(!checkRxTypeMultiFocal());
                if (lens.FarPd) {
                    self.leftFarPd(convertValue(lens.FarPd, 1));
                }

                if (lens.NearPd) {
                    self.leftNearPd(convertValue(lens.NearPd, 1));
                    $("#leftNearPd").change();
                }

                setupMeasurementFields(egLens.LEFT);

                break;
            default: // right

                self.rightSegmentHeight(convertValue(lens.SegHeight, 2));
                self.rightBase(convertValue(lens.BaseCurve, 1));
                self.rightOcHeight(convertValue(lens.OcHeight, 0));
                self.rightLensIsSingleVision(!checkRxTypeMultiFocal());
                if (lens.FarPd) {
                    self.rightFarPdInput(convertValue(lens.FarPd, 1));
                    self.rightFarPd(convertValue(lens.FarPd, 1));
                }

                if (lens.NearPd) {
                    self.rightNearPdInput(convertValue(lens.NearPd, 1));
                    $("#rightNearPupilDistance").change();
                    self.rightNearPd(convertValue(lens.NearPd, 1));
                }

                setupMeasurementFields(egLens.RIGHT);
            }
        }
    };

    self.importRx = function (data) {
        if (data !== undefined && data !== null) {
            self.rightIsBalanced(data.RightIsBalance);
            self.leftIsBalanced(data.LeftIsBalance);
            self.rightIsSlabOff(data.RightIsSlabOff);
            self.leftIsSlabOff(data.LeftIsSlabOff);
            if (data.RightIsBalance === true || data.LeftIsBalance === true ||
                    data.RightIsSlabOff === true || data.LeftIsSlabOff === true) {
                customizeLens();
                self.isCustom(true);
            }
        }
    };

    self.importListData = function (data) {
        if (data !== undefined && data !== null) {
            self.lensTypeFilterList(data.LensTypes);
            if (data.LensTypes.count() === 2) {
                if (!self.rightLensTypeId()) {
                    self.rightLensTypeId(getDefaultLookupValue(data.LensTypes));
                }
                if (!self.leftLensTypeId()) {
                    if (self.isCustom()) {
                        self.leftLensTypeId(getDefaultLookupValue(data.LensTypes));
                    } else {
                        self.leftLensTypeId(self.rightLensTypeId());
                    }
                }
            }

            self.additionalCoatings(data.LensAddCoatings);
            self.lensTintTypes(data.LensTintTypes);
            self.lensTintColors(data.LensTintColors);
            self.lensEdgeTypes(data.LensEdgeTypes);
        }
    };

    self.importModel = function (data) {
        var i;
        if (data !== undefined && data !== null) {
            tintColorId = data.TintColorId;
            tintTypeDescription = data.TintTypeDescription;
            self.importPDMeasurements(data);
            if (data.LeftLens && data.RightLens) {
                if (data.LeftLens.Id !== data.RightLens.Id || self.RightOptions() !== self.LeftOptions()) {
                    data.IsCustomLenses = true;
                }
            }

            if (data.IsCustomLenses) {
                customizeLens();
                self.isCustom(true);
            }

            // right-eye attributes
            self.rightLens(data.RightLens);
            self.importLensMeasurements(egLens.RIGHT, data.RightLens);
            self.importLensSelections(egLens.RIGHT, data.RightLens);
            self.importLensInfo(egLens.RIGHT, data.RightLens);
            if (window.isIof(self.rightStyleDescription())) {
                self.rightIsIof(true);
            }

            // left eye attributes
            self.leftLens(data.LeftLens);
            self.importLensMeasurements(egLens.LEFT, data.LeftLens);
            self.importLensSelections(egLens.LEFT, data.LeftLens);
            self.importLensInfo(egLens.LEFT, data.LeftLens);
            if (window.isIof(self.leftStyleDescription())) {
                self.leftIsIof(true);
            }
            // additional lens information
            self.lensTintTypeDescription(data.TintTypeDescription);
            self.lensTintTypeId(data.TintTypeId);
            getTintColors(tintTypeDescription, tintColorId);
            self.lensTintColorId(data.TintColorId);
            self.lensTintColorDescription(data.TintColorDescription);
            self.lensEdgeTypeId(data.LensEdgeTypeId);
            self.lensEdgeTypeDescription(data.LensEdgeTypeDescription);
            self.labInstructions(data.LabInstructions);

            if (data.AddCoatingsId && data.AddCoatingsId.length !== 0) {
                self.additionalCoatingsId(data.AddCoatingsId.split(", "));
            }

            for (i = 0; i < self.additionalCoatingsId().length; i++) {
                if (self.displayAdditionalCoatings === undefined || self.displayAdditionalCoatings()[i] === undefined) {
                    self.addAdditionalCoatings(i, self.additionalCoatingsId()[i]);
                } else if (i > self.displayAdditionalCoatings().length) {
                    self.addAdditionalCoatings(i, self.additionalCoatingsId()[i]);
                } else if (self.displayAdditionalCoatings()[i].AdditionalCoatingsId !== self.additionalCoatingsId()[i] && self.displayAdditionalCoatings()[i].AdditionalCoatingsId === 0) {
                    self.addAdditionalCoatings(i, self.additionalCoatingsId()[i]);
                }
            }

            self.backsideAr(data.BacksideAr);
        } else {
            self.isDirty(false);
            self.isCustom(false);
            self.lensTypeFilterList(null);

            // drop-down lists
            self.rightMaterialList(null);
            self.rightStyleList(null);
            self.rightColorList(null);
            self.rightMfgCoatings(null);

            self.leftMaterialList(null);
            self.leftStyleList(null);
            self.leftColorList(null);
            self.leftMfgCoatings(null);

            self.additionalCoatings([]);
            self.lensTintTypes(null);
            self.lensTintColors(null);
            self.lensEdgeTypes(null);

            // right-eye attributes
            self.rightLens(null);
            self.rightLensId(0);
            self.rightLensTypeId(0);
            self.rightLensTypeItemNum("");
            self.rightLensIsSingleVision(null);
            self.rightLensTypeDescription("");
            self.rightMaterialId(0);
            self.rightMaterialDescription("");
            self.rightMaterialThickness("");
            self.rightStyleId(0);
            self.rightStyleDescription("");
            self.rightColorId(0);
            self.rightColorDescription("");
            self.rightCoatingId(0);
            self.rightCoatingDescription("");
            self.rightFarPd("");
            self.rightNearPd("");
            self.rightBase("");
            self.rightOcHeight("");
            self.rightSegmentHeight("");
            self.rightThickness("");
            self.rightIsBalanced("");
            self.rightIsSlabOff("");
            self.rightIsUncut("");
            self.rightIsIof("");

            // left eye attributes
            self.leftLens(null);
            self.leftLensId("");
            self.leftLensTypeId(0);
            self.leftLensTypeItemNum("");
            self.leftLensIsSingleVision(null);
            self.leftLensTypeDescription("");
            self.leftMaterialId(0);
            self.leftMaterialDescription("");
            self.leftMaterialThickness("");
            self.leftStyleId(0);
            self.leftStyleDescription("");
            self.leftColorId(0);
            self.leftColorDescription("");
            self.leftCoatingId(0);
            self.leftCoatingDescription("");
            self.leftFarPd("");
            self.leftNearPd("");
            self.leftBase("");
            self.leftOcHeight("");
            self.leftSegmentHeight("");
            self.leftThickness("");
            self.leftIsBalanced("");
            self.leftIsSlabOff("");
            self.leftIsUncut("");
            self.leftIsIof("");
            // additional lens information
            self.distanceType("");
            self.binocularFarPd("");
            self.binocularNearPd("");
            self.rightFarPdInput("");
            self.rightNearPdInput("");
            self.lensTintTypeId("");
            self.lensTintTypeDescription("");
            self.lensTintColorId("");
            self.lensTintColorDescription("");
            self.lensEdgeTypeId("");
            self.lensEdgeTypeDescription("");
            self.labInstructions("");

            self.additionalCoatingsId([]);
            self.displayAdditionalCoatings([]);
            self.backsideAr(0);
            self.rx(null);

        }
    };
}; // end EyeglassLensViewModel

function getRightLensFromViewModel() {
    var lens = {
        Id: vmLenses.rightLensId(),
        TypeId: vmLenses.rightLensTypeId(),
        TypeItemNum: vmLenses.rightLensTypeItemNum(),
        IsSingleVision: vmLenses.rightLensIsSingleVision(),
        TypeDescription: vmLenses.rightLensTypeDescription(),
        MaterialTypeId: vmLenses.rightMaterialId(),
        MaterialDescription: vmLenses.rightMaterialDescription(),
        MaterialThickness: vmLenses.rightMaterialThickness(),
        StyleId: vmLenses.rightStyleId(),
        StyleDescription: vmLenses.rightStyleDescription(),
        ColorId: vmLenses.rightColorId(),
        ColorDescription: vmLenses.rightColorDescription(),
        MfgCoatingId: vmLenses.rightCoatingId(),
        MfgCoatingDescription: vmLenses.rightCoatingDescription(),
        SlabOff: vmLenses.rightIsSlabOff(),
        Uncut: vmLenses.rightIsUncut(),
        Balanced: vmLenses.rightIsBalanced(),
        FarPd: vmLenses.rightFarPd(),
        NearPd: vmLenses.rightNearPd(),
        BaseCurve: vmLenses.rightBase(),
        OcHeight: vmLenses.rightOcHeight(),
        SegHeight: vmLenses.rightSegmentHeight(),
        LensOptions: vmLenses.RightOptions(),
        LensMaterial: vmLenses.rightMaterialList(),
        LensStyles: vmLenses.rightStyleList(),
        LensColors: vmLenses.rightColorList(),
        LensMfgCoatings: vmLenses.rightMfgCoatings()
    };
    return lens;
}

function getLeftLensFromViewModel() {
    var lens = {
        Id: vmLenses.leftLensId(),
        TypeId: vmLenses.leftLensTypeId(),
        TypeItemNum: vmLenses.leftLensTypeItemNum(),
        IsSingleVision: vmLenses.leftLensIsSingleVision(),
        TypeDescription: vmLenses.leftLensTypeDescription(),
        MaterialTypeId: vmLenses.leftMaterialId(),
        MaterialDescription: vmLenses.leftMaterialDescription(),
        MaterialThickness: vmLenses.leftMaterialThickness(),
        StyleId: vmLenses.leftStyleId(),
        StyleDescription: vmLenses.leftStyleDescription(),
        ColorId: vmLenses.leftColorId(),
        ColorDescription: vmLenses.leftColorDescription(),
        MfgCoatingId: vmLenses.leftCoatingId(),
        MfgCoatingDescription: vmLenses.leftCoatingDescription(),
        SlabOff: vmLenses.leftIsSlabOff(),
        Uncut: vmLenses.leftIsUncut(),
        Balanced: vmLenses.leftIsBalanced(),
        FarPd: vmLenses.leftFarPd(),
        NearPd: vmLenses.leftNearPd(),
        BaseCurve: vmLenses.leftBase(),
        OcHeight: vmLenses.leftOcHeight(),
        SegHeight: vmLenses.leftSegmentHeight(),
        LensOptions: vmLenses.LeftOptions(),
        LensMaterial: vmLenses.leftMaterialList(),
        LensStyles: vmLenses.leftStyleList(),
        LensColors: vmLenses.leftColorList(),
        LensMfgCoatings: vmLenses.leftMfgCoatings()
    };
    return lens;
}
function getAdditionalCoatingDescriptionById(id) {
    if (vmLenses.additionalCoatings() !== null) {
        var match = ko.utils.arrayFirst(vmLenses.additionalCoatings(), function (coating) {
            return (id === coating.Key);
        });

        if (match !== undefined && match !== null) {
            return match.Description;
        }
    }
    return "";
}
function getSelectedLensesFromViewModel() {
    var addCoatingIds = "",
        addCoatingDescription = "",
        pd,
        i,
        isCustomLens = false,
        isIof = false;

    for (i = 0; i < vmLenses.displayAdditionalCoatings().length; i++) {
        var id = vmLenses.displayAdditionalCoatings()[i].AdditionalCoatingsId;
        if (id > 0) {
            addCoatingIds += addCoatingIds.length > 0 ? ", " + id : id;
            var description = getAdditionalCoatingDescriptionById(id);
            if (description && description.length > 0) {
                addCoatingDescription += addCoatingDescription.length > 0 ? ", " + description : description;
                if (description.toUpperCase().indexOf("BACKSIDE") > 0) {
                    vmLenses.backsideAr(true);
                }
            }
        }
    }

    if ((vmLenses.leftIsIof() !== undefined && vmLenses.leftIsIof() !== null && vmLenses.leftIsIof() === true) || (vmLenses.rightIsIof() !== undefined && vmLenses.rightIsIof() !== null && vmLenses.rightIsIof() === true)) {
        isIof = true;
    }

    if (vmLenses.distanceType() !== "2") {
        if (vmLenses.binocularFarPd()) {
            pd = parseFloat(vmLenses.binocularFarPd()) / 2;
            vmLenses.rightFarPd(convertValue(pd + 0.01, 1));
            vmLenses.leftFarPd(convertValue(pd - 0.01, 1));
        }
        if (vmLenses.binocularNearPd()) {
            pd = parseFloat(vmLenses.binocularNearPd()) / 2;
            vmLenses.rightNearPd(convertValue(pd + 0.01, 1));
            vmLenses.leftNearPd(convertValue(pd - 0.01, 1));
        }
    }
    var leftLens = getLeftLensFromViewModel();
    var rightLens = getRightLensFromViewModel();
    if (leftLens && rightLens) {
        if (leftLens.Id !== rightLens.Id || leftLens.LensOptions !== rightLens.LensOptions) {
            isCustomLens = true;
        }
    }
    var lenses = {
        IsIof: isIof,
        IsValid: eyeglassOrderViewModel.selectedLens() !== null ? eyeglassOrderViewModel.selectedLens().IsValid : false,
        IsCustomLenses: isCustomLens,
        IsLensTreatments: eyeglassOrderViewModel.eyeglassOrderType() === egOrderType.EXTRAS_ONLY && eyeglassOrderViewModel.selectedAuthorization().NonInsurance ? true : false,
        PdType: vmLenses.distanceType(),
        TintTypeId: vmLenses.lensTintTypeId() || 0,
        TintTypeDescription: vmLenses.lensTintTypeDescription(),
        TintColorId: vmLenses.lensTintColorId() || 0,
        TintColorDescription: vmLenses.lensTintColorDescription(),
        LensEdgeTypeId: vmLenses.lensEdgeTypeId() || 0,
        LensEdgeTypeDescription: vmLenses.lensEdgeTypeDescription(),
        LabInstructions: vmLenses.labInstructions(),
        AddCoatingsId: addCoatingIds,
        AddCoatingsDescription: addCoatingDescription,
        BacksideAr: vmLenses.backsideAr(),
        RightLens: rightLens,
        LeftLens: leftLens
    };
    return lenses;
}

/* Validates the lenses page */
function validateEgLenses() {
    var result = false;
    $.validator.addMethod("validatePrice", function (value, element) {
        if ($("#modalLensSearch").is(':visible') === true) {
            if (element.value !== '' && element.name === "retailPrice") {
                var price = parseFloat(element.value);
                if (price !== 0) {
                    result = true;
                } else {
                    if (element.value === "0.00") {
                        result = true;
                    }
                }
            }
        }
        return result;
    });

    $("#chooseEgLensesForm").alslValidate({
        ignore: ":hidden, .ignore",
        onfocusout: false,
        onclick: false,
        rules:
            {
                rightLensType: { selectBox: true },
                rightLensMaterial: { selectBox: true },
                rightLensStyle: { selectBox: true },
                rightLensColor: { selectBox: true },
                rightManufacturerCoating: { selectBox: true },
                leftLensType: { selectBox: true },
                leftLensMaterial: { selectBox: true },
                leftLensStyle: { selectBox: true },
                leftLensColor: { selectBox: true },
                leftManufacturerCoating: { selectBox: true },
                rightFarPupilDistance: { required: true, range: [getPdMinValue(), getPdMaxValue()] },
                rightNearPupilDistance: { required: true, range: [getPdMinValue(), getPdMaxValue()] },
                leftFarPd: { required: true, range: [getPdMinValue(), getPdMaxValue()] },
                leftNearPd: { required: true, range: [getPdMinValue(), getPdMaxValue()] },
                measureRightThickness: { range: [1.0, 3.0] },
                measureLeftThickness: { range: [1.0, 3.0] },
                measureRightOCH: { range: [0.0, 99.9] },
                measureLeftOCH: { range: [0.0, 99.9] },
                measureRightSH: { required: true, range: [10.0, 30.0] },
                measureLeftSH: { required: true, range: [10.0, 30.0] },
                measureRightBC: { range: [0.0, 99.9] },
                measureLeftBC: { range: [0.0, 99.9] },
                searchLensStyle: { selectBox: true },
                retailPrice: { required: true, validatePrice: true }
            },
        messages:
            {
                rightLensType: { selectBox: "Select a type." },
                rightLensMaterial: { selectBox: "Select a material." },
                rightLensStyle: { selectBox: "Select a style." },
                rightLensColor: { selectBox: "Select a color." },
                rightManufacturerCoating: { selectBox: "Select a coating." },
                leftLensType: { selectBox: "Select a type." },
                leftLensMaterial: { selectBox: "Select a material." },
                leftLensStyle: { selectBox: "Select a style." },
                leftLensColor: { selectBox: "Select a color." },
                leftManufacturerCoating: { selectBox: "Select a coating." },
                rightFarPupilDistance: { required: "Enter the distant PD.", range: "Enter the distant PD. (" + getPdMinValue() + " to " + getPdMaxValue() + ")." },
                rightNearPupilDistance: { required: "Enter the near PD.", range: "Enter the near PD. (" + getPdMinValue() + " to " + getPdMaxValue() + ")." },
                leftFarPd: { required: "Enter the distant PD.", range: "Enter the distant PD. (" + getPdMinValue() + " to " + getPdMaxValue() + ")." },
                leftNearPd: { required: "Enter the near PD.", range: "Enter the near PD. (" + getPdMinValue() + " to " + getPdMaxValue() + ")." },
                measureRightThickness: { range: "Enter the thickness measurement (1.0 to 3.0)." },
                measureLeftThickness: { range: "Enter the thickness measurement (1.0 to 3.0)." },
                measureRightOCH: { range: "Enter the right O.C. Height. (0.0 to 99.9)" },
                measureLeftOCH: { range: "Enter the left O.C. Height. (0.0 to 99.9)" },
                measureRightSH: { required: "Enter the right Seg Height.", range: "Enter the right Seg Height. (10.0 to 30.0)" },
                measureLeftSH: { required: "Enter the left Seg Height.", range: "Enter the left Seg Height. (10.0 to 30.0)" },
                measureRightBC: { range: "Enter the right Base Curve. (0.0 to 99.9)" },
                measureLeftBC: { range: "Enter the left Base Curve. (0.0 to 99.9)" },
                searchLensStyle: { selectBox: "Select a lens." },
                retailPrice: { required: "Enter the retail price.", validatePrice: "Enter the retail price." }
            }
    });
} // validateEgLenses

$("#btnLensesToBuildOrder").click(function (e) {
    e.preventDefault();
    $("#chooseLenses").addClass("hidden");
    resetTintColor();
    window.initChooseBuildOrderPage(window.buildOrder.LENSES);
});

$("#btnCustomizeLenses").click(function (e) {
    e.preventDefault();
    customizeLens();
    vmLenses.isCustom(true);
});

$("#btnAddMoreCoatings").click(function (e) {
    e.preventDefault();
    var id = vmLenses.additionalCoatingsIndex(), i;
    for (i = 0; i < vmLenses.displayAdditionalCoatings().length; i++) {
        if (!vmLenses.displayAdditionalCoatings()[i].AdditionalCoatingsId) {
            return;
        }
    }
    vmLenses.addAdditionalCoatings(id, 0);
});

$("#btnAddLensesToOrder").click(function (e) {
    e.preventDefault();

    if (!$("#chooseEgLensesForm").valid()) {
        return;
    }

    $("#chooseLenses").addClass("hidden");

    var lenses = getSelectedLensesFromViewModel();
    lenses.IsValid = true;

    // reset any Lens server validation messages
    clearLensError();

    var msg = "Lens selections have been added to the eyeglass order.";

    //lens treatments
    if (lenses.IsLensTreatments) {
        var add = false;
        if ((lenses.TintTypeId !== undefined && lenses.TintTypeId > 0) || (lenses.LabInstructions !== undefined && lenses.LabInstructions !== null && lenses.LabInstructions !== '') ||
                lenses.AddCoatingsId !== '' || (lenses.LensEdgeTypeId !== undefined && lenses.LensEdgeTypeId > 0)) {
            add = true;
        }
        msg = add ? "Lens Treatments have been added to the eyeglass order." : eyeglassOrderViewModel.selectedLens() !== null ? "Lens Treatments have been removed from the eyeglass order." : "";
        if (!add) {
            lenses = null;
            if (msg === '') {
                msg = "No Lens Treatments were added to the order.";
            }
        }
    }

    window.updateSelectedEntity(window.buildOrder.LENSES, lenses);
    window.displayPage(window.buildOrder.LENSES, msg);
});

$("#measureRightSH, #measureLeftSH, #measureRightBC, #measureLeftBC, #measureRightThickness, #measureLeftThickness, " +
    "#rightFarPupilDistance, #leftFarPd, #rightNearPupilDistance, #leftNearPd").keydown(function (e) {
    return $(this).decimalFilter(e, true);
});

$("#measureRightOCH, #measureLeftOCH").keydown(function (e) {
    return $(this).integerFilter(e);
});

$("#measureRightSH, #measureLeftSH, #measureRightBC, #measureLeftBC, #measureRightThickness, #measureLeftThickness").on("blur", function () {
    if (this.value && this.value.length !== 0) {
        var decimalPlaces = this.id === "measureRightSH" || this.id === "measureLeftSH" ? 2 : 1;
        $(this).val(convertValue(this.value, decimalPlaces));
    }

    if (!isLensLoading) {
        switch (this.id) {
        case "measureRightSH":
        case "measureLeftSH":
            if (this.value && this.value.length !== 0) {
                $("#" + this.id).removeClass("requiredField");
            } else {
                $("#" + this.id).addClass("requiredField");
            }
            break;
        default:
            break;
        }
    }
});

$("#tintColor").change(function () {
    $(this).clearField();
    if (this.value.length === 0 || this.value.toNumber() === 0) {
        $(this).parents("div.bootstrap-select").addClass("requiredField");
        $("#" + this.id).addClass("requiredField").rules("add", { selectBox: true, messages: { selectBox: "Select a tint color." } });
        $("#tintColorRequired").removeClass("hidden");
    } else {
        $(this).parents("div.bootstrap-select").removeClass("requiredField");
        $("#" + this.id).removeClass("requiredField");
        $("#tintColorRequired").addClass("hidden");
    }
    $("select#tintColor").selectpicker('refresh');
});

$("#rightLensType, #rightLensMaterial, #rightLensStyle, #rightLensColor, #leftLensType, #leftLensMaterial, " +
    "#leftLensStyle, #leftLensColor, #rightFarPupilDistance, #leftFarPd, #rightNearPupilDistance, #leftNearPd, #rightManufacturerCoating, #leftManufacturerCoating").change(function () {
    if (!isLensLoading) {
        switch (this.id) {
        case "rightLensType":
            materialIdRight = styleIdRight = colorIdRight = coatingIdRight = 0;
            break;
        case "leftLensType":
            materialIdLeft = styleIdLeft = colorIdLeft = coatingIdLeft = 0;
            break;
        case "rightLensMaterial":
            styleIdRight = colorIdRight = coatingIdRight = 0;
            break;
        case "leftLensMaterial":
            styleIdLeft = colorIdLeft = coatingIdLeft = 0;
            break;
        case "rightLensStyle":
            colorIdRight = coatingIdRight = 0;
            break;
        case "leftLensStyle":
            colorIdLeft = coatingIdLeft = 0;
            break;
        case "rightLensColor":
            coatingIdRight = 0;
            break;
        case "leftLensColor":
            coatingIdLeft = 0;
            break;
        default:
            break;
        }

        $(this).clearField();
        if (this.selectedIndex <= 0 && (this.value === "0" || this.value === "")) {
            $(this).parents("div.bootstrap-select").addClass("requiredField");
        } else {
            $(this).parents("div.bootstrap-select").removeClass("requiredField");
        }
    }
});

$("#measureRightThickness, #measureLeftThickness").change(function () {
    if (!isNaN($(this).val())) {
        $(this).val(convertValue(this.value, 1));
    }
});

$("#pdDistanceMono, #pdDistanceBi").click(function (e) {
    if (this.id === "pdDistanceMono") {
        var pd;
        vmLenses.distanceType("2");
        if (vmLenses.binocularFarPd()) {
            pd = parseFloat(vmLenses.binocularFarPd()) / 2;
            vmLenses.rightFarPd(convertValue(pd + 0.01, 1));
            vmLenses.leftFarPd(convertValue(pd - 0.01, 1));
            vmLenses.rightFarPdInput(vmLenses.rightFarPd());
        }
        if (vmLenses.binocularNearPd()) {
            pd = parseFloat(vmLenses.binocularNearPd()) / 2;
            vmLenses.rightNearPd(convertValue(pd + 0.01, 1));
            vmLenses.leftNearPd(convertValue(pd - 0.01, 1));
            vmLenses.rightNearPdInput(vmLenses.rightNearPd());
        }
    } else {
        vmLenses.distanceType("1");
        if (vmLenses.rightFarPd() && vmLenses.leftFarPd()) {
            var binocularFarPd = parseFloat(vmLenses.rightFarPd()) + parseFloat(vmLenses.leftFarPd());
            vmLenses.binocularFarPd(parseFloat(binocularFarPd).toFixed(1));
            vmLenses.rightFarPdInput(vmLenses.binocularFarPd());
        }
        if (vmLenses.rightNearPd() && vmLenses.leftNearPd()) {
            var binocularNearPd = parseFloat(vmLenses.rightNearPd()) + parseFloat(vmLenses.leftNearPd());
            vmLenses.binocularNearPd(parseFloat(binocularNearPd).toFixed(1));
            vmLenses.rightNearPdInput(vmLenses.binocularNearPd());
        }
    }

    setPdValidations();
});

$("#rightLinkAddStyles, #leftLinkAddStyles").click(function (e) {
    searchLensSide = this.id === 'leftLinkAddStyles' ? egLens.LEFT : egLens.RIGHT;
    //var rightLensMaterial = $("#rightLensMaterial");
    $("#searchLensStyle").addClass("requiredField");
    $("#searchLensStyle").clearField();

    getLensStylesToMap(searchLensSide);
});

$("#btnCancelSearchLens").click(function (e) {
    e.preventDefault();
    $("#searchLensStyle").addClass("requiredField");
    $("#lensAttributes").addClass("hidden");
    lensAttributeTable.fnClearTable();
    lensAttributeTable.fnSettings().oLanguage.sEmptyTable = "Select a lens from the Lens Description list above.";
    lensAttributeTable.fnDraw();
    $("#searchLensStyle")[0].selectedIndex = 0;
    $("#lensMainForm .ignore").removeClass("ignore");
    $("#lensMainForm .requiredField").clearField();
    $("#modalLensSearch").modal("hide");
});

$("#btnSearchLensSave").click(function (e) {
    e.preventDefault();

    if (!$("#chooseEgLensesForm").valid()) {
        return;
    }

    saveSelectedLensAndMap(vmLenses.searchLensId(), vmLenses.eyeglassLensPricingItems());
});

$("#searchLensStyle").change(function (e) {
    e.preventDefault();
    // get lens id
    if (!this.value || this.value.toNumber().isNaN) {
        $("#searchLensStyle").addClass("requiredField");
        return;
    }

    $("#searchLensStyle").clearField();
    vmLenses.searchLensId(this.value.toNumber());
    // CheckItemAttributesForPricing
    client
        .action("GetEyeglassLensAttributes")
        .get({
            itemId: vmLenses.searchLensId()
        })
        .done(function (data) {
            if (!!data) {
                $("#searchLensStyle").removeClass("requiredField");
                $("#lensAttributes").removeClass("hidden");
                vmLenses.eyeglassLensPricingItems(data);
                lensAttributeTable.refreshDataTable(vmLenses.eyeglassLensPricingItems());
            } else {
                saveSelectedLensAndMap(vmLenses.searchLensId(), null);
            }
        })
        .fail(function (xhr) {
            $("#searchLensStyle").addClass("requiredField");
        });
});

function updateLensBuildOrderTile() {
    if (window.eyeglassOrderViewModel.selectedLens() !== null && window.eyeglassOrderViewModel.selectedLens() !== undefined) {
        var selExCnt = (window.eyeglassOrderViewModel.selectedLens().length);
        if (selExCnt > 0) {
            var lenses = eyeglassOrderViewModel.selectedLens();
            var lensAddedTitle = "";

            // Right Lens Title
            if (lenses && lenses.RightLens) {
                lensAddedTitle += "(R) " + lenses.RightLens.TypeDescription + "<br>";
            }

            // Left Lens Title
            if (lenses && lenses.LeftLens && eyeglassOrderViewModel.eyeglassOrderType() !== egOrderType.EXTRAS_ONLY) {
                lensAddedTitle += "(L) " + lenses.LeftLens.TypeDescription;
            }

            $("#space_1").addClass("hidden");
            $("#info_1").html(lensAddedTitle);
            setBuildOrderTileComplete(window.buildOrder.LENSES);
        } else {
            $("#space_1").removeClass("hidden");
            $("#info_1").html("Add Lenses");
            setBuildOrderTileReset(window.buildOrder.LENSES);
        }
    }
}

function clearIds() {
    if (vmLenses) {
        vmLenses.importModel(null);
        $("#chooseEgLensesForm select:not(.additional-coatings select)").selectpicker('refresh');
        $("#rightFarPupilDistance").addClass("requiredField");
        $("#rightNearPupilDistance").addClass("requiredField");
        $("#leftFarPd").addClass("requiredField");
        $("#leftNearPd").addClass("requiredField");
    }

    materialIdRight = 0;
    styleIdRight = 0;
    colorIdRight = 0;
    coatingIdRight = 0;
    materialIdLeft = 0;
    styleIdLeft = 0;
    colorIdLeft = 0;
    coatingIdLeft = 0;
    tintColorId = 0;
    tintTypeDescription = "";
    $("#rightFarPupilDistance").clearField();
    $("#rightNearPupilDistance").clearField();
    $("#leftFarPd").clearField();
    $("#leftNearPd").clearField();
    $("#segHeightRequired").removeClass("hidden");
    $("#measureLeftSH").clearField();
    $("#measureRightSH").clearField();
}

function clearLenses() {
    clearIds();
    lensMeasurements = null;
    if (eyeglassOrderViewModel.selectedLens() !== null) {
        lensMeasurements = eyeglassOrderViewModel.selectedLens();
    }

    isMultiFocal = false;
    window.updateSelectedEntity(window.buildOrder.LENSES, null);
    window.eyeglassOrderViewModel.selectedLens(null);
    window.updateLensBuildOrderTile();
    resetPage();
}

function initLensesPage() {
    $("#btnSaveForLater").removeClass("hidden");
    if (window.egDataSource === "order") {
        $("#btnSaveForLater").addClass("hidden");
    }

    $("#btnContinue").addClass("hidden");
    $("#btnContinueToPricing").addClass("hidden");
    isMultiFocal = checkRxTypeMultiFocal();
    client
        .action("GetEyeglassOrderLensDetail")
        .get({
            patientId: window.patientOrderExam.PatientId,
            orderId: window.patientOrderExam.OrderId,
            examId: window.patientOrderExam.ExamId,
            isMultiFocal: isMultiFocal,
            isVsp: isVspInsurance
        })
        .done(function (data) {
            if (data !== undefined && data !== null) {
                if (eyeglassOrderViewModel.selectedRx() !== null) {
                    vmLenses.importRx(eyeglassOrderViewModel.selectedRx());
                }
                vmLenses.importListData(data);

                if (eyeglassOrderViewModel.selectedLens() !== null) {
                    isLensLoading = true;
                    resetTintColor();
                    vmLenses.importModel(eyeglassOrderViewModel.selectedLens());

                    isLensLoading = false;
                } else {
                    if (!!lensMeasurements) {
                        vmLenses.importPDMeasurements(lensMeasurements);
                        vmLenses.importLensMeasurements(egLens.LEFT, lensMeasurements.LeftLens);
                        vmLenses.importLensMeasurements(egLens.RIGHT, lensMeasurements.RightLens);
                    }
                }

                if (window.eyeglassOrderViewModel.selectedLens() && window.eyeglassOrderViewModel.selectedLens().IsValid === false) {
                    showLensError();
                }

                changeDropdownList();

                $("#chooseEgLensesForm select:not(.additional-coatings select)").selectpicker('refresh');
                var addCoating = document.getElementById("addCoating_0");
                if (!addCoating) {
                    vmLenses.addAdditionalCoatings(0, 0);
                }
                addCoating = document.getElementById("addCoating_1");
                if (!addCoating) {
                    vmLenses.addAdditionalCoatings(1, 0);
                }
                lensMeasurements = null;
            }
        });
} // initLensesPage

function makeLensPageEditableForInvoicedLabOnHoldOrder() {
    // ALSL-6958
    /*jslint todo: true */
    // TODO: If IsOrderStatusLabOnHold, make these fields editable.
    // Thickness, OC Height, Seg Height, Monocular PD, Binocular PD, Base Curve
    $("#chooseLenses :input").prop("disabled", true);
    $("#chooseLenses :input").removeClass("requiredField");
    $("#pdDistanceMono, #pdDistanceBi, #rightFarPupilDistance, #rightNearPupilDistance, #leftFarPd, #leftNearPd").removeAttr("disabled");
    $("#measureRightOCH, #measureRightSH, #measureRightBC, #measureRightThickness, #measureLeftOCH, #measureLeftSH, #measureLeftBC, #measureLeftThickness").removeAttr("disabled");
    $("#btnAddLensesToOrder").removeAttr("disabled");
}

/* Initialize the Choose Lenses page (and Right-hand Insurance panel) */
var initChooseOrderLensesPage = function () {
    $("#egOrderTitle").html(window.patient.FirstName + " " + window.patient.LastName + " : Eyeglass Order (" + eyeglassOrderViewModel.eyeglassOrderTypeDescription() + ") : Choose Lenses");
    resetPage();

    if (!isPageInitialized) {
        $("select:not(.additional-coatings select)").alslSelectPicker();
        clearAddCoatingsSelects();
        vmLenses = new EyeglassLensesViewModel();
        ko.applyBindings(vmLenses, $('#chooseEgLensesForm')[0]);
    } else {
        clearIds();
    }

    $("#chooseEgLensesForm select:not(.additional-coatings select)").selectpicker('refresh');
    vmLenses.distanceType("2");

    if (window.InsPanelViewModel !== undefined && window.InsPanelViewModel !== null && window.InsPanelViewModel.PrimaryEligibility()) {
        isVspInsurance = window.InsPanelViewModel.PrimaryEligibility().IsVsp();
    }

    if (isVspInsurance) {
        $("#stylesTooltipRight, #stylesTooltipLeft").removeClass("hidden");
    } else {
        $("#stylesTooltipRight, #stylesTooltipLeft").addClass("hidden");
    }

    $(".tt").popover({ container: 'body', html: true, placement: 'top' });
    $("#measureRightOCH, #measureRightSH, #measureLeftOCH, #measureLeftSH").attr("disabled", "disabled");
    $("#rightLensType, #leftLensType").addClass("requiredField");

    if (!isPageInitialized) {
        //$("select:not(.additional-coatings select)").alslSelectPicker();
        buildLensAttributeTable();
    }
    initLensesPage();
    isPageInitialized = true;

    // show the page
    $("#chooseLenses").removeClass("hidden");
    $("#btnContinue").addClass("hidden");
    validateEgLenses();
    setupPage(window.buildOrder.LENSES);
    window.setTimeout(function () {
        $("#rightType").removeClass("hidden");
    }, 250);

    $("select:not(.additional-coatings select)").refreshSelectPicker();

    window.scrollTo(0, 0);
};