/*jslint browser: true, vars: true, nomen:true, sloppy:true*/
/*global $, document, alert, window, ko, ApiClient, msgType, commonDate, msgType, loadPatientSideNavigation, console, getDateDiff, updatePageTitle*/

var client = new ApiClient(window.config.baseUrl, "PatientContactLensRx");
var viewModel, initLoad, edit;

var underlyingType = {
    SELECT: 0,
    BALANCE_LENS: 60,
    NO_LENS: 61,
    NOT_RECORDED: 62,
    PROSTHESIS: 63,
    PLANO: 64
};

var lens = {
    RIGHT: '#r',
    LEFT: '#l'
};

var savedExam;
var lensParam;
var originalData;
var showMsgTimer;
var rxExamId = -1;
var isRecheck = false;
var examRxTypeId;
var ulConditionErrorMsg = "You cannot select a Balanced Lens, No Lens, Not Recorded, and/or Prosthesis underlying condition for both the right and left lens.";

function setDoctorDdls(data) {
    if (data.ProviderType === 0) {
        $("#radioOption3").prop("checked", true);
        $("#OutsideProviderDiv").hide();
        $("#ProviderDiv").show();
        if (data.Provider > 0) {
            $('select#provider').parents("div.bootstrap-select").removeClass('requiredField');
        } else {
            $('select#provider').parents("div.bootstrap-select").addClass('requiredField');
        }
    } else {
        $("#radioOption4").prop("checked", true);
        $("#OutsideProviderDiv").show();
        $("#ProviderDiv").hide();
    }
}
var ContactLensRxViewModel = function (data) {
    originalData = data;
    var self = this;
    self.providers = ko.observableArray(data.Providers);
    self.providers.unshift({ "Key": 0, "Description": '' });
    self.provider = ko.observable(data.Provider);
    self.outsideProviderName = ko.observable(data.OutsideProviderName);
    self.outsideProviderId = ko.observable(data.OutsideProviderId || 0);
    self.providerType = ko.observable(data.ProviderType);
    setDoctorDdls(data);
    self.lenstypes = ko.observableArray(data.ContactLensRxType);
    self.lenstypes.unshift({ "Key": 0, "Description": '' });
    self.lenstype = ko.observable(data.LensRxTypeId);

    self.WearSchedules = ko.observableArray(data.WearScheduleList);
    self.WearSchedules.unshift({ "Key": 0, "Description": 'Select' });
    self.WearSchedule = ko.observable(data.WearScheduleId);

    self.Replenishments = ko.observableArray(data.ReplenishmentList);
    self.Replenishments.unshift({ "Key": 0, "Description": 'Select' });
    self.Replenishment = ko.observable(data.ReplenishmentId);

    self.DFRegimens = ko.observableArray(data.DisinfectingRegimenList);
    self.DFRegimens.unshift({ "Key": 0, "Description": 'Select' });
    self.DFRegimen = ko.observable(data.DisinfectingRegimenId);

    self.RuConditions = ko.observableArray(data.UnderlyingConditionList);
    self.RuConditions.unshift({ "Key": 0, "Description": 'Select' });
    self.RuCondition = ko.observable(data.RightUlConditionId);
    self.LuConditions = ko.observableArray(data.UnderlyingConditionList);
    self.LuCondition = ko.observable(data.LeftUlConditionId);

    if (data.Suppliers !== null) {
        self.RSuppliers = ko.observableArray(data.Suppliers);
        self.RSuppliers.unshift({ "Key": 0, "Description": 'Select' });
    } else {
        self.RSuppliers = ko.observableArray();
    }
    self.RSupplier = ko.observable(data.RightSupplierId);
    self.LSuppliers = ko.observableArray(data.Suppliers);
    self.LSupplier = ko.observable(data.LeftSupplierId);

    self.RManufacturers = ko.observableArray(data.Manufacturers);
    self.RManufacturers.unshift({ "KeyStr": '0', "Description": '' });
    self.RManufacturer = ko.observable(data.RightManufacturerId);
    self.LManufacturers = ko.observableArray(data.Manufacturers);
    self.LManufacturer = ko.observable(data.LeftManufacturerId);

    if (data.RightStyleList !== null) {
        self.RStyles = ko.observableArray(data.RightStyleList);
        self.RStyles.unshift({ "Key": 0, "Description": '' });
    } else {
        self.RStyles = ko.observableArray();
    }
    self.RStyle = ko.observable(data.RightStyleId);

    if (data.LeftStyleList !== null) {
        self.LStyles = ko.observableArray(data.LeftStyleList);
        self.LStyles.unshift({ "Key": 0, "Description": '' });
    } else {
        self.LStyles = ko.observableArray();
    }
    self.LStyle = ko.observable(data.LeftStyleId);

    if (data.RightLensParamsList !== null) {
        self.RLensparams = ko.observableArray(data.RightLensParamsList);
        self.RLensparams.unshift({ "Key": 0, "Description": '' });
    } else {
        self.RLensparams = ko.observableArray();
    }
    self.RLensparam = ko.observable(data.RightLensPowerId);

    if (data.LeftLensParamsList !== null) {
        self.LLensparams = ko.observableArray(data.LeftLensParamsList);
        self.LLensparams.unshift({ "Key": 0, "Description": '' });
    } else {
        self.LLensparams = ko.observableArray();
    }
    self.LLensparam = ko.observable(data.LeftLensPowerId);

    if (data.RightColorList !== null) {
        self.RColors = ko.observableArray(data.RightColorList);
        self.RColors.unshift({ "KeyStr": "0", "Description": '' });
    } else {
        self.RColors = ko.observableArray();
    }
    self.RColor = ko.observable(data.RightLensColorId);

    if (data.LeftColorList !== null) {
        self.LColors = ko.observableArray(data.LeftColorList);
        self.LColors.unshift({ "KeyStr": "0", "Description": '' });
    } else {
        self.LColors = ko.observableArray();
    }
    self.LColor = ko.observable(data.LeftColorId);
    if (data.RightSphereList !== null) {
        if (data.RightUlConditionId === '64') {
            self.RSpheres = ko.observableArray([{ "KeyStr": '', "Description": '' }, { "KeyStr": '0.00', "Description": '0.00' }]);
            data.RightSphereId = '0.00';
        } else {
            self.RSpheres = ko.observableArray(data.RightSphereList);
            self.RSpheres.unshift({ "KeyStr": '', "Description": '' });
        }
    } else {
        self.RSpheres = ko.observableArray();
    }
    self.RSphere = ko.observable(data.RightSphereId);

    if (data.LeftSphereList !== null) {
        if (data.LeftUlConditionId === '64') {
            self.LSpheres = ko.observableArray([{ "KeyStr": '', "Description": '' }, { "KeyStr": '0.00', "Description": '0.00' }]);
            data.LeftSphereId = '0.00';
        } else {
            self.LSpheres = ko.observableArray(data.LeftSphereList);
            self.LSpheres.unshift({ "KeyStr": '0', "Description": 'Select' });
        }
    } else {
        self.LSpheres = ko.observableArray();
    }
    self.LSphere = ko.observable(data.LeftSphereId);

    if (data.RightCylinderList !== null) {
        self.RCylinders = ko.observableArray(data.RightCylinderList);
        self.RCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.RCylinders = ko.observableArray();
    }
    self.RCylinder = ko.observable(data.RightCylinderId);

    if (data.LeftCylinderList !== null) {
        self.LCylinders = ko.observableArray(data.LeftCylinderList);
        self.LCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.LCylinders = ko.observableArray();
    }
    self.LCylinder = ko.observable(data.LeftCylinderId);

    if (data.RightAxisList !== null) {
        if (data.RightAxisList.length > 0 && data.RightAxisList[0].KeyStr === '0') {
            data.RightAxisList[0].KeyStr = '-1';
        }
        self.RAxisList = ko.observableArray(data.RightAxisList);
        self.RAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.RAxisList = ko.observableArray();
    }
    if (data.RightAxisId === '0') {
        data.RightAxisId = '-1';
    }
    self.RAxis = ko.observable(data.RightAxisId);
    if (data.LeftAxisList !== null) {
        if (data.LeftAxisList.length > 0 && data.LeftAxisList[0].KeyStr === '0') {
            data.LeftAxisList[0].KeyStr = '-1';
        }
        self.LAxisList = ko.observableArray(data.LeftAxisList);
        self.LAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.LAxisList = ko.observableArray();
    }
    if (data.LeftAxisId === '0') {
        data.LeftAxisId = '-1';
    }
    self.LAxis = ko.observable(data.LeftAxisId);

    if (data.RightAddList !== null) {
        self.RAddList = ko.observableArray(data.RightAddList);
        self.RAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.RAddList = ko.observableArray();
    }
    self.RAdd = ko.observable(data.RightAddId);

    if (data.LeftAddList !== null) {
        self.LAddList = ko.observableArray(data.LeftAddList);
        self.LAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        self.LAddList = ko.observableArray();
    }
    self.LAdd = ko.observable(data.LeftAddId);

    self.RBaseTxt = ko.observable(data.RightBaseTxt);
    self.RDiameterTxt = ko.observable(data.RightDiameterTxt);
    self.RQuantityTxt = ko.observable(data.RightMaxQuantityTxt);
    self.LBaseTxt = ko.observable(data.LeftBaseTxt);
    self.LDiameterTxt = ko.observable(data.LeftDiameterTxt);
    self.LQuantityTxt = ko.observable(data.LeftMaxQuantityTxt);
    self.examDate = ko.observable(data.ExamDate);
    self.expirationDate = ko.observable(data.ExpirationDate);
    self.RStar = ko.observable(data.Star);
    self.LStar = ko.observable(data.Star);
    if (data.Notes !== null) {
        self.RxNote = ko.observable(data.Notes[0].Description);
    } else {
        self.RxNote = ko.observable();
    }
    self.showRxType = ko.computed(function () {
        return data.ExamRxTypeId !== window.CLRxType.EXAM_TYPE_NORX;
    });
    // When data becomes avaiable import it into the ViewModel.
    // With Knockout you never replace your base ViewModel and
    // you only call applyBindings once unless you are using
    // partial views.
    self.importModel = function (model, controlname) {

        // When completely overwriting observableArrays use the
        // replaceAll method (it takes a regular JS array).
        // Keep in mind the dataTypes. Qualifier is an int but the Qualifiers
        // array is coming over as Array[{Text:string, Value:string}]. This
        // here works but Value really should be coming from the server as
        // an int instead of string
        switch (controlname) {
        case 'rstyle':
            self.RStyles(model);
            self.RStyles.unshift({ "Key": 0, "Description": '' });
            if (model.length > 2) {
                self.RStyle(0);
            }
            break;
        case 'lstyle':
            self.LStyles(model);
            self.LStyles.unshift({ "Key": 0, "Description": 'Select' });
            if (model.length > 2) {
                self.LStyle(0);
            }
            break;
        case 'rlensparam':
            self.RLensparams(model);
            self.RLensparams.unshift({ "Key": 0, "Description": 'Select' });
            if (model.length > 2) {
                self.RLensparam(0);
            }
            break;
        case 'llensparam':
            self.LLensparams(model);
            self.LLensparams.unshift({ "Key": 0, "Description": 'Select' });
            if (model.length > 2) {
                self.LLensparam(0);
            }
            break;
        case 'rcolor':
            self.RColors(model);
            self.RColors.unshift({ "KeyStr": '', "Description": 'Select' });
            if (model.length > 2) {
                self.RColor(0);
            }
            break;
        case 'lcolor':
            self.LColors(model);
            self.LColors.unshift({ "KeyStr": '', "Description": 'Select' });
            if (model.length > 2) {
                self.LColor(0);
            }
            break;
        case 'rsphere':
            if (viewModel.RuCondition() === 64) {
                self.RSpheres([{ "KeyStr": '0', "Description": 'Select' }, { "KeyStr": '0.00', "Description": '0.00' }]);
                self.RSphere('0.00');
            } else {
                self.RSpheres(model);
                self.RSpheres.unshift({ "KeyStr": '0', "Description": 'Select' });
                if (model.length > 2) {
                    self.RSphere('0');
                }
            }
            break;
        case 'lsphere':
            if (viewModel.LuCondition() === 64) {
                self.LSpheres([{ "KeyStr": '0', "Description": 'Select' }, { "KeyStr": '0.00', "Description": '0.00' }]);
                self.LSphere('0.00');
            } else {
                self.LSpheres(model);
                self.LSpheres.unshift({ "KeyStr": '0', "Description": 'Select' });
                if (model.length > 2) {
                    self.LSphere('0');
                }
            }
            break;
        case 'rcylinder':
            self.RCylinders(model);
            self.RCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.RCylinder('0');
            }
            break;
        case 'lcylinder':
            self.LCylinders(model);
            self.LCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.LCylinder('0');
            }
            break;
        case 'raxis':
            if (model[0].KeyStr === '0') {
                model[0].KeyStr = '-1';
            }
            self.RAxisList(model);
            self.RAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.RAxis('0');
            }
            break;
        case 'laxis':
            if (model[0].KeyStr === '0') {
                model[0].KeyStr = '-1';
            }
            self.LAxisList(model);
            self.LAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.LAxis('0');
            }
            break;
        case 'radd':
            self.RAddList(model);
            self.RAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.RAdd('0');
            }
            break;
        case 'ladd':
            self.LAddList(model);
            self.LAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
            if (model.length > 2) {
                self.LAdd('0');
            }
            break;
        default:
            break;
        }
    };
};

function initRequireFields() {
    if (viewModel.RCylinders() !== null && viewModel.RCylinders().length > 1 && viewModel.RuCondition() !== 64) {
        $('#rcylinderstar').show();
    } else {
        $('#rcylinderstar').hide();
    }
    if (viewModel.RAxisList() !== null && viewModel.RAxisList().length > 1 && viewModel.RuCondition() !== 64) {
        $('#raxisstar').show();
    } else {
        $('#raxisstar').hide();
    }
    if (viewModel.RAddList() !== null && viewModel.RAddList().length > 1 && viewModel.RuCondition() !== 64) {
        $('#raddstar').show();
    } else {
        $('#raddstar').hide();
    }
    if (viewModel.RColors() !== null && viewModel.RColors().length > 1) {
        $('#rcolorstar').show();
    } else {
        $('#rcolorstar').hide();
    }

    if (viewModel.LCylinders() !== null && viewModel.LCylinders().length > 1 && viewModel.LuCondition() !== 64) {
        $('#lcylinderstar').show();
    } else {
        $('#lcylinderstar').hide();
    }
    if (viewModel.LAxisList() !== null && viewModel.LAxisList().length > 1 && viewModel.LuCondition() !== 64) {
        $('#laxisstar').show();
    } else {
        $('#laxisstar').hide();
    }
    if (viewModel.LAddList() !== null && viewModel.LAddList().length > 1 && viewModel.LuCondition() !== 64) {
        $('#laddstar').show();
    } else {
        $('#laddstar').hide();
    }
    if (viewModel.LColors() !== null && viewModel.LColors().length > 1) {
        $('#lcolorstar').show();
    } else {
        $('#lcolorstar').hide();
    }

}

function clearViewModel(name, star) {
    if (name === '#r') {
        viewModel.RStar(star);
        viewModel.RStyles(null);
        viewModel.RLensparams(null);
        viewModel.RColors(null);
        viewModel.RSpheres(null);
        viewModel.RCylinders(null);
        viewModel.RAxisList(null);
        viewModel.RAddList(null);
        viewModel.RBaseTxt('');
        viewModel.RDiameterTxt('');
        viewModel.RQuantityTxt('');
    } else {
        viewModel.LStar(star);
        viewModel.LStyles(null);
        viewModel.LLensparams(null);
        viewModel.LColors(null);
        viewModel.LSpheres(null);
        viewModel.LCylinders(null);
        viewModel.LAxisList(null);
        viewModel.LAddList(null);
        viewModel.LBaseTxt('');
        viewModel.LDiameterTxt('');
        viewModel.LQuantityTxt('');
    }
}

function disableInputs(name) {
    clearViewModel(name, '');
    $(name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'color,' + name + 'add,' + name + 'sphere' + name + 'cylinder' + name + 'axis').parents("div.bootstrap-select").removeClass('requiredField');
    $(name + 'supplier,' + name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' +
        name + 'color,' + name + 'sphere,' + name + 'cylinder,' + name + 'axis,' + name + 'add')
        .each(function () {
            $(this).attr("disabled", "disabled");
            if (('#' + this.name !== name + 'manufacturer') && ('#' + this.name !== name + 'supplier')) {
                $(this).html('');
            } else {
                if (this.name === 'rmanufacturer') {
                    viewModel.RManufacturer(0);
                }
                if (this.name === 'lmanufacturer') {
                    viewModel.LManufacturer(0);
                }
                if (this.name === 'rsupplier') {
                    viewModel.RSupplier(0);
                }
                if (this.name === 'lsupplier') {
                    viewModel.LSupplier(0);
                }
            }
            $(this).recreateSelectPickerDefaultText('N/A');
            $(this).clearField();
        });
    $(name + 'quantity').clearField();
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').val('N/A');
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').attr("disabled", "disabled");
    $(name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'sphere')
        .parents("div.bootstrap-select").removeClass('requiredField');
    $(name + 'cylinderstar').hide();
    $(name + 'axisstar').hide();
    $(name + 'addstar').hide();
    $(name + 'colorstar').hide();
}

function disableInputsForPlano(name) {
    initLoad = true;
    clearViewModel(name, '*');
    $(name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'sphere').parents("div.bootstrap-select").addClass('requiredField');
    $(name + 'supplier,' + name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'color,' + name + 'add,' + name + 'sphere').each(function () {
        $(this).removeAttr("disabled");
        $(this).clearField();
        if (('#' + this.name !== name + 'manufacturer') && ('#' + this.name !== name + 'supplier')) {
            $(this).html('');
        } else {
            if (this.name === 'rmanufacturer') {
                viewModel.RManufacturer(0);
            }
            if (this.name === 'lmanufacturer') {
                viewModel.LManufacturer(0);
            }
            if (this.name === 'rsupplier') {
                viewModel.RSupplier(0);
            }
            if (this.name === 'lsupplier') {
                viewModel.LSupplier(0);
            }
        }
        $(this).refreshSelectPicker();
    });
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').removeAttr("disabled");
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').val('');
    $(name + 'add,' + name + 'cylinder,' + name + 'axis').each(function () {
        $(this).attr("disabled", "disabled");
        $(this).recreateSelectPickerDefaultText('N/A');
    });
    $(name + 'quantity').clearField();
    $(name + 'cylinderstar').hide();
    $(name + 'axisstar').hide();
    $(name + 'addstar').hide();
    $(name + 'colorstar').hide();
    initLoad = false;
}

function disableInputsForSelect(name) {
    clearViewModel(name, '*');
    $(name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'sphere').parents('div.bootstrap-select').addClass('requiredField');
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').val('');
    $(name + 'supplier,' + name + 'manufacturer,' + name + 'style,' + name + 'lensparam,' + name + 'color,' + name + 'sphere,' + name + 'cylinder,' + name + 'axis,' + name + 'add')
        .each(function () {
            $(this).removeAttr("disabled");
            if (initLoad === false) {
                if (('#' + this.name !== name + 'manufacturer') && ('#' + this.name !== name + 'supplier')) {
                    $(this).html('');
                } else {
                    if (this.name === 'rmanufacturer') {
                        viewModel.RManufacturer(0);
                    }
                    if (this.name === 'lmanufacturer') {
                        viewModel.LManufacturer(0);
                    }
                    if (this.name === 'rsupplier') {
                        viewModel.RSupplier(0);
                    }
                    if (this.name === 'lsupplier') {
                        viewModel.LSupplier(0);
                    }
                }
                $(this).recreateSelectPickerDefaultText('Select');
                $(this).clearField();
            }
        });
    $(name + 'basecurve,' + name + 'diameter,' + name + 'quantity').removeAttr("disabled");
    $(name + 'quantity').clearField();
}


function initializeSelectedOptions() {

    initLoad = false;
    // check for balance lens, no lens, not recorded, prothesis
    var array = ['60', '61', '62', '63'];

    if (array.indexOf(viewModel.RuCondition().toString()) >= 0) {
        disableInputs(lens.RIGHT);
    }

    if (array.indexOf(viewModel.LuCondition().toString()) >= 0) {
        disableInputs(lens.LEFT);
    }

    // check for plano

    if (viewModel.RuCondition() === 64) {
        $('#radd, #rcylinder, #raxis').each(function () {
            $(this).attr("disabled", "disabled");
            $(this).refreshSelectPicker();
        });
    }
    if (viewModel.LuCondition() === 64) {
        $('#ladd, #lcylinder, #laxis').each(function () {
            $(this).attr("disabled", "disabled");
            $(this).refreshSelectPicker();
        });
    }
    initRequireFields();
}

function setSelectBoxOptions() {
    $('select').not("#rsupplier, #lsupplier, #wearschedule, #replenishment, #dfregimen, #rucondition, #lucondition").each(function () {
        $(this).recreateSelectPicker();
    });
    $('#rsupplier, #lsupplier, #lucondition, #rucondition, #wearschedule, #replenishment, #dfregimen').each(function () {
        $(this).recreateSelectPickerShowFirstOption();
    });

    if (edit === 'Edit') {
        initializeSelectedOptions();
        $('select').parents("div.bootstrap-select").removeClass('requiredField');
    } else {
        $('#rcolorstar, #lcolorstar, #rcylinderstar, #lcylinderstar, #raxisstar, #laxisstar, #raddstar, #laddstar').hide();
    }
}

$("#radioOption3, #radioOption4").click(function (e) {
    switch (this.id) {
    case "radioOption3":
        $("#ProviderDiv").show();
        $("#OutsideProviderDiv").hide();
        viewModel.providerType(0);
        if (viewModel.provider() === 0) {
            $('select#provider').parents("div.bootstrap-select").addClass('requiredField');
        }
        break;
    case "radioOption4":
        $("#OutsideProviderDiv").show();
        $("#ProviderDiv").hide();
        viewModel.providerType(1);
        break;
    }
});


function isValidForm() {
    var valid = true;
    if ($("#radioOption4").prop("checked") && viewModel.outsideProviderId() === 0) {
        $("#outsider").showFieldMessage(msgType.ERROR, "Select the prescribing Outside Provider.");
        valid = false;
    }
    if ($("#ContactLensRxForm").valid() && valid) {
        return true;
    }

    $(this).scrollToPosition(null);
    return false;
}

function updateCLOutsideProvider(name, id) {
    viewModel.outsideProviderId(id);
    viewModel.outsideProviderName(name);
    $("#outsider").clearField();
}

$('#outsideProvider').click(function (e) {
    e.preventDefault();
    $("#outsideProviderDialog").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
    var mode = $("#outsideProvider").attr('data-text');
    if (mode === '') {
        $("#outsideProvider").attr('data-text', 'ClearFields');
    }
    window.setupOutsideProviderFields(mode, updateCLOutsideProvider);
});

function refreshViewModel(data) {
    initLoad = true;
    if (edit !== 'Edit') {
        //$('input').val('');
        $('#expireDatePicker').val(data.ExpirationDate);
        $('#examDatePicker').val(data.ExamDate);
        $('#expireDatePicker').clearField();
        $('#examDatePicker').clearField();
        $('#rcolorstar, #lcolorstar, #rcylinderstar, #lcylinderstar, #raxisstar, #laxisstar, #raddstar, #laddstar').hide();
    }
    $('#rxnote').clearField();
    viewModel.RxNote('');
    viewModel.providers(data.Providers);
    viewModel.providers.unshift({ "Key": 0, "Description": 'Select' });
    viewModel.provider(data.Provider);
    viewModel.outsideProviderName(data.OutsideProviderName);
    viewModel.outsideProviderId(data.OutsideProviderId);
    viewModel.providerType(data.ProviderType);
    setDoctorDdls(data);

    viewModel.lenstypes(data.ContactLensRxType);
    viewModel.lenstypes.unshift({ "Key": 0, "Description": 'Select' });
    viewModel.lenstype(data.LensRxTypeId);

    viewModel.WearSchedules(data.WearScheduleList);
    viewModel.WearSchedules.unshift({ "Key": 0, "Description": 'Select' });
    if (data.WearScheduleId !== null) {
        viewModel.WearSchedule(data.WearScheduleId);
    } else {
        viewModel.WearSchedule(0);
    }

    viewModel.Replenishments(data.ReplenishmentList);
    viewModel.Replenishments.unshift({ "Key": 0, "Description": 'Select' });
    if (data.ReplenishmentId !== null) {
        viewModel.Replenishment(data.ReplenishmentId);
    } else {
        viewModel.Replenishment(0);
    }

    viewModel.DFRegimens(data.DisinfectingRegimenList);
    viewModel.DFRegimens.unshift({ "Key": 0, "Description": 'Select' });
    if (data.DisinfectingRegimenId !== null) {
        viewModel.DFRegimen(data.DisinfectingRegimenId);
    } else {
        viewModel.DFRegimen(0);
    }
    viewModel.RuConditions(data.UnderlyingConditionList);
    viewModel.RuConditions.unshift({ "Key": 0, "Description": 'Select' });
    if (data.RightUlConditionId !== null) {
        viewModel.RuCondition(data.RightUlConditionId);
    } else {
        viewModel.RuCondition(0);
    }

    viewModel.LuConditions(data.UnderlyingConditionList);
    if (data.LeftUlConditionId !== null) {
        viewModel.LuCondition(data.LeftUlConditionId);
    } else {
        viewModel.LuCondition(0);
    }

    if (data.Suppliers !== null) {
        viewModel.RSuppliers(data.Suppliers);
    } else {
        viewModel.RSuppliers(null);
    }
    viewModel.RSuppliers.unshift({ "Key": 0, "Description": 'Select' });
    if (data.RightSupplierId !== null) {
        viewModel.RSupplier(data.RightSupplierId);
    } else {
        viewModel.RSupplier(0);
    }
    viewModel.LSuppliers(data.Suppliers);
    if (data.LeftSupplierId !== null) {
        viewModel.LSupplier(data.LeftSupplierId);
    } else {
        viewModel.LSupplier(0);
    }
    viewModel.RManufacturers(data.Manufacturers);
    viewModel.RManufacturers.unshift({ "KeyStr": '0', "Description": '' });
    if (data.RightManufacturerId !== null) {
        viewModel.RManufacturer(data.RightManufacturerId);
    } else {
        viewModel.RManufacturer(0);
    }

    viewModel.LManufacturers(data.Manufacturers);
    if (data.LeftManufacturerId !== null) {
        viewModel.LManufacturer(data.LeftManufacturerId);
    } else {
        viewModel.LManufacturer(0);
    }

    if (data.RightStyleList !== null) {
        viewModel.RStyles(data.RightStyleList);
        viewModel.RStyles.unshift({ "Key": 0, "Description": 'Select' });
    } else {
        viewModel.RStyles(null);
    }
    viewModel.RStyle(data.RightStyleId);

    if (data.LeftStyleList !== null) {
        viewModel.LStyles(data.LeftStyleList);
        viewModel.LStyles.unshift({ "Key": 0, "Description": 'Select' });
    } else {
        viewModel.LStyles(null);
    }
    viewModel.LStyle(data.LeftStyleId);

    if (data.RightLensParamsList !== null) {
        viewModel.RLensparams(data.RightLensParamsList);
        viewModel.RLensparams.unshift({ "Key": 0, "Description": 'Select' });
    } else {
        viewModel.RLensparams(null);
    }
    viewModel.RLensparam = ko.observable(data.RightLensPowerId);

    if (data.LeftLensParamsList !== null) {
        viewModel.LLensparams(data.LeftLensParamsList);
        viewModel.LLensparams.unshift({ "Key": 0, "Description": 'Select' });
    } else {
        viewModel.LLensparams(null);
    }
    viewModel.LLensparam(data.LeftLensPowerId);

    if (data.RightColorList !== null) {
        viewModel.RColors(data.RightColorList);
        viewModel.RColors.unshift({ "KeyStr": '', "Description": 'Select' });
    } else {
        viewModel.RColors(null);
    }
    viewModel.RColor(data.RightLensColorId);

    if (data.LeftColorList !== null) {
        viewModel.LColors(data.LeftColorList);
        viewModel.LColors.unshift({ "KeyStr": '', "Description": 'Select' });
    } else {
        viewModel.LColors(null);
    }
    viewModel.LColor(data.LeftLensColorId);

    if (data.RightSphereList !== null) {
        if (data.RightUlConditionId === '64') {
            viewModel.RSpheres([{ "KeyStr": '0', "Description": 'Select' }, { "KeyStr": '0.00', "Description": '0.00' }]);
            data.RightSphereId = '0.00';
        } else {
            viewModel.RSpheres(data.RightSphereList);
            viewModel.RSpheres.unshift({ "KeyStr": '0', "Description": 'Select' });
        }
    } else {
        viewModel.RSpheres(null);
    }
    viewModel.RSphere(data.RightSphereId);

    if (data.LeftSphereList !== null) {
        if (data.LeftUlConditionId === '64') {
            viewModel.LSpheres([{ "KeyStr": '0', "Description": 'Select' }, { "KeyStr": '0.00', "Description": '0.00' }]);
            data.LeftSphereId = '0.00';
        } else {
            viewModel.LSpheres(data.LeftSphereList);
            viewModel.LSpheres.unshift({ "KeyStr": '0', "Description": 'Select' });
        }
    } else {
        viewModel.LSpheres(null);
    }
    viewModel.LSphere(data.LeftSphereId);

    if (data.RightCylinderList !== null) {
        viewModel.RCylinders(data.RightCylinderList);
        viewModel.RCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.RCylinders(null);
    }
    viewModel.RCylinder = ko.observable(data.RightCylinderId);

    if (data.LeftCylinderList !== null) {
        viewModel.LCylinders(data.LeftCylinderList);
        viewModel.LCylinders.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.LCylinders(null);
    }
    viewModel.LCylinder(data.LeftCylinderId);

    if (data.RightAxisList !== null) {
        if (data.RightAxisList.length > 0 && data.RightAxisList[0].KeyStr === '0') {
            data.RightAxisList[0].KeyStr = '-1';
        }
        viewModel.RAxisList(data.RightAxisList);
        viewModel.RAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.RAxisList(null);
    }
    if (data.RightAxisId === '0') {
        data.RightAxisId = '-1';
    }
    viewModel.RAxis(data.RightAxisId);

    if (data.LeftAxisList !== null) {
        if (data.LeftAxisList.length > 0 && data.LeftAxisList[0].KeyStr === '0') {
            data.LeftAxisList[0].KeyStr = '-1';
        }
        viewModel.LAxisList(data.LeftAxisList);
        viewModel.LAxisList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.LAxisList(null);
    }
    if (data.LeftAxisId === '0') {
        data.LeftAxisId = '-1';
    }
    viewModel.LAxis(data.LeftAxisId);

    if (data.RightAddList !== null) {
        viewModel.RAddList(data.RightAddList);
        viewModel.RAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.RAddList(null);
    }
    viewModel.RAdd(data.RightAddId);

    if (data.LeftAddList !== null) {
        viewModel.LAddList(data.LeftAddList);
        viewModel.LAddList.unshift({ "KeyStr": '0', "Description": 'Select' });
    } else {
        viewModel.LAddList(null);
    }
    viewModel.LAdd(data.LeftAddId);
    if (edit === 'Edit') {
        $('select').not('#rsupplier, #lsupplier, #lucondition, #rucondition, #wearschedule, #replenishment, #dfregimen').each(function () {
            $(this).recreateSelectPicker();
            $(this).clearField();
            $('select#' + this.name).parents("div.bootstrap-select").removeClass('requiredField');
            $('select#' + this.name).removeClass('requiredField');
        });
        $('#rsupplier, #lsupplier, #lucondition, #rucondition, #wearschedule, #replenishment, #dfregimen').each(function () {
            $(this).recreateSelectPickerShowFirstOption();
        });
        initializeSelectedOptions();
    } else {
        $('#rsupplier, #lsupplier, #lucondition, #rucondition, #wearschedule, #replenishment, #dfregimen').each(function () {
            $('#' + this.name).clearField();
            $('#' + this.name).recreateSelectPickerShowFirstOption();
        });
        $('select').not('#rsupplier, #lsupplier, #lucondition, #rucondition, #wearschedule, #replenishment, #dfregimen').each(function () {
            $('#' + this.name).clearField();
            $('#' + this.name).refreshSelectPicker();
            if (this.name === 'rmanufacturer' || this.name === 'lmanufacturer' || this.name === 'rstyle' || this.name === 'lstyle' ||
                    this.name === 'rlensparam' || this.name === 'llensparam' || this.name === 'rsphere' || this.name === 'lsphere' ||
                    this.name === 'provider' || this.name === 'outsider' || this.name === 'lenstype') {
                $('#' + this.name).parents("div.bootstrap-select").addClass('requiredField');
            } else {
                $('#' + this.name).parents("div.bootstrap-select").removeClass('requiredField');
            }
        });
    }
    $('#rquantity, #lquantity').removeClass('error');
    $('#rquantity').clearField();
    $('#lquantity').clearField();

    var array = ['60', '61', '62', '63'];
    if (array.indexOf(viewModel.RuCondition().toString()) >= 0) {
        viewModel.RStar('');
    } else {
        //$('#rquantity').addClass('requiredField');
        viewModel.RStar(data.Star);
    }
    if (array.indexOf(viewModel.LuCondition().toString()) >= 0) {
        viewModel.LStar('');
    } else {
        //$('#lquantity').addClass('requiredField');
        viewModel.LStar(data.Star);
    }
    viewModel.RBaseTxt(data.RightBaseTxt || '');
    viewModel.RDiameterTxt(data.RightDiameterTxt || '');
    viewModel.RQuantityTxt(data.RightMaxQuantityTxt || '');
    viewModel.LBaseTxt(data.LeftBaseTxt || '');
    viewModel.LDiameterTxt(data.LeftDiameterTxt || '');
    viewModel.LQuantityTxt(data.LeftMaxQuantityTxt || '');
    viewModel.examDate(data.ExamDate);
    viewModel.expirationDate(data.ExpirationDate);

    if (data.Notes !== null) {
        viewModel.RxNote(data.Notes[0].Description);
    } else {
        viewModel.RxNote();
    }

    initLoad = false;
    viewModel.dirtyFlag.reset();
    $('#provider').parents("div.bootstrap-select").removeClass('requiredField');
}

var copyToLeftLens = function () {
    initLoad = true;
    $('input, select').removeAttr("disabled");
    $('select#lmanufacturer, select#lstyle, select#llensparam, select#lcolor, select#lsphere, select#lcylinder, select#laxis, select#ladd')
        .parents("div.bootstrap-select").removeClass('requiredField');

    $('select#lmanufacturer, select#lsphere, select#ladd, select#llensparam').removeClass('requiredField');
    clearViewModel('#l', '*');

    viewModel.LSuppliers(viewModel.RSuppliers());
    viewModel.LSupplier(viewModel.RSupplier());
    $('#lsupplier').recreateSelectPickerShowFirstOption();

    viewModel.LManufacturer(viewModel.RManufacturer());
    $('#lmanufacturer').recreateSelectPicker();

    viewModel.LuCondition(viewModel.RuCondition());
    $('#lucondition').selectpicker('refresh');

    viewModel.LStyles(viewModel.RStyles());
    viewModel.LStyle(viewModel.RStyle());
    $('#lstyle').recreateSelectPicker();

    viewModel.LLensparams(viewModel.RLensparams());
    viewModel.LLensparam(viewModel.RLensparam());
    $('#llensparam').recreateSelectPicker();

    viewModel.LColors(viewModel.RColors());
    viewModel.LColor(viewModel.RColor());
    $('#lcolor').recreateSelectPicker();

    viewModel.LSpheres(viewModel.RSpheres());
    viewModel.LSphere(viewModel.RSphere());
    $('#lsphere').recreateSelectPicker();

    viewModel.LCylinders(viewModel.RCylinders());
    viewModel.LCylinder(viewModel.RCylinder());
    $('#lcylinder').recreateSelectPicker();

    viewModel.LAxisList(viewModel.RAxisList());
    viewModel.LAxis(viewModel.RAxis());
    $('#laxis').recreateSelectPicker();

    viewModel.LAddList(viewModel.RAddList());
    viewModel.LAdd(viewModel.RAdd());
    $('#ladd').recreateSelectPicker();

    $('#rmanufacturer, #rstyle, #rlensparam, #rcolor, #rsphere, #rcylinder, #raxis, #radd').each(function () {
        if ($('#' + this.name).parents("div.bootstrap-select").hasClass('requiredField') || $('#' + this.name).parents("div.bootstrap-select").hasClass('error')) {
            $('#' + this.name.replace('r', 'l')).next('div.bootstrap-select').addClass('requiredField');
        }
        $('#' + this.name.replace('r', 'l')).clearField();
    });

    if (viewModel.LuCondition() === 64) { //plano
        $('#lbasecurve, #ldiameter, #lquantity').removeAttr("disabled");
        $('#ladd, #lcylinder, #laxis').each(function () {
            $(this).attr("disabled", "disabled");
            $(this).refreshSelectPicker();
        });
    }

    if (!($('#rquantity').hasClass('requiredField') || $('#rquantity').hasClass('error') || $('#rquantity').val() === '')) {
        $('#lquantity').removeClass('error');
        $('#lquantity').clearField();
    }

    $('#lbasecurve, #ldiameter, #lquantity').val('');
    viewModel.LBaseTxt(viewModel.RBaseTxt());
    viewModel.LDiameterTxt(viewModel.RDiameterTxt());
    viewModel.LQuantityTxt(viewModel.RQuantityTxt());

    initRequireFields();
    initLoad = false;
};

$.validator.addMethod("ruconditioncheck", function (value) {
    switch (parseInt(value, 10)) {
    case underlyingType.NO_LENS:
    case underlyingType.NOT_RECORDED:
    case underlyingType.PROSTHESIS:
    case underlyingType.BALANCE_LENS:
        var array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.BALANCE_LENS];
        var val = parseInt(viewModel.LuCondition(), 10);
        if (array.indexOf(val) >= 0) {
            return false;
        }
        break;
    default:
        break;
    }
    return true;
});

function validatePatientContactLensInfo() {
    $.validator.addMethod(
        "validateExpiredDate",
        function (value, element) {
            if (element.value !== '') {
                if ($("#examDatePicker").val() !== '' && $("#expireDatePicker").val() !== 'mm/dd/yyyy') {
                    if (getDateDiff($("#examDatePicker").val(), $("#expireDatePicker").val(), "days") < 0) {
                        return false;
                    }
                }
            } else {
                return false;
            }
            return true;
        }
    );
    $("#ContactLensRxForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            provider: {
                selectBox: true
            },
            outsider: {
                selectBox: true
            },
            lenstype: {
                selectBox: true
            },
            rmanufacturer: {
                selectBox: true
            },
            lmanufacturer: {
                selectBox: true
            },
            rstyle: {
                selectBox: true
            },
            lstyle: {
                selectBox: true
            },
            rlensparam: {
                selectBox: true
            },
            llensparam: {
                selectBox: true
            },
            rsphere: {
                selectBox: true
            },
            lsphere: {
                selectBox: true
            },
            rquantity: {
                //required: true,
                min: 1
            },
            lquantity: {
                //required: true,
                min: 1
            },
            rcylinder: {
                selectBox1: true
            },
            lcylinder: {
                selectBox1: true
            },
            raxis: {
                selectBox1: true
            },
            laxis: {
                selectBox1: true
            },
            radd: {
                selectBox1: true
            },
            ladd: {
                selectBox1: true
            },
            rcolor: {
                selectBox1: true
            },
            lcolor: {
                selectBox1: true
            },
            rucondition: {
                ruconditioncheck: true
            },
            examDatePicker: {
                required: true,
                commonDate: true,
                notFutureDate: true
            },
            expireDatePicker: {
                required: true,
                commonDate: true,
                validateExpiredDate: true
            },
            rxnote: {
                maxlength: 2048
            }
        },
        messages: {
            provider: {
                selectBox: "Select the prescribing Provider."
            },
            outsider: {
                selectBox: "Select the prescribing Outside Provider."
            },
            lenstype: {
                selectBox: 'Select the Lens Rx Type.'
            },
            rmanufacturer: {
                selectBox: 'Select the Manufacturer.'
            },
            lmanufacturer: {
                selectBox: 'Select the Manufacturer.'
            },
            rstyle: {
                selectBox: 'Select the Style.'
            },
            lstyle: {
                selectBox: 'Select the Style.'
            },
            rlensparam: {
                selectBox: 'Select the Lens Parameters.'
            },
            llensparam: {
                selectBox: 'Select the Lens Parameters.'
            },
            rsphere: {
                selectBox: 'Select the Sphere.'
            },
            lsphere: {
                selectBox: 'Select the Sphere.'
            },
            rquantity: {
                //required: 'Enter a valid Quantity.',
                min: 'Enter a quantity value greater than or equal to 1.'
            },
            lquantity: {
                //required: 'Enter a valid Quantity.',
                min: 'Enter a quantity value greater than or equal to 1.'
            },
            rcylinder: {
                selectBox1: 'Select the Cylinder.'
            },
            lcylinder: {
                selectBox1: 'Select the Cylinder.'
            },
            raxis: {
                selectBox1: 'Select the Axis.'
            },
            laxis: {
                selectBox1: 'Select the Axis.'
            },
            radd: {
                selectBox1: 'Select the Add Power.'
            },
            ladd: {
                selectBox1: 'Select the Add Power.'
            },
            rcolor: {
                selectBox1: 'Select the Color.'
            },
            lcolor: {
                selectBox1: 'Select the Color.'
            },
            rucondition: {
                ruconditioncheck: "You cannot select a Balanced Lens, No Lens, Not Recorded, and/or Prosthesis underlying condition for both the right and left lens."
            },
            examDatePicker: {
                required: "Enter the Exam Date.",
                commonDate: "Enter a valid Exam Date.",
                notFutureDate: "Exam Date cannot be a future date."
            },
            expireDatePicker: {
                required: "Enter the Rx Expiration Date.",
                commonDate: "Enter a valid Rx Expiration Date.",
                validateExpiredDate: "Expiration Date must be later than Exam Date."
            },
            rxnote: {
                maxlength: "Note can only be 2048 characters long."
            }
        }
    });
}

// Date picker events
$(function () {
    $('#examDatePicker').datepicker({
        constrainInput: true,
        maxDate: 0
    });
    $('#expireDatePicker').datepicker({
        constrainInput: true
    });
});

$("#examDatePicker").click(function () {
    $("#examDatePicker").focus();
});

$("expireDatePicker").click(function () {
    $("#expireDatePicker").focus();
});

$('#examDatePicker, #expireDatePicker').change(function () {
    var date;
    var valid = $(this).valid();

    if (valid) {
        switch (this.name) {
        case 'examDatePicker':
            if ($(this).val() !== "" && commonDate($(this).val(), $('#examDatePicker'))) {
                date = new Date($('#examDatePicker').val());
                var strDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 1);
                viewModel.expirationDate(strDate);
                $('#expireDatePicker').clearField();
                $(this).clearField();
            }
            break;
        case 'expireDatePicker':
            var startDate;
            if ($(this).val().length === 0) { // if expiration date is empty , set to exam Date
                if ($('#examDatePicker').val() !== "") {
                    date = new Date($('#examDatePicker').val());
                    startDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 1);
                    viewModel.ExpirationDate(startDate);
                    $('#expireDatePicker').clearField();
                }
            } else { // if expiration date is not empty, check against exam date, if it is before exam, set to 1 years exam date
                date = Date.parse($(this).val());
                setTimeout(function () {
                    startDate = Date.parse($('#examDatePicker').val());
                    if (date < startDate) {
                        startDate = (startDate.getMonth() + 1) + "/" + startDate.getDate() + "/" + (startDate.getFullYear() + 1);
                        viewModel.ExpirationDate(startDate);
                        $('#expireDatePicker').clearField();
                    }
                }, 100);
            }
            break;
        default:
            break;
        }
    }
});

var goToRxPage = function () {
    var redirectUrl = window.config.baseUrl + "Patient/Rx?id=" + window.patient.PatientId;
    window.location.href = redirectUrl;
};

function saveNote(data) {
    var noteData, action, reqmode;
    if ($("#rxnote").val() !== "") {
        if (edit === "Edit" && $("#noteIdHidden").val() !== "") {
            action = "UpdateNote";
            reqmode = "put";
            noteData = {
                PatientId: window.patient.PatientId,
                EntityId: "",
                NoteId: $('#noteIdHidden').val(),
                ResourceId: window.config.userId,
                Resource: "",
                Date: "",
                Note: $("#rxnote").val(),
                NoteType: "Exam",
                OfficeId: window.config.officeNumber
            };
        } else {
            action = "AddNote";
            reqmode = "post";
            noteData = {
                PatientId: window.patient.PatientId,
                EntityId: data,
                NoteId: "",
                ResourceId: window.config.userId,
                Resource: "",
                Date: "",
                Note: $("#rxnote").val(),
                NoteType: "Exam",
                OfficeId: window.config.officeNumber
            };
        }
        // save the note
        new ApiClient("PatientNotes")
            .action(action)[reqmode](noteData)
            .done(function () {
                console.WriteLine("Done");
            });
    }
}

var saveExam = function () {
    var contactLensRxData = {
        examId: rxExamId,
        UserId: window.config.userId,
        PatientId: window.patient.PatientId,
        ExamRxTypeId: examRxTypeId === null ? window.CLRxType.EXAM_TYPE_CONTACTLENS : examRxTypeId,
        Provider: viewModel.provider(),
        OutsideProviderId: viewModel.outsideProviderId(),
        ProviderType: viewModel.providerType(),
        LensRxTypeId: viewModel.lenstype(),
        ExamDate: viewModel.examDate(),
        ExpirationDate: viewModel.expirationDate(),
        RightSupplierId: viewModel.RSupplier(),
        LeftSupplierId: viewModel.LSupplier(),
        RightSphereTxt: $('#rsphere option:selected').text(),
        RightCylinderTxt: $('#rcylinder option:selected').text(),
        RightAxisTxt: $('#raxis option:selected').text(),
        RightAddTxt: $('#radd option:selected').text(),
        RightBaseTxt: $("#rbasecurve").val(),
        RightDiameterTxt: $("#rdiameter").val(),
        RightMaxQuantityTxt: $("#rquantity").val(),
        LeftSphereTxt: $('#lsphere option:selected').text(),
        LeftCylinderTxt: $('#lcylinder option:selected').text(),
        LeftAxisTxt: $('#laxis option:selected').text(),
        LeftAddTxt: $('#ladd option:selected').text(),
        LeftBaseTxt: viewModel.LBaseTxt(),
        LeftDiameterTxt: viewModel.LDiameterTxt(),
        LeftMaxQuantityTxt: viewModel.LQuantityTxt(),
        RightLensPowerId: viewModel.RLensparam(),
        RightLensColorId: viewModel.RColor(),
        LeftLensPowerId: viewModel.LLensparam(),
        LeftLensColorId: viewModel.LColor(),
        LeftUlConditionId: viewModel.LuCondition(),
        RightUlConditionId: viewModel.RuCondition(),
        WearScheduleId: viewModel.WearSchedule(),
        DisinfectingRegimenId: viewModel.DFRegimen(),
        ReplenishmentId: viewModel.Replenishment(),
        HasBeenRechecked: isRecheck,
        RecheckExamId: isRecheck ? rxExamId : 0
    };

    //save exam details
    client
        .action("Put")
        .queryStringParams({
            "officeNumber": window.config.officeNumber
        })
        .put(contactLensRxData)
        .done(function (data) {
            if (data.length !== 0 && data.indexOf('is not supplied by supplier') > 0) {
                $(this).showSummaryMessage(msgType.ERROR, data, true);
                window.scroll(0, 0);
            } else {
                saveNote(data);
                $('#confirmationDialog').modal('hide');
                //var msg = (edit === 'Edit') ? 'saved.' : 'saved.';
                $(this).showSystemSuccess('Contact Lens Rx saved.');
                setTimeout(goToRxPage, 1000);
            }
        });
};

var hasUnlikeSignsDialog = function (msg) {
    $('#btnIgnoreConfirmation').show();
    if (msg !== null && msg.length > 0) {
        $('#btnIgnoreConfirmation').hide();
        $('#confirmationDialog .modal-body').html(msg);
    }
    $('#confirmationDialog').modal({
        keyboard: false,
        backdrop: false,
        show: true
    });
};

var validateAndSaveExam = function () {
    if (((parseFloat($('#rsphere').val()) > 0) && (parseFloat($('#lsphere').val())) < 0) ||
            ((parseFloat($('#rsphere').val()) < 0) && (parseFloat($('#lsphere').val())) > 0) ||
            ((parseFloat($('#rcylinder').val()) > 0) && (parseFloat($('#lcylinder').val())) < 0) ||
            ((parseFloat($('#rcylinder').val()) < 0) && (parseFloat($('#lcylinder').val())) > 0)) {
        hasUnlikeSignsDialog(null);
    } else {
        saveExam();
        viewModel.dirtyFlag.reset();
    }
};

function getDataByCriteria(len, name, data, selectName) {
    initLoad = true;
    var error, ucondition;

    if ($('#' + len + name).hasClass("error")) {
        error = 1;
    } else {
        error = 0;
    }
    viewModel.importModel(data, len + name);
    $('select#' + len + name).recreateSelectPicker();
    if (error === 1) {
        $('#' + len + name).addClass("error");
    } else {
        if (name === 'cylinder' || name === 'axis') {
            if (data.length > 1) {
                $('#' + len + name).parents("div.bootstrap-select").addClass("requiredField");
                $('#' + len + name + 'star').show();
            }
        } else {
            $('#' + len + name).parents("div.bootstrap-select").addClass("requiredField");
        }
    }

    if (data.length === 2) {
        $('#' + len + name).next('div.bootstrap-select').removeClass("error");
        $('#' + len + name).next('div.bootstrap-select').removeClass("requiredField");
        $('#' + len + name).clearField();
        $('#' + len + name + 'star').show();
        initLoad = false;
    }
    $('select#' + len + name).change();
    if (len === 'r') {
        ucondition = viewModel.RuCondition();
        if (selectName === len + 'manufacturer') {
            viewModel.RLensparams(null);
        }
        if (selectName === len + 'manufacturer' || selectName === len + 'style') {
            viewModel.RColors(null);
        }
        if (selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam') {
            viewModel.RSpheres(null);
        }
        if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' || selectName === len + 'color') && ucondition !== 64) {
            viewModel.RCylinders(null);
        }
        if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' || selectName === len + 'color' || selectName === len + 'sphere') && ucondition !== 64) {
            viewModel.RAxisList(null);
        }
    } else {
        ucondition = viewModel.LuCondition();
        if (selectName === len + 'manufacturer') {
            viewModel.LLensparams(null);
        }
        if (selectName === len + 'manufacturer' || selectName === len + 'style') {
            viewModel.LColors(null);
        }
        if (selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam') {
            viewModel.LSpheres(null);
        }
        if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' || selectName === len + 'color') && ucondition !== 64) {
            viewModel.LCylinders(null);
        }
        if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' || selectName === len + 'color' || selectName === len + 'sphere') && ucondition !== 64) {
            viewModel.LAxisList(null);
        }
    }
    if (selectName === len + 'manufacturer') {
        if ($('#' + len + 'lensparam').hasClass("error")) {
            $('#' + len + 'lensparam').recreateSelectPicker();
            $('#' + len + 'lensparam').addClass("error");
        } else {
            $('#' + len + 'lensparam').recreateSelectPicker();
            $('#' + len + 'lensparam').addClass('requiredField');
        }
    }
    if (selectName === len + 'manufacturer' || selectName === len + 'style') {
        if ($('#' + len + 'color').hasClass("error")) {
            $('#' + len + 'color').recreateSelectPicker();
            $('#' + len + 'color').removeClass("requiredField").addClass("error");
        } else {
            $('#' + len + 'color').next('div.bootstrap-select').removeClass('requiredField');
            $('#' + len + 'color').recreateSelectPicker();
        }
    }
    if (selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam') {
        if ($('#' + len + 'sphere').hasClass("error")) {
            $('#' + len + 'sphere').addClass("error");
        } else {
            $('#' + len + 'sphere').addClass('requiredField');
        }
        $('#' + len + 'sphere').recreateSelectPicker();
    }
    if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' ||
        selectName === len + 'color') && ucondition !== 64) {
        if ($('#' + len + 'cylinder').next('div.bootstrap-select').hasClass("error")) {
            $('#' + len + 'cylinder').clearField();
            $('#' + len + 'cylinder').next('div.bootstrap-select').removeClass("error");
        }
        $('#' + len + 'cylinder').next('div.bootstrap-select').removeClass('requiredField');
        $('#' + len + 'cylinderstar').hide();
        $('#' + len + 'cylinder').recreateSelectPicker();
    }
    if ((selectName === len + 'manufacturer' || selectName === len + 'style' || selectName === len + 'lensparam' ||
        selectName === len + 'color' || selectName === len + 'sphere') && ucondition !== 64) {
        if ($('#' + len + 'axis').next('div.bootstrap-select').hasClass("error")) {
            $('#' + len + 'axis').clearField();
            $('#' + len + 'axis').next('div.bootstrap-select').removeClass("error");
        }
        $('#' + len + 'axis').next('div.bootstrap-select').removeClass('requiredField');
        $('#' + len + 'axisstar').hide();
        $('#' + len + 'axis').recreateSelectPicker();
    }

    if (selectName === len + 'color') {
        if ($('#' + len + 'add').next('div.bootstrap-select').hasClass("error")) {
            $('#' + len + 'add').removeClass("requiredField").addClass("error");
        }
        if (ucondition !== 64) {
            $('#' + len + 'add').recreateSelectPicker();
        }
    }

    if (name === 'sphere' && ucondition === 64) {
        $('#' + len + 'sphere').next('div.bootstrap-select').removeClass('requiredField');
        $('#' + len + 'sphere').next('div.bootstrap-select').removeClass('error');
        $('#' + len + 'sphere').clearField();
        $('#' + len + name).recreateSelectPicker();
    }
    if (viewModel.RColors() !== null && viewModel.RColors().length !== 0) {
        $('#rcolorstar').show();
    } else {
        $('#rcolorstar').hide();
    }
    if (viewModel.LColors() !== null && viewModel.LColors().length !== 0) {
        $('#lcolorstar').show();
    } else {
        $('#lcolorstar').hide();
    }
    initLoad = false;
}

//Change Events
$('#provider, #lenstype, #laxis, #raxis, #ladd, #radd').change(function () {
    if (initLoad === false) {
        $(this).clearField();
        $(this).parents("div.bootstrap-select").removeClass("requiredField");
    }
});

$('#lquantity, #rquantity').change(function () {
    if ($(this).val().length > 0 && !isNaN($(this).val())) {
        $(this).val(Math.round($(this).val()));
    }
    //$('#' + this.name).removeClass('requiredField');
    $(this).clearField();
});

$('#rmanufacturer, #lmanufacturer, #rstyle, #lstyle, #rlensparam, #llensparam, #rcolor, #lcolor, #lsphere, #rsphere, #rcylinder, #lcylinder').change(function () {
    if ($(this).val() !== '0' && initLoad === false) {
        $(this).clearField();
        $('#' + this.name).parents("div.bootstrap-select").removeClass('requiredField');
        var len = (this.name.indexOf('r') === 0) ? 'r' : 'l';
        var powerid, clcolorcode, cylinder, sphere, ucondition;
        switch (this.name) {
        case 'rmanufacturer':
        case 'lmanufacturer':
            client
                .action("GetStylesByManufacturer")
                .get({ "officeNumber": window.config.officeNumber, "manufacturerId": $(this).val(), "isHardLens": 'false' })
                .done(function (data) {
                    getDataByCriteria(len, "style", data, len + 'manufacturer');
                });
            break;
        case 'rstyle':
        case 'lstyle':
            client
                .action("GetPowersByStyleId")
                .get({ "styleId": $('#' + len + 'style').val() })
                .done(function (data) {
                    getDataByCriteria(len, "lensparam", data, len + 'style');
                    if (len === 'r') {
                        lensParam = $("#rlensparam option:selected").text();
                    } else {
                        lensParam = $("#llensparam option:selected").text();
                    }
                });
            break;
        case 'rlensparam':
        case 'llensparam':
            client
                .action("GetColorsByPowerId")
                .get({ "officeNumber": window.config.officeNumber, "powerId": $('#' + len + 'lensparam').val() })
                .done(function (data) {
                    getDataByCriteria(len, "color", data, len + 'lensparam');
                });
            break;
        case 'rcolor':
        case 'lcolor':
            if (this.name === 'rcolor') {
                powerid = viewModel.RLensparam();
                clcolorcode = $(this).val();
                len = 'r';
                ucondition = viewModel.RuCondition();
            } else {
                powerid = viewModel.LLensparam();
                clcolorcode = $(this).val();
                len = 'l';
                ucondition = viewModel.LuCondition();
            }
            // get spheres
            client
                .action("GetItemContactLensSphereByCriteria")
                .get({ "powerid": powerid, "clcolorcode": clcolorcode })
                .done(function (data) {
                    getDataByCriteria(len, "sphere", data, len + 'color');
                });
            if (ucondition !== 64) {     // no need to get power for  plano
                // get add powers
                client
                    .action("GetItemContactLensAddPowerByCriteria")
                    .get({ "powerid": powerid, "clcolorcode": clcolorcode })
                    .done(function (data) {
                        initLoad = true;
                        viewModel.importModel(data, len + 'add');
                        $('#' + len + 'add').parents("div.bootstrap-select").removeClass("requiredField");
                        $('select#' + len + 'add').recreateSelectPicker();
                        if (data.length > 1) {
                            $('#' + len + 'add').removeClass("error");
                            $('#' + len + 'add').parents("div.bootstrap-select").addClass("requiredField");
                            $('#' + len + 'addstar').show();
                            $('#' + len + 'add').clearField();
                        } else {
                            $('#' + len + 'addstar').hide();
                        }
                        initLoad = false;
                    });
            }
            //get basecurve and diameter values
            if (len === 'r') {
                if ($("#rlensparam option:selected").text() !== 'Select') {
                    viewModel.RBaseTxt($("#rlensparam option:selected").text().split(',')[0].slice(6));
                    viewModel.RDiameterTxt($("#rlensparam option:selected").text().split(',')[1].slice(10));
                } else {
                    viewModel.RBaseTxt(lensParam.split(',')[0].slice(6));
                    viewModel.RDiameterTxt(lensParam.split(',')[1].slice(10));
                }
            } else {
                if ($("#llensparam option:selected").text() !== 'Select') {
                    viewModel.LBaseTxt($('#llensparam option:selected').text().split(',')[0].slice(6));
                    viewModel.LDiameterTxt($('#llensparam option:selected').text().split(',')[1].slice(10));
                } else {
                    viewModel.LBaseTxt(lensParam.split(',')[0].slice(6));
                    viewModel.LDiameterTxt(lensParam.split(',')[1].slice(10));
                }
            }

            break;
        case 'rsphere':
        case 'lsphere':
            if (this.name === 'rsphere') {
                powerid = viewModel.RLensparam();
                clcolorcode = viewModel.RColor();
                ucondition = viewModel.RuCondition();
                sphere = $(this).val();
                len = 'r';
            } else {
                powerid = viewModel.LLensparam();
                clcolorcode = viewModel.LColor();
                sphere = $(this).val();
                ucondition = viewModel.LuCondition();
                len = 'l';
            }
            //get cylinder items
            if (ucondition !== 64) {
                client
                    .action("GetItemContactLensCylinderByCriteria")
                    .get({ "powerid": powerid, "clcolorcode": clcolorcode, "sphere": sphere })
                    .done(function (data) {
                        getDataByCriteria(len, "cylinder", data, len + 'sphere');
                    });
            }
            break;
        case 'rcylinder':
        case 'lcylinder':
            if (this.name === 'rcylinder') {
                powerid = viewModel.RLensparam();
                clcolorcode = viewModel.RColor();
                sphere = viewModel.RSphere();
                cylinder = $(this).val();
                len = 'r';
            } else {
                powerid = viewModel.LLensparam();
                clcolorcode = viewModel.LColor();
                sphere = viewModel.LSphere();
                cylinder = $(this).val();
                len = 'l';
            }
            //get axis items
            client
                .action("GetItemContactLensAxisByCriteria")
                .get({ "powerid": powerid, "clcolorcode": clcolorcode, "sphere": sphere, "cylinder": cylinder })
                .done(function (data) {
                    getDataByCriteria(len, "axis", data, len + 'cylinder');
                });
            break;
        default:
            break;
        }
    }
});

//Click Events
$('#hard').click(function () {
    var redirectUrl = window.config.baseUrl + "Patient/HardContactLenses?id=" + window.patient.PatientId + "&examId=-1" + "&isRecheck=false";
    window.location.href = redirectUrl;
});

// Save event
$('#btnSave').click(function (e) {
    e.preventDefault();
    if (!isValidForm()) {
        return;
    }
    validateAndSaveExam();
});

$("#rxnote").keyup(function (e) {
    //tab or shift tab key
    if (e.keyCode === 9 || e.keyCode === 16) {
        return;
    }
    if ($("#rxnote").val().length > 2048) {
        $("#rxnote").showFieldMessage(msgType.ERROR, ["Note can only be 2048 characters long."], true);
    } else {
        $("#rxnote").clearField();
    }
});

$("#rbtnCopy").click(function (e) {
    e.preventDefault();
    $(this).focus();
    var array = ['60', '61', '62', '63'];
    var val = viewModel.RuCondition().toString();
    if (array.indexOf(val) >= 0) {
        hasUnlikeSignsDialog(ulConditionErrorMsg);
        return;
    }
    copyToLeftLens();
});

$("#btnReset").click(function (e) {
    e.preventDefault();
    $(".summaryMessages").clearMsgBlock();
    client
        .action("GetContactLensRxById")
        .get({ "officeNumber": window.config.officeNumber, "patientId": window.patient.PatientId, "examId": rxExamId, "isRecheck": isRecheck})
        .done(function (data) {
            initLoad = true;
            if (data.Notes !== null) {
                $('#noteIdHidden').val(data.Notes[0].Key);
            }
            $('select, input').removeAttr("disabled");
            $('select, input').each(function () {
                $(this).clearField();
            });
            viewModel.RBaseTxt(null);
            viewModel.RDiameterTxt(null);
            viewModel.RQuantityTxt(null);
            viewModel.LBaseTxt(null);
            viewModel.LDiameterTxt(null);
            viewModel.LQuantityTxt(null);
            viewModel.WearSchedule(null);
            viewModel.Replenishment(null);
            viewModel.DFRegimen(null);
            refreshViewModel(data);
            initLoad = false;
        });
});

$('#btnIgnoreConfirmation').click(function () {
    saveExam();
    viewModel.dirtyFlag.reset();
});

/* click handler for View Diagnosis Codes link */
$("#linkEhrCodes").click(function (e) {
    e.preventDefault();
    $("#ehrCodesModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});

// ReSharper disable NotAllPathsReturnValue
window.onbeforeunload = function () {
    $("#btnSave").focus(); //ko needs to update entity, since user hits F5 
    if (viewModel.dirtyFlag.isDirty()) {
        showMsgTimer = setTimeout(function () {
            $("#soft").click();
        }, 1);
        return "You are trying to navigate to a new page, but you have not saved the data on this page.";
    }
};

window.onunload = function () {
    clearTimeout(showMsgTimer);
};

// ReSharper restore NotAllPathsReturnValue

function setUnderlyingCondition(newValue, oldValue, name) {
    if (initLoad === false) {
        $('#rucondition').clearField();
        var val, array;
        switch (newValue) {
        case underlyingType.NO_LENS:
        case underlyingType.NOT_RECORDED:
        case underlyingType.PROSTHESIS:
        case underlyingType.BALANCE_LENS:
            array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.BALANCE_LENS];
            val = (name === lens.RIGHT) ? viewModel.LuCondition() : viewModel.RuCondition();
            if (array.indexOf(val) >= 0) {
                initLoad = true;
                hasUnlikeSignsDialog(ulConditionErrorMsg);
                if (name === lens.RIGHT) {
                    viewModel.RuCondition(oldValue.toString());
                } else {
                    viewModel.LuCondition(oldValue.toString());
                }
                initLoad = false;
                return;
            }
            disableInputs(name);
            break;
        case underlyingType.PLANO:
            initLoad = true;
            disableInputsForPlano(name);
            initLoad = false;
            break;
        case underlyingType.SELECT:
            disableInputsForSelect(name);
            break;
        default:
            break;
        }
    }
}

ko.observable.fn.beforeAndAfterSubscribe = function (callback, target) {
    // ReSharper disable InconsistentNaming
    var _oldValue;
    // ReSharper restore InconsistentNaming
    this.subscribe(function (oldValue) {
        _oldValue = oldValue;
    }, null, 'beforeChange');

    this.subscribe(function (newValue) {
        callback.call(target, _oldValue, newValue);
    });
};

function getCLDetails() {
    //  In general you want to construct your viewModel and call
    //  applyBindings as early as possible.
    client
        .action("GetContactLensRxById")
        .get({ "officeNumber": window.config.officeNumber, "patientId": window.patient.PatientId, "examId": rxExamId, "isRecheck": isRecheck})
        .done(function (data) {
            initLoad = true;
            savedExam = data;
            examRxTypeId = data.ExamRxTypeId;
            if (data.Notes !== null) {
                $('#noteIdHidden').val(data.Notes[0].Key);
            }
            viewModel = new ContactLensRxViewModel(data);
            viewModel.RuCondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
                setUnderlyingCondition(newValue, oldValue, lens.RIGHT);
            });
            viewModel.LuCondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
                setUnderlyingCondition(newValue, oldValue, lens.LEFT);
            });
            ko.applyBindings(viewModel);
            setSelectBoxOptions();

            initLoad = false;
            viewModel.dirtyFlag = ko.dirtyFlag(viewModel);
            if (data.Provider > 0) {
                $('select#provider').parents("div.bootstrap-select").removeClass('requiredField');
            }
            // check for completeness and validity for EPM. 
            if (data.Source === 1 && data.ExamRxTypeId !== window.CLRxType.EXAM_TYPE_NORX) {
                if (!$("#ContactLensRxForm").valid()) {
                    data.ExamRxTypeId = window.CLRxType.EXAM_CL_INCOMPLETE;
                }
            }
            // EHR integration setup
            window.setupEHRFields(data);
        });
}

$(document).ready(function () {
    loadPatientSideNavigation(window.patient.PatientId, "contactLenses");
    if ($.urlParam(window.location.href, "examId", true) !== null) {
        rxExamId = $.urlParam(window.location.href, "examId", true);
    }
    if ($.urlParam(window.location.href, "isRecheck", true) !== null) {
        isRecheck = $.urlParam(window.location.href, "isRecheck", true);
    }
    updatePageTitle();
    validatePatientContactLensInfo();
    edit = (rxExamId > 0) ? "Edit" : "Add";
    $("select, input").removeAttr("disabled");
    getCLDetails();
    $("button[data-id='provider']").focus();
});