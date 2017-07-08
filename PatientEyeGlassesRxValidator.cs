namespace Eyefinity.PracticeManagement.Business.Validators
{
    using System;
    using System.Collections.Generic;

    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Patient;

    using FluentValidation;

    public class PatientEyeGlassesRxValidator : AbstractValidator<PatientRx>
    {
        public PatientEyeGlassesRxValidator()
        {
            this.RuleSet("ValidatePatientExamDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.RxTypeID).NotEmpty().NotEqual(0).When(exam => exam != null && exam.PatientExamRxDetails != null);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.ExamRxTypeID).NotEmpty().NotEqual(0).When(exam => exam != null && exam.PatientExamRxDetails != null);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Doctor).NotEmpty().When(exam => exam != null && exam.PatientExamRxDetails != null && exam.PatientExamRxDetails.PatientExam.OutsideDoctor == null);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.ExpirationDate).GreaterThanOrEqualTo(exam => exam.PatientExamRxDetails.PatientExam.ExamDate).When(exam => exam != null && exam.PatientExamRxDetails != null && exam.PatientExamRxDetails.PatientExam.ExamDate.HasValue && exam.PatientExamRxDetails.PatientExam.ExpirationDate.HasValue);
            });

            this.RuleSet("ValidateRightLensDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Sphere).NotEmpty();
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Axis).NotEmpty().When(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Cylinder.HasValue);

                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism1).Must(BeAValidPrism).When(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism1.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism2).Must(BeAValidPrism).When(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism2.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism1Direction).NotEmpty().NotEqual("0").When(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism1.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism2Direction).NotEmpty().NotEqual("0").When(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Prism2.HasValue);

                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].AddPower1).NotEmpty().When(exam =>
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.MultiFocal ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.SunglassMultiFocal ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.MultiFocalOverContacts ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.ComputerMultiFocal);
            });

            this.RuleSet("ValidateLeftLensDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Sphere).NotEmpty();
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Axis).NotEmpty().When(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Cylinder.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism1).Must(BeAValidPrism).When(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism1.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism2).Must(BeAValidPrism).When(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism2.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism1Direction).NotEmpty().NotEqual("0").When(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism1.HasValue);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism2Direction).NotEmpty().NotEqual("0").When(exam => exam.PatientExamRxDetails.PatientExam.Details[1].Prism2.HasValue);

                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[1].AddPower1).NotEmpty().When(exam =>
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.MultiFocal ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.SunglassMultiFocal ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.MultiFocalOverContacts ||
                        exam.PatientExamRxDetails.PatientExam.RxTypeID == (int)EyeGlassRxCategory.ComputerMultiFocal);
            });

            this.RuleSet("ValidateUnderlyingCondition", () => this.RuleFor(x => x.PatientExamDetailAlslList).Must(BeAValidUnderlyingConditions).WithMessage("You cannot select a No Lens, Not Recorded, Prosthesis and/or Plano underlying condition for both the right and left lens."));
        }

        private static bool BeAValidPrism(double? prism)
        {
            const double Minval = 0.25;
            const double Maxval = 20.00;
            return prism >= Minval && prism <= Maxval;
        }

        private static bool BeAValidUnderlyingConditions(IList<PatientExamDetailAlsl> list)
        {
            int[] array = { (int)RxLookupCategory.NoLens, (int)RxLookupCategory.NotRecorded, (int)RxLookupCategory.Prosthesis, 
                                   (int)RxLookupCategory.Plano };

            if (list.Count <= 0)
            {
                return true;
            }

            switch (list[0].UnderlyingCondition)
            {
                case (int)RxLookupCategory.NoLens:
                case (int)RxLookupCategory.NotRecorded:
                case (int)RxLookupCategory.Prosthesis:
                case (int)RxLookupCategory.Plano:
                    var index = Array.FindIndex(array, x => x == list[1].UnderlyingCondition);
                    if (index >= 0)
                    {
                        return false;
                    }
                    break;
                default:
                    return true;
            }

            return true;
        }
    }
}
