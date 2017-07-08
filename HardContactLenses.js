/*jslint browser: true, vars: true, nomen:true, sloppy:true*/
/*global $, document, window, patient, alslContactLenses, ko, alert, msgType, ApiClient, convertDate, commonDate, successMsg, msgType, isRecheck, loadPatientSideNavigation, updatePageTitle, CLRxType, getDateDiff, rxExamId */

(function () {
    var underlyingType = {
        SELECT: 0,
        BALANCE_LENS: 60,
        NO_LENS: 61,
        NOT_RECORDED: 62,
        PROSTHESIS: 63,
        PLANO: 64
    };

    var initLoad = false,
        showMsgTimer,
        viewModel = null,
        client = new ApiClient("PatientContactLensRx");

    window.onbeforeunload = function () {
        $("#btnSave").focus();
        var self = this;
        if ((viewModel && viewModel.isDirty()) || self.isDirty()) {
            showMsgTimer = setTimeout(function () {
                $("#hard").click();
            }, 1);
            return "You are trying to navigate to a new page, but you have not saved the data on this page.";

        }
        return true;
    };

    window.onunload = function () {
        clearTimeout(showMsgTimer);
    };

    $.validator.addMethod("validateProvider", function (value, element) {
        if (element.value !== '') {
            if ($("#radioOption3").is(":checked") && viewModel.provider() === 0) {
                return false;
            }
        }
        return true;
    });

    $.validator.addMethod("validateOutsider", function (value, element) {
        if (element.value !== '') {
            if ($("#radioOption4").is(":checked") && viewModel.outsideProviderId() === 0) {
                return false;
            }
        }
        return true;
    });

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

    $('#examDatePicker, #expireDatePicker').change(function () {
        var date;
        switch (this.name) {
        case 'examDatePicker':
            if ($(this).val() !== "" && commonDate($(this).val(), $('#examDatePicker'))) {
                if (rxExamId <= 0) {
                    date = new Date($('#examDatePicker').val());
                    var strDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 1);
                    viewModel.expirationDate(strDate);
                    $('#expireDatePicker').clearField();
                }
                $(this).clearField();
            }
            break;
        case 'expireDatePicker':
            var startDate;
            if ($(this).val().length === 0) {
                // if expiration date is empty , set to exam Date
                if ($('#examDatePicker').val() !== "") {
                    date = new Date($('#examDatePicker').val());
                    startDate = (date.getMonth() + 1) + "/" + date.getDate() + "/" + (date.getFullYear() + 1);
                    viewModel.expirationDate(startDate);
                    $(this).clearField();
                }
            }
            break;
        default:
            break;
        }
    });

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
            if (data.OutsideProviderId > 0) {
                $('select#outsider').parents("div.bootstrap-select").removeClass('requiredField');
            } else {
                $('select#outsider').parents("div.bootstrap-select").addClass('requiredField');
            }
        }
    }

    function formattingFormatNumber(numberInput, infirstChar, inNumberString, numberOfDecimalPlaces, hasPlusOrMinus, controlName) {
        var plusOrMinusSign = "";
        var numberString;
        var numberObject;
        if (numberInput.length === 0) {
            return "";
        }

        if (hasPlusOrMinus) {
            var firstChar = infirstChar;
            if (firstChar === '+' || firstChar === '-') {
                plusOrMinusSign = firstChar;
                numberString = inNumberString;
            } else {
                plusOrMinusSign = '+';
                if (controlName === 'leftcylinder' || controlName === "rightcylinder" || controlName === 'leftcylinder2' || controlName === "rightcylinder2") {
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

    function pad(str, max) {
        return str.length < max ? pad("0" + str, max) : str;
    }

    function initialize() {
        $('select').not('select#rightulc, select#leftulc, select#leftblend, select#lefttint, select#rightulc, select#rightblend, select#righttint,' +
            'select#wearschedule, select#dfregimen, select#dot, select#monovision').each(function () {
            $(this).recreateSelectPicker();
        });

        $('select#rightulc, select#leftulc, select#leftblend, select#lefttint, select#rightulc, select#rightblend, select#righttint,' +
            'select#wearschedule, select#dfregimen, select#dot, select#monovision').each(function () {
            $(this).refreshSelectPicker();
        });

        $('select#provider, select#outsider').parents("div.bootstrap-select").removeClass('requiredField');
        if (rxExamId > 0) {
            $('select').each(function () {
                $('select#' + this.name).parents("div.bootstrap-select").removeClass('error').removeClass('requiredField');
            });
            $('input').not('#expireDatePicker').change();
            setTimeout(function () {
                viewModel.dirtyFlag.reset();
            }, 1);
        }
    }

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

    function setUnderlyingCondition(rightLeftLen, oldValue, newValue) {
        if (initLoad === false) {
            if (newValue === 0) { //Select
                if (rightLeftLen === '#right') {
                    viewModel.hasrightulc(false);
                    viewModel.rightbasecurve('');
                    viewModel.rightdiameter('');
                    viewModel.rightsphere('');
                    viewModel.rightquantity('');
                    viewModel.RStar('*');
                } else {
                    viewModel.hasleftulc(false);
                    viewModel.leftbasecurve('');
                    viewModel.leftdiameter('');
                    viewModel.leftsphere('');
                    viewModel.leftquantity('');
                    viewModel.LStar('*');
                }
                $(rightLeftLen + 'manufacturer,' + rightLeftLen + 'style,' + rightLeftLen + 'blend,' + rightLeftLen + 'tint').each(function () {
                    $(this).selectpicker('refresh');
                    $(this).clearField();
                });
                $(rightLeftLen + 'manufacturer,' + rightLeftLen + 'style').parents("div.bootstrap-select").removeClass('error').addClass('requiredField');
                $(rightLeftLen + 'basecurve, ' + rightLeftLen + 'diameter,  ' + rightLeftLen + 'sphere, ' + rightLeftLen + 'quantity').each(function () { $(this).clearField(); });
                $(rightLeftLen + 'basecurve, ' + rightLeftLen + 'diameter,  ' + rightLeftLen + 'sphere,' + rightLeftLen + 'quantity').removeClass('error').addClass('requiredField');

            } else { // No lens, Balance Lens, Not Recorded, Prosthesis
                var ulConditionErrorMsg = "You cannot select a Balanced Lens, No Lens, Not Recorded, and/or Prosthesis underlying condition for both the right and left lens.";
                var array = [underlyingType.NO_LENS, underlyingType.NOT_RECORDED, underlyingType.PROSTHESIS, underlyingType.BALANCE_LENS];
                var val = (rightLeftLen === "#right") ? viewModel.leftulcondition() : viewModel.rightulcondition();
                if (array.indexOf(val) >= 0) {
                    initLoad = true;
                    hasUnlikeSignsDialog(ulConditionErrorMsg);
                    if (rightLeftLen === "#right") {
                        viewModel.rightulcondition(oldValue.toString());
                    } else {
                        viewModel.leftulcondition(oldValue.toString());
                    }
                    initLoad = false;
                    return;
                }
                if (rightLeftLen === '#right') {
                    viewModel.hasrightulc(true);
                    viewModel.rightmanufacturer(null);
                    viewModel.rightstyle(0);
                    viewModel.rightbasecurve('N/A');
                    viewModel.rightdiameter('N/A');
                    viewModel.rightsphere('N/A');
                    viewModel.rightcylinder('');
                    viewModel.rightaxis('');
                    viewModel.rightpcwidth('');
                    viewModel.rightpcradius('');
                    viewModel.rightbasecurve2('');
                    viewModel.rightsphere2('');
                    viewModel.rightcylinder2('');
                    viewModel.rightaxis2('');
                    viewModel.rightradius2('');
                    viewModel.rightwidth2('');
                    viewModel.rightaddpower('');
                    viewModel.rightprism('');
                    viewModel.rightct('');
                    viewModel.rightet('');
                    viewModel.rightopticalzone('');
                    viewModel.rightradius3('');
                    viewModel.rightwidth3('');
                    viewModel.rightsegheight('');
                    viewModel.rightblend(0);
                    viewModel.righttint(0);
                    viewModel.rightquantity('N/A');
                    viewModel.RStar('');
                } else {
                    viewModel.hasleftulc(true);
                    viewModel.leftmanufacturer(null);
                    viewModel.leftstyle(0);
                    viewModel.leftbasecurve('N/A');
                    viewModel.leftdiameter('N/A');
                    viewModel.leftsphere('N/A');
                    viewModel.leftcylinder('');
                    viewModel.leftaxis('');
                    viewModel.leftpcradius('');
                    viewModel.leftpcwidth('');
                    viewModel.leftbasecurve2('');
                    viewModel.leftsphere2('');
                    viewModel.leftcylinder2('');
                    viewModel.leftaxis2('');
                    viewModel.leftradius2('');
                    viewModel.leftwidth2('');
                    viewModel.leftaddpower('');
                    viewModel.leftprism('');
                    viewModel.leftct('');
                    viewModel.leftet('');
                    viewModel.leftopticalzone('');
                    viewModel.leftradius3('');
                    viewModel.leftwidth3('');
                    viewModel.leftsegheight('');
                    viewModel.leftblend(0);
                    viewModel.lefttint(0);
                    viewModel.leftquantity('N/A');
                    viewModel.LStar('');
                }
                $(rightLeftLen + 'manufacturer,' + rightLeftLen + 'style,' + rightLeftLen + 'blend,' + rightLeftLen + 'tint').each(function () {
                    $(this).selectpicker('refresh');
                    $(this).clearField();
                });
                $(rightLeftLen + 'manufacturer,' + rightLeftLen + 'style').parents("div.bootstrap-select").removeClass('error').removeClass('requiredField');
                $(rightLeftLen + 'basecurve, ' + rightLeftLen + 'diameter,  ' + rightLeftLen + 'sphere, ' + rightLeftLen + 'quantity').each(function () {
                    $(this).clearField();
                });
                $(rightLeftLen + 'basecurve, ' + rightLeftLen + 'diameter,  ' + rightLeftLen + 'sphere, ' + rightLeftLen + 'quantity').removeClass('error').removeClass('requiredField');
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

    function getClViewDetails() {
        var action, reqmode, xdata;
        xdata = {
            officeNumber: window.config.officeNumber,
            patientId: window.patient.PatientId,
            examId: rxExamId,
            isRecheck: isRecheck
        };

        action = "GetHardContactLensRxById";
        reqmode = "get";
        client
            .action(action)[reqmode](xdata)
            .done(function (data) {
                initLoad = true;
                if (data.Source !== 1) {
                    viewModel.setupValidation();
                    viewModel.RStar("*");
                    viewModel.LStar("*");
                } else {
                    if (data.ExamRxTypeId === CLRxType.EXAM_HARDCL_INCOMPLETE) {
                        viewModel.RStar("*");
                        viewModel.LStar("*");
                    } else {
                        viewModel.RStar("");
                        viewModel.LStar("");
                    }
                }

                viewModel.importModel(data);
                if (data.Notes !== null) {
                    $('#noteIdHidden').val(data.Notes[0].Key);
                }

                if (viewModel.rightcylinder() && viewModel.rightcylinder() !== null && viewModel.rightcylinder() > 0) {
                    $('#rightcylinder').val('+' + viewModel.rightcylinder().toString());
                }

                if (viewModel.leftcylinder() && viewModel.leftcylinder() !== null && viewModel.leftcylinder() > 0) {
                    $('#leftcylinder').val('+' + viewModel.leftcylinder().toString());
                }

                if (viewModel.rightcylinder2() && viewModel.rightcylinder2() !== null && viewModel.rightcylinder2() > 0) {
                    $('#rightcylinder2').val('+' + viewModel.rightcylinder2().toString());
                }

                if (viewModel.leftcylinder2() && viewModel.leftcylinder2() !== null && viewModel.leftcylinder2() > 0) {
                    $('#leftcylinder2').val('+' + viewModel.leftcylinder2().toString());
                }

                initLoad = false;
                if (data.Rightulcondition !== 0) {
                    setTimeout(function () {
                        setUnderlyingCondition('#right', 0, data.Rightulcondition);
                    }, 1);
                } else {
                    viewModel.hasrightulc(false);
                }
                if (data.Leftulcondition !== 0) {
                    setTimeout(function () {
                        setUnderlyingCondition("#left", 0, data.Leftulcondition);
                    }, 1);
                } else {
                    viewModel.hasleftulc(false);
                }
                viewModel.dirtyFlag = ko.dirtyFlag(viewModel);

                initialize();
                setTimeout(function () {
                    window.setupEHRFields(data);
                    if (data.ExamRxTypeId === CLRxType.EXAM_HARDCL_INCOMPLETE && data.Rightulcondition === 0) {
                        if (viewModel.rightmanufacturer() === undefined) {
                            $('#right' + 'manufacturer,' + '#right' + 'style').parents("div.bootstrap-select").addClass('requiredField');
                        }
                    }
                }, 1000);
                setDoctorDdls(data);
            });
    }

    $("#btnReset").click(function (e) {
        e.preventDefault();
        $(".summaryMessages").clearMsgBlock();
        //viewModel.importModel(saveExam);
        getClViewDetails();
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
        return false;
    }

    function updateHardClOutsideProvider(name, id) {
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
        window.setupOutsideProviderFields(mode, updateHardClOutsideProvider);
    });

    $(function () {
        viewModel = new window.HardContactLensViewModel({}).commitChanges();
        ko.applyBindings(viewModel);
        loadPatientSideNavigation(window.patient.PatientId, "contactLenses");

        if ($.urlParam(window.location.href, "examId", true) !== null) {
            window.rxExamId = $.urlParam(window.location.href, "examId", true);
        }
        if ($.urlParam(window.location.href, "isRecheck", true) !== null) {
            window.isRecheck = $.urlParam(window.location.href, "isRecheck", true);
        }

        updatePageTitle();
        getClViewDetails();
        viewModel.rightulcondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
            setUnderlyingCondition('#right', oldValue, newValue);
        });
        viewModel.leftulcondition.beforeAndAfterSubscribe(function (oldValue, newValue) {
            setUnderlyingCondition('#left', oldValue, newValue);
        });
        $("#hard").attr('checked', true);
        $("button[data-id='provider']").focus();
    });

    $('#soft').click(function () {
        //var self = this;
        var redirectUrl = window.config.baseUrl + "Patient/ContactLenses?id=" + window.patient.PatientId + "&isRecheck= false";
        window.location.href = redirectUrl;
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

    $("#btnCopy").click(function (e) {
        e.preventDefault();
        if (viewModel.rightmanufacturer() !== "" && viewModel.rightmanufacturer() !== undefined) {
            viewModel.isCopy = true;
            viewModel.leftmanufacturer(viewModel.rightmanufacturer());
            $('#leftmanufacturer').recreateSelectPicker();
            $('#leftmanufacturer, #leftstyle').each(function () {
                $(this).clearField();
                $('select#' + this.name).parents("div.bootstrap-select").removeClass('error').removeClass('requiredField');
            });
            if (viewModel.rightmanufacturer() === viewModel.leftmanufacturer()) {
                viewModel.leftstyle(viewModel.rightstyle());
                $("select#leftstyle").change();
            }
        }
    });

    $("#radioOption3").click(function (e) {
        if ($("#radioOption3").is(":checked")) {
            $("#ProviderDiv").show();
            $("#OutsideProviderDiv").hide();
            viewModel.providerType(0);
            if (viewModel.provider() === 0) {
                $('select#provider').parents("div.bootstrap-select").addClass('requiredField');
            }
        }
    });

    $("#radioOption4").click(function (e) {
        if ($("#radioOption4").is(":checked")) {
            $("#OutsideProviderDiv").show();
            $("#ProviderDiv").hide();
            viewModel.providerType(1);
        }
    });

    $("select").change(function () {
        if ($(this).prop("selectedIndex") !== 0) {
            $(this).clearField();
            $('.bootstrap-select:has(button[data-id="' + $(this).attr('id') + '"])').removeClass('error').removeClass("requiredField");
        }
    });

    $("#rightsphere, #leftsphere, #rightsphere2, #leftsphere2,  #rightcylinder, #leftcylinder,  #rightcylinder2, #leftcylinder2").change(function () {
        if (!isNaN($(this).val())) {
            $(this).val(formattingFormatNumber($(this).val(), $(this).val().substr(0, 1),
                $(this).val().substr(1, $(this).val().length - 1), 2, true, this.name));
        }
    });

    $("#rightaxis, #rightaxis2, #leftaxis, #leftaxis2").change(function () {
        if ($(this).val().length > 0 && !isNaN($(this).val()) && (Number($(this).val()) >= 0)) {
            $(this).val(pad(Math.round($(this).val()).toString(), 3));
        }
    });

    $("#rightbasecurve, #rightbasecurve2, #leftbasecurve, #leftbasecurve2, #rightdiameter, #leftdiameter, " +
        "#rightopticalzone, #leftopticalzone, #rightaddpower, #leftaddpower, #rightpcradius, #leftpcradius, " +
        "#rightpcwidth, #leftpcwidth, #rightradius2, #leftradius2, #rightwidth2, #leftwidth2, #rightprism," +
        " #leftprism, #rightct, #leftct, #rightet, #leftet, #rightradius3, #leftradius3, #rightwidth3, #leftwidth3, #rightsegheight, #leftsegheight").change(function () {
        $(this).val(formattingFormatNumber($(this).val(), $(this).val().substr(0, 1),
                $(this).val().substr(1, $(this).val().length - 1), 2, false, this.name));
    });
}());