/*jslint  browser: true, vars:true,  nomen:true, sloppy:true,  plusplus: true*/
/*global $, ko, ApiClient, BaseClass, Modal, window, document, msgType, DirtyViewModelBase, setTimeout,
EligibilityConfigurationViewModel, EligibilityModal, EligibilityModel, loadPatientSideNavigation, updatePageTitle,
PatientInsuranceViewModel, blankPatientInsurance, EligibilityModalPmi, toggleSummaryPanel, summaryPanel*/

// ReSharper disable InconsistentNaming
(function () {
    $.validator.addMethod("atLeastOneChecked", function (value, element, other) {
        return $(element).closest(".panel-body").find(":checkbox:visible:checked").length;
    });
    var viewModel,
        isAppointmentView,
        pmiMemberInfo = null,
        checkboxUpdate = true,
        insuranceBenefitType = '';
    var client = new ApiClient("PatientInsurance");

    var AuthorizeViewModel = DirtyViewModelBase.extend({
        // Alternate form of mappedProperties allows you to set default values
        mappedProperties: {
            benefitType: null,
            chkAll: false,
            chkExam: false,
            chkFrame: false,
            chkLens: false,
            chkContactLens: false,
            chkContactLensExam: false,
            chkAllVisible: true,
            chkExamVisible: true,
            chkFrameVisible: true,
            chkLensVisible: true,
            chkContactLensVisible: true,
            chkContactLensExamVisible: true
        },
        constructor: function () {
            AuthorizeViewModel.__super__.constructor.apply(this, arguments);

            var updateCheckbox = function ($this) {
                var checkboxVisible = 0;
                var checkboxCount = 0;
                if ($this.chkExamVisible()) {
                    checkboxVisible++;
                    if ($this.chkExam()) {
                        checkboxCount++;
                    }
                }
                if ($this.chkFrameVisible()) {
                    checkboxVisible++;
                    if ($this.chkFrame()) {
                        checkboxCount++;
                    }
                }
                if ($this.chkLensVisible()) {
                    checkboxVisible++;
                    if ($this.chkLens()) {
                        checkboxCount++;
                    }
                }
                if ($this.chkContactLensVisible()) {
                    checkboxVisible++;
                    if ($this.chkContactLens()) {
                        checkboxCount++;
                    }
                }
                if ($this.chkContactLensExamVisible()) {
                    checkboxVisible++;
                    if ($this.chkContactLensExam()) {
                        checkboxCount++;
                    }
                }
                if (checkboxCount === 0) {
                    checkboxUpdate = false;
                    $this.chkAll(false);
                    checkboxUpdate = true;
                    if ($("button[data-id='btnModalSave']").html() !== 'Retrieve') {
                        $("button[data-id='btnModalSave']").attr('disabled', 'disabled');
                    }
                    $("button[data-id='btnModalSave']").attr('data-text', '');
                } else {
                    checkboxUpdate = false;
                    $this.chkAll(checkboxCount === checkboxVisible ? true : false);
                    checkboxUpdate = true;
                    $("button[data-id='btnModalSave']").removeAttr('disabled');
                    $("button[data-id='btnModalSave']").attr('data-text', 'Authorize');
                }
            };

            this.chkAll.subscribe(function (newValue) {
                if (checkboxUpdate === true) {
                    this.chkExam(this.chkExamVisible() ? newValue : false);
                    this.chkFrame(this.chkFrameVisible() ? newValue : false);
                    this.chkLens(this.chkLensVisible() ? newValue : false);
                    this.chkContactLens(this.chkContactLensVisible() ? newValue : false);
                    this.chkContactLensExam(this.chkContactLensExamVisible() ? newValue : false);
                }
            }, this);
            this.chkExam.subscribe(function (newValue) {
                updateCheckbox(this);
            }, this);
            this.chkFrame.subscribe(function (newValue) {
                updateCheckbox(this);
            }, this);
            this.chkLens.subscribe(function (newValue) {
                updateCheckbox(this);
            }, this);
            this.chkContactLens.subscribe(function (newValue) {
                updateCheckbox(this);
            }, this);
            this.chkContactLensExam.subscribe(function (newValue) {
                updateCheckbox(this);
            }, this);
        },
        // importModel sets the value of each of the mappedProperties from the
        // passed in model (model comes from the server JSON response). It
        // matches based on name and expects that the viewModel properties
        // are camelCase and the model proerties are PascalCase.
        importModel: function (model) {
            this.benefitType(model.BenefitName);     // since we have BenefitName mapped to benefitType the base importModel will skip it.
            this.chkExamVisible(model.IsExamEligible);
            this.chkFrameVisible(model.IsFrameEligible);
            this.chkLensVisible(model.IsLensEligible);
            this.chkContactLensVisible(model.IsClEligible);
            this.chkContactLensExamVisible(model.IsClExamEligible);
            if (!model.IsExamEligible && !model.IsFrameEligible && !model.IsLensEligible && !model.IsClEligible && !model.IsClExamEligible) {
                this.chkAllVisible(false);
            }

            // Even though we don't have any other properties that match the
            // convention we still need to call the base importModel to get
            // the other automagic benefits like setting the dirtyFlag
            return AuthorizeViewModel.__super__.importModel.call(this, model);
        }
    });

    // EligibilityViewModel actually kind of acts more like a controller.
    // It holds references to all of the different models used by eligibities
    // and modal dialog object. But it also controls all cross-object messaging.
    // The models themselves are not responsible for things that involve showing
    // the eligibility modal, inoking the ApiClient, and displaying prompts
    function EligibilityViewModel(model) {
        var me = this;

        // These functions get called by all sorts of different functions
        // that try to set  "this" to something else.
        this.editEligibility = this.editEligibility.bind(this);
        this.editEligibilityPmi = this.editEligibilityPmi.bind(this);
        this.addEligibility = this.addEligibility.bind(this);
        this.saveEligibility = this.saveEligibility.bind(this);
        this.authorizeEligibility = this.authorizeEligibility.bind(this);
        this.cancelEligibilityEdit = this.cancelEligibilityEdit.bind(this);
        this.deleteEligibility = this.deleteEligibility.bind(this);

        this.modal = new EligibilityModal({ save: this.saveEligibility, cancel: this.cancelEligibilityEdit });
        this.modalPmi = new EligibilityModalPmi({ save: this.authorizeEligibility, cancel: this.cancelEligibilityEdit });

        // The eligibilities themselves
        this.items = ko.viewModelArray(EligibilityModel);

        // KO Projections would have made this ssooo much easier

        this.activeItems = ko.observableArray();
        this.historyItems = ko.observableArray();

        this.hasHistory = ko.computed(function () {
            return me.historyItems().length > 0;
        });

        this.hasActive = ko.computed(function () {
            return me.activeItems().length > 0;
        });

        this.items.subscribe(function () {
            me.activeItems(me.items().filter(function (item) {
                return item.isActive();
            }));

            me.historyItems(me.items().filter(function (item) {
                return !item.isActive();
            }));

            // History is shown/hidden by changing the height of the grid's
            // wrapping div. If we add/remove eligibilities this makes the
            // previously calculated height invalid. By calling toggleHistory
            // twice we are effectively forcing it to recalculate the height.
            // An interesting side effect to this is that adds/removes are
            // now animated as well.
            me.toggleHistory();
            me.toggleHistory();
        });
        this.showHistory = ko.observable(false);

        this.configuration = null;
        this.insurance = ko.observable(null);

        if (model) {
            if (model.Configuration.SurgeryEye.Options) {
                model.Configuration.SurgeryEye.Options.unshift({
                    "KeyStr": 0,
                    "Description": 'Select'
                });
            }
            if (model.Configuration.EyeglassPairType.Options) {
                model.Configuration.EyeglassPairType.Options.unshift({
                    "KeyStr": 0,
                    "Description": 'Select'
                });
            }
            var config = this.configuration = new EligibilityConfigurationViewModel(model.Configuration);
            this.items.importModels(model.Eligibilities);
            this.items().forEach(function (eligibilityModel) {
                eligibilityModel.configuration = config;
            });

        }
    }
    BaseClass.subclass(EligibilityViewModel);

    function eligibilityRefresh(model) {
        if (!model.insurances.length) {
            model.insurances.push(blankPatientInsurance);
        }

        viewModel.insurances.importModels(model.insurances);

        // This is kind of a holdover from the old design where insurances
        // were all fetched up front and eligibilities were fetched on
        // demand when the link in the insurance grid is clicked
        viewModel.insurances().forEach(function (ins) {
            ins.eligibilities.insurance(ins);
        });

        if (!viewModel.insurances()[0].Id()) {
            viewModel.insurances()[0].noEligibilityGridMessage("To request eligibilities / authorizations, first add and/or activate Insurance Carrier(s) / Plan(s)");
        }

        $("#eligibilityModal").modal({
            keyboard: false,
            backdrop: false,
            show: true
        });

        $(".eligibilityGrid").each(function () {
            var grid = this;
            var wrapper = grid.parentNode;
            // var footer = wrapper.nextElementSibling;
            var elig = ko.dataFor(grid).eligibilities;

            elig.toggleHistory = function () {
                if (!this.hasHistory()) {
                    return;
                }
                this.showHistory(!this.showHistory());

                var height = grid.offsetHeight;

                if (this.showHistory()) {
                    height = (grid.tHead.offsetHeight + grid.tBodies[0].offsetHeight);
                }
                wrapper.style.height = height + "px";
            };
            elig.toggleHistory();
        });
    }

    function eligibilityDialogModal($this, eligibility, title, mode) {
        switch (mode) {
        case 1:     //Other
        case 3:     //VSP Override
        case 4:     //Edit VSP Plan            
            $this.modal.title(title);
            $this.modal.show(eligibility).done(function () {
                if (!$this.items().find(function (el) { return el.authNumber() === eligibility.authNumber(); })) {
                    $this.items.push(eligibility);
                }
                $(".summaryMessages").clearMsgBlock();
                $(document).showSystemSuccess("Eligibility saved successfully.");
            });
            $("button[data-id='btnModalOverride']").hide();
            $("button[data-id='btnModalSave']").show();
            $("button[data-id='btnModalSave']").html('Save').removeAttr('disabled');
            //disable everything if edit VSP Plan, except note
            if (mode === 4) {
                eligibility.isVsp(true);
                eligibility.itemEnabled(false);
                setTimeout(function () {
                    var els = $this.modal.elements;
                    var el = els.form.find("textarea[data-id='note']");
                    var data = el.val();
                    el.focus().val('').val(data);   //set the cursor to the end of the text
                }, 500);
            } else {
                eligibility.itemEnabled(true);
            }
            break;
        case 2:     //Add VSP Plan
            $this.modalPmi.title(title);
            $this.modalPmi.show(eligibility);
            $("button[data-id='btnModalOverride']").show();
            $("button[data-id='btnModalSave']").html('Authorize').attr('disabled', 'disabled');
            break;
        }
        if (isAppointmentView) {
            $("#eligibilityModal").hide();
            setTimeout(function () {
                $("#eligibilityModal").show();
                $("#patientInsurance").addClass("modalCollapse");
            }, 200);
        }
    }

    EligibilityViewModel.prototype.showPatientRecordReport = function showPatientRecordReport(eligibility, e) {
        var url = window.config.baseUrl + "Patient/GetRecordReport?id=" + eligibility.authNumber();
        window.open(url, null, "height=800,width=650,toolbar=0,location=0,status=0,menubar=0,resizable=1,scrollbars=1");
    };

    EligibilityViewModel.prototype.showPlanSummaryReport = function showPlanSummaryReport(eligibility, e) {
        Modal.ok("Show Plan Summary Report");
    };

    EligibilityViewModel.prototype.editEligibility = function editEligibility(eligibility, e) {
        if (e) {
            e.stopImmediatePropagation();
        }

        var canEditMessage = eligibility.canEditMessage();
        if (canEditMessage) {
            Modal.ok(canEditMessage, "Cannot edit eligibility");
            return;
        }

        window.patient.fullName = window.patient.FirstName + " " + window.patient.LastName;
        var title = "Eligibilities - " + this.insurance().carrierDisplay();
        eligibilityDialogModal(this, eligibility, title, 1);
    };

    EligibilityViewModel.prototype.editEligibilityPmi = function editEligibilityPmi(eligibility, e) {
        if (e) {
            e.stopImmediatePropagation();
        }

        var canEditMessage = eligibility.canEditMessage();
        if (canEditMessage) {
            Modal.ok(canEditMessage, "Cannot edit eligibility");
            return;
        }
        if (isAppointmentView) {
            $('#patientInsurance').addClass("modalCollapse");
        }
        window.patient.fullName = window.patient.FirstName + " " + window.patient.LastName;
        var title = "Eligibilities - " + this.insurance().carrierDisplay();
        eligibilityDialogModal(this, eligibility, title, 4);
    };

    EligibilityViewModel.prototype.addEligibility = function addEligibility(insurance, e) {
        window.patient.fullName = window.patient.FirstName + " " + window.patient.LastName;
        var me = this;
        var title;
        var elig = EligibilityModel.create(me.configuration);
        elig.isVsp = insurance.isVsp();
        if (elig.isVsp) {
            var isOverride = $("button[data-id='btnModalOverride']").attr('data-value');
            var id = $(e.currentTarget).attr('id');
            $("button[data-id='btnModalOverride']").attr('data-value', id);
            $("button[data-id='btnModalSave']").attr('data-value', insurance.Id());
            pmiMemberInfo = null;

            if (isOverride && isOverride === 'true') {
                var confirmCallback = function (status) {
                    if (status === 'OK') {
                        $("button[data-id='btnModalCancel']").click();
                        title = "Eligibilities - " + insurance.carrierDisplay();
                        elig.isVsp = insurance.isVsp();
                        eligibilityDialogModal(me, elig, title, 3);
                    }
                };
                var options = {
                    data: null,
                    title: 'Warning',
                    message: "If you manually enter VSP eligibility and authorization information, you may also have to manually calculate the patient's benefits.",
                    buttons: ['Cancel', 'OK'],
                    callback: confirmCallback
                };
                $.messageDialog(options);
            } else {
                var a = insurance.carrierDisplay().split(' / ');
                insuranceBenefitType = a[1];
                title = insurance.carrierDisplay() + " - Eligibility / Authorization";
                viewModel.authorizeList.importModels([]);
                elig.vspAddlEligibility([]);
                client
                    .action("GetPmiMemberInfo")
                    .get({
                        officeNumber: window.config.officeNumber,
                        patientId: window.patient.PatientId,
                        insuranceId: insurance.Id()
                    })
                    .done(function (list) {
                        // viewModelArray.importModels does all the mapping for us
                        // by constructing a new AuthorizeViewModel for each item
                        // in data and then call its importModel method, passing in
                        // the model from data.
                        elig.vspError(false);
                        var i, data = list[0];

                        if (list[0].length > 0) {
                            viewModel.authorizeList.importModels(list[0]);
                            pmiMemberInfo = list[0];
                            var jDate = new Date(data[0].PatientDob);
                            var strDate = jDate.getMonth() + 1 + '/' + jDate.getDate() + '/' + jDate.getFullYear();
                            elig.vspBirthDate(strDate);
                            elig.vspRelationshipType(data[0].PatientRelation);
                            elig.vspMemberName(data[0].MemberName);
                            elig.vspMembershipId(data[0].MembershipId);
                            elig.vspInsuredGroupName(data[0].GroupName);

                            var model = viewModel.authorizeList()[0];
                            model.chkExamVisible(data[0].IsExamEligible);
                            model.chkFrameVisible(data[0].IsFrameEligible);
                            model.chkLensVisible(data[0].IsLensEligible);
                            model.chkContactLensVisible(data[0].IsClEligible);
                            model.chkContactLensExamVisible(data[0].IsClExamEligible);
                        }

                        var additional = [];
                        if (list[1].length > 0) {
                            data = list[1];
                            for (i = 0; i < data.length; i++) {
                                additional.push(data[i].BenefitName);
                            }
                        }
                        elig.vspAddlEligibility(additional);

                        eligibilityDialogModal(me, elig, title, 2);
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON.Message || xhr.responseJSON;
                        msg = msg.replace(/\"/g, "");
                        elig.vspError(true);
                        elig.vspErrorMsg(msg);
                        eligibilityDialogModal(me, elig, title, 2);
                    });
            }
        } else {
            title = "Eligibilities - " + insurance.carrierDisplay();
            eligibilityDialogModal(me, elig, title, 1);
        }
    };

    EligibilityViewModel.prototype.authorizeEligibility = function (eligibility) {
        if (eligibility.vspBackDateError() === true) {
            return false;
        }
        eligibility.vspError(false);

        var model = viewModel.authorizeList()[0];
        var insuranceId = $("button[data-id='btnModalSave']").attr('data-value');
        var memberList = [];
        if (pmiMemberInfo) {
            memberList = [
                {
                    BenefitName: model ? model.benefitType() : insuranceBenefitType,
                    GroupName: pmiMemberInfo[0].GroupName,
                    GroupNum: pmiMemberInfo[0].GroupNum,
                    MemberName: pmiMemberInfo[0].MemberName,
                    MembershipId: pmiMemberInfo[0].MembershipId,
                    PatientDob: pmiMemberInfo[0].PatientDob,
                    PatientFullName: pmiMemberInfo[0].PatientFullName,
                    PatientRelation: pmiMemberInfo[0].PatientRelation,
                    AvailableDate: pmiMemberInfo[0].AvailableDate,
                    AuthorizationNumber: '',
                    IsExamEligible: model ? model.chkExam() : false,
                    IsFrameEligible: model ? model.chkFrame() : false,
                    IsLensEligible: model ? model.chkLens() : false,
                    IsClEligible: model ? model.chkContactLens() : false,
                    IsClExamEligible: model ? model.chkContactLensExam() : false,
                    ErrorMessage: ''
                }
            ];
        }

        var parms = {
            OfficeNum: window.config.officeNumber,
            PatientInsuranceId: insuranceId,
            PatientId: window.patient.PatientId,
            SelectedBenefit: model ? model.benefitType() : insuranceBenefitType,
            UserId: window.config.userId,
            BackDate: eligibility.vspBackDate(),
            VspAuthorizationId: eligibility.vspAuthorization(),
            MemberList: memberList
        };

        if (model) {
            insuranceBenefitType = model.benefitType() + ' Plan';
        }

        var resultDialog = function (data) {
            var info = '<ul>';
            if (data.IsExamEligible === true) {
                info += '<li>Exam</li>';
            }
            if (data.IsFrameEligible === true) {
                info += '<li>Frame</li>';
            }
            if (data.IsLensEligible === true) {
                info += '<li>Lens</li>';
            }
            if (data.IsClEligible === true) {
                info += '<li>Contact Lens</li>';
            }
            if (data.IsClExamEligible === true) {
                info += '<li>Contact Lens Exam</li>';
            }
            info += '</ul>';
            var msg = 'VSP has authorized the following benefit types for VSP ' + insuranceBenefitType + ':<br/><br/>' + info;

            if (!$("#successDialog").length) {
                var s = '<div id="successDialog" class="modal fade">' +
                        '    <div class="modal-dialog">' +
                        '       <div class="vertical-align-middle">' +
                        '           <div class="modal-content">' +
                        '               <div class="modal-header">' +
                        '                   <button type="button" class="close callback-btn" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                        '                   <h3 class="modal-title">Eligibility / Authorization</h3>' +
                        '               </div>' +
                        '               <div class="modal-body">' +
                        '                   <h1>Success</h1>' +
                        '                   <label id="msgDialog"></label>' +
                        '               </div>' +
                        '               <div class="clearfix"></div>' +
                        '               <div class="modal-footer">' +
                        '                   <button type="button" class="btn btn-primary callback-btn" data-dismiss="modal">OK</button>' +
                        '               </div>' +
                        '           </div>' +
                        '       </div>' +
                        '   </div>' +
                        '</div>';
                $('body').append(s);
                $('#successDialog').find('.callback-btn').on('click.callback', function () {
                    $("button[data-id='btnModalCancel']").click();
                    setTimeout(function () {
                        client
                            .action("GetEligibilities")
                            .get({ patientId: window.patient.PatientId })
                            .done(function (dat) {
                                var i, j;
                                for (i = 0; i < dat.insurances.length; i++) {
                                    for (j = 0; j < dat.insurances[i].Eligibilities.Eligibilities.length; j++) {
                                        if (dat.insurances[i].Eligibilities.Eligibilities[j].VspBenefitType === "MEDICAID" &&
                                                dat.insurances[i].Eligibilities.Eligibilities[j].EyeglassPairType === null) {
                                            dat.insurances[i].Eligibilities.Eligibilities[j].EyeglassPairType = "  NU";
                                            client
                                                .action("SaveEligibility")
                                                .post({
                                                    Eligibility: dat.insurances[i].Eligibilities.Eligibilities[j],
                                                    InsuranceId: dat.insurances[i].Id
                                                })
                                                .fail(function (xhr) {
                                                    eligibility.serverError(xhr.responseJSON.Message);
                                                });
                                        }
                                    }
                                }
                                eligibilityRefresh(dat);
                            });
                    }, 100);
                });
            }

            $("#msgDialog").html(msg);
            $('#successDialog').modal({
                keyboard: false,
                backdrop: false,
                show: true
            });
        };

        var actionRoute = $("button[data-id='btnModalSave']").html() === 'Authorize' ? 'GetPmiAuthorization' : 'RetrieveExistingVspAuthorization';
        client
            .action(actionRoute)
            .get(parms)
            .done(function (data) {
                if (data.ErrorMessage === null) {
                    resultDialog(data);
                } else {
                    eligibility.vspError(true);
                    eligibility.vspErrorMsg(data.ErrorMessage);
                }
            });

        return false;
    };

    EligibilityViewModel.prototype.saveEligibility = function saveEligibility(eligibility) {
        var me = this;
        return client
            .action("SaveEligibility")
            .post({
                Eligibility: eligibility.toModel(),
                InsuranceId: me.insurance().Id()
            })
            .fail(function (xhr) {
                eligibility.serverError(xhr.responseJSON.Message);
            })
            .done(function (data) {
                if (isAppointmentView) {
                    if (window.eligibilityCount > 1) {
                        $("#eligibilityModal").hide();
                        setTimeout(function () {
                            $("#eligibilityModal").show();
                            $("#patientInsurance").removeClass("modalCollapse");
                            EligibilityViewModel.prototype.showInsuranceEligibilityDialog(window.patient.PatientId);
                        }, 1);
                    } else {
                        window.viewModel.selectedInsurance(me.insurance().Id());
                        setTimeout(function () { $("#eligibilityModal").modal("toggle"); }, 1);
                    }
                } else {
                    window.location.reload();
                }
            });
    };

    EligibilityViewModel.prototype.cancelEligibilityEdit = function (flag) {
        if (flag === 'override') {
            var id = '#' + $("button[data-id='btnModalOverride']").attr('data-value');
            $("button[data-id='btnModalOverride']").attr('data-value', 'true');
            $(id).click();
        } else {
            var elig = this.modal.data();
            if (elig && elig.dirtyFlag.isDirty()) {
                elig.revertChanges();
            }
            if (isAppointmentView) {
                $("#eligibilityModal").hide();
                setTimeout(function () {
                    $("#eligibilityModal").show();
                    $("#patientInsurance").removeClass("modalCollapse");
                }, 1);
            }
        }
    };


    EligibilityViewModel.prototype.deleteEligibility = function deleteEligibility(eligibility, e) {
        e.stopImmediatePropagation();
        var canDeleteMessage = eligibility.canDeleteMessage();
        if (canDeleteMessage) {
            Modal.ok(canDeleteMessage, "Cannot delete authorization");
            return;
        }

        var me = this;
        Modal.yesCancel("Delete this authorization?", "Confirm Delete").done(function () {

            client
                .action("DeleteEligibility")
                .queryStringParams({ insuranceId: me.insurance().Id(), eligibilityId: eligibility.insuranceEligibilityId(), authorizationNumber: eligibility.authNumber() })["delete"]()
                .done(function () {
                    me.items.remove(function (el) {
                        return el.insuranceEligibilityId() === eligibility.insuranceEligibilityId();
                    });
                    $(document).showSystemSuccess("Authorization deleted.");
                })
                .fail(function (data) {
                    Modal.ok(data.responseText.replace(/"/g, ''), "Unable to delete authorization.");
                });
        });
    };


    EligibilityViewModel.prototype.showInsuranceEligibilityDialog = function showInsuranceEligibilityDialog(patientId) {
        window.patient.PatientId = patientId;
        updatePageTitle();
        client
            .action("GetEligibilities")
            .get({ patientId: window.patient.PatientId })
            .done(function (model) {
                window.eligibilityCount = model.insurances.length;
                eligibilityRefresh(model);
            });

        $("#patientInsurance").removeClass("modalCollapse");
    };

    $(document).ready(function () {
        if (!isAppointmentView) {
            if (!window.loadPatientSideNavigation) {
                return;
            }
            ////// load the side nav
            loadPatientSideNavigation(window.patient.PatientId, "eligibilities");
            updatePageTitle();
            client
                .action("GetEligibilities")
                .get({
                    patientId: window.patient.PatientId
                })
                .done(function (model) {
                    eligibilityRefresh(model);
                });
        }

    });

    window.EligibilityModal = EligibilityModal;
    window.EligibilityModel = EligibilityModel;
    EligibilityViewModel.prototype.toggleHistory = $.noop;
    window.EligibilityViewModel = EligibilityViewModel;
    window.patient = {
        PatientId: Object.fromQueryString(window.location.search.toLowerCase()).id
    };
    window.EligibilityModel.prototype.vspPlanSummaryReport = function (model, e) {
        //alert('VSP Plan Summary Report is not done yet');
    };

    viewModel = {
        insurances: ko.viewModelArray(PatientInsuranceViewModel),
        // viewModelArray is an observableArray with extra benefits
        // which we'll see later on in the call to GetPmiMemeberInfo
        authorizeList: ko.viewModelArray(AuthorizeViewModel)
    };
    //ko.applyBindings(viewModel);
    if (window.location.href.indexOf("Calendar") > -1) {
        isAppointmentView = true;
        ko.applyBindings(viewModel, $("#eligibilityModal")[0]);
        $("#patientInsurance").removeClass("col-lg-10").addClass("col-lg-12");
    } else {
        ko.applyBindings(viewModel);
        isAppointmentView = false;
    }
}());

function removeRequiedFieldFromSelectPicker() {
    $('select').each(function () {
        if ($(this).prop("selectedIndex") !== 0) {
            $(this).parents("div.bootstrap-select").removeClass("requiredField");
        }
    });
}

function refreshEligibilitySelectPickers(args) {
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

$("#eligibilityModal").on("hidden.bs.modal", function (e) {
//alert('test');
  //setTimeout(function () {
    if (e.target.firstElementChild.innerText.indexOf("Add Eligibility/Authorization") > -1) {
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
        $("#patientInsurance").empty();
        $("#addApptModal").modal("show");
        toggleSummaryPanel(summaryPanel.SHOW);
        $("#btnToggleSummaryPanel").removeClass("disabled");
        if (window.eligibilityCount === 1) {
            $("#insurance").change();
        }
    }
    //}, 100);
});


