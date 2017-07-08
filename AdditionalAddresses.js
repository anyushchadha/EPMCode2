/*jslint browser: true, vars: true, plusplus: true */
/*global $, Modernizr, alert, console, config, confirm,  alslContactLenses, msgType, loadPatientSideNavigation, regex, updatePageTitle, ApiClient*/

var addressTable, transmitData, addressId;
var client = new ApiClient("PatientAddress");
/* show the modal "add" window */
function modalDialog(mode, aData) {
    var i, name = ["#address_1", "#zipcode", "#city", "#state"];
    addressId = 0;
    $("select#addresstype").clearField();

    if (mode === "Edit") {
        addressId = aData.AddressId;
        $("select#addresstype").selectpicker('val', aData.AddressTypeId);
        $("#address_1").val(aData.Address1);
        $("#address_2").val(aData.Address2);
        $("#zipcode").val(aData.ZipCode);
        $('#zipcode').attr('data-text', aData.ZipCode);
        $("#city").val(aData.City);
        $("#state").val(aData.State);
    } else {
        $("select#addresstype").selectpicker('val', '0');
        $('#zipcode').attr('data-text', '');
        $("#address_1, #address_2, #zipcode, #city, #state").each(function () {
            $(this).val('');
        });
    }

    for (i = 0; i < name.length; i++) {
        $(name[i]).clearField();
        if ($(name[i]).val() === '') {
            $(name[i]).addClass("requiredField");
        } else {
            $(name[i]).removeClass("requiredField");
        }
    }
    if ($("select#addresstype").val() === null) {
        $("#addresstype").parents("div.bootstrap-select").addClass("requiredField");
    } else {
        $("#addresstype").parents("div.bootstrap-select").removeClass("requiredField");
    }

    $(".summaryMessages").clearMsgBlock();

    $("#addressModal #modalTitle").html(mode + " Address");
    $("#addressModal").modal({
        keyboard: false,
        backdrop: 'static',
        show: true
    });
    setTimeout(function () {
        $("button[data-id='addresstype']").focus();
    }, 200);
}

/* builds the address table */
var buildAddressTable = function () {
    addressTable = $('#addressTable').alslDataTable({
        "aoColumns": [{
            "sTitle": "AddressType",
            "mData": "AddressTypeId",
            "sType": "integer",
            "bVisible": false,
            "sClass": "left"
        }, {
            "sTitle": "Type",
            "mData": "AddressType",
            "sType": "string",
            "sWidth": "13%",
            "sClass": "left"
        }, {
            "sTitle": "Address 1",
            "mData": "Address1",
            "sType": "string",
            "sWidth": "21%",
            "sClass": "left"
        }, {
            "sTitle": "Address 2",
            "mData": "Address2",
            "sType": "string",
            "sWidth": "21%",
            "sClass": "left"
        }, {
            "sTitle": "City",
            "mData": "City",
            "sType": "string",
            "sWidth": "15%",
            "sClass": "left"
        }, {
            "sTitle": "Zip",
            "mData": "ZipCode",
            "sType": "string",
            "sWidth": "12%",
            "sClass": "left"
        }, {
            "sTitle": "State",
            "mData": "State",
            "sType": "string",
            "sWidth": "7%",
            "sClass": "left"
        }, {
            "sTitle": "",
            "mData": "AddressId",
            "sType": "integer",
            "sWidth": "5%",
            "sClass": "center",
            "mRender": function (data) {
                return '<a class="remove" href="" id="' + data + '"><i class="btn icon-remove" title="Delete Address" data-id="' + data + '"></i></a>';
            }
        }],
        "bAutoWidth": false,
        "bSort": false,
        "oLanguage": {
            "sEmptyTable": "No additional addresses are listed."
        },
        selectableRows: true
    });

    /* click handler for click anywhere on row (except inside the last cell to edit */
    addressTable.delegate("tbody td:not(:last-of-type)", "click", function () {
        if (addressTable.fnPagingInfo().iTotal !== 0) {
            var aPos = addressTable.fnGetPosition(this);
            var aData = addressTable.fnGetData(aPos[0]);
            modalDialog("Edit", aData);
        }
    });
};



/* loads the address table w/data */
function loadAddressTable() {
    client
        .action("Get")
        .get({ "patientId": window.patient.PatientId })
        .done(function (data) {
            addressTable.refreshDataTable(data);
        });
}

/* loads the address type drop-down in the modal window */
function loadAddressType() {
    client
        .action("GetAddressType")
        .get()
        .done(function (data) {
            var i, description, key;
            $("select#addresstype").append($("<option />", { "text": 'Select', "value": 0, "disabled": true }));

            for (i = 0; i < data.length; i++) {
                description = data[i].Description;
                key = data[i].Key;
                $("select#addresstype").append($("<option />", { "text": description, "value": key }));
            }
            $("select#addresstype").selectpicker('refresh');
        });
}

/* hookup form validation */
function validateForm() {
    $("#addAddressForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            addresstype: { required: true, selectBox: true },
            address_1: { required: true, maxlength: 35 },
            address_2: { maxlength: 35 },
            zipcode: { required: true, Regex: regex.ZIP },
            city: { required: true, Regex: regex.ALPHADOT, maxlength: 30 },
            state: { required: true, Regex: regex.STATE }
        },
        messages: {
            addresstype: { required: "Select an Address Type.", selectBox: "Select an Address Type." },
            address_1: { required: "Enter the Address.", maxlength: "Address 1 cannot be more than 35 characters." },
            address_2: { maxlength: "Address 2 cannot be more than 35 characters." },
            zipcode: { required: "Enter the ZIP code.", Regex: "Enter a valid ZIP code." },
            city: { required: "Enter the City.", Regex: "City can only contain alpha characters. Enter a valid City.", maxlength: "City cannot be more than 30 characters." },
            state: { required: "Enter the State.", Regex: "Enter a valid 2 character State." }
        }
    });
}

function getAddressDataToSave() {
    var requestData = {
        PatientId: window.patient.PatientId,
        AddressId: addressId,
        Address: {
            AddressTypeId: $("#addresstype").val(),
            ZipCode: $("#zipcode").val().trim(),
            City: $("#city").val().trim(),
            State: $("#state").val().toUpperCase(),
            Address1: $("#address_1").val().trim(),
            Address2: $("#address_2").val().trim()
        }
    };
    return requestData;
}

function zipCodeSave(status) {
    if (status === 'OK') {
        $("#btnSaveAddress").attr('data-value', 'confirmed').click();
    }
}

// save the address click event.
$('#btnSaveAddress').click(function (e) {
    e.preventDefault();
    $(".summaryMessages").clearMsgBlock();
    if (!$("#addAddressForm").valid()) {
        return;
    }

    var reqMode = (addressId === 0) ? 'post' : 'put';
    var addressData = getAddressDataToSave();
    var isZipCodeConfirmed = $("#btnSaveAddress").attr('data-value') === 'confirmed' ? true : false;
    $("#btnSaveAddress").attr('data-value', '');

    if (!isZipCodeConfirmed) {
        $.isZipCodeValid(['#zipcode'], [addressData.Address.ZipCode], zipCodeSave);
    } else {
        client
            .action(reqMode)[reqMode](addressData)
            .done(function (data) {
                loadAddressTable();
                $(document).showSystemSuccess(data);
                $("#addressModal").modal("hide");
            });
    }
});

//fix IE9 hit enter causes modal dialog to be closed
$("#address_1, #address_2, #zipcode, #city, #state").keydown(function (e) {
    var keyStroke = e.keyCode || e.charCode || e.which;
    return (keyStroke === 13) ? false : true;
});

// delete the address click event.
$('#btnDeleteAddress').click(function () {
    if (transmitData) {
        client
            .action("Delete")
            .queryStringParams()["delete"](transmitData)
            .done(function () {
                loadAddressTable();
                $(document).showSystemSuccess("Address deleted.");
                $('#deleteAddressModal').modal('hide');
            })
            .fail(function (jqXhr) {
                $('#deleteAddressModal').modal('hide');
            });
    }
    transmitData = null;
});

/* click handler for delete icon */
$("#addressTable").on("click", ".remove", function (e) {
    e.preventDefault();
    var $this = $(this);
    var addressId = $this.attr("id");
    transmitData = {
        AddressId: addressId,
        PatientId: window.patient.PatientId
    };

    //$("#confirmDialog").html('Are you sure you want to delete this address?');
    $("#deleteAddressModal").modal({
        title: 'Delete Address',
        keyboard: false,
        backdrop: 'static',
        show: true
    });
});


/* click handler for Add Address button */
$("#addAddressBtn").click(function (e) {
    e.preventDefault();
    modalDialog("Add Additional", null);
});

$(document).ready(function () {
    // load the side nav
    loadPatientSideNavigation(window.patient.PatientId, "additionalAddresses");
    updatePageTitle();
    buildAddressTable();
    loadAddressTable();
    loadAddressType();
    validateForm();

    $("select#addresstype").bind({
        "change": function () {
            $(this).clearField();
            $("#" + this.id).parents("div.bootstrap-select").removeClass("requiredField");
        }
    });
    $("select#addresstype").selectpicker({ "hideDisabled": true });

    $("#divzipcode").alslZipCode(["#zipcode", "#city", "#state"]);
});