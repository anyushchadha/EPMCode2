/*jslint browser: true, vars: true, nomen:true, sloppy:true, plusplus:true, continue:true, regexp:true */
/*global $:true , Modernizr, ko, ApiClient, DataGridOptions,  validateModalInformation, validateOnSave, validRegex, msgType, loadModalWindow, PatientSearchViewModel, DirtyViewModelBase*/
(function () {
    var formatMoney = function (observable) {
        var self = this;
        return ko.computed({
            read: function () {
                var obj = observable();
                if (Object.isNumber(observable())) {
                    obj = "  $" + observable().format(2).toString();
                }
                return obj;
            },
            write: function (value) {
                var item = value.replace(/[^\d\.]/g, '');
                var parsedDollarAmount = parseFloat(item);
                if (!isNaN(parsedDollarAmount)) {
                    observable(parsedDollarAmount);
                } else {
                    observable(null);
                }
            },
            owner: self
        });
    };


    var PatientCenterViewModel = DirtyViewModelBase.extend({
        mappedProperties: {
            address: "",
            cityStateZip: "",
            dateOfBirth: "",
            email: "",
            fullName: "",
            nickName: "",
            isHipaaSignatureOnFile: false,
            lastExamDate: "",
            patientUid: "",
            patientUidTruncated: "",
            primaryPhone: "",
            providerName: "",
            responsibleParty: "",
            secondaryPhone: "",
            insuranceOne: "",
            insuranceTwo: "",
            insuranceThree: "",
            outstandingBalance: "",
            patientInsuranceBalance: "",
            patientCredit: "",
            totalCancellations: "",
            totalNoShows: ""
        },
        constructor: function () {
            var self = this;
            self.showAppointments = ko.observable(true);
            self.showNotes = ko.observable(true);
            self.showDependents = ko.observable(true);
            self.showPrescription = ko.observable(true);
            self.showRecall = ko.observable(true);
            self.showOrders = ko.observable(true);
            self.showPatientBalance = ko.observable(true);
            self.showOutstandingBalance = ko.observable(true);
            self.showInsuranceBalance = ko.observable(true);
            self.showPatientCredit = ko.observable(true);
            self.showSecondInsurance = ko.observable(false);
            self.showThirdInsurance = ko.observable(false);
            PatientCenterViewModel.__super__.constructor.apply(this, arguments);
            self.patientBalanceCount = ko.computed(function () {
                var count = 3;
                return count;
            });
            self.showOutstandingBalance = ko.computed(function () {
                return self.outstandingBalance() !== 0;
            });
            self.outstandingBalanceFormatted = formatMoney(self.outstandingBalance);
            self.showPatientCredit = ko.computed(function () {
                return self.patientCredit() !== 0;
            });
            self.patientCreditFormatted = formatMoney(self.patientCredit);
            self.showInsuranceBalance = ko.computed(function () {
                return self.patientInsuranceBalance() !== 0;
            });
            self.patientInsuranceBalanceFormatted = formatMoney(self.patientInsuranceBalance);
            self.showPatientBalance = ko.computed(function () {
                return self.showInsuranceBalance() || self.showPatientCredit() || self.showOutstandingBalance();
            });
            self.showSecondInsurance = ko.computed(function () {
                return self.insuranceTwo() !== null && self.insuranceTwo() !== "";
            });
            self.showThirdInsurance = ko.computed(function () {
                return self.insuranceThree() !== null && self.insuranceThree() !== "";
            });
            self.patientUidTruncated = ko.computed({
                read: function () {
                    var retVal = "";
                    if (self.patientUid() !== null) {
                        retVal = self.patientUid();
                        if (self.patientUid().length >= 12) {
                            retVal = self.patientUid().substr(self.patientUid().length - 12);
                        }
                    }
                    return retVal;
                },
                write: function () {
                    return self.patientUid();
                },
                owner: self
            });
        },
        importModel: function (model) {
            PatientCenterViewModel.__super__.importModel.call(this, model);
            // patiend UI full value
            var patientUidText = "<strong>" + this.patientUid() + "</strong><br><br> <small>This is a code that uniquely identifies this patient between the Practice Management and EHR systems.</small>";
            $("#patientUid").attr("data-content", patientUidText);
        }
    });

    var viewModel = new PatientSearchViewModel();

    viewModel.showNewPatientButton = false;
    viewModel.isMiniSearch = true;
    viewModel.selectionOverride = function (result) {
        var patients = viewModel.merge.patients().slice(0);
        var index;
        for (index = 0; index < 2; index++) {
            if (patients[index]) {
                continue;
            } else {
                var client = new ApiClient("Patient");
                var parms = {
                    PatientId: result.PatientId
                };
                var action = "GetPatientOverview";
                var reqmode = "get";
                client
                    .action(action)[reqmode](parms)
                    .done(function (data) {
                        var patientModel = new PatientCenterViewModel();
                        patientModel.patientId = result.PatientId;
                        patientModel.importModel(data);
                        patientModel.selected = ko.observable(false);
                        patients[index] = patientModel;
                        viewModel.merge.patients(patients);
                        $(".patientMergeTable")[0].scrollIntoView();
                    });
                break;
            }
        }
    };

    var self = this;
    viewModel.merge = {
        patients: ko.observableArray([null, null]),
        selected: self.patientBalanceCount = ko.observable(0),
        patientInsuranceBalance: ko.observable(0),
        mergedPatient: ko.observable(" "),
        mergedIntoPatient: ko.observable(" "),
        hasPatientCreditOrBalance: ko.observable(false),
        credits: ko.observable(0),
        claimsCount: ko.observable(0),
        selectPatientProfile: function (patientProfile) {
            var selectedPatient = viewModel.merge.patients().indexOf(patientProfile) + 1;
            viewModel.merge.patients().forEach(function (p) {
                p.selected(false);
            });
            patientProfile.selected(true);
            viewModel.merge.selected(selectedPatient);
        },
        clearPatients: function () {
            viewModel.merge.patients([null, null]);
            viewModel.merge.selected(0);
        },
        clearSelectedPatients: function (patient) {
            viewModel.merge.patients.remove(patient);
        },
        getCreditsAndClaimNumber: function (result) {
            var mergedPatientId;
            var mergedIntoPatientId;
            if (viewModel.merge.selected() === 1) {
                mergedPatientId = 1;
                mergedIntoPatientId = 0;
            } else {
                mergedPatientId = 0;
                mergedIntoPatientId = 1;
            }
            var client = new ApiClient("Patient");
            var parms = {
                PatientId: viewModel.merge.patients()[mergedPatientId].patientId,
                PatientId2: viewModel.merge.patients()[mergedIntoPatientId].patientId
            };
            var action = "GetPatientMergeConfirmation";
            var reqmode = "get";
            client
                .action(action)[reqmode](parms)
                .done(function (data) {
                    viewModel.merge.credits('$' + data.Credits.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,'));
                    viewModel.merge.claimsCount(data.PatientClaimsCount);
                    viewModel.merge.patientInsuranceBalance('$' + data.PatientInsuranceBalance.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,'));
                    viewModel.merge.mergedPatient(data.MergedPatient);
                    viewModel.merge.mergedIntoPatient(data.MergedIntoPatient);
                    viewModel.merge.hasPatientCreditOrBalance(data.HasPatientCreditOrBalance);

                    if (!viewModel.merge.selected()) {
                        $('#mergeConfirmationSelectionDialog').modal({
                            keyboard: false,
                            backdrop: false,
                            show: true
                        });
                    } else if (viewModel.merge.hasPatientCreditOrBalance()) {
                        $('#mergeConfirmationCreditsAndPaymentsDialog').modal({
                            keyboard: false,
                            backdrop: false,
                            show: true
                        });
                    } else {
                        $('#mergeConfirmationDialog').modal({
                            keyboard: false,
                            backdrop: false,
                            show: true
                        });
                    }
                });
        },
        confirmDialog: function () {
            viewModel.merge.getCreditsAndClaimNumber();
        },
        mergePatients: function () {
            var client = new ApiClient("Patient");
            client
                .action("MergePatients")
                .post({
                    patient1Id: viewModel.merge.patients()[0].patientId,
                    patient2Id: viewModel.merge.patients()[1].patientId,
                    SelectedPatient: viewModel.merge.selected()
                })
                .done(function () {
                    viewModel.merge.patients([null, null]);
                    $(document).showSystemSuccess("Patients Merged Sucessfully.");
                })
                .fail(function () { });
        },
        cancel: function () {
            // Do nothing.
        },
        confirm: function () {
            viewModel.merge.mergePatients();
        }
    };

    viewModel.merge.ready = ko.computed(function () {
        return viewModel.merge.patients()[0] !== null && viewModel.merge.patients()[1] !== null;
    });

    var client = new ApiClient("PracticeInformation");

    $(document).ready(function () {
        // highlight the menu
        $(".navbar .navbar-nav li#navPatients").addClass("nav-default-open");
        ko.applyBindings(viewModel);
    });

    client
        .action("IsPatientOverviewEnabled")
        .get({
            "officeNumber": window.config.officeNumber
        })
        .done(function (data) {
            viewModel.isPatientOverviewEnabled(data);
        });
}());