using System.Collections.Generic;

namespace SFA.DAS.Payments.Monitoring.Metrics.Model.Submission
{
    public class SubmissionsSummaryModel
    {
        public decimal Percentage { get; set; }
        public decimal ContractType1 { get; set; }
        public decimal ContractType2 { get; set; }
        public decimal DifferenceContractType1 { get; set; }
        public decimal DifferenceContractType2 { get; set; }
        public decimal PercentageContractType1 { get; set; }
        public decimal PercentageContractType2 { get; set; }
        public decimal EarningsDCContractType1 { get; set; }
        public decimal EarningsDCContractType2 { get; set; }
        public decimal EarningsDASContractType1 { get; set; }
        public decimal EarningsDASContractType2 { get; set; }
        public decimal EarningsDifferenceContractType1 { get; set; }
        public decimal EarningsDifferenceContractType2 { get; set; }
        public decimal EarningsPercentageContractType1 { get; set; }
        public decimal EarningsPercentageContractType2 { get; set; }
        public decimal RequiredPaymentsContractType1 { get; set; }
        public decimal RequiredPaymentsContractType2 { get; set; }
        public decimal AdjustedDataLockedEarnings { get; set; }
        public decimal AlreadyPaidDataLockedEarnings { get; set; }
        public decimal TotalDataLockedEarnings { get; set; }
        public decimal HeldBackCompletionPaymentsContractType1 { get; set; }
        public decimal HeldBackCompletionPaymentsContractType2 { get; set; }
        public decimal PaymentsYearToDateContractType1 { get; set; }
        public decimal PaymentsYearToDateContractType2 { get; set; }
    }
}