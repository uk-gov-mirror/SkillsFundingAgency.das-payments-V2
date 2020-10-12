using System;
using System.Threading.Tasks;
using SFA.DAS.Payments.Monitoring.Metrics.Data;
using SFA.DAS.Payments.Monitoring.Metrics.Model.Submission;

namespace SFA.DAS.Payments.Monitoring.Metrics.Application.Submission
{
    public interface ISubmissionWindowSummaryService
    {
        Task<(bool, SubmissionsSummaryModel)> ValidateSubmissionWindow(short academicYear, byte collectionPeriod);
    }

    public class SubmissionWindowSummaryService: ISubmissionWindowSummaryService
    {
        private readonly IMetricsPersistenceDataContext dataContext;
        private readonly ISubmissionWindowSummaryFactory factory;

        public SubmissionWindowSummaryService(IMetricsPersistenceDataContext dataContext, ISubmissionWindowSummaryFactory factory)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<(bool, SubmissionsSummaryModel)> ValidateSubmissionWindow(short academicYear, byte collectionPeriod)
        {
            var model = await dataContext.GetSubmissionsSummary(academicYear, collectionPeriod);
            var submissionWindowSummary = factory.Create(academicYear, collectionPeriod, model);
            return (submissionWindowSummary.IsWithinTolerance(), model);
        }
    }
}