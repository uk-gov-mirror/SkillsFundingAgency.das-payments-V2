using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent
{
    public interface IEarningsDuplicateEliminator
    {
        List<EarningEventModel> RemoveDuplicates(List<EarningEventModel> earningEvents);
    }

    public class EarningsDuplicateEliminator : IEarningsDuplicateEliminator
    {
        private readonly IPaymentLogger logger;

        public EarningsDuplicateEliminator(IPaymentLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<EarningEventModel> RemoveDuplicates(List<EarningEventModel> earningEvents)
        {
            logger.LogDebug($"Removing duplicates from batch. Batch size: {earningEvents.Count}");
            var deDuplicatedEvents = earningEvents
                .GroupBy(earningEvent => new
                {
                    earningEvent.Ukprn,
                    earningEvent.GetType().FullName,
                    earningEvent.CollectionPeriod,
                    earningEvent.AcademicYear,
                    earningEvent.LearnerReferenceNumber,
                    earningEvent.LearnerUln,
                    earningEvent.LearningAimReference,
                    earningEvent.LearningAimProgrammeType,
                    earningEvent.LearningAimStandardCode,
                    earningEvent.LearningAimFrameworkCode,
                    earningEvent.LearningAimPathwayCode,
                    earningEvent.LearningAimFundingLineType,
                    earningEvent.LearningAimSequenceNumber,
                    earningEvent.StartDate,
                    earningEvent.JobId,
                })
                .Select(group => group.FirstOrDefault())
                .Where(earningEvent => earningEvent != null)
                .ToList();
            if (deDuplicatedEvents.Count != earningEvents.Count)
                logger.LogInfo($"Removed '{earningEvents.Count - deDuplicatedEvents.Count}' duplicates from the batch.");
            else
                logger.LogDebug("Found no duplicates in the batch.");
            return deDuplicatedEvents;
        }
    }
}