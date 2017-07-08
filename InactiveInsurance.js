/*global ApiClient, window, document*/
window.addEventListener("message", function (e) {
    if (e.data === "refresh") {
        window.jQuery(document).showSystemBlockerDialog("Session Timeout", "Your session has timed out.", function () {
            window.location.href = window.config.baseUrl + "Login?" + window.jQuery.param({ ReturnUrl: window.location.href });
        });
    }
}, false);