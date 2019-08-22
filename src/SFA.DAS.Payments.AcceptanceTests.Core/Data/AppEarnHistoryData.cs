namespace SFA.DAS.Payments.AcceptanceTests.Core.Data
{
    public class AppEarnHistoryData
    {
        public string Provider { get; set; }

        public string Employer { get; set; }

        public string ActualDuration { get; set; }

        public string CompletionStatus { get; set; }

        public string HistoryPeriod { get; set; }

        public bool CapPreviousEarningsToHistoryPeriod { get; set; }
    }
}
