namespace SFA.DAS.Payments.Monitoring.Metrics.Model.Submission
{
    public class MetricsTolerance
    {
        public int Id { get; set; }
        public short AcademicYear { get; set; }
        public byte CollectionPeriod { get; set; }
        public decimal SubmissionToleranceMin { get; set; }
        public decimal SubmissionToleranceMax { get; set; }
        public decimal PeriodEndToleranceMin { get; set; }
        public decimal PeriodEndToleranceMax { get; set; }
    }
}