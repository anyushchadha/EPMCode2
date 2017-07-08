/*jslint browser: true, vars: true, plusplus: true, nomen:true, sloppy: true */
/*global $, document, window, ko, getDateDiff, msgType, loadPatientSideNavigation, ApiClient, getDateDiff, updatePageTitle, regex, console */

var client = new ApiClient("PatientEyeGlassesRx");
var underlyingType = {
    SELECT: 0,
    BALANCE_LENS: 60,
    NO_LENS: 61,
    NOT_RECORDED: 62,
    PROSTHESIS: 63,
    PLANO: 64
};

var rxType = {
    MULTI_FOCAL: 749,
    MULTI_FOCAL_OVER_CONTACTS: 90004,
    SUNGLASSES_MULTIFOCAL: 90009,
    COMPUTER_MULTIFOCAL: 90001,
    OCCUPATIONAL: 90005,
    SAFETY_MULTI_FOCAL: 90041
};

var EGRxType = {
    EXAM_EG_INCOMPLETE: 462,
    EXAM_TYPE_EYEGLASS: 743,
    EXAM_TYPE_RECHECK_EYEGLASS: 745
};


var lens = {
    RIGHT: '#r',
    LEFT: '#l'
};

var viewModel, ehrCodesTable1;
var editInitPage = false;
var originalExamData;
var initLoad = false;
var rxExamId = -1;
var isRecheck = false;
var examRxTypeId;
var msg = "You cannot select a No Lens, Not Recorded, Prosthesis and/or Plano underlying condition for both the " +
                    "right and left lens.";
var HBaseList = ([
    { Description: "", Key: '0' },
    { Description: "In", Key: 'In' },
    { Description: "Out", Key: 'Out' }
]);

var VBaseList = ([
    { Description: "", Key: '0' },
    { Description: "Up", Key: 'Up' },
    { Description: "Down", Key: 'Down' }
]);

function saveNote(data) {
    var noteData;
    var action;
    var reqmode;
    if ($("#rxnote").val() !== "") {
        if ((rxExamId > 0) && $("#noteIdHidden").val() !== "") {
            action = "UpdateNote";
            reqmode = "put";
            noteData = {
                PatientId: window.patient.PatientId,
                EntityId: "",
                NoteId: $("#noteIdHidden").val(),
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
            });
    }
}

function disableInputs() {
    var array = [underlyingType.BALANCE_LENS, underlyingType.NO_LENS, underlyingType.NOT_RECORDED,
        underlyingType.PROSTHESIS, underlyingType.PLANO];
    if (array.indexOf(viewModel.selectedRulCondition()) >= 0) {
        $('#rtextsphere, #rtextcylinder, #rtextaxis, #rtextadd, #rhprism, ' +
              '#rhbase, #rvprism, #rvbase, #rslaboff, #rtextbase').each(function () {
            $('#rspherestar').addClass("hidden");
            $('#raddstar').addClass("hidden");
            viewModel.rtextaxisstar(false);
            viewModel.rhbasestar(false);
            viewModel.rvbasestar(false);
            $(this).clearField();
            $(this).removeClass('requiredField');
            $(this).next('div.bootstrap-select').removeClass('requiredField');
            $(this).attr("disabled", "disabled");
            $(this).refreshSelectPicker();
            viewModel.RightSphereTxt('N/A');
            viewModel.RightCylinderTxt('N/A');
            viewModel.RightAxisTxt('N/A');
            viewModel.RightAddTxt('N/A');
            viewModel.RightHPrismTxt('N/A');
            viewModel.RightVPrismTxt('N/A');
            viewModel.RightBaseTxt('N/A');
            viewModel.selectedRhBase('0');
            viewModel.selectedRvBase('0');
            viewModel.RightIsSlabOff(false);
        });
    }
    if (array.indexOf(viewModel.selectedLulCondition()) >= 0) {
        $('#ltextsphere, #ltextcylinder, #ltextaxis, #ltextadd, #lhprism, ' +
              '#lhbase, #lvprism, #lvbase, #lslaboff, #ltextbase').each(function () {
            $(this).attr("disabled", "disabled");
            $(this).clearField();
            $(this).removeClass('requiredField');
            $(this).next('div.bootstrap-select').removeClass('requiredField');
            $(this).refreshSelectPicker();
            $('#lspherestar').addClass("hidden");
            $('#laddstar').addClass("hidden");
            viewModel.ltextaxisstar(false);
            viewModel.lhbasestar(false);
            viewModel.lvbasestar(false);
            viewModel.LeftSphereTxt('N/A');
            viewModel.LeftCylinderTxt('N/A');
            viewModel.LeftAxisTxt('N/A');
            viewModel.LeftAddTxt('N/A');
            viewModel.LeftHPrismTxt('N/A');
            viewModel.LeftVPrismTxt('N/A');
            viewModel.LeftBaseTxt('N/A');
            viewModel.selectedLhBase('0');
            viewModel.selectedLvBase('0');
            viewModel.LeftIsSlabOff(false);
        });
    }
}

function clearViewModel(name) {
    if (name === lens.RIGHT) {
        viewModel.RightSphereTxt(null);
        viewModel.RightCylinderTxt(null);
        viewModel.RightAxisTxt(null);
        viewModel.RightAddTxt(null);
        viewModel.RightHPrismTxt(null);
        viewModel.RightVPrismTxt(null);
        viewModel.RightBaseTxt(null);
        viewModel.selectedRhBase(null);
        viewModel.selectedRvBase(null);
        viewModel.rtextaxisstar(false);
        viewModel.rhbasestar(false);
        viewModel.rvbasestar(false);
    } else {
        viewModel.LeftSphereTxt(null);
        viewModel.LeftCylinderTxt(null);
        viewModel.LeftAxisTxt(null);
        viewModel.LeftAddTxt(null);
        viewModel.LeftHPrismTxt(null);
        viewModel.LeftVPrismTxt(null);
        viewModel.LeftBaseTxt(null);
        viewModel.selectedLhBase(null);
        viewModel.selectedLvBase(null);
        viewModel.ltextaxisstar(false);
        viewModel.lhbasestar(false);
        viewModel.lvbasestar(false);
    }
}

function showULValidationForm(oldValue, type) {
    $('#btnIgnoreConfirmation').hide();
    $('#confirmationDialog .modal-body').html(msg);
    $('#confirmationDialog').modal({
        keyboard: false,
        backdrop: false,
        show: true
    });
    if (type === lens.RIGHT) {
        viewModel.selectedRulCondition(oldValue.toString());
    } else {
        viewModel.selectedLulCondition(oldValue.toString());
    }
}

function setLensForSelectOption(type) {
    if (type === lens.RIGHT) {
        $('#rtextsphere, #rtextcylinder, #rtextaxis, #rtextadd, #rhprism, #rhbase, #rvprism, #rvbase, #rslaboff, #rtextbase')
            .each(function () {
                $(this).not('#rhbase, #rvbase, #rtextaxis, #rtextadd').removeAttr("disabled");
                $('#rhbase, #rvbase, #rtextaxis, #rtextadd').attr("disabled", "true");
                $(this).clearField();
                $(this).removeClass('requiredField');
                $('#rtextsphere').addClass('requiredField');
                $(this).refreshSelectPicker();
                $('#rspherestar').removeClass("hidden");
                $('#rxtype').change();
            });
    } else {
        $('#ltextsphere, #ltextcylinder, #ltextaxis, #ltextadd, #lhprism, #lhbase, #lvprism, #lvbase, #lslaboff, #ltextbase')
            .each(function () {
                $(this).not('#lhbase, #lvbase, #ltextaxis, #ltextadd').removeAttr("disabled");
                $('#lhbase, #lvbase, #ltextaxis, #ltextadd').attr("disabled", "true");
                $(this).clearField();
                $(this).removeClass('requiredField');
                $('#ltextsphere').addClass('requiredField');
                $(this).refreshSelectPicker();
                $('#lspherestar').removeClass("hidden");
                $('#rxtype').change();
            });
    }
}

function setUnderlyingCondition(newValue, oldValue, type) {
    if (initLoad === false) {
        $('#rulcondition').clearField();
        switch (newValue) {
        case underlyingType.NO_LENS:
        case underlyingType.NOT_RECORDED:
        case underlyingType.PROSTHESIS:
        case underlyingType.PLANO:
            var array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.PLANO];
            var val = (type === lens.RIGHT) ? viewModel.selectedLulCondition() : viewModel.selectedRulCondition();
            if (array.indexOf(val) >= 0) {
                showULValidationForm(oldValue, type);
            } else {
                disableInputs();
            }
            break;
        case underlyingType.BALANCE_LENS:
            disableInputs();
            break;
        case underlyingType.SELECT:
            if (type === lens.RIGHT) {
                clearViewModel(type);
                setLensForSelectOption(type);
            } else {
                clearViewModel(type);
                setLensForSelectOption(type);
            }
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

function setDoctorDdls(data) {
    if (data.ProviderType === 0) {
        $("#radioOption3").prop("checked", true);
        $("#OutsideProviderRow").hide();
        $("#ProviderRow").show();
    } else {
        $("#radioOption4").prop("checked", true);
        $("#OutsideProviderRow").show();
        $("#ProviderRow").hide();
    }
}

var EyeGlassesViewModel = function (data) {
    var self = this;

    self.Recheck = ko.observable(false);
    self.rxTypeList = ko.observableArray(data.EyeGlassRxType);
    self.rxTypeList.unshift({ "Key": 0, "Description": '' });
    self.selectedRxType = ko.observable(data.EyeglassRxTypeId || 0);

    self.providerList = ko.observableArray(data.Providers);
    self.providerList.unshift({ "Key": 0, "Description": '' });
    self.selectedProvider = ko.observable(data.ProviderId || 0);
    self.outsideProviderName = ko.observable(data.OutsideProviderName);
    self.outsideProviderId = ko.observable(data.OutsideProviderId);
    self.providerType = ko.observable(data.ProviderType);

    self.rulconditionList = ko.observableArray(data.UnderlyingConditionList);
    self.rulconditionList.unshift({ "Key": 0, "Description": 'Select' });
    self.selectedRulCondition = ko.observable(data.RightUlConditionId || 0);
    self.lulconditionList = ko.observableArray(data.UnderlyingConditionList);
    self.selectedLulCondition = ko.observable(data.LeftUlConditionId || 0);
    self.rhbaseList = ko.observableArray(HBaseList);
    self.selectedRhBase = ko.observable(data.RightPrism1Direction || '0');
    self.rvbaseList = ko.observableArray(VBaseList);
    self.selectedRvBase = ko.observable(data.RightPrism2Direction || '0');
    self.lhbaseList = ko.observableArray(HBaseList);
    self.selectedLhBase = ko.observable(data.LeftPrism1Direction || '0');
    self.lvbaseList = ko.observableArray(VBaseList);
    self.selectedLvBase = ko.observable(data.LeftPrism2Direction || '0');

    self.examDate = ko.observable(data.ExamDate);
    self.expirationDate = ko.observable(data.ExpirationDate);
    self.RightSphereTxt = ko.observable(data.RightSphereTxt || '');
    self.RightCylinderTxt = ko.observable(data.RightCylinderTxt || '');
    self.RightAxisTxt = ko.observable(data.RightAxisTxt || '');
    self.RightAddTxt = ko.observable(data.RightAddTxt || '');
    self.RightHPrismTxt = ko.observable(data.RightPrism1Txt || '');
    self.RightVPrismTxt = ko.observable(data.RightPrism2Txt || '');

    self.LeftSphereTxt = ko.observable(data.LeftSphereTxt || '');
    self.LeftCylinderTxt = ko.observable(data.LeftCylinderTxt || '');
    self.LeftAxisTxt = ko.observable(data.LeftAxisTxt || '');
    self.LeftAddTxt = ko.observable(data.LeftAddTxt || '');
    self.LeftHPrismTxt = ko.observable(data.LeftPrism1Txt || '');
    self.LeftVPrismTxt = ko.observable(data.LeftPrism2Txt || '');

    self.RightIsSlabOff = ko.observable(data.RightIsSlabOff || false);
    self.RightIsBalance = ko.observable(data.RightIsBalance || false);
    self.RightBaseTxt = ko.observable(data.RightBaseTxt || '');
    self.LeftIsSlabOff = ko.observable(data.LeftIsSlabOff || false);
    self.LeftIsBalance = ko.observable(data.LeftIsBalance || false);
    self.LeftBaseTxt = ko.observable(data.LeftBaseTxt || '');
    if (data.Notes !== null) {
        self.rxnote = ko.observable(data.Notes[0].Description);
    } else {
        self.rxnote = ko.observable('');
    }
    self.rtextaxisstar = ko.observable(false);
    self.ltextaxisstar = ko.observable(false);
    self.rhbasestar = ko.observable(false);
    self.lhbasestar = ko.observable(false);
    self.rvbasestar = ko.observable(false);
    self.lvbasestar = ko.observable(false);
};

var resetExamData = function (data) {
    viewModel.Recheck(false);
    viewModel.selectedRxType(data.EyeglassRxTypeId || 0);
    viewModel.selectedProvider(data.ProviderId || 0);
    viewModel.outsideProviderName(data.OutsideProviderName);
    viewModel.outsideProviderId(data.OutsideProviderId);
    viewModel.providerType(data.ProviderType);
    viewModel.examDate(data.ExamDate);
    viewModel.expirationDate(data.ExpirationDate);
    viewModel.RightSphereTxt(data.RightSphereTxt || '');
    viewModel.RightCylinderTxt(data.RightCylinderTxt || '');
    viewModel.RightAxisTxt(data.RightAxisTxt || '');
    viewModel.RightAddTxt(data.RightAddTxt || '');
    viewModel.RightHPrismTxt(data.RightPrism1Txt || '');
    viewModel.selectedRhBase(data.RightPrism1Direction || '0');
    viewModel.RightVPrismTxt(data.RightPrism2Txt || '');
    viewModel.selectedRvBase(data.RightPrism2Direction || '0');
    viewModel.selectedRulCondition(data.RightUlConditionId || 0);
    viewModel.selectedLulCondition(data.LeftUlConditionId || 0);
    viewModel.LeftSphereTxt(data.LeftSphereTxt || '');
    viewModel.LeftCylinderTxt(data.LeftCylinderTxt || '');
    viewModel.LeftAxisTxt(data.LeftAxisTxt || '');
    viewModel.LeftAddTxt(data.LeftAddTxt || '');
    viewModel.LeftHPrismTxt(data.LeftPrism1Txt || '');
    viewModel.selectedLhBase(data.LeftPrism1Direction || '0');
    viewModel.LeftVPrismTxt(data.LeftPrism2Txt || '');
    viewModel.selectedLvBase(data.LeftPrism2Direction || '0');
    viewModel.RightIsSlabOff(data.RightIsSlabOff || false);
    viewModel.RightIsBalance(data.RightIsBalance || false);
    viewModel.RightBaseTxt(data.RightBaseTxt || '');
    viewModel.LeftIsSlabOff(data.LeftIsSlabOff || false);
    viewModel.LeftIsBalance(data.LeftIsBalance || false);
    viewModel.LeftBaseTxt(data.LeftBaseTxt || '');
    if (data.Notes !== null) {
        viewModel.rxnote(data.Notes[0].Description);
    } else {
        viewModel.rxnote('');
    }
};

$.validator.addMethod(
    "validateEndDate",
    function (element) {
        if (element.value !== '') {
            if ($("#examDatePicker").val() !== '' && $("#examDatePicker").val() !== 'mm/dd/yyyy') {
                if (getDateDiff($("#examDatePicker").val(), $("#expireDatePicker").val(), "days") >= 0) {
                    $("#expireDatePicker").clearField();
                } else {
                    return false;
                }
            }
        }
        return true;
    }
);

// Date picker events
$(function datePickerEvents() {
    $("#icon_datePicker_1").click(function () {
        $("#examDatePicker").focus();
    });

    $("#icon_datePicker_2").click(function () {
        $("#expireDatePicker").focus();
    });

    $('#examDatePicker').datepicker({
        constrainInput: true,
        maxDate: 0
    });
    $('#expireDatePicker').datepicker({
        constrainInput: true
    });

    $('#examDatePicker').change(function () {
        if (!editInitPage) {
            var $this = $(this);
            $this.validate();
            window.setTimeout(function () {
                if (!$this.hasClass('error')) {
                    if (rxExamId <= 0) {
                        var date = new Date($this.val());
                        var strDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 2);
                        viewModel.expirationDate(strDate);
                        $('#expireDatePicker').clearField();
                    }
                }
            }, 500);
        }
    });

    $('#expireDatePicker').change(function () {
        var startDate;
        var date;
        if (!editInitPage) {
            if ($(this).val().length === 0) { // if expiration date is empty , set to exam Date
                if ($('#examDatePicker').val() !== "") {
                    date = new Date($('#examDatePicker').val());
                    startDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 2);
                    viewModel.expirationDate(startDate);
                    $(this).clearField();
                }
            } else { // if expiration date is not empty, check against exam date, if it is before exam, set to 2 yrs exam date
                date = Date.parse($(this).val());
                setTimeout(function () {
                    startDate = Date.parse($('#examDatePicker').val());
                    if (date < startDate) {
                        startDate = (startDate.getMonth() + 1) + "/" + startDate.getDate() + "/" + (startDate.getFullYear() + 2);
                        viewModel.expirationDate(startDate);
                    }
                }, 100);
            }
        }
    });
});

// SlabOff and Balanced events
$(function slaboffAndBalanceEvents() {
    $('#rslaboff, #lslaboff').click(function () {
        switch (this.name) {
        case 'rslaboff':
            if ($(this).prop('checked')) {
                viewModel.LeftIsSlabOff(false);
            }
            break;
        case 'lslaboff':
            if ($(this).prop('checked')) {
                viewModel.RightIsSlabOff(false);
            }
            break;
        default:
            break;
        }
    });
});

function copyToLeftLens() {
    initLoad = true;
    $('#ltextsphere, #ltextcylinder, #ltextaxis, #ltextadd, #lhprism, ' +
        '#lhbase, #lvprism, #lvbase, #lslaboff, #ltextbase').each(function () {
        $(this).removeAttr("disabled");
        $(this).clearField();
    });
    clearViewModel(lens.LEFT);
    viewModel.LeftSphereTxt(viewModel.RightSphereTxt());
    viewModel.LeftCylinderTxt(viewModel.RightCylinderTxt());
    viewModel.LeftAxisTxt(viewModel.RightAxisTxt());
    viewModel.LeftAddTxt(viewModel.RightAddTxt());
    viewModel.LeftHPrismTxt(viewModel.RightHPrismTxt());
    viewModel.LeftVPrismTxt(viewModel.RightVPrismTxt());
    viewModel.selectedLulCondition(viewModel.selectedRulCondition().toString());

    if (viewModel.LeftSphereTxt() !== null && viewModel.LeftSphereTxt() !== "" && viewModel.LeftSphereTxt() !== 'N/A') {
        $('#ltextsphere').clearField();
        $('#ltextsphere').removeClass("requiredField");
    } else {
        //$('#lspherestar').removeClass('hidden');
        $('#ltextsphere').addClass("requiredField");
    }
    $('#lspherestar').removeClass('hidden');
    if (viewModel.LeftAxisTxt() !== null && viewModel.LeftAxisTxt() !== "" && viewModel.LeftAxisTxt() !== "N/A") {
        $('#ltextaxis').clearField();
        $('#ltextaxis').removeClass('requiredField');
    }

    if (viewModel.LeftCylinderTxt() !== null && viewModel.LeftCylinderTxt() !== "" && viewModel.LeftCylinderTxt() !== "N/A") {
        $('#ltextaxis').removeAttr("disabled");
        viewModel.ltextaxisstar(true);
        if (viewModel.LeftAxisTxt() !== null && viewModel.LeftAxisTxt() !== "" && viewModel.LeftAxisTxt() !== "N/A") {
            $('#ltextaxis').clearField();
            $('#ltextaxis').removeClass('requiredField');
        } else {
            $('#ltextaxis').addClass('requiredField');
        }
    } else {
        $('#ltextaxis').clearField();
        $('#ltextaxis').removeClass("requiredField");
        $('#ltextaxis').attr("disabled", "true");
        viewModel.ltextaxisstar(false);
    }

    if (viewModel.LeftAddTxt() !== null && viewModel.LeftAddTxt() !== "" && viewModel.LeftAddTxt() !== "N/A") {
        $('#ltextadd').clearField();
        $('#ltextadd').removeClass('requiredField');
    } else {
        $('#ltextadd').addClass('requiredField');
    }

    if (viewModel.selectedRxType() !== rxType.MULTI_FOCAL &&
            viewModel.selectedRxType() !== rxType.MULTI_FOCAL_OVER_CONTACTS &&
            viewModel.selectedRxType() !== rxType.SUNGLASSES_MULTIFOCAL &&
            viewModel.selectedRxType() !== rxType.COMPUTER_MULTIFOCAL &&
            viewModel.selectedRxType() !== rxType.OCCUPATIONAL &&
            viewModel.selectedRxType() !== rxType.SAFETY_MULTI_FOCAL) {
        $('#rtextadd, #ltextadd').each(function () {
            $(this).attr("disabled", "disabled");
            $(this).removeClass("requiredField");
        });
    } else {
        $('#rtextadd, #ltextadd').each(function () {
            $(this).removeAttr("disabled");
            $('#laddstar').removeClass('hidden');
        });
    }

    $('#rhbase, #rvbase').each(function () {
        viewModel.selectedLvBase(viewModel.selectedRvBase());
        viewModel.selectedLhBase(viewModel.selectedRhBase());
        if (viewModel.LeftVPrismTxt() !== '0' && viewModel.LeftVPrismTxt() !== '' && viewModel.LeftVPrismTxt() !== null) {
            $('#lvbase').removeAttr("disabled");
            viewModel.lvbasestar(true);
        } else {
            $('#lvbase').attr("disabled", "true");
            viewModel.lvbasestar(false);
        }
        if (viewModel.LeftHPrismTxt() !== '0' && viewModel.LeftHPrismTxt() !== '' && viewModel.LeftHPrismTxt() !== null) {
            $('#lhbase').removeAttr("disabled");
            viewModel.lhbasestar(true);
        } else {
            $('#' + this.name.replace('r', 'l')).next('div.bootstrap-select').removeClass('requiredField');
            $('#lhbase').attr("disabled", "true");
            viewModel.lhbasestar(false);
        }
        $('#' + this.name.replace('r', 'l')).clearField();
        $('#' + this.name.replace('r', 'l')).refreshSelectPicker();
        if ($('#' + this.name).parents("div.bootstrap-select").hasClass('requiredField') ||
                $('#' + this.name).parents("div.bootstrap-select").hasClass('error')) {
            $('#' + this.name.replace('r', 'l')).next('div.bootstrap-select').addClass('requiredField');
        } else {
            $('#' + this.name.replace('r', 'l')).next('div.bootstrap-select').removeClass('requiredField');
        }
    });

    if (viewModel.selectedRulCondition() === underlyingType.BALANCE_LENS) {
        disableInputs();
    }
    initLoad = false;
}

/* click handler for View Diagnosis Codes link */
$("#linkEhrCodes").click(function (e) {
    e.preventDefault();
    $("#ehrCodesModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});

$("#radioOption3").click(function () {
    if ($("#radioOption3").is(":checked")) {
        $("#ProviderRow").show();
        $("#OutsideProviderRow").hide();
        viewModel.providerType(0);
    }
});

$("#radioOption4").click(function () {
    if ($("#radioOption4").is(":checked")) {
        $("#OutsideProviderRow").show();
        $("#ProviderRow").hide();
        viewModel.providerType(1);
    }
});

function isValidForm() {
    var valid = true;
    if ($("#radioOption4").prop("checked") && viewModel.outsideProviderId() === 0) {
        $("#outsider").showFieldMessage(msgType.ERROR, "Select the prescribing Outside Provider.");
        valid = false;
    }
    if ($("#EyeglassesRxForm").valid() && valid) {
        return true;
    }
    $(this).scrollToPosition(null);
    return false;
}

function updateEGOutsideProvider(name, id) {
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
    window.setupOutsideProviderFields(mode, updateEGOutsideProvider);
});

function initialize(data, firstTime) {
    $('#provider, #outsider, #rxtype, #ltextsphere, #rtextsphere').each(function () {
        $(this).addClass("requiredField");
        $(this).recreateSelectPicker();
    });

    $("#rulcondition, #lulcondition").each(function () {
        $(this).recreateSelectPickerShowFirstOption();
    });

    $('input, select').each(function () {
        $(this).clearField();
    });

    // disable textboxes and radio buttons
    $('#rtextaxis, #ltextaxis, #lvbase, #rhbase, #lhbase, #rvbase').each(function () {
        $(this).attr("disabled", "true");
        $(this).recreateSelectPicker();
    });

    if (data.ProviderId && data.ProviderId !== 0) {
        $("select#provider").parents("div.bootstrap-select").removeClass('requiredField');
    }

    if (data.RightSphereTxt !== null || data.LeftSphereTxt !== null || data.RightCylinderTxt !== null || data.LeftCylinderTxt !== null) {
        $('input').keyup();
        $('input').not('#examDatePicker, #expireDatePicker').change();
    }

    $('#rxtype').change();

    if (data.RightPrism1Txt !== null) {
        $("select#rhbase").removeAttr("disabled");
        $("#rhbase").parents("div.bootstrap-select").removeClass('requiredField');
    }
    if (data.RightPrism2Txt !== null) {
        $("select#rvbase").removeAttr("disabled");
        $("#rvbase").parents("div.bootstrap-select").removeClass('requiredField');
    }
    if (data.LeftPrism1Txt !== null) {
        $("select#lhbase").removeAttr("disabled");
        $("#lhbase").parents("div.bootstrap-select").removeClass('requiredField');
    }
    if (data.LeftPrism2Txt !== null) {
        $("select#lvbase").removeAttr("disabled");
        $("#lvbase").parents("div.bootstrap-select").removeClass('requiredField');
    }

    if (data.RightCylinderTxt !== null && data.RightCylinderTxt !== "") {
        viewModel.rtextaxisstar(true);
    }

    if (data.LeftCylinderTxt !== null && data.LeftCylinderTxt !== "") {
        viewModel.ltextaxisstar(true);
    }

    if (firstTime) {
        viewModel.dirtyFlag = ko.dirtyFlag(viewModel);
    } else {
        viewModel.dirtyFlag.reset();
    }
    $('#rxnote').clearField();
    disableInputs();

    setDoctorDdls(data);

    if (data.HasBeenRechecked) {
        $("#EyeglassesRxForm input, select, #rbtnTranspose, #lbtnTranspose, #rbtnCopy, #rxnote").attr("disabled", "disabled").removeClass("requiredField");
        $("#EyeglassesRxForm select").refreshSelectPicker();
        $("#btnReset, #btnSave").addClass("hidden");
        if (data.Source === 1) {
            $("#source").html("Eyefinity EHR");
        } else {
            $("#source").html("Manual");
        }
    } else {
        // Exam Source (EHR) 
        if (data.Source === 1 && isRecheck === "false") {
            if (!isValidForm()) {
                data.ExamRxTypeId = EGRxType.EXAM_EG_INCOMPLETE;
            }
            $("#source").html("Eyefinity EHR");

            // disable inputs if the order is in any state except INCOMPLETE
            if (data.ExamRxTypeId !== EGRxType.EXAM_EG_INCOMPLETE) {
                $("input, select, #rbtnTranspose, #lbtnTranspose, #rbtnCopy").attr("disabled", "disabled").removeClass("requiredField");
                $("select").refreshSelectPicker();
                $("#btnReset").addClass("hidden");
            }

            // procedure codes & pqri lookups
            if ((data.Procedures && data.Procedures.length > 0) || (data.PqriLookups && data.PqriLookups.length > 0) || (data.ConditionsList && data.ConditionsList.length > 0)) {
                window.buildEhrCodesTable(data);
                $("#ehrImportDate").html(data.CreatedDate);
                $("#ehrExamMemo").html(data.RxMemo);
                $("#linkEhrCodes").removeClass("hidden");
            }
        } else {
            if (data.HasOrders && isRecheck === "false") {
                $("#EyeglassesRxForm input, select, #rbtnTranspose, #lbtnTranspose, #rbtnCopy").attr("disabled", "disabled").removeClass("requiredField");
                $("#EyeglassesRxForm select").refreshSelectPicker();
                $("#btnReset").addClass("hidden");
            }
            $("#source").html("Manual");
        }

        var result = $("#radioOption3").attr("disabled");
        if (result !== undefined && result === "disabled") {
            $("#outsideProvider").attr("disabled", "disabled");
        }
    }
}

function loadEyeGlassesRx(data) {
    viewModel = new EyeGlassesViewModel(data);
    initLoad = true;
    viewModel.selectedRulCondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
        setUnderlyingCondition(newValue, oldValue, lens.RIGHT);
    });
    viewModel.selectedLulCondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
        setUnderlyingCondition(newValue, oldValue, lens.LEFT);
    });
    ko.applyBindings(viewModel);
    initialize(data, true);
    initLoad = false;
}

function getGlassesDetails() {
    client
        .action("GetEyeGlassesRxById")
        .get({
            "patientId": window.patient.PatientId,
            "examId": rxExamId,
            "isRecheck": isRecheck
        })
        .done(function (data) {
            originalExamData = data;
            if (data.Notes !== null) {
                $('#noteIdHidden').val(data.Notes[0].Key);
            }
            if (rxExamId > 0) { //edit mode
                editInitPage = true;
            }

            examRxTypeId = data.ExamRxTypeId;
            loadEyeGlassesRx(data);
            editInitPage = false;
        });
}

$("#rbtnCopy").click(function (e) {
    e.preventDefault();
    var array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.PLANO];
    var val = viewModel.selectedRulCondition();
    if (array.indexOf(val) >= 0) {
        $('#btnIgnoreConfirmation').hide();
        $('#confirmationDialog .modal-body').html(msg);
        $('#confirmationDialog').modal({
            keyboard: false,
            backdrop: false,
            show: true
        });
    } else {
        copyToLeftLens();
    }
});

$("#btnReset").click(function (e) {
    e.preventDefault();
    $(".summaryMessages").clearMsgBlock();
    clearViewModel(lens.LEFT);
    clearViewModel(lens.RIGHT);
    initLoad = true;
    $('input, select').each(function () {
        $(this).removeAttr("disabled");
        $(this).removeClass("requiredField");
    });
    resetExamData(originalExamData);
    initialize(originalExamData, false);
    initLoad = false;
    setDoctorDdls(originalExamData);
});

// Validation events

$.validator.addMethod("rhbasecheck", function () {
    if (viewModel.RightHPrismTxt() !== "" && viewModel.RightHPrismTxt() !== null) {
        if (viewModel.selectedRhBase() !== "" && viewModel.selectedRhBase() !== null && viewModel.selectedRhBase() !== '0') {
            return true;
        }
        return false;
    }
    return true;
});

$.validator.addMethod("rvbasecheck", function () {
    if (viewModel.RightVPrismTxt() !== "" && viewModel.RightVPrismTxt() !== null) {
        if (viewModel.selectedRvBase() !== "" && viewModel.selectedRvBase() !== null && viewModel.selectedRvBase() !== '0') {
            return true;
        }
        return false;
    }
    return true;
});

$.validator.addMethod("lhbasecheck", function () {
    if (viewModel.LeftHPrismTxt() !== "" && viewModel.LeftHPrismTxt() !== null) {
        if (viewModel.selectedLhBase() !== "" && viewModel.selectedLhBase() !== null && viewModel.selectedLhBase() !== '0') {
            return true;
        }
        return false;
    }
    return true;
});

$.validator.addMethod("rulconditioncheck", function (value) {
    switch (parseInt(value, 10)) {
    case underlyingType.NO_LENS:
    case underlyingType.NOT_RECORDED:
    case underlyingType.PROSTHESIS:
    case underlyingType.PLANO:
        var array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.PLANO];
        var val = parseInt(viewModel.selectedLulCondition(), 10);
        if (array.indexOf(val) >= 0) {
            return false;
        }
        break;
    default:
        break;
    }
    return true;
});

$.validator.addMethod("lvbasecheck", function () {
    if (viewModel.LeftVPrismTxt() !== "" && viewModel.LeftVPrismTxt() !== null) {
        if (viewModel.selectedLvBase() !== "" && viewModel.selectedLvBase() !== null && viewModel.selectedLvBase() !== '0') {
            return true;
        }
        return false;
    }
    return true;
});

function validateEyeglassesRxInfo() {
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

    $("#EyeglassesRxForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            rxtype: {
                selectBox: true
            },
            provider: {
                selectBox: true
            },
            rtextsphere: {
                required: true,
                range: [-99.99, 99.99]
            },
            ltextsphere: {
                required: true,
                range: [-99.99, 99.99]
            },
            rtextcylinder: {
                range: [-99.99, 99.99]
            },
            ltextcylinder: {
                range: [-99.99, 99.99]
            },
            rtextaxis: {
                required: function () {
                    return viewModel.RightCylinderTxt().length > 0;
                },
                range: [1, 180]
            },
            ltextaxis: {
                required: function () {
                    return viewModel.LeftCylinderTxt().length > 0;
                },
                range: [1, 180]
            },
            rtextadd: {
                required: function () {
                    return viewModel.selectedRxType() === rxType.MULTI_FOCAL ||
                        viewModel.selectedRxType() === rxType.SUNGLASSES_MULTIFOCAL ||
                        viewModel.selectedRxType() === rxType.MULTI_FOCAL_OVER_CONTACTS ||
                        viewModel.selectedRxType() === rxType.COMPUTER_MULTIFOCAL ||
                        viewModel.selectedRxType() === rxType.OCCUPATIONAL ||
                        viewModel.selectedRxType() === rxType.SAFETY_MULTI_FOCAL;
                },
                range: [0.00, 9.99]
            },
            ltextadd: {
                required: function () {
                    return viewModel.selectedRxType() === rxType.MULTI_FOCAL ||
                        viewModel.selectedRxType() === rxType.SUNGLASSES_MULTIFOCAL ||
                        viewModel.selectedRxType() === rxType.MULTI_FOCAL_OVER_CONTACTS ||
                        viewModel.selectedRxType() === rxType.COMPUTER_MULTIFOCAL ||
                        viewModel.selectedRxType() === rxType.OCCUPATIONAL ||
                        viewModel.selectedRxType() === rxType.SAFETY_MULTI_FOCAL;
                },
                range: [0.00, 9.99]
            },
            rtextbase: {
                range: [0.00, 99.9]
            },
            ltextbase: {
                range: [0.00, 99.9]
            },
            rhprism: {
                range: [0.25, 20.00]
            },
            lhprism: {
                range: [0.25, 20.00]
            },
            rvprism: {
                range: [0.25, 20.00]
            },
            lvprism: {
                range: [0.25, 20.00]
            },
            rhbase: {
                rhbasecheck: true
            },
            rvbase: {
                rvbasecheck: true
            },
            lhbase: {
                lhbasecheck: true
            },
            lvbase: {
                lvbasecheck: true
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
            rulcondition: {
                rulconditioncheck: true
            },
            rxnote: {
                maxlength: 2048
            }
        },
        messages: {
            rxtype: {
                selectBox: "Select the Eyeglass Rx Type."
            },
            provider: {
                selectBox: "Select the prescribing Provider."
            },
            rtextsphere: {
                required: "Enter a valid Sphere value (-99.99 to 99.99).",
                range: "Enter a valid Sphere value (-99.99 to 99.99)."
            },
            ltextsphere: {
                required: "Enter a valid Sphere value (-99.99 to 99.99).",
                range: "Enter a valid Sphere value (-99.99 to 99.99)."
            },
            rtextcylinder: {
                range: "Enter a valid Cylinder value (-99.99 to 99.99)."
            },
            ltextcylinder: {
                range: "Enter a valid Cylinder value (-99.99 to 99.99)."
            },
            rtextaxis: {
                required: "Enter a valid Axis value (1 to 180).",
                range: "Enter a valid Axis value (1 to 180)."
            },
            ltextaxis: {
                required: "Enter a valid Axis value (1 to 180).",
                range: "Enter a valid Axis value (1 to 180)."
            },
            rtextadd: {
                required: "Enter a valid Add value (0.00 to 9.99).",
                range: "Enter a valid Add value (0.00 to 9.99)."
            },
            ltextadd: {
                required: "Enter a valid Add value (0.00 to 9.99).",
                range: "Enter a valid Add value (0.00 to 9.99)."
            },
            rtextbase: {
                range: "Enter a valid Base Curve value (0.00 to 99.9)."
            },
            ltextbase: {
                range: "Enter a valid Base Curve value (0.00 to 99.9)."
            },
            rhprism: {
                range: "Enter a valid Horizontal Prism value (0.25 to 20.00)."
            },
            lhprism: {
                range: "Enter a valid Horizontal Prism value (0.25 to 20.00)."
            },
            rvprism: {
                range: "Enter a valid Vertical Prism value (0.25 to 20.00)."
            },
            lvprism: {
                range: "Enter a valid Vertical Prism value (0.25 to 20.00)."
            },
            rhbase: {
                rhbasecheck: "Select In or Out direction."
            },
            rvbase: {
                rvbasecheck: "Select Up or Down direction."
            },
            lhbase: {
                lhbasecheck: "Select In or Out direction."
            },
            lvbase: {
                lvbasecheck: "Select Up or Down direction."
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
            },
            rulcondition: {
                rulconditioncheck: "You cannot select a No Lens, Not Recorded, Prosthesis and/or Plano underlying condition for both the right and left lens."
            }
        }
    });
}

var goToRxPage = function () {
    var redirectUrl = window.config.baseUrl + "Patient/Rx?id=" + window.patient.PatientId;
    window.location.href = redirectUrl;
};

function clearLensByULCondition() {
    var array = [underlyingType.BALANCE_LENS, underlyingType.NO_LENS, underlyingType.NOT_RECORDED,
                    underlyingType.PROSTHESIS, underlyingType.PLANO];
    var val = viewModel.selectedRulCondition();
    if (array.indexOf(val) >= 0) {
        clearViewModel(lens.RIGHT);
    }
    val = viewModel.selectedLulCondition();
    if (array.indexOf(val) >= 0) {
        clearViewModel(lens.LEFT);
    }
}

var saveExam = function () {
    clearLensByULCondition();
    var eyeglassesRxData = {
        UserId: window.config.userId,
        PatientId: window.patient.PatientId,
        ExamRxTypeId: examRxTypeId === null ? EGRxType.EXAM_TYPE_EYEGLASS : examRxTypeId,
        ExamId: rxExamId,
        ProviderId: viewModel.selectedProvider(),
        OutsideProviderId: viewModel.outsideProviderId(),
        ProviderType: viewModel.providerType(),
        EyeglassRxTypeId: viewModel.selectedRxType(),
        ExamDate: viewModel.examDate(),
        ExpirationDate: viewModel.expirationDate(),
        RightSphereTxt: viewModel.RightSphereTxt(),
        RightCylinderTxt: viewModel.RightCylinderTxt(),
        RightAxisTxt: viewModel.RightAxisTxt(),
        RightAddTxt: viewModel.RightAddTxt(),
        RightBaseTxt: viewModel.RightBaseTxt(),
        RightPrism1Txt: viewModel.RightHPrismTxt(),
        RightPrism1Direction: viewModel.selectedRhBase(),
        RightPrism2Txt: viewModel.RightVPrismTxt(),
        RightPrism2Direction: viewModel.selectedRvBase(),
        RightIsSlabOff: viewModel.RightIsSlabOff(),
        RightUlConditionId: viewModel.selectedRulCondition(),
        LeftSphereTxt: viewModel.LeftSphereTxt(),
        LeftCylinderTxt: viewModel.LeftCylinderTxt(),
        LeftAxisTxt: viewModel.LeftAxisTxt(),
        LeftAddTxt: viewModel.LeftAddTxt(),
        LeftBaseTxt: viewModel.LeftBaseTxt(),
        LeftPrism1Txt: viewModel.LeftHPrismTxt(),
        LeftPrism1Direction: viewModel.selectedLhBase(),
        LeftPrism2Txt: viewModel.LeftVPrismTxt(),
        LeftPrism2Direction: viewModel.selectedLvBase(),
        LeftIsSlabOff: viewModel.LeftIsSlabOff(),
        LeftUlConditionId: viewModel.selectedLulCondition(),
        HasBeenRechecked: isRecheck,
        RecheckExamId: isRecheck ? rxExamId : 0
    };
    client
        .action("Put")
        .queryStringParams({
            "userId": window.config.userId
        })
        .put(eyeglassesRxData)
        .done(function (data) {
            if (data.length !== 0 && data.indexOf('not valid') > 0) {
                $(this).showSummaryMessage(msgType.ERROR, data, true);
            } else {
                saveNote(data);
                $('#confirmationDialog').modal('hide');
                $(this).showSystemSuccess('Eyeglass Rx saved.');
                setTimeout(goToRxPage, 1000);
            }
        })
        .fail(function (xhr) {
            if (xhr.status === 400) {
                $('#confirmationDialog').modal('hide');
                $(document).showSummaryMessage(msgType.ERROR, JSON.parse(xhr.responseText));
            }
        });
};

var hasUnlikeSignsDialog = function (msg) {
    $('#btnIgnoreConfirmation').show();
    $('#confirmationDialog .modal-body').html(msg);
    $('#confirmationDialog').modal({
        keyboard: false,
        backdrop: false,
        show: true
    });
};

var validateAndSaveExam = function () {
    var unsignWarning = false;
    var addPowerWarning = false;
    var msg;
    var unsignWaringMsg = "The sphere or cylinder values have unlike (opposite) signs.<br />";
    var addPowerWarningMsg = "The Add Power values for the right and left eyes are different.<br />";
    var commonMsg = "Click Cancel to change the values so that they are identical or click Save to keep the values and record the Rx. <br /><br />";

    if (((parseFloat($('#rtextsphere').val()) > 0) && (parseFloat($('#ltextsphere').val())) < 0) ||
            ((parseFloat($('#rtextsphere').val()) < 0) && (parseFloat($('#ltextsphere').val())) > 0) ||
            ((parseFloat($('#rtextcylinder').val()) > 0) && (parseFloat($('#ltextcylinder').val())) < 0) ||
            ((parseFloat($('#rtextcylinder').val()) < 0) && (parseFloat($('#ltextcylinder').val())) > 0)) {
        unsignWarning = true;
    }
    if (viewModel.RightAddTxt() !== viewModel.LeftAddTxt() && viewModel.RightAddTxt() !== "N/A" && viewModel.LeftAddTxt() !== "N/A") {
        addPowerWarning = true;
    }
    if (unsignWarning && addPowerWarning) {
        msg = unsignWaringMsg + addPowerWarningMsg + commonMsg;
        hasUnlikeSignsDialog(msg);
    } else if (unsignWarning) {
        msg = unsignWaringMsg + commonMsg;
        hasUnlikeSignsDialog(msg);
    } else if (addPowerWarning) {
        msg = addPowerWarningMsg + commonMsg;
        hasUnlikeSignsDialog(msg);
    } else {
        saveExam();
        viewModel.dirtyFlag.reset();
    }
};

// Save event
$('#btnSave').click(function (e) {
    e.preventDefault();
    if (!isValidForm()) {
        return;
    }
    validateAndSaveExam();
});

$('#btnIgnoreConfirmation').click(function () {
    saveExam();
    viewModel.dirtyFlag.reset();
});

function formatting_FormatNumber(numberInput, numberOfDecimalPlaces, hasPlusOrMinus, controlName) {
    var plusOrMinusSign = "";
    var numberString;
    var numberObject;
    if (numberInput.length === 0) {
        return "";
    }

    if (hasPlusOrMinus) {
        var firstChar = numberInput.substr(0, 1);
        if (firstChar === '+' || firstChar === '-') {
            plusOrMinusSign = firstChar;
            numberString = numberInput.substr(1, numberInput.length - 1);
        } else {
            plusOrMinusSign = '+';
            if (controlName === 'rtextcylinder' || controlName === "ltextcylinder") {
                plusOrMinusSign = '-';
            }
            numberString = numberInput;
        }
    } else {
        numberString = numberInput;
    }

    if (isNaN(numberString)) {
        return plusOrMinusSign + numberString;
    }

    numberObject = Number(numberString);
    return plusOrMinusSign + numberObject.toFixed(numberOfDecimalPlaces);
}

// Transpose events
function orders_Transpose(whichLen) {
    var sphere, cylinder, axis;
    if (whichLen === "Right") {
        sphere = viewModel.RightSphereTxt();
        cylinder = viewModel.RightCylinderTxt();
        axis = viewModel.RightAxisTxt();
    } else {
        sphere = viewModel.LeftSphereTxt();
        cylinder = viewModel.LeftCylinderTxt();
        axis = viewModel.LeftAxisTxt();
    }
    if (!isNaN(sphere) && !isNaN(cylinder) && !isNaN(axis)) {
        if (Number(cylinder) !== 0) {
            var transposedSphere = Number(sphere) + Number(cylinder);
            var transposedCylinder = Number(cylinder) * -1;

            var transposedAxis;
            if (Number(cylinder) > 0 && Number(axis) === 90) {
                transposedAxis = 180;
            } else if (Number(cylinder) < 0 && Number(axis) === 90) {
                transposedAxis = 180;
            } else {
                transposedAxis = (Number(axis) + 90) % 180;
            }
            if (whichLen === "Right") {
                viewModel.RightSphereTxt(formatting_FormatNumber(transposedSphere.toString(), 2, true, null));
                viewModel.RightCylinderTxt(formatting_FormatNumber(transposedCylinder.toString(), 2, true, null));
                viewModel.RightAxisTxt(formatting_FormatNumber(transposedAxis.toString(), 0, false, null));
                $('#rtextaxis, #rtextsphere, #rtextcylinder').change();
            } else {
                viewModel.LeftSphereTxt(formatting_FormatNumber(transposedSphere.toString(), 2, true, null));
                viewModel.LeftCylinderTxt(formatting_FormatNumber(transposedCylinder.toString(), 2, true, null));
                viewModel.LeftAxisTxt(formatting_FormatNumber(transposedAxis.toString(), 0, false, null));
                $('#ltextaxis, #ltextsphere, #ltextcylinder').change();
            }
        }
    }
}

$('#lbtnTranspose').click(function (e) {
    e.preventDefault();
    orders_Transpose("Left");
});

$('#rbtnTranspose').click(function (e) {
    e.preventDefault();
    orders_Transpose("Right");
});

function pad(str, max) {
    return str.length < max ? pad("0" + str, max) : str;
}

function setBaseSelectPicker(prismVal, target) {

    if (prismVal.length > 0) {
        $('#' + target).removeAttr("disabled");
        if ($('#' + target).val() === "0") {
            $('#' + target).parents("div.bootstrap-select").addClass('requiredField');
        }
        $('#' + target).refreshSelectPicker();
    } else {
        $('#' + target).attr("disabled", "true");
        switch (target) {
        case 'rhbase':
            viewModel.selectedRhBase('0');
            break;
        case 'lhbase':
            viewModel.selectedLhBase('0');
            break;
        case 'rvbase':
            viewModel.selectedRvBase('0');
            break;
        case 'lvbase':
            viewModel.selectedLvBase('0');
            break;
        default:
            break;
        }
        $('#' + target).refreshSelectPicker();
        $('#' + target).clearField();
        $('#' + target).parents("div.bootstrap-select").removeClass('requiredField');
    }
}

// Validation events
$(function validationEvents() {
    $('#provider, #outsider, #rxtype, #rhbase, #rvbase, #lhbase, #lvbase').change(function () {
        switch (this.name) {
        case 'rxtype':
            if ($(this).val() > 0) {
                $('#rxtype').parents("div.bootstrap-select").removeClass('requiredField');
                $(this).clearField();
            }
            if ($(this).val() !== rxType.MULTI_FOCAL.toString() &&
                    $(this).val() !== rxType.MULTI_FOCAL_OVER_CONTACTS.toString() &&
                    $(this).val() !== rxType.SUNGLASSES_MULTIFOCAL.toString() &&
                    $(this).val() !== rxType.COMPUTER_MULTIFOCAL.toString() &&
                    $(this).val() !== rxType.OCCUPATIONAL.toString() &&
                    $(this).val() !== rxType.SAFETY_MULTI_FOCAL.toString()) {
                $('#rtextadd, #ltextadd').each(function () {
                    viewModel.RightAddTxt(null);
                    viewModel.LeftAddTxt(null);
                    $(this).attr("disabled", "disabled");
                    $(this).clearField();
                    $(this).removeClass('requiredField');
                    $('#raddstar').addClass('hidden');
                    $('#laddstar').addClass('hidden');
                    disableInputs();
                });
            } else {
                $('#rtextadd, #ltextadd').each(function () {
                    if ($(this).val().length === 0) {
                        $(this).addClass("requiredField");
                    }
                    $(this).removeAttr("disabled");
                    $('#raddstar').removeClass('hidden');
                    $('#laddstar').removeClass('hidden');
                    disableInputs();
                });
            }
            break;
        default:
            $(this).clearField();
            $(this).parents("div.bootstrap-select").removeClass('requiredField');
            break;
        }
    });

    $('#rhprism, #lhprism, #rvprism, #lvprism, #rtextcylinder, #ltextcylinder').keyup(function () {
        switch (this.name) {
        case 'rtextcylinder':
            if ($('#rtextcylinder').val().length > 0) { //if cylinder has value, then enable axis
                $('#rtextaxis').removeAttr("disabled");
                if ($('#rtextaxis').val().length === 0) {
                    $('#rtextaxis').addClass("requiredField");
                    viewModel.rtextaxisstar(true);
                }
            } else {
                viewModel.RightAxisTxt('');
                $('#rtextaxis').removeClass('requiredField');
                $('#rtextaxis').attr("disabled", "disabled");
                $('#rtextaxis').clearField();
                viewModel.rtextaxisstar(false);
            }
            break;
        case 'ltextcylinder':
            if ($('#ltextcylinder').val().length > 0) { //if cylinder has value, then enable axis
                $('#ltextaxis').removeAttr("disabled");
                if ($('#ltextaxis').val().length === 0) {
                    $('#ltextaxis').addClass("requiredField");
                    viewModel.ltextaxisstar(true);
                }
            } else {
                viewModel.LeftAxisTxt('');
                $('#ltextaxis').removeClass('requiredField');
                $('#ltextaxis').attr("disabled", "disabled");
                $('#ltextaxis').clearField();
                viewModel.ltextaxisstar(false);
            }
            break;
        case 'rhprism':
            setBaseSelectPicker($('#rhprism').val(), 'rhbase');
            if ($('#rhprism').val() !== null && $('#rhprism').val() !== "") {
                viewModel.rhbasestar(true);
            } else {
                viewModel.rhbasestar(false);
            }
            break;
        case 'lhprism':
            setBaseSelectPicker($('#lhprism').val(), 'lhbase');
            if ($('#lhprism').val() !== null && $('#lhprism').val() !== "") {
                viewModel.lhbasestar(true);
            } else {
                viewModel.lhbasestar(false);
            }
            break;
        case 'rvprism':
            setBaseSelectPicker($('#rvprism').val(), 'rvbase');
            if ($('#rvprism').val() !== null && $('#rvprism').val() !== "") {
                viewModel.rvbasestar(true);
            } else {
                viewModel.rvbasestar(false);
            }
            break;
        case 'lvprism':
            setBaseSelectPicker($('#lvprism').val(), 'lvbase');
            if ($('#lvprism').val() !== null && $('#lvprism').val() !== "") {
                viewModel.lvbasestar(true);
            } else {
                viewModel.lvbasestar(false);
            }
            break;
        default:
            break;
        }
    });

    $("#rtextsphere, #ltextsphere, #rtextcylinder, #ltextcylinder").change(function () {
        if (!isNaN($(this).val())) {
            $(this).val(formatting_FormatNumber($(this).val().toString(), 2, true, this.name));
        }
    });

    $("#rtextadd, #ltextadd, #rhprism, #lhprism, #rvprism, #lvprism").change(function () {
        if (!isNaN($(this).val())) {
            $(this).val(formatting_FormatNumber($(this).val().toString(), 2, false, this.name));
        }
    });

    $("#rtextbase, #ltextbase").change(function () {
        $(this).val(formatting_FormatNumber($(this).val().toString(), 1, false, this.name));
    });

    $("#rtextaxis, #ltextaxis").change(function () {
        if ($(this).val().length > 0 && !isNaN($(this).val()) && (Number($(this).val()) > 0)) {
            $(this).val(pad(Math.round($(this).val()).toString(), 3));
        }
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
});

// ReSharper disable NotAllPathsReturnValue
window.onbeforeunload = function () {
    $("#btnSave").focus(); //ko needs to update entity, since user hits F5 
    if (viewModel.dirtyFlag.isDirty()) {
        return "You are trying to navigate to a new page, but you have not saved the data on this page.";
    }
};
// ReSharper restore NotAllPathsReturnValue

// Initial load events
$(document).ready(function () {
    // load the side nav
    $(".tt").popover({ container: 'body', html: true, placement: 'top' });
    loadPatientSideNavigation(window.patient.PatientId, "glasses");
    updatePageTitle();
    if ($.urlParam(window.location.href, "examId", true) !== null) {
        rxExamId = $.urlParam(window.location.href, "examId", true);
    }
    if ($.urlParam(window.location.href, "isRecheck", true) !== null) {
        isRecheck = $.urlParam(window.location.href, "isRecheck", true);
    }
    validateEyeglassesRxInfo();
    getGlassesDetails();
    $("button[data-id='rxtype']").focus();
});