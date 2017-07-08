/*jslint browser: true, vars: true, nomen:true*/
/*global $, document, alert, Json, window, ko, ApiClient, validRegex, ViewModelBase, msgType, storageKeys, DOMException, CryptoJS, decrypt */
// ReSharper disable InconsistentNaming
var ViewModel = ViewModelBase.extend({
    // ReSharper restore InconsistentNaming
    mappedProperties: [
        "PracticeLocationId",
        "Action",
        "LoginName",
        "OldPassword",
        "Password",
        "ConfirmPassword",
        "RememberMyUsername",
        "RememberOfficeNumber",
        "EmergencyAccess",
        "RedirectUrl",
        "NumberOfAttempts",
        "CompanyZipCode",
        "CompanyPhoneNumber",
        "SecurityQuestion",
        "SecurityAnswer",
        "NumberOfPasswordResetAttempts"
    ],
    constructor: function () {
        ViewModel.__super__.constructor.call(this);
        this.emergencyAccess(false);
        this.rememberOfficeNumber(false);
        this.rememberMyUsername(false);
        this.numberOfAttempts(0);
        this.numberOfPasswordResetAttempts(0);
    },
    toModel: function () {
        var model = ViewModel.__super__.toModel.apply(this, arguments);

        model.Action = "Verify";
        return model;
    }
});

var client = new ApiClient(window.config.baseUrl, "Login");
var viewModel = new ViewModel();
var container, top, middle, bottom;
var key = CryptoJS.enc.Utf8.parse("5990678078928356");
var iv = CryptoJS.enc.Utf8.parse("5990678078928356");

var showResetPassword = function (data) {
    $("#password").val("");
    $("#oldPassword, #newPassword, #newConfirmPassword").val("");
    $("#oldPassword, #newPassword, #newConfirmPassword").addClass("requiredField");
    $("#oldPassword").clearField();
    $("#newPassword").clearField();
    $("#newConfirmPassword").clearField();
    $("#resetYourPassword").clearField();
    if (data.PasswordPolicy === "") {
        $("#pwdPolicyLabel")[0].innerHTML = "";
    }
    $("#pwdPolicy")[0].innerHTML = data.PasswordPolicy;
    $("#newPasswordModal").modal({
        keyboard: false,
        backdrop: false,
        show: true
    });
    $("#oldPassword").focus();
};

var saveRemembers = function (officeId) {
    try {
        window.sessionStorage.setItem(window.storageKeys.officeId, officeId);

        if (viewModel.rememberMyUsername()) {
            window.localStorage.setItem(window.storageKeys.loginName, viewModel.loginName());
        } else {
            window.localStorage.removeItem(window.storageKeys.loginName);
            viewModel.loginName("");
        }

        if (viewModel.rememberOfficeNumber()) {
            window.localStorage.setItem(window.storageKeys.practiceLocationId, viewModel.practiceLocationId());
        } else {
            window.localStorage.removeItem(window.storageKeys.practiceLocationId);
        }
    } catch (e) {
        if (e.code === DOMException.QUOTA_EXCEEDED_ERR) {
            if (sessionStorage.length) {
                //// Workaround for abug seen on iPads. Sometimes the quota exceeded error comes up and simply
                //// removing/resetting the storage can work.
                sessionStorage.removeItem("window.storageKeys.officeId");
                sessionStorage.setItem(window.storageKeys.officeId, officeId);
            } else {
                //// Otherwise, we're probably private browsing in Safari, so we'll ignore the exception and alert users.
                alert("Your browser is set to Private Browsing, which may cause some settings in Eyefinity Practice Management to not work properly. To avoid these errors, change your browser settings to disable Private Browsing.");
            }
        } else {
            throw e;
        }
    }
};

var loadRemembers = function () {
    var loginName = window.localStorage.getItem(window.storageKeys.loginName);
    var practiceLocationId = window.localStorage.getItem(window.storageKeys.practiceLocationId);

    if (loginName) {
        $("#loginName").val(decrypt(loginName)).change();
        viewModel.loginName(loginName);
        viewModel.rememberMyUsername(true);
    }

    if (practiceLocationId) {
        viewModel.practiceLocationId(practiceLocationId);
        viewModel.rememberOfficeNumber(true);
    }
};

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

function queryString(key) {
    var item;
    key = key.toLowerCase();
    if (!queryString.parsed) {
        queryString.parsed = Object.fromQueryString(window.location.search);
        for (item in queryString.parsed) {
            if (queryString.parsed.hasOwnProperty(item)) {
                queryString.parsed[item.toLowerCase()] = queryString.parsed[item];
            }
        }
    }
    var returnValue = queryString.parsed[key];
    if (!returnValue) {
        returnValue = "";
    }
    return returnValue;
}

var getRedirectUrl = function () {
    return queryString("ReturnUrl") || window.config.baseUrl;
};

$("#btnSave").click(function (e) {
    e.preventDefault();
    if (!$("#newPasswordForm").valid()) {
        return;
    }
    var vm = viewModel.toModel();

    vm.Action = "Reset";
    $("#hdoldPassword").val(encrypt($("#oldPassword").val()));
    $("#hdnewPassword").val(encrypt($("#newPassword").val()));
    $("#hdnewConfirmPassword").val(encrypt($("#newConfirmPassword").val()));
    vm.PasswordPolicy = $("#pwdPolicy")[0].innerHTML;
    vm.loginName = $("#hdloginName").val();
    vm.OldPassword = $("#hdoldPassword").val();
    vm.Password = $("#hdnewPassword").val();
    vm.ConfirmPassword = $("#hdnewConfirmPassword").val();
    client
        .action("ProcessPassword")
        .post(vm)
        .done(function (result) {
            switch (result.PasswordCheckResult) {
            case "MeetsPolicy":
                $(document).showSystemSuccess("New password successfully saved.");
                $("#newPasswordModal").modal("hide");
                $("#password").focus();
                break;
            case "FailsPolicy":
                if ($("#pwdPolicyLabel")[0].innerHTML === "") {
                    $("#resetYourPassword").showFieldMessage(msgType.ERROR, ["All passwords must be 8-15 characters."], true);
                } else {
                    $("#resetYourPassword").showFieldMessage(msgType.ERROR, [
                        "All passwords must be 8-15 characters, " +
                            "contain at least one numeric character, and abide by your company’s password policy. " +
                            "Your new password must be different from your last 5 passwords."
                    ], true);
                }
                break;
            case "NoMatch":
                $("#resetYourPassword").showFieldMessage(msgType.ERROR, ["Incorrect password. Please try again."], true);
                break;
            }
        });
});

function getPasswordResetQueryString() {
    var officeId = "";
    var userName = "";
    if (viewModel.practiceLocationId()) {
        officeId = viewModel.practiceLocationId();
    }
    var loginName = $("#loginName").val();
    if (loginName !== "") {
        $("#hdloginName").val(encrypt(loginName)).change();
        if (viewModel.loginName($("#hdloginName").val())) {
            userName = viewModel.loginName();
        }
    }

    return "?officeNumber=" + officeId + "&userName=" + userName;
}

$("#passwordReset").click(function (e) {
    window.location.href = window.config.baseUrl + $(this).attr("data-url") + getPasswordResetQueryString();
});

function validateNewPassword() {
    $.validator.addMethod("characters", function (value, element) {
        if (element.value.length < 8) {
            return false;
        }
        return true;
    });

    $.validator.addMethod("notEqual", function (value, element, param) {
        return this.optional(element) || value !== $(param).val();
    }, false);

    $("#newPasswordForm").alslValidate({
        onfocusout: false,
        onclick: false,
        rules: {
            oldPassword: {
                required: true
            },
            newPassword: {
                required: true,
                characters: true,
                notEqual: "#oldPassword"
            },
            newConfirmPassword: {
                required: true,
                equalTo: "#newPassword"
            }
        },
        messages: {
            oldPassword: {
                required: "Enter your old Password."
            },
            newPassword: {
                required: "Enter a New Password.",
                characters: "New password must be 8-15 characters.",
                notEqual: "The password you entered has already been used. Enter a new password that is different from your last 5 passwords."
            },
            newConfirmPassword: {
                required: "Enter a Confirm Password.",
                equalTo: "New Password and Confirm Password do not match."
            }
        }
    });
}

function getLockoutContacts(result, showResetPassword) {
    var lockWarning = "";
    if (showResetPassword) {
        lockWarning = '<a id="passwordReset" href="Login/VerifyUser' + getPasswordResetQueryString() + '">Click here</a> to reset your password, or contact ';
    } else {
        lockWarning = "Contact ";
    }
    if (result.Note) {
        if (result.Note.indexOf(" or ") > 0) {
            lockWarning += "one of your Administrators, ";
        } else {
            lockWarning += "your Administrator, ";
        }
        lockWarning += " {admins}";
    }

    lockWarning += " or Eyefinity Customer Care at 1.800.942.5353 if you need assistance.";
    var admins = result.Note || "";
    return lockWarning.assign({
        admins: admins
    });
}

/* Loads system messaging from CMS system */
function loadCMSMessaging() {
    document.stopLoader();
    client
        .action("GetCmsContent")
        .post()
        .done(function (result) {
            // parse the cms content into a DOM object and then extract the specific content we need
            var parsedHtml = $("<div/>").append($.parseHTML(result));
            var title = $(parsedHtml).find("#title").html();
            var text = $(parsedHtml).find("#text").html();
            var showMessage = title.length > 0 || text.length > 0;

            // show the message if either the title or text has content
            if (showMessage === true) {
                $("#cmsMessaging #title").html(title);
                $("#cmsMessaging #text").html(text);
                $("#cmsMessaging").removeClass("hidden");
                $(window).resize();
            }
        });
}

$(function () {
    client
        .action("IsAutomationOn")
        .get({
            "isBrowserIe": navigator.appVersion.indexOf("MSIE") !== -1 ? true : false
        })
        .done(function (data) {
            if (data === false) {
                $("#container").addClass("hidden");
                var redirectUrl = window.config.baseUrl + "Login/Compatibility";
                window.location.href = redirectUrl;
            } else {
                $("#container").removeClass("hidden");
                ko.applyBindings(viewModel);

                $("hdloginName").val("").change();
                $("hdPassword").val("").change();
                loadRemembers();
                if (!viewModel.practiceLocationId()) {
                    $("#practiceLocationId").focus();
                } else if (!viewModel.loginName()) {
                    $("#loginName").focus();
                } else {
                    $("#password").focus();
                }

                loadCMSMessaging();

                validateNewPassword();

                $("form").alslValidate({
                    ignore: ":hidden", // This is jQuery Validation default but alslValidate overrides it
                    rules: {
                        practiceLocationId: { required: true },
                        loginName: { required: true },
                        password: { required: true },
                        zipCode: { required: true },
                        officePhoneNumber: { required: true },
                        securityAnswer: { required: true },
                        confirmPassword: { required: true, equalTo: "#password" }
                    },
                    messages: {
                        practiceLocationId: { required: "Enter an Office Identifier." },
                        loginName: { required: "Enter a Login Name." },
                        password: { required: "Enter a Password." },
                        companyZipCode: { required: "Enter Company ZipCode." },
                        companyPhoneNumber: { required: "Enter Company Phone Number." },
                        securityAnswer: { required: "Enter your security answer." },
                        confirmPassword: { required: "Confirm Password.", equalTo: "Password and confirmation do not match." }
                    },
                    submitHandler: function () {
                        var errors;
                        $(".summaryMessages").clearMsgBlock();

                        // Workaround for FireFox Saved Password issue
                        $("#hdpassword").val(encrypt($("#password").val()));
                        $("#hdloginName").val(encrypt($("#loginName").val()));
                        viewModel.loginName($("#hdloginName").val());
                        viewModel.password($("#hdpassword").val());
                        viewModel.numberOfAttempts(viewModel.numberOfAttempts() + 1);
                        viewModel.numberOfPasswordResetAttempts(viewModel.numberOfPasswordResetAttempts() + 1);
                        viewModel.redirectUrl(getRedirectUrl());
                        client
                            .action("ProcessPassword")
                            .post(viewModel.toModel())
                            .done(function (result) {
                                if (result.PasswordCheckResult === null) {
                                    $(document).showSummaryMessage(msgType.ERROR, ["Your Login Name does not have access to this Office, or the Office Identifier, Login Name, and/or Password is incorrect. Enter correct information to log in. "], undefined, "Incorrect login information");
                                    $("#password").focus();
                                }
                                switch (result.PasswordCheckResult) {
                                case "Verified":
                                case "MeetsPolicy":
                                    window.localStorage.setItem("IsEcrOnPrem", result.IsOnPremEcrVault);
                                    window.localStorage.setItem('IsNewExamUiEnabled', result.IsNewExamUiEnabled);
                                    window.localStorage.setItem('IsNewClOrderUiEnabled', result.IsNewClOrderUiEnabled);
                                    window.localStorage.setItem('IsNewEgOrderUiEnabled', result.IsNewEgOrderUiEnabled);
                                    saveRemembers(result.OfficeId);
                                    location.href = result.RedirectUrl;
                                    break;
                                case "ChangePassword":
                                    showResetPassword(result);
                                    break;
                                case "Inactive":
                                    errors = ["<br/>You are either not authorized or your role is not active to perform this operation." +
                                        " Please log in with authorized credentials."];
                                    $(document).showSummaryMessage(msgType.ERROR, errors);
                                    break;
                                case "IsLockedOut":
                                    var resetCount = 5 - result.NumberOfPasswordResetAttempts;
                                    if (resetCount > 0) {
                                        $(document).showSummaryMessage(msgType.ERROR, ["You have exceeded the maximum number of login attempts and your account is locked. " + getLockoutContacts(result, true)]);
                                    } else {
                                        $(document).showSummaryMessage(msgType.ERROR, ["You have exceeded the maximum number of reset password/login attempts and your account is locked. " + getLockoutContacts(result, false)]);
                                    }
                                    break;
                                case "NoMatch":
                                    errors = ["Your Login Name does not have access to this Office, or the Office Identifier, Login Name, and/or Password is incorrect. Enter correct information to log in. "];
                                    if (result.NumberOfAttempts > 2) {
                                        var left = 5 - result.NumberOfAttempts;
                                        var resetLeft = 5 - result.NumberOfPasswordResetAttempts;
                                        if (left > 0) {
                                            errors.push("<br/>Your account will be locked after {left} more failed login attempts. ".assign({left: left}) + getLockoutContacts(result, true));
                                        } else {
                                            if (resetLeft > 0) {
                                                errors = ["You have exceeded the maximum number of login attempts and your account is locked. " + getLockoutContacts(result, true)];
                                            } else {
                                                errors = ["You have exceeded the maximum number of reset password/login attempts and your account is locked. " + getLockoutContacts(result, false)];
                                            }
                                        }
                                    }
                                    $(document).showSummaryMessage(msgType.ERROR, errors, undefined, "Incorrect login information");
                                    $("#password").focus();
                                    break;
                                case "FailsPolicy":
                                    $(document).showSummaryMessage(msgType.ERROR, ["Reset your password. All passwords must be 8-15 characters, " +
                                        "contain at least one numeric character, and abide by your company’s password policy. " +
                                        "Your new password must be different from your last 5 passwords."]);
                                    break;
                                }

                                $(window).resize();
                            }).fail(function (result) {
                                $(document).showSummaryMessage(msgType.ERROR, ["Your Login Name does not have access to this Office, or the Office Identifier, Login Name, and/or Password is incorrect. Enter correct information to log in. "]);
                                $("#password").focus();
                            });

                        $(window).resize();
                    }
                });

                container = $("#container");
                top = $("#top");
                middle = $("#middle");
                bottom = $("#containerbottom");
                var passwordLink = $("#resetPasswordLink");

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

                $("#btnLogin").click(function () {
                    $("form").valid();
                    $(window).resize();
                });

                $(".tt").popover({
                    container: "body",
                    html: true
                });

                $("body").on("click", ".popover.popover.fade.in", function () {
                    $(".popover.fade.in").removeClass("in").addClass("out");
                });


            }
        });
});