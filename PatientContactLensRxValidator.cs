namespace Eyefinity.PracticeManagement.Model.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Patient;

    using FluentValidation;

    using IT2.Core;
    //// ReSharper disable once RedundantUsingDirective

    public class PatientContactLensRxValidator : AbstractValidator<PatientRx>
    {
        public PatientContactLensRxValidator()
        {
            this.RuleSet("ValidatePatientExamDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.RxTypeID).NotEmpty().NotEqual(0).When(exam => exam != null && exam.PatientExamRxDetails != null);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.ExamRxTypeID).NotEmpty().NotEqual(0).When(exam => exam != null && exam.PatientExamRxDetails != null);
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(BeAValidDoctor);
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.ExpirationDate).GreaterThanOrEqualTo(exam => exam.PatientExamRxDetails.PatientExam.ExamDate).When(exam => exam != null && exam.PatientExamRxDetails != null && exam.PatientExamRxDetails.PatientExam.ExamDate.HasValue && exam.PatientExamRxDetails.PatientExam.ExpirationDate.HasValue);
            });

            this.RuleSet("ValidateRightLensDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidManufacturer(exam, LensRightLeft.Right)).WithMessage("Invalid Right Manufacturer");
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidStyle(exam, LensRightLeft.Right)).WithMessage("Invalid Right Style");
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidColor(exam, LensRightLeft.Right)).WithMessage("Invalid Color");
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Sphere).NotEmpty();
            });

            this.RuleSet("ValidateLeftLensDetails", () =>
            {
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidManufacturer(exam, LensRightLeft.Left)).WithMessage("Invalid Left Manufacturer");
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidStyle(exam, LensRightLeft.Left)).WithMessage("Invalid Left Style");
                this.RuleFor(exam => exam.PatientExamRxDetails).Must(exam => BeAvalidColor(exam, LensRightLeft.Left)).WithMessage("Invalid Left Color");
                this.RuleFor(exam => exam.PatientExamRxDetails.PatientExam.Details[0].Sphere).NotEmpty();
            });

            this.RuleSet("ValidateUnderlyingCondition", () => this.RuleFor(x => x.PatientExamDetailAlslList).Must(BeAValidUnderlyingConditions).WithMessage("You cannot select a No Lens, Not Recorded, Prosthesis and/or Plano underlying condition for both the right and left lens."));
        }

        private static bool BeAValidDoctor(PatientRxExamDetails patientExamDetails)
        {
            if (patientExamDetails.PatientExam.Doctor != null)
            {
                var doctorId = patientExamDetails.PatientExam.Doctor.ID;
                var doctors = patientExamDetails.Doctors;
                return doctors.Any(x => x.Key == doctorId);
            }

            return patientExamDetails.PatientExam.OutsideDoctor != null;
        }

        private static bool BeAvalidManufacturer(PatientRxExamDetails patientExamDetails, LensRightLeft rightLeft)
        {
            var patientExam = patientExamDetails.PatientExam;
            var lensRightLeft = rightLeft == LensRightLeft.Right ? 0 : 1;
            if (patientExam == null || patientExam.Details == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower.Style == null)
            {
                return false;
            }

            return patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower.Style.Manufacturer != null && patientExamDetails.Manufacturers.Any(x => x.KeyStr.Contains(patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower.Style.Manufacturer.ID));
        }

        private static bool BeAvalidStyle(PatientRxExamDetails patientExamDetails, LensRightLeft rightLeft)
        {
            var patientExam = patientExamDetails.PatientExam;
            var lensRightLeft = rightLeft == LensRightLeft.Right ? 0 : 1;
            if (patientExam == null || patientExam.Details == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower.Style == null)
            {
                return false;
            }
            
            var styles = rightLeft == LensRightLeft.Right ? patientExamDetails.RightLensStyles : patientExamDetails.LeftLenStyles;
            return styles.Any(x => x.Key == patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower.Style.ID);
        }

        private static bool BeAvalidColor(PatientRxExamDetails patientExamDetails, LensRightLeft rightLeft)
        {
            var patientExam = patientExamDetails.PatientExam;
            var lensRightLeft = rightLeft == LensRightLeft.Right ? 0 : 1;
            if (patientExam == null || patientExam.Details == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLPower == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLColor == null)
            {
                return false;
            }

            if (patientExam.Details[lensRightLeft].ContactLensStockItem.CLColor.ID == null)
            {
                return false;
            }

            var colors = rightLeft == LensRightLeft.Right ? patientExamDetails.RightColors : patientExamDetails.LeftColors;
            return colors.Any(x => x.KeyStr == patientExam.Details[lensRightLeft].ContactLensStockItem.CLColor.ID);
        }

        private static bool BeAValidUnderlyingConditions(IList<PatientExamDetailAlsl> list)
        {
            int[] array = { (int)RxLookupCategory.NoLens, (int)RxLookupCategory.NotRecorded, (int)RxLookupCategory.Prosthesis, 
                                   (int)RxLookupCategory.BalanceLens };

            if (list.Count <= 0)
            {
                return true;
            }

            switch (list[0].UnderlyingCondition)
            {
                case (int)RxLookupCategory.NoLens:
                case (int)RxLookupCategory.NotRecorded:
                case (int)RxLookupCategory.Prosthesis:
                case (int)RxLookupCategory.BalanceLens:
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
