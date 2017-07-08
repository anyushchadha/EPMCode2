/*jslint browser: true, vars: true, plusplus: true*/
/*global $, Modernizr, ko, document, alert, confirm, convertDate, config, patient, alslContactLenses, escape, loadPatientSideNavigation, msgType, ApiClient, updatePageTitle*/
(function () {
    var client = new ApiClient("Patient");
    var appointmentsTable, viewModel, showStatusTypes, confirmationStatusTypes, initialSelect;
    var buildAppointmentsTable = function () {
        appointmentsTable = $('#appointmentsTable').alslDataTable({
            "aaSorting": [[0, "desc"]],
            "aoColumns": [{
                "sTitle": "",
                "mData": "AppointmentDateTime",
                "sWidth": "10%",
                "sType": "numeric",
                "sClass": "left",
                "bVisible": false,
                "bSortable": true
            }, {
                "sTitle": "Date",
                "mData": "AppointmentDate",
                "sType": "date",
                "sWidth": "8%",
                "sClass": "left",
                "bSortable": true,
                "iDataSort": 0
            }, {
                "sTitle": "Time",
                "mData": "AppointmentTime",
                "sType": "date",
                "sWidth": "8%",
                "sClass": "left",
                "bSortable": true
            }, {
                "sTitle": "Office",
                "mData": "OfficeId",
                "sType": "string",
                "sWidth": "10%",
                "sClass": "left",
                "bSortable": true
            }, {
                "sTitle": "Resource",
                "mData": "DoctorFullName",
                "sType": "string",
                "sWidth": "17%",
                "sClass": "left",
                "bSortable": true
            }, {
                "sTitle": "Type",
                "mData": "AppointmentType",
                "sType": "string",
                "sWidth": "20%",
                "sClass": "left",
                "bSortable": true
            }, {
                "sTitle": "Status",
                "mData": "AppointmentShowStatus",
                "sType": "string",
                "sWidth": "12%",
                "sClass": "left",
                "bSortable": true,
                "mRender": function (data) {
                    return '<select id="showstatus" stat="' + data + '" class="showstatus" data-bind="options: appointmentShowStatusTypes"></select>';
                }
            }, {
                "sTitle": "Confirmation",
                "mData": "AppointmentConfirmationStatus",
                "sType": "string",
                "sWidth": "15%",
                "sClass": "left",
                "bSortable": true,
                "mRender": function (data) {
                    return '<select id="confirmationstatus" stat="' + data +
                        '" class="confirmationstatus" data-bind="options: appointmentConfirmationStatusTypes"></select>';
                }
            }
            //, {
            //    "sTitle": "",
            //    "mData": "AppointmentId",
            //    "sType": "integer",
            //    "sWidth": "13%",
            //    "sClass": "center",
            //    "bSortable": false,
            //    "mRender": function (data) {
            //        return "<i class='btn icon-calendar-3' title='Reschedule this appointment' data-id='" + data + "' ></i>" +
            //            "<i class='btn icon-file-2' title='See notes about this appointment' data-id='" + data + "' ></i>" +
            //            "<i class='btn icon-print' title='Print this appointment' data-id='" + data + "' ></i>";
            //    }
            //}
                ],
            "bAutoWidth": false,
            "oLanguage": {
                "sEmptyTable": "No appointments scheduled for this patient."
            }
        });
    };

    var appointmentViewModel = function (data) {
        var self = this;
        self.appointmentData = ko.observableArray(data);
        self.appointmentShowStatusTypes = showStatusTypes;
        self.appointmentConfirmationStatusTypes = confirmationStatusTypes;
    };

    function loadAppointmentShowStatusTypes() {
        client
            .action("GetAppointmentShowStatusTypes")
            .get()
            .done(function (data) {
                showStatusTypes = data;
            });
    }

    function loadAppointmentConfirmationStatusTypes() {
        client
            .action("GetAppointmentConfirmationStatusTypes")
            .get()
            .done(function (data) {
                confirmationStatusTypes = data;
            });
    }

    $("#appointmentsTable").on("change", "select.showstatus", function (e) {
        e.preventDefault();
        if (initialSelect === true) {
            return;
        }
        var $this = $(this);
        var selectedShowStatus = $this.val();
        var row = $this.closest("tr").get(0);
        var rownum = appointmentsTable.fnGetPosition(row);
        var sData = appointmentsTable.fnGetData(rownum);
        sData.AppointmentShowStatus = selectedShowStatus;
        client
            .action("UpdatePatientAppointmentStatus")
            .queryStringParams({
                "userId": window.config.userId
            })
            .put(sData)
            .done(function () {
                $(document).showSystemSuccess("Appointment saved.");
            });
    });

    $("#appointmentsTable").on("change", "select.confirmationstatus", function (e) {
        e.preventDefault();
        if (initialSelect === true) {
            return;
        }
        var $this = $(this);
        var selectedConfirmationStatus = $this.val();
        var row = $this.closest("tr").get(0);
        var rownum = appointmentsTable.fnGetPosition(row);
        var sData = appointmentsTable.fnGetData(rownum);
        sData.AppointmentConfirmationStatus = selectedConfirmationStatus;
        client
            .action("UpdatePatientAppointmentStatus")
            .queryStringParams({
                "userId": window.config.userId
            })
            .put(sData)
            .done(function () {
                $(document).showSystemSuccess("Appointment saved.");
            });
    });

    $("#btnNewAppointment").click(function (e) {
        e.preventDefault();

        if (window.sessionStorage.getItem("appointmentsOfficeContext")) {
            window.sessionStorage.removeItem("appointmentsOfficeContext");
        }
        var redirectUrl = window.config.baseUrl + "Appointments/Calendar?id=" + window.patient.PatientId;
        window.location.href = redirectUrl;
    });

    /*jslint newcap: true, nomen: true */
    function loadAppointmentsTable() {
        var patientId = window.patient.PatientId;
        client
            .action("GetAllAppointmentsByPatientId")
            .get({
                "officeNumber": window.config.officeNumber,
                "patientId": patientId
            })
            .done(function (data) {
                initialSelect = true;
                var oSettings = appointmentsTable.fnSettings();
                oSettings._iDisplayLength = data.length;
                appointmentsTable.fnDraw();
                appointmentsTable.refreshDataTable(data);
                viewModel = new appointmentViewModel(data);
                ko.applyBindings(viewModel);
                appointmentsTable.find("select").each(function (index, selectElement) {
                    var select = $(selectElement);
                    var status = select.attr("stat");
                    select.val(status);
                });
                oSettings = appointmentsTable.fnSettings();
                oSettings._iDisplayLength = 20;
                appointmentsTable.fnDraw();
                initialSelect = false;
                $("select").selectpicker('refresh');
            });
    }
    /*jslint newcap: false, nomen: false */

    $(document).ready(function () {
        // load the side nav
        loadPatientSideNavigation(window.patient.PatientId, "appointments");
        updatePageTitle();
        buildAppointmentsTable();
        loadAppointmentShowStatusTypes();
        loadAppointmentConfirmationStatusTypes();
        loadAppointmentsTable();
    });
}());