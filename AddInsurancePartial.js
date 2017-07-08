/*jslint vars:true, plusplus:true */
/*global ApiClient, window, $, regex, document, ko, showSubscriberInsuranceDialog,
loadPatientSideNavigation, updatePageTitle, blankPatientInsurance, PatientInsuranceinsuranceViewModel, setTimeout*/

var initLoad, patientInsuranceTable, isAppointmentView, pid;
var insuranceClient = new ApiClient("PatientInsurance");
var insuranceViewModel = {
    patientId: Object.fromQueryString(window.location.search.toLowerCase()).id,
    patientInsurance: new window.PatientInsuranceViewModel(blankPatientInsurance)
};

function removeRequiedFieldFromSelectPicker() {
    $("select").each(function () {
        if ($(this).prop("selectedIndex") !== 0) {
            $(this).parents("div.bootstrap-select").removeClass("requiredField");
        }
    });
}

function refreshSelectPickers(args) {
    $('select#carriers').refreshSelectPicker();
    $('select#plans').refreshSelectPicker();
    $('select#relationship').refreshSelectPicker();

    // args is passed from optionsAfterRender ko binding
    // wrapping it in a setTimeout becuase the optionsAfterRender
    // is called before uniqueName. recreateSelectPicker will
    // throw an exception if the selectPicker doesn't have a name
    if (args) {
        window.setTimeout(function () {
            $(args[1]).closest("select").recreateSelectPicker();
        }, 1);
    }

    removeRequiedFieldFromSelectPicker();
}

/* refreshes the insuranceViewModel with the supplied data */
function refreshinsuranceViewModel(data) {
    insuranceViewModel.patientInsurance.importModel(data);
    refreshSelectPickers();
}

/* Clears out the Insurance details after closing the modal dialog */
function clearInsuranceDetails() {
    window.insuranceViewModel.patientInsurance.reset();
    refreshSelectPickers();
    window.insuranceViewModel.patientInsurance.clearSubscriberInfo();
    $("#unknownPhone").mask("?(999) 999-9999");
    if (!isAppointmentView) {
        $("input").not("#subscriberAddress2, #subscriberEmployer").addClass("requiredField");
        $("select").parents("div.bootstrap-select").addClass("requiredField");
    }
}

$('#addPatientInsuranceModal').on('hide.bs.modal', function () {
    clearInsuranceDetails();
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        if ($("#insurancestar").hasClass("required hide")) {
            $("#insurance").refreshSelectPicker();
        } else {
            if (window.viewModel.selectedInsurance() === 0 || window.viewModel.selectedInsurance() === undefined) {
                $("#insurance").addClass("requiredField");
            } else {
                $("#insurance").removeClass("requiredField");
            }
            $("#insurance").recreateSelectPicker();
        }
        $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
        $("#eligibilityStatus").addClass("hidden");
    }
});

$("#btnSubscriberCancel").click(function (e) {
    e.preventDefault();
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        if ($("#insurancestar").hasClass("required hide")) {
            $("#insurance").refreshSelectPicker();
        } else {
            if (window.viewModel.selectedInsurance() === 0 || window.viewModel.selectedInsurance() === undefined) {
                $("#insurance").addClass("requiredField");
            } else {
                $("#insurance").removeClass("requiredField");
            }
            $("#insurance").recreateSelectPicker();
        }
        $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
        $("#eligibilityStatus").addClass("hidden");
    }
});

// builds the patient search grid 
function getPatientInsurances() {
    var activeOnly = $('#chkActiveOnly').is(':checked');
    insuranceClient
        .action("GetPatientInsurances")
        .get({
            "patientId": insuranceViewModel.patientId,
            "activeOnly": activeOnly
        })
        .done(function (data) {
            patientInsuranceTable.refreshDataTable(data);
            $("#chkActiveOnly").prop("checked", activeOnly);
        });
}

$("#chkActiveOnly").click(function (e) {
    e.preventDefault();
    getPatientInsurances();
});

function getPatientInsuranceDataToSave() {
    var unknownCarrierInfo = null;
    if (!$("#unknownDiv").hasClass('hidden')) {
        unknownCarrierInfo = {
            PatientId: insuranceViewModel.patientId,
            InsurancePlanId: $('#plans').val(),
            CarrierId: $('#carriers').val(),
            AddressId: insuranceViewModel.patientInsurance.unknownAddressId(),
            Address1: $('#unknownAddress1').val(),
            Address2: $('#unknownAddress2').val(),
            ZipCode: $('#unknownZipCode').val(),
            City: $('#unknownCity').val(),
            State: $('#unknownState').val(),
            ContactName: $('#unknownContactName').val(),
            PrimaryPhone: $('#unknownPhone').val(),
            Extension: $('#unknownExtension').val(),
            Fax: $('#unknownFax').val()
        };
    }
    var patientInsuranceData = {
        Id: insuranceViewModel.patientInsurance.Id(),
        PatientId: insuranceViewModel.patientId,
        PatientInfo: insuranceViewModel.patientInsurance.patientInfo(),
        InsuranceCarrier: {
            Code: insuranceViewModel.patientInsurance.selectedCarrier()
        },
        PlanId: insuranceViewModel.patientInsurance.selectedPlan(),
        InsuredId: $('#insuredId').val(),
        IsPrimaryInsurance: $('#isPrimaryInsurance').is(':checked'),
        IsActive: insuranceViewModel.patientInsurance.isActive(),
        PolicyGroup: $('#policyGroup').val(),
        RelationToSubscriberId: insuranceViewModel.patientInsurance.selectedRelationship(),
        Subscriber: {
            FirstName: $('#subscriberFirstName').val(),
            LastName: $('#subscriberLastName').val(),
            //Sex: insuranceViewModel.patientInsurance.subscriber().Sex,
            Sex: $("#radioGenderMale").is(':checked') ? "M" : "F",
            Address: {
                Address1: $('#subscriberAddress1').val(),
                Address2: $('#subscriberAddress2').val(),
                City: $('#subscriberCity').val(),
                State: $('#subscriberState').val(),
                ZipCode: $('#subscriberZipCode').val()
            },
            BirthDate: $('#subscriberDob').val(),
            Employer: $('#subscriberEmployer').val(),
            Phone: {
                //PhoneNumber: insuranceViewModel.patientInsurance.subscriberPhone().PhoneNumber.replace(/^\_+|\(+|\)+|\-+|\s+|\s+$/g, '')
                PhoneNumber: $('#subscriberPhone').val().replace(/^\_+|\(+|\)+|\-+|\s+|\s+$/g, '')
            }
        },
        UnknownCarrierInfo: unknownCarrierInfo
    };

    return patientInsuranceData;
}

// does the patient insurance validation
function validatePatientInsuranceInfo() {
    $("#PatientInsuranceSetupForm").alslValidate({
        ignore: ":hidden",
        onfocusout: false,
        onclick: false,
        rules: {
            carriers: { selectBox: true },
            plans: { selectBox: true },
            insuredId: { maxlength: 50, required: true },
            policyGroup: { maxlength: 150 },
            relationship: { selectBox: true },
            subscriberFirstName: { required: true, Regex: regex.NAME, maxlength: 15 },
            subscriberLastName: { required: true, Regex: regex.NAME, maxlength: 25 },
            subscriberAddress1: { required: true, maxlength: 35 },
            subscriberAddress2: { maxlength: 35 },
            subscriberZipCode: { required: true, maxlength: 10, Regex: regex.ZIP },
            subscriberCity: { required: true, maxlength: 30, Regex: regex.ALPHADOT },
            subscriberState: { required: true, maxlength: 2, Regex: regex.STATE },
            subscriberPhone: { required: true, Regex: regex.PHONE },
            gender: { required: true },
            subscriberDob: { required: true, commonDate: true, notFutureDate: true },
            subscriberEmployer: { maxlength: 100 },
            unknownAddress1: { required: true, maxlength: 35 },
            unknownAddress2: { maxlength: 35 },
            unknownZipCode: { required: true, maxlength: 10, Regex: regex.ZIP },
            unknownCity: { required: true, maxlength: 30, Regex: regex.ALPHADOT },
            unknownState: { required: true, maxlength: 2, Regex: regex.STATE },
            unknownPhone: { required: true, Regex: regex.PHONE },
            unknownFax: { Regex: regex.PHONE }
        },
        messages: {
            carriers: { selectBox: "Select a Carrier Name." },
            plans: { selectBox: "Select a Plan Name." },
            insuredId: { maxlength: "Insured ID cannot be more than 50 characters.", required: "Enter the Insured ID" },
            policyGroup: { maxlength: "Policy Group # cannot be more than 150 characters." },
            relationship: { selectBox: "Select a Relationship to Subscriber." },
            subscriberFirstName: {
                required: "Enter the First Name.",
                Regex: "Numbers and/or the special characters you entered are not allowed. Enter a valid First Name.",
                maxlength: "First Name cannot be more than 15 characters."
            },
            subscriberLastName: {
                required: "Enter the Last Name.",
                Regex: "Numbers and/or the special characters you entered are not allowed. Enter a valid Last Name.",
                maxlength: "Last Name cannot be more than 25 characters."
            },
            subscriberAddress1: { required: "Enter the Address.", maxlength: "Address 1 cannot be more than 35 characters." },
            subscriberAddress2: { maxlength: "Address 2 cannot be more than 35 characters." },
            subscriberZipCode: { required: "Enter the ZIP code.", maxlength: "Enter a valid ZIP code.", Regex: "Enter a valid ZIP code." },
            subscriberCity: { required: "Enter the City.", maxlength: "City cannot be more than 30 characters.", Regex: "City can only contain alpha characters. Enter a valid City." },
            subscriberState: { required: "Enter the State.", maxlength: "State cannot be more than 2 characters.", Regex: "Enter a valid 2 character State." },
            subscriberPhone: { required: "Enter a Phone Number.", Regex: "Enter a valid Phone number." },
            gender: { required: "Select a Sex." },
            subscriberDob: { required: "Enter the Date Of Birth.", commonDate: "Enter a valid Date Of Birth.", notFutureDate: "Birthdays can't be in the future. Enter a valid Date Of Birth." },
            subscriberEmployer: { maxlength: "Employer cannot be more than 100 characters." },
            unknownAddress1: { required: "Enter the Address.", maxlength: "Address 1 cannot be more than 35 characters." },
            unknownAddress2: { maxlength: "Address 2 cannot be more than 35 characters." },
            unknownZipCode: { required: "Enter the ZIP code.", maxlength: "Enter a valid ZIP code.", Regex: "Enter a valid ZIP code." },
            unknownCity: { required: "Enter the City.", maxlength: "City cannot be more than 30 characters.", Regex: "City can only contain alpha characters. Enter a valid City." },
            unknownState: { required: "Enter the State.", maxlength: "State cannot be more than 2 characters.", Regex: "Enter a valid 2 character State." },
            unknownPhone: { required: "Enter a Phone Number.", Regex: "Enter a valid Phone number." },
            unknownFax: { Regex: "Enter a valid Fax number." }
        }
    });
}

/* applies masks to applicable field */
function applyMasks() {
    window.setTimeout(function () {
        $("#subscriberPhone").mask("?(999) 999-9999");
        $("button[data-id='carriers']").focus();
    }, 150);
}

// gets the date that can be the max date for a date field so it can't go into the future. i.e. DateOfBirth
function getYearRange() {
    var today, year, month, day, range, startYear, endYear;
    startYear = "1900";
    today = new Date();
    year = today.getFullYear();
    month = today.getMonth() + 1;
    day = today.getDate();
    endYear = year;
    if (month === 12 && day === 31) {
        endYear = year + 1;
    }
    range = startYear + ':' + endYear;
    return range;
}

// resets the modal window to defaults
function resetModal() {
    // clear errors
    $(".summaryMessages").clearMsgBlock();
    $('input, select').each(function () {
        $(this).clearField();
    });
    document.getElementById("useSsn").checked = false;

    // reset required fields if no data present
    $('#subscriberFirstName, #subscriberLastName, #subscriberAddress1, #subscriberZipCode, #subscriberCity, #insuredId, ' +
        '#subscriberState, #subscriberPhone, #subscriberDob, select#plans, select#relationship, select#carriers').each(function () {
        $(this).addClass("requiredField");
        if ($(this).id === 'subscriberZipCode') {
            $(this).attr('data-value', '');
        }
    });
    refreshSelectPickers();
}

function getUnknownEntity() {
    insuranceClient
        .action("GetUnknownCarrierInfoVm")
        .get({ patientId: insuranceViewModel.patientId, planId: insuranceViewModel.patientInsurance.selectedPlan() })
        .done(function (data) {
            insuranceViewModel.patientInsurance.importUnknownEntity(data);

            $("#unknownZipCode").attr('data-text', insuranceViewModel.patientInsurance.unknownZipCode());
            var array = [
                "#unknownAddress2", "#unknownContactName", "#unknownExtension", "#unknownFax",
                "#unknownAddress1", "#unknownZipCode", "#unknownCity", "#unknownState", "#unknownPhone"
            ];

            $.each(array, function (index, value) {
                var valid = true;
                switch (index) {
                case 0:
                case 1:
                case 2:
                case 3:
                    valid = false;
                    break;
                }
                $(value).clearField();
                if (valid) {
                    $(value).addClass('requiredField');
                    if ($(value).val() !== '') {
                        $(value).change();
                    }
                }
            });
        });
}

function displayUnknownEntity() {
    var name, display = false;
    name = $("#carriers > option:selected").text().toLowerCase();
    if (name.indexOf('unknown') >= 0) {
        display = true;
    } else {
        name = $("#plans > option:selected").text().toLowerCase();
        if (name.indexOf('unknown') >= 0) {
            display = true;
        }
    }

    if (display) {
        $("#unknownDiv").removeClass('hidden');
        $('#carrierAddressDiv').addClass('hidden');
        $('#carrierContactDiv').addClass('hidden');
        getUnknownEntity();
    } else {
        $("#unknownDiv").addClass('hidden');
        $('#carrierAddressDiv').removeClass('hidden');
        $('#carrierContactDiv').removeClass('hidden');
    }
}

function getInsuranceDetails(insuranceId) {
    initLoad = true;

    insuranceClient
        .action("GetPatientInsuranceById")
        .get({
            "patientId": insuranceViewModel.patientId,
            "insuranceId": insuranceId
        })
        .done(function (data) {
            //ALSL-3004
            if (insuranceId > 0) {
                if (data.Eligibilities.Eligibilities.count() > 0) {
                    insuranceViewModel.patientInsurance.carrierName(false);
                    insuranceViewModel.patientInsurance.planName(false);
                }
                data.Eligibilities = null;
            }
            if (data.ResponsiblePartyInfo) {
                data.ResponsiblePartyInfo.Employer = ''; //ALSL-3528 
            }

            $('#useSsn').prop('checked', false);
            refreshinsuranceViewModel(data);
            applyMasks();

            initLoad = false;
            displayUnknownEntity();

            if (insuranceViewModel.patientInsurance) {
                if (insuranceViewModel.patientInsurance.insuredId() !== null && insuranceViewModel.patientInsurance.insuredId().length !== 0) {
                    if (insuranceViewModel.patientInsurance.patientInfo().Ssn !== null && insuranceViewModel.patientInsurance.patientInfo().Ssn.length !== 0) {
                        if (insuranceViewModel.patientInsurance.insuredId() === insuranceViewModel.patientInsurance.patientInfo().Ssn) {
                            document.getElementById("useSsn").checked = true;
                        }
                    }
                }
            }
        });
}

/* show the Check VSP partial modal dialog */
function showCheckForVspInsuranceDialog(patientId) {
    insuranceViewModel.patientId = patientId;
    window.subscriberViewModel.patientId = patientId;
    //show Are you VSP Scubscriber? modal
    $('#vspInsuranceModal div.modal-content').addClass("hidden");
    $('#vspInsuranceModal div#modalCheckIfSubscriber').removeClass("hidden");
    $('#vspInsuranceModal').modal({
        height: '500',
        keyboard: false,
        backdrop: 'static',
        show: true
    });

    $("#ssn, #vspMemberId").clearField();
    $("#ssn, #vspMemberId").val('');
    $("#ssn, #vspMemberId").addClass('requiredField');
}

/* show the modal dialog */
function showPatientInsuranceDialog(insuranceId, patientId) {
    var title = "Edit Insurance";
    if (insuranceId === 0) {
        title = "Add Insurance";
        $('#carrierAddressDiv').hide();
        $('#carrierContactDiv').hide();
    }
    insuranceViewModel.patientId = patientId;
    insuranceViewModel.patientInsurance.carrierName(true);
    insuranceViewModel.patientInsurance.planName(true);
    resetModal();
    getInsuranceDetails(insuranceId);
    window.setTimeout(function () {
        if (insuranceId === 0) {
            $("#subscriberZipCode").attr('data-text', '');
        } else {
            $('#insuredId').focus();
            if (insuranceViewModel.patientInsurance) {
                $("#subscriberZipCode").attr('data-text', insuranceViewModel.patientInsurance.subscriberAddress().ZipCode);
            }
        }
    }, 500);

    $('#addPatientInsuranceModal .modal-title').html(title);
    $('#addPatientInsuranceModal').modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });

}

function zipCodeSave(status) {
    if (status === 'OK') {
        $("#btnSaveInsurance").attr('data-value', 'confirmed').click();
    }
}

$('#btnSaveInsurance').click(function (e) {
    e.preventDefault();
    // Work-around for the select validation issue when the first element is selected during Edit
    $('select#carriers').val(insuranceViewModel.patientInsurance.selectedCarrier());
    $('select#plans').val(insuranceViewModel.patientInsurance.selectedPlan());
    $('select#relationship').val(insuranceViewModel.patientInsurance.selectedRelationship());

    var isZipCodeConfirmed = $(this).attr('data-value') === 'confirmed' ? true : false;
    $(this).attr('data-value', '');

    //due to invisible fields, we need an extra validation step for selectpicker
    var validator = $("#PatientInsuranceSetupForm").validate();
    validator.element("#carriers");
    validator.element("#plans");
    validator.element("#relationship");
    var count = validator.numberOfInvalids();
    if (!$("#PatientInsuranceSetupForm").valid() || count > 0) {
        return;
    }

    var patientInsuranceData = getPatientInsuranceDataToSave();
    if (!isZipCodeConfirmed) {
        var valZipCodes = [];
        var idZipCodes = [];
        idZipCodes[0] = '#subscriberZipCode';
        valZipCodes[0] = patientInsuranceData.Subscriber.Address.ZipCode;
        if (!$("#unknownDiv").hasClass("hidden")) {
            idZipCodes[1] = '#unknownZipCode';
            valZipCodes[1] = patientInsuranceData.UnknownCarrierInfo.ZipCode;
        }
        $.isZipCodeValid(idZipCodes, valZipCodes, zipCodeSave);
    } else {
        insuranceClient
            .action("Put")
            .queryStringParams({ "userId": window.config.userId })
            .put(patientInsuranceData)
            .done(function (data) {
                $(".requiredField").each(function () {
                    $(this).parents("div.bootstrap-select").removeClass("requiredField");
                    $(this).clearField();
                });

                if (!isAppointmentView) {
                    $(document).showSystemSuccess("Insurance saved.");
                    getPatientInsurances();
                } else {
                    $("#addApptModal").modal("show");
                    window.viewModel.Insurances(data.Carriers);
                    window.viewModel.selectedInsurance(data.Id);
                    $("#insurance").val(data.Id).change();
                    $("#insurance").refreshSelectPicker();
                    $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
                    $(".quicklink_complete").removeClass("hidden");
                    window.viewModel.statusMessage("Insurance added.");
                }

                $("#addPatientInsuranceModal").modal("hide");
            });
    }
});

/* builds the patient insurnance grid */
function buildPatientInsuranceTable() {
    patientInsuranceTable = $('#patientInsuranceTable').alslDataTable({
        "aaSorting": [],
        "aoColumns": [
            {
                "sTitle": "Id",
                "mData": "Id",
                "sType": "integer",
                "bSearchable": false,
                "bVisible": false
            },
            {
                "sTitle": "Carrier / Plan",
                "mData": "CarrierDisplay",
                "sType": "string",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Group",
                "mData": "GroupName",
                "sType": "string",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Guarantor / Subscriber",
                "mData": "Subscriber.DisplayName",
                "sType": "string",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Date",
                "mData": "InputDate",
                "sType": "date",
                "sClass": "left",
                "bSortable": true
            },
            {
                "sTitle": "Primary",
                "mData": "IsPrimaryInsurance",
                "sType": "string",
                "sClass": "center",
                "bSortable": true,
                "mRender": function (data) {
                    if (data) {
                        return '<input type="checkbox" name="chkgridIsPrimary" data-value="true" value="' + data + '" checked />';
                    }
                    return '<input type="checkbox" name="chkgridIsPrimary" data-value="false" value="' + data + '" />';
                }
            },
            {
                "sTitle": "Active",
                "mData": "IsActive",
                "sType": "string",
                "sClass": "center",
                "bSortable": true,
                "mRender": function (data) {
                    if (data) {
                        return '<input type="checkbox" name="chkgridActive" data-value="true" value="' + data + '" checked />';
                    }
                    return '<input type="checkbox" name="chkgridActive" data-value="false" value="' + data + '" />';
                }
            },
            {
                "mData": "Id",
                "sType": "integer",
                "sClass": "center",
                "bSortable": false,
                "mRender": function (data, mode, insurance) {
                    if (insurance.EligibilityCount) {
                        return "<i class='btn icon-remove' style='display:none' title='Delete Insurance' data-id='" + data + "' ></i>";
                    }
                    return "<i class='btn icon-remove' title='Delete Insurance' data-id='" + data + "' ></i>";
                }
            }
        ],
        "bAutoWidth": false,
        "oLanguage": {
            "sEmptyTable": "Use the fields above to search for insurances"
        },
        "fnInitComplete": function (oSettings) {
            oSettings.oLanguage.sEmptyTable = "No insurances found.";
        },
        selectableRows: true
    });

// Any cell not handled by another click handler edits that row's insurance
    patientInsuranceTable.on("click", "tbody td:not(:nth-child(4), :nth-child(5), :nth-child(6))", function () {
        var aPos, sData;
        if (patientInsuranceTable.fnPagingInfo().iTotal !== 0) {
            aPos = patientInsuranceTable.fnGetPosition(this);
            sData = patientInsuranceTable.fnGetData(aPos[0]);
            showPatientInsuranceDialog(sData.Id, Object.fromQueryString(window.location.search.toLowerCase()).id);
        }
    });

 //Insurance grid used  to have a save button. Now saves happen
 //immediately when any active checkbox changes
    patientInsuranceTable.on("click", ":checkbox", function (e) {
        // So that the grid's global click handler isn't invoked
        e.stopPropagation();

        var totalPages = patientInsuranceTable.fnPagingInfo().iTotalPages;
        var insuranceData = patientInsuranceTable.fnGetData();

        // figure out which checkboxes are checked
        var itemBody = function () {
            var pos = patientInsuranceTable.fnGetPosition(this);
            insuranceData[pos].IsPrimaryInsurance = $(this.cells[4]).find("input").is(':checked');
            insuranceData[pos].IsActive = $(this.cells[5]).find("input").is(':checked');
        };

        if (totalPages > 1) {
            var i, currentPage = patientInsuranceTable.fnPagingInfo().iPage;
            patientInsuranceTable.fnPageChange('first');
            for (i = 0; i < totalPages; i++) {
                if (i > 0) {
                    patientInsuranceTable.fnPageChange('next');
                }
                $('#patientInsuranceTable tbody tr').each(itemBody);
            }
            patientInsuranceTable.fnPageChange(currentPage); //redisplay current page
        } else if (totalPages === 1) {
            $('#patientInsuranceTable tbody tr').each(itemBody);
        }

        // save any changed data
        if (totalPages > 0) {
            // Since this a "Live save" we're no longer displaying the Save toast.
            // Errors and antiforgery token are handled by ApiClient.
            insuranceClient
                .action("SavePatientInsurances")
                .queryStringParams({ patientId: insuranceViewModel.patientId })
                .post(insuranceData)
                .done(function () {
                    $(document).showSystemSuccess("Insurance saved.");
                    if (!isAppointmentView) { getPatientInsurances(); }
                });
        }
    });

    // Delete insurance
    patientInsuranceTable.on("click", ".icon-remove", function (e) {
        e.stopPropagation();
        $(".summaryMessages").clearMsgBlock(false);
        var obj = $(this);
        var id = obj.attr("data-id");
        $('#deleteInsuranceModal').data('id', id).modal({
            keyboard: false,
            backdrop: false,
            show: true
        });
    });
}

/* click handler for Add Insurance button */
$("#btnAddInsurance, #btnAddInsuranceXs").click(function (e) {
    e.preventDefault();
    $('#addApptModal').hide();
    showPatientInsuranceDialog(0,  Object.fromQueryString(window.location.search.toLowerCase()).id);
});

function showCheckingVspDialogYes() {
    $('#vspInsuranceModal div.modal-content').addClass("hidden");
    $('#vspInsuranceModal div#modalCheckingVsp').removeClass("hidden");
    $('#vspInsuranceModal').modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
}

$("#btnCheckVspInsurance").click(function (e) {
    e.preventDefault();
    showCheckForVspInsuranceDialog(Object.fromQueryString(window.location.search.toLowerCase()).id);
});


$("#btnModalCheckIfSubscriberNo").click(function (e) {
    e.preventDefault();
    showSubscriberInsuranceDialog(0);
});

$("#btnConfirmation").click(function () {
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        if ($("#insurancestar").hasClass("required hide")) {
            $("#insurance").refreshSelectPicker();
        } else {
            if (window.viewModel.selectedInsurance() === 0 || window.viewModel.selectedInsurance() === undefined) {
                $("#insurance").addClass("requiredField");
            } else {
                $("#insurance").removeClass("requiredField");
            }
            $("#insurance").recreateSelectPicker();
        }
        $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
        $("#eligibilityStatus").addClass("hidden");
    }
});

function showCheckforVspErrorModal(msg) {
    $("#btnIgnoreConfirmation").hide();
    $("#checkForVspErrorDialog .modal-body #errorMsgs").html(msg);
    $("#checkForVspErrorDialog").modal({
        keyboard: false,
        backdrop: false,
        show: true
    });
}

function handleAddVspInsurancePlanResponse(response) {
    $('#vspInsuranceModal div#modalCheckingVsp').addClass("hidden");
    if (response.error) {
        if (response.error === "Patient SSN must be 4 or 9 digits.") {
            $('#vspInsuranceModal div#modalMissingDemographicsInfo').removeClass("hidden");
        } else {
            if (isAppointmentView) {
                showCheckforVspErrorModal(response.error);
            } else {
                $(document).showSystemFailure(response.error);
            }
            $("#vspInsuranceModal").modal("hide");
        }
        return;
    }

    $("#vspInsuranceModal").modal("hide");

    if (isAppointmentView) {
        var i, j;
        var preInsurances = [];
        var newInsurances = [];
        for (i = 0; i < window.viewModel.Insurances().length; i++) {
            preInsurances.push(window.viewModel.Insurances()[i].Key);
        }
        $("#addApptModal").modal("show");
        window.viewModel.Insurances(response.insuranceList);
        for (j = 0; j < response.insuranceList.length; j++) {
            newInsurances.push(response.insuranceList[j].Key);
        }
        // get the differences between old and new insurance ddl.
        var diff = $(newInsurances).not(preInsurances).get();
        if (response.addedCount === 1) {
            window.viewModel.selectedInsurance(diff[1]);
        } else {
            if (response.selectedInsurance !== 0) {
                window.viewModel.selectedInsurance(response.selectedInsurance);
            }
        }
        //$("#insurance").recreateSelectPicker();
        if ($("#insurancestar").hasClass("required hide")) {
            $("#insurance").refreshSelectPicker();
        } else {
            if (window.viewModel.selectedInsurance() === 0 || window.viewModel.selectedInsurance() === undefined) {
                $("#insurance").addClass("requiredField");
            } else {
                $("#insurance").removeClass("requiredField");
            }
            $("#insurance").recreateSelectPicker();
        }
        $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
        $(".quicklink_complete").removeClass("hidden");
        $("#insurance").val(window.viewModel.selectedInsurance()).change();
        $("#eligibilityStatus").addClass("hidden");
        window.viewModel.statusMessage(response.addedCount + " VSP Insurances added.");
    } else {
        $(document).showSystemSuccess(response.addedCount + " VSP Insurances added.");
        if (response.addedCount) {
            getPatientInsurances();
        }
    }
}

$("#btnModalCheckIfSubscriberYes").click(function (e) {
    e.preventDefault();
    $('#vspInsuranceModal div.modal-content').addClass("hidden");

    // show Checking VSP modal
    showCheckingVspDialogYes();

    insuranceClient
        .action("AddVspInsurancePlansToMemberPatient")
        .post({ PatientId: insuranceViewModel.patientId, Ssn: "" })
        .done(function (data) {
            handleAddVspInsurancePlanResponse(data);
        })
        .fail(function () {
            $('#vspInsuranceModal').modal("hide");
        });

    // VSP Modal has it's own spinner
    document.stopLoader();
});

$('#btnModalIgnoreConfirmation').click(function (e) {
    e.preventDefault();
    var insuranceId = $('#confirmationDataChangeDialog').data('id');
    $('#confirmationDataChangeDialog').modal('hide');
    showPatientInsuranceDialog(insuranceId,  Object.fromQueryString(window.location.search.toLowerCase()).id);
});

$('#btnModalIgnoreSearchConfirmation').click(function (e) {
    e.preventDefault();
    $('#confirmationSearchDialogModal').modal('hide');
    getPatientInsurances();
});

$('#btnModalMissingSubscriberInfoFindVsp').click(function (e) {

    e.preventDefault();
    $('#vspInsuranceModal div.modal-content').addClass("hidden");

    // show Checking VSP modal
    showCheckingVspDialogYes();

    insuranceClient
        .action("AddVspInsurancePlansToMemberPatient")
        .post({
            PatientId: insuranceViewModel.patientId,
            Ssn: $("#ssn").val() || $("#vspMemberId").val()
        })
        .done(function (data) {
            handleAddVspInsurancePlanResponse(data);
        })
        .fail(function () {
            $('#vspInsuranceModal').modal("hide");
        });

    // VSP Modal has it's own spinner
    document.stopLoader();
});

$('#btnModalMissingSubscriberInfoCancel').click(function (e) {
    e.preventDefault();
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        if ($("#insurancestar").hasClass("required hide")) {
            $("#insurance").refreshSelectPicker();
        } else {
            if (window.viewModel.selectedInsurance() === 0 || window.viewModel.selectedInsurance() === undefined) {
                $("#insurance").addClass("requiredField");
            } else {
                $("#insurance").removeClass("requiredField");
            }
            $("#insurance").recreateSelectPicker();
        }
        $('.bootstrap-select:has(button[data-id="insurance"])').find("ul li:eq(-4)").append("<li class= 'divider'></li>");
        $("#eligibilityStatus").addClass("hidden");
    }
});

$('#btnDeleteInsurance').click(function () {
    var id = $('#deleteInsuranceModal').data('id');
    insuranceClient
        .action("Delete")
        .queryStringParams({ "patientId": insuranceViewModel.patientId, "id": id })["delete"]()
        .done(function () {
            getPatientInsurances();
            $(document).showSystemSuccess("Insurance deleted.");
            $('#deleteInsuranceModal').modal('hide');
        })
        .fail(function () {
            $('#deleteInsuranceModal').modal('hide');
        });
});

$("#checkForVspErrorDialog").on("hidden.bs.modal", function () {
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        $("#eligibilityStatus").addClass("hidden");
    }
});

$("#btnmodalSubscriberInfoClose").click(function () {
    if (isAppointmentView) {
        $("#addApptModal").modal("show");
        $("#eligibilityStatus").addClass("hidden");
    }
});


// sets the focus to the date of birth textbox when the user clicks on the date picker icon
$("#icon_datePicker").click(function () {
    $("#subscriberDob").focus();
});

// cleanup validation
function cleanupValidation() {
    $('#subscriberFirstName, #subscriberLastName, #subscriberAddress1, #subscriberZipCode, #subscriberCity, #subscriberState, ' +
        '#subscriberPhone, #subscriberDob, #radioGenderMale, #radioGenderFemale').each(function () {
        if ($(this).hasClass('error')) {
            if (this.id === 'radioGenderMale' || this.id === 'radioGenderFemale') {
                $(this).removeClass('error');
                $(this).clearField();
            } else {
                if ($(this).val() === '') {
                    $(this).removeClass('error').addClass('requiredField');
                    $(this).clearField();
                } else {
                    $(this).change();
                }
            }
        } else if ($(this).val() === '') {
            $(this).addClass('requiredField');
            if (this.id === 'subscriberPhone') {
                applyMasks();
            }
        }
    });
}

// Validation events, main ones are the carriers and the relationships
function validationEvents() {
    // carrier select box change event
    $('#carriers').change(function () {
        $(this).clearField();
        var carrierId = $(this).val();
        if (carrierId === '0' || carrierId === "" || initLoad) {
            return;
        }

        insuranceClient
            .action("GetCarrierInformation")
            .get({ "carrierId": carrierId })
            .done(function (data) {
                var value = (data.Plans.length === 1) ? data.Plans[0].Key : 0;
                insuranceViewModel.patientInsurance.planList(data.Plans);
                insuranceViewModel.patientInsurance.planList.unshift({ "Key": 0, "Description": 'Select' });
                insuranceViewModel.patientInsurance.selectedPlan(value);
                $("select#plans").refreshSelectPicker();
                $("select#plans").change();

                insuranceViewModel.patientInsurance.carrierAddress1(data.Address.Address1);
                insuranceViewModel.patientInsurance.carrierAddress2(data.Address.Address2);
                insuranceViewModel.patientInsurance.carrierCity(data.Address.City);
                insuranceViewModel.patientInsurance.carrierState(data.Address.State);
                insuranceViewModel.patientInsurance.carrierZipCode(data.Address.ZipCode);
                insuranceViewModel.patientInsurance.carrierContact(data.Contact);
                insuranceViewModel.patientInsurance.carrierPhone(data.Phone.PhoneNumber);
                insuranceViewModel.patientInsurance.carrierPhoneExtension(data.PhoneExtension);
                insuranceViewModel.patientInsurance.carrierFax(data.Fax.PhoneNumber);
                displayUnknownEntity();
            });
    });

    // insurance plans change event.
    $('#plans').change(function () {
        $(this).clearField();
        var val = $(this).val() || '0';
        if (val === '0') {
            $(this).parents("div.bootstrap-select").addClass('requiredField');
        } else {
            $(this).parents("div.bootstrap-select").removeClass('requiredField');
        }
        if (val !== '0' && initLoad === false) {
            insuranceViewModel.patientInsurance.selectedPlan($(this).val());
            displayUnknownEntity();
        }
    });

    // relationship to insured change event.
    $('#relationship').change(function () {
        $(this).clearField();
        if ($(this).val() !== '0' && initLoad === false) {
            var obj, relationId = $(this).val();
            switch (relationId) {
            case '101': //unknown
            case '301': //Self
                obj = $.extend(true, {}, insuranceViewModel.patientInsurance.patientInfo()); //clone object
                insuranceViewModel.patientInsurance.subscriber(obj);
                insuranceViewModel.patientInsurance.subscriberAddress(obj.Address);
                insuranceViewModel.patientInsurance.subscriberPhone(obj.Phone);
                cleanupValidation();
                break;
            case '302': //Spouse
            case '303': //Child
            case '304': //Student
                obj = $.extend(true, {}, insuranceViewModel.patientInsurance.responsiblePartyInfo()); //clone object
                insuranceViewModel.patientInsurance.subscriber(obj);
                insuranceViewModel.patientInsurance.subscriberAddress(obj.Address);
                insuranceViewModel.patientInsurance.subscriberPhone(obj.Phone);
                cleanupValidation();
                break;
            default:
                insuranceViewModel.patientInsurance.subscriber("");
                insuranceViewModel.patientInsurance.subscriberAddress("");
                insuranceViewModel.patientInsurance.subscriberPhone("");
                cleanupValidation();
                break;
            }
        }
    });
}

$('#useSsn').click(function () {
    if ($(this).prop('checked') === true) {
        insuranceViewModel.patientInsurance.insuredId(insuranceViewModel.patientInsurance.patientInfo().Ssn);
    }
    if ($(this).prop('checked') === false) {
        insuranceViewModel.patientInsurance.insuredId('');
    }
    $("#insuredId").change();
});

$('select').change(function () {
    if ($(this).prop("selectedIndex") !== 0) {
        $('.bootstrap-select:has(button[data-id="' + $(this).attr('id') + '"])').removeClass('requiredField');
    }
});

$(document).ready(function () {
    $('#subscriberDob').datepicker({
        changeYear: true,
        changeMonth: true,
        constrainInput: true,
        onChangeMonthYear: function (y, m, i) {
            window.changeMonthYear(y, m, i);
        }
    });
    $("#subscriberDob").datepicker("option", "yearRange", getYearRange());
    validatePatientInsuranceInfo();

    $("#divzipcode").alslZipCode(["#subscriberZipCode", "#subscriberCity", "#subscriberState"]);
    $("#unknownZipCodeDiv").alslZipCode(["#unknownZipCode", "#unknownCity", "#unknownState"]);

    ko.applyBindings(insuranceViewModel, $('#addInsurance')[0]);
    $("#unknownPhone").mask("?(999) 999-9999");
    $("#unknownFax").mask("?(999) 999-9999");

    $('#vspMemberId').keydown(function (evt) {
        return $.fn.alphaNumericFilter(evt);
    });

    $('#ssn').keydown(function (evt) {
        return $.fn.integerFilter(evt);
    });

    validationEvents();
});
