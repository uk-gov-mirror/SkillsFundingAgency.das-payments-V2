using SFA.DAS.Payments.Monitoring.Metrics.Domain.Submission;
using SFA.DAS.Payments.Monitoring.Metrics.Model.Submission;

namespace SFA.DAS.Payments.Monitoring.Metrics.Application.Submission
{
    public interface ISubmissionWindowSummaryFactory
    {
        ISubmissionWindowSummary Create(short academicYear, byte collectionPeriod, SubmissionsSummaryModel model);
    }

    public class SubmissionWindowSummaryFactory: ISubmissionWindowSummaryFactory
    {
        public ISubmissionWindowSummary Create(short academicYear, byte collectionPeriod, SubmissionsSummaryModel model)
        {
            return new SubmissionWindowSummary(model);
        }
    }
}