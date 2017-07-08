/*jslint browser: true, plusplus: true, vars: true, nomen:true, sloppy: true, regexp: true */
/*global $, document, alert, Json, window, ko, ApiClient, regex, validRegex, ViewModelBase, msgType, storageKeys, CryptoJS */
var ViewModel = ViewModelBase.extend({
    // ReSharper restore InconsistentNaming
    mappedProperties: [
        "PracticeLocationId",
        "Action",
        "LoginName",
        "CompanyZipCode",
        "CompanyPhoneNumber",
        "SecurityQuestion",
        "SecurityAnswer",
        "RedirectUrl",
        "NumberOfPasswordResetAttempts",
        "Password",
        "ConfirmPassword"
    ],
    constructor: function () {
        ViewModel.__super__.constructor.call(this);
        this.numberOfPasswordResetAttempts(0);
        this.action("StepOne");
    },
    toModel: function () {
        var model = ViewModel.__super__.toModel.apply(this, arguments);
        model.CompanyPhoneNumber = (model.CompanyPhoneNumber || "").replace("/[^0-9]/g", "");
        return model;
    }
});

var client = new ApiClient(window.config.baseUrl, "Login");
var viewModel = new ViewModel();
var container, top, middle, bottom;
var key = CryptoJS.enc.Utf8.parse("5990678078928356");
var iv = CryptoJS.enc.Utf8.parse("5990678078928356");

function submitHandler(form, e) {
    e.preventDefault();
    $(".btn:visible").click();
}

function goToLoginPage() {
    window.location.href = window.config.baseUrl + "Login";
}

function decrypt(data) {
    return CryptoJS.AES.decrypt(data, key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    }).toString(CryptoJS.enc.Utf8);
}

function encrypt(data) {
    return CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(data), key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
}

function validateVerifyUserForm() {
    $("#companyPhoneNumber").mask("?(999) 999-9999");

    $("form").alslValidate({
        ignore: ":hidden", // This is jQuery Validation default but alslValidate overrides it
        rules: {
            practiceLocationId: {
                required: true
            },
            loginName: {
                required: true
            },
            companyZipCode: {
                required: true,
                Regex: regex.ZIP
            },
            companyPhoneNumber: {
                required: true,
                Regex: regex.PHONE
            },
            securityAnswer: {
                required: true
            },
            password: {
                required: true
            },
            confirmPassword: {
                required: true,
                equalTo: "#password"
            }
        },
        messages: {
            practiceLocationId: {
                required: "Enter an Office Identifier."
            },
            loginName: {
                required: "Enter a Login Name."
            },
            companyZipCode: {
                required: "Enter the Company Zip Code.",
                Regex: "Enter a valid ZIP code."
            },
            companyPhoneNumber: {
                required: "Enter the Company Phone Number.",
                Regex: "Enter a valid Phone Number."
            },
            securityAnswer: {
                required: "Enter your security answer."
            },
            password: {
                required: "Enter a Password."
            },
            confirmPassword: {
                required: "Confirm Password.",
                equalTo: "Password and confirmation do not match."
            }
        },
        submitHandler: submitHandler
    });
}

function getLockoutContacts(result) {
    var lockWarning = "Contact ";
    if (result.Note) {
        /*ignore jslint start*/
        lockWarning += (result.Note.indexOf(" or ") ? "one of your Administrators, " : "your Administrator, ") + " {admins}";
        /*ignore jslint end*/
    }

    lockWarning += " or Eyefinity Customer Care at 1.800.942.5353 if you need assistance.";
    /*ignore jslint start*/
    var admins = result.Note ? result.Note : "";
    /*ignore jslint end*/
    return lockWarning.assign({admins: admins});
}

function doneProcessing(result) {
    if (result.PasswordCheckResult === null) {
        $(document).showSummaryMessage(msgType.ERROR, ["The Office Identifier, Login Name, Company ZipCode and/or Company Phone Number is incorrect. Please enter correct information in order to proceed further."]);
    }
    var errors;
    var left = 5 - result.NumberOfPasswordResetAttempts;
    switch (result.PasswordCheckResult) {
    case "NoSecurityQuestion":
        $("#verifyua").css("display", "none");
        $("p#admins").text(getLockoutContacts(result));
        $("#login").hide();
        $("#noquestion").css("display", "block");
        break;
    case "MaxFailedPasswordAnswerAttemptReached":
        $(document).showSummaryMessage(msgType.ERROR, ["You have exceeded the maximum number of login attempts and your account is locked." + getLockoutContacts(result)]);
        break;
    case "EmployeeInactive":
        errors = ["<br/>You are either not authorized or your role is not active to perform this operation." + getLockoutContacts(result)];
        $(document).showSummaryMessage(msgType.ERROR, errors);
        break;
    case "UserNotFound":
        errors = ["<br/>You are either not authorized or your role is not active to perform this operation."];
        $(document).showSummaryMessage(msgType.ERROR, errors);
        break;
    case "ZipCodeNotValid":
    case "PhoneNumberNotValid":
    case "CompanyDoesNotExist":
        errors = ["The Office Identifier, Login Name, Company ZipCode and/or Company Phone Number is incorrect. Please enter correct information in order to proceed further."];
        if (result.NumberOfPasswordResetAttempts > 2) {
            if (left > 0) {
                errors.push("<br/>Your account will be locked after {left} more failed attempts. ".assign({left: left}) + getLockoutContacts(result));
            } else {
                errors = ["You have exceeded the maximum number of attempts and your account is locked. " + getLockoutContacts(result)];
            }
        }
        $(document).showSummaryMessage(msgType.ERROR, errors, undefined, "Incorrect login information");
        break;
    case "SecurityAnswerNotvalid":
        errors = ["The Security answer is incorrect. Please enter correct information in order to proceed further."];
        if (result.NumberOfPasswordResetAttempts > 2) {
            if (left > 0) {
                errors.push("<br/>Your account will be locked after {left} more failed attempts. ".assign({left: left}) + getLockoutContacts(result));
            } else {
                errors = ["You have exceeded the maximum number of attempts and your account is locked. " + getLockoutContacts(result)];
            }
        }
        $(document).showSummaryMessage(msgType.ERROR, errors, undefined, "Incorrect login information");
        break;
    case "VerifiedPracticeInfoSuccess":
        $("#verifyua").css("display", "none");
        $("#lblsecurityQuestion").text(result.SecurityQuestion);
        $("#verifysq").css("display", "block");
        break;
    case "UserIdentityVerifiedSuccessfully":
        $("#verifysq").css("display", "none");
        $("#pwdPolicy").html(result.PasswordPolicy);
        $("#resetpwd").css("display", "block");
        break;
    case "MeetsPolicy":
        $(document).showSystemSuccess("New password successfully saved.");
        setTimeout(function () {
            goToLoginPage();
        }, 500);
        break;
    case "FailsPolicy":
        if ($("#pwdPolicyLabel")[0].innerHTML === "") {
            $(document).showSummaryMessage(msgType.ERROR, ["All passwords must be 8-15 characters."], true);
        } else {
            $(document).showSummaryMessage(msgType.ERROR, [
                "All passwords must be 8-15 characters, " +
                    "contain at least one numeric character, and abide by your company’s password policy. " +
                    "Your new password must be different from your last 5 passwords."], true);
        }
        break;
    }
    $(window).resize();
}

function failProcessing(xhr) {
    if (xhr.statusText === "abort") {
        xhr.handled = true;
    } else if (xhr.status === 500) {
        $(document).showSummaryMessage(msgType.SERVER_ERROR, "An error occurred while performing Reset password operation.", true);
        window.scrollTo(0, 0);
    }
}

var getViewModel = function () {
    var model = viewModel.toModel();
    var companyPhoneNumber = $("#companyPhoneNumber").val();
    model.CompanyPhoneNumber = companyPhoneNumber.replace(/[^0-9]/g, "");
    model.loginName = $("#hdloginName").val();
    model.securityAnswer = $("#hdsecurityAnswer").val();
    model.password = $("#hdpassword").val();
    model.confirmPassword = $("#hdconfirmPassword").val();
    return model;
};

$("#btnReturnLogin").click(function (e) {
    e.preventDefault();
    goToLoginPage();
});

$("#btnStepOne").click(function (e) {
    $(".summaryMessages").clearMsgBlock();
    e.preventDefault();
    if (!$("form").valid()) {
        return;
    }

    $("#hdloginName").val(encrypt($("#loginName").val()));
    viewModel.action("StepOne");
    client
        .action("ResetPassword")
        .post(getViewModel())
        .done(function (result) {
            doneProcessing(result);
            viewModel.action("StepTwo");
            $("#securityAnswer").focus();
        })
        .fail(function (xhr) {
            failProcessing(xhr);
        });
    $(window).resize();
});

$("#btnStepTwo").click(function (e) {
    $(".summaryMessages").clearMsgBlock();
    e.preventDefault();
    if (!$("form").valid()) {
        return;
    }

    $("#hdloginName").val(encrypt($("#loginName").val()));
    $("#hdsecurityAnswer").val(encrypt($("#securityAnswer").val()));

    viewModel.action("StepTwo");
    client
        .action("ResetPassword")
        .post(getViewModel())
        .done(function (result) {
            doneProcessing(result);
            viewModel.action("StepThree");
            $("#password").focus();
        })
        .fail(function (xhr) {
            failProcessing(xhr);
        });
    $(window).resize();
});

$("#btnStepThree").click(function (e) {
    $(".summaryMessages").clearMsgBlock();
    e.preventDefault();
    if (!$("form").valid()) {
        return;
    }

    $("#hdloginName").val(encrypt($("#loginName").val()));
    $("#hdsecurityAnswer").val(encrypt($("#securityAnswer").val()));
    $("#hdpassword").val(encrypt($("#password").val()));
    $("#hdconfirmPassword").val(encrypt($("#confirmPassword").val()));

    viewModel.action("StepThree");
    client
        .action("ResetPassword")
        .post(getViewModel())
        .done(function (result) {
            doneProcessing(result);
        })
        .fail(function (xhr) {
            failProcessing(xhr);
        });
    $(window).resize();
});

function queryString(key) {
    /*ignore jslint start*/
    key = key.replace(/[*+?^$.\[\]{}()|\\\/]/g, "\\$&"); //// escape RegEx control chars
    /*ignore jslint end*/
    var match = location.search.match(new RegExp("[?&]" + key + "=([^&]+)(&|$)"));
    return match && decodeURIComponent(match[1].replace(/\+/g, " "));
}

var getPracticeLocationId = function () {
    return queryString("officeNumber");
};

var getLoginName = function () {
    return queryString("userName");
};


$("#login").click(function (e) {
    e.preventDefault();
    goToLoginPage();
});

// Initial load events
$(document).ready(function () {
    $("#container").removeClass("hidden");
    $("#hdloginName").val("").change();
    var loginName = getLoginName();

    if (loginName !== null) {
        $("#loginName").val(decrypt(loginName)).change();
    }

    viewModel.loginName(getLoginName());
    viewModel.practiceLocationId(getPracticeLocationId());

    if (viewModel.practiceLocationId()) {
        $("#loginName").focus();
    } else {
        $("#practiceLocationId").focus();
    }

    if (viewModel.loginName()) {
        $("#companyZipCode").focus();
    }
    ko.applyBindings(viewModel);
    validateVerifyUserForm();

    container = $("#container");
    top = $("#top");
    middle = $("#middle");
    bottom = $("#containerbottom");
    var passwordLink = $("#loginLink");

    $(window).resize(function () {
        var isLarge = $("#lg").is(":visible");
        $(document.body).toggleClass("lg", isLarge);

        var windowHeight = $(window).height();
        var targetHeight = Math.min(isLarge ? 799 : windowHeight, windowHeight);
        var containerHeight = container.outerHeight();
        var padding = Math.floor((targetHeight - containerHeight) / 2);
        padding += parseInt(middle.css("paddingTop"), 10);

        if (passwordLink) {
            passwordLink.css({
                paddingTop: padding
            });
        }

        middle.css({
            paddingTop: padding,
            paddingBottom: passwordLink ? 0 : padding
        });
        container.css({
            marginTop: Math.max(Math.floor(windowHeight - container.outerHeight()) / 2, 0)
        });
    });

    $(document).on("focusout", function () {
        $(window).resize();
    });

    $(window).resize();

    $(".tt").popover({
        container: "body",
        html: true
    });

    $("body").on("click", ".popover.popover.fade.in", function () {
        $(".popover.fade.in").removeClass("in").addClass("out");
    });
});