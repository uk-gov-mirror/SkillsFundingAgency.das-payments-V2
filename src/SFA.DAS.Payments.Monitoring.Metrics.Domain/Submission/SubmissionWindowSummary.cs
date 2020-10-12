using System;
using SFA.DAS.Payments.Monitoring.Metrics.Model.Submission;

namespace SFA.DAS.Payments.Monitoring.Metrics.Domain.Submission
{
    public interface ISubmissionWindowSummary
    {
        bool IsWithinTolerance();
    }

    public class SubmissionWindowSummary: ISubmissionWindowSummary
    {
        private readonly SubmissionsSummaryModel model;

        public SubmissionWindowSummary(SubmissionsSummaryModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public bool IsWithinTolerance()
        {
            var accuracy = Math.Round(model.Percentage, 2, MidpointRounding.AwayFromZero);
            return accuracy >= 99.97M && accuracy <= 100.03M;
        }
    }
}