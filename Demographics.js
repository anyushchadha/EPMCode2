/*jslint browser:true*/
/*global $, ko, loadPatientSideNavigation, updatePatientSideNavigation, msgType, DemographicsViewModel, ApiClient */

(function () {
    var doneLoad = false,
        client = new ApiClient("PatientDemographics"),
        viewModel = new DemographicsViewModel({
            isDuplicatePatientEnabled: true,
            isResponsiblePartyEnabled: true,
            isLoading: true,
            saveDone: function (newModel) {
                //$(document).showSystemSuccess("Patient  " + (this.id() ? "Saved" : "Added"));
                $(document).showSystemSuccess("Patient Demographics saved.");
                this.importModel(newModel);
                loadPatientSideNavigation(this.id() || 0, "demographics");
            }
        }).commitChanges();

    viewModel.firstName.subscribe(function (newValue) {
        if (newValue !== '' && newValue !== null) {
            document.title = "EPM: " + newValue + " : Demographics";
        } else {
            document.title = "EPM: Add Patient";
        }
    });

    window.onbeforeunload = function () {
        if (viewModel && viewModel.isDirty() && doneLoad) {
            return "You are trying to navigate to a new page, but you have not saved the data on this page.";
        }
    };

    $(function () {
        document.title = "EPM: Patient : Demographics";
        ko.applyBindings(viewModel);

        var lastName = $.urlParam(window.location.href, "lastName", true),
            firstName = $.urlParam(window.location.href, "firstName", true),
            dOb = $.urlParam(window.location.href, "dOb", true),
            phone = $.urlParam(window.location.href, "phone", true),
            patientId = $.urlParam(window.location.href, "id", true);

        loadPatientSideNavigation(patientId || 0, "demographics");
        client
            .action("Get")
            .get({ id: patientId || 0 })
            .done(function (data) {
                updatePatientSideNavigation(patientId || 0, "demographics");
                viewModel.isLoading(false);
                if (data.Id === 0) {
                    data.FirstName = firstName !== null ? firstName.trim() : "";
                    data.LastName = lastName !== null ? lastName.trim() : "";
                    data.DateOfBirth = dOb;
                    data.PrimaryPhone = phone;
                }
                viewModel.importModel(data);
                viewModel.elements.firstName.focus();
                doneLoad = true;
            })
            .fail(function (xhr) {
                updatePatientSideNavigation(0, "demographics");
                viewModel.dirtyFlag.reset();
                if (xhr.status === 404) {
                    $(document).showSystemFailure("Patient not found.");
                } else {
                    $(document).showSystemFailure("You do not have security permission to access this functionality/information.");
                }
                doneLoad = true;
                //put screen demographics to mode 'add a new patient'
                setTimeout(function () {
                    var x = window.location.href.split('?');
                    window.location.href = x[0];
                }, 500);
            });

        // click event handler for PatientID link
        $("#patientUid").click(function (e) {
            e.preventDefault();
        });
    });
}());