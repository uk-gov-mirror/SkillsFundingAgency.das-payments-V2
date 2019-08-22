using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Tests.Core;
using TechTalk.SpecFlow.Assist.Attributes;

namespace SFA.DAS.Payments.AcceptanceTests.Core.Data
{
    public class Learner
    {
        public long Ukprn { get; set; }
        [TableAliases("Learner[ ]?Reference[ ]?Number")]
        public string LearnRefNumber { get; set; }
        public long Uln { get; set; }
        public Course Course { get; set; }

        public string LearnerIdentifier { get; set; }
        public List<Aim> Aims { get; set; } = new List<Aim>();

        public int? EefCode { get; set; }
        public string PostcodePrior { get; set; }
        public bool IsLevyLearner { get; set; }

        public List<EmploymentStatusMonitoring> EmploymentStatusMonitoring { get; set; } = new List<EmploymentStatusMonitoring>();

        public long? OriginalUln { get; set; }

        public bool Restart { get; set; }

        public LearnerEarningsHistory EarningsHistory { get; set; }

        public override string ToString()
        {
            return $"Learn Ref Number: [ {LearnRefNumber} ]\tUln: [ {Uln} ]\t\tLearner Identifier: [ {LearnerIdentifier} ]";
        }

        public void UpdateFromTrainingType(Training trainingIlrRecord, Training previousIlrTraining = null)
        {
            PostcodePrior = trainingIlrRecord.PostcodePrior;
            EefCode = trainingIlrRecord.EefCode;
            EmploymentStatusMonitoring =
                CreateEmploymentStatusMonitoringRecordsFromTrainingType(trainingIlrRecord, previousIlrTraining);
            Restart = trainingIlrRecord.Restart;
            Aims = CreateAimsFromTrainingType(trainingIlrRecord);
        }

        public List<EmploymentStatusMonitoring> CreateEmploymentStatusMonitoringRecordsFromTrainingType(
            Training trainingIlrRecord, Training previousIlr = null)
        {
            var employmentStatusMonitoringList = new List<EmploymentStatusMonitoring>();
            if (!string.IsNullOrWhiteSpace(previousIlr?.Employer) || !string.IsNullOrWhiteSpace(previousIlr?.SmallEmployer))
            {
                employmentStatusMonitoringList.Add(new EmploymentStatusMonitoring()
                {
                    LearnerId = previousIlr.LearnerId,
                    EmploymentStatusApplies = !string.IsNullOrWhiteSpace(previousIlr.EmploymentStatusApplies) ? previousIlr.EmploymentStatusApplies : previousIlr.StartDate.ToDate().AddMonths(-6).ToString(),
                    EmploymentStatus = !string.IsNullOrWhiteSpace(previousIlr.EmploymentStatus) ? previousIlr.EmploymentStatus : "in paid employment",
                    Employer = previousIlr.Employer,
                    SmallEmployer = previousIlr.SmallEmployer
                });
            }

            if (previousIlr?.EmploymentStatusApplies != trainingIlrRecord.EmploymentStatusApplies)
            {
                employmentStatusMonitoringList.Add(new EmploymentStatusMonitoring()
                {
                    LearnerId = trainingIlrRecord.LearnerId,
                    EmploymentStatusApplies = trainingIlrRecord.EmploymentStatusApplies,
                    EmploymentStatus = trainingIlrRecord.EmploymentStatus,
                    Employer = trainingIlrRecord.Employer,
                    SmallEmployer = trainingIlrRecord.SmallEmployer
                });
            }

            return employmentStatusMonitoringList;
        }

        public List<Aim> CreateAimsFromTrainingType(Training trainingIlrRecord)
        {
            var listOfAims = new List<Aim>();
            var aim = new Aim(trainingIlrRecord);
            listOfAims.Add(aim);
            aim.PriceEpisodes.Add(new Price()
            {
                TotalTrainingPriceEffectiveDate = trainingIlrRecord.TotalTrainingPriceEffectiveDate,
                TotalTrainingPrice = trainingIlrRecord.TotalTrainingPrice,
                TotalAssessmentPriceEffectiveDate = trainingIlrRecord.TotalAssessmentPriceEffectiveDate,
                TotalAssessmentPrice = trainingIlrRecord.TotalAssessmentPrice,
                ContractType = trainingIlrRecord.ContractType,
                AimSequenceNumber = trainingIlrRecord.AimSequenceNumber,
                SfaContributionPercentage = trainingIlrRecord.SfaContributionPercentage,
                CompletionHoldBackExemptionCode = trainingIlrRecord.CompletionHoldBackExemptionCode,
                Pmr = trainingIlrRecord.Pmr
            });

            return listOfAims;
        }
    }
}