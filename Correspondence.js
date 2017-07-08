/*global $, document, window, alslContactLenses, msgType, loadPatientSideNavigation, updatePageTitle*/

$(document).ready(function () {
    // load the side nav
    loadPatientSideNavigation(window.patient.PatientId, "correspondence");
    updatePageTitle();
});