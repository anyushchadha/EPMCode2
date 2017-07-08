/*jslint vars:true, plusplus:true */
/*global ApiClient, window, $, regex, document, ko, showSubscriberInsuranceDialog,
loadPatientSideNavigation, updatePageTitle, blankPatientInsurance, PatientInsuranceViewModel, setTimeout*/


(function () {
    $(document).ready(function () {
        // Unit testing.
        if (!window.loadPatientSideNavigation) {
            return;
        }
        // load the side nav
        loadPatientSideNavigation(window.insuranceViewModel.patientId, "insurance");
        updatePageTitle();
        $(".tt").popover({ container: 'body', html: true, placement: 'top' });
        $('#chkActiveOnly').prop('checked', true);
        window.buildPatientInsuranceTable();
        window.getPatientInsurances();
        window.isAppointmentView = false;
    });
}());
