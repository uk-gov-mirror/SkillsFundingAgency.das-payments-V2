using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.DataLock
{
    public interface IDataLockDuplicateEliminator
    {
        List<DataLockEventModel> RemoveDuplicates(List<DataLockEventModel> datalockEvents);
    }

    public class DataLockDuplicateEliminator : IDataLockDuplicateEliminator
    {
        private readonly IPaymentLogger logger;

        public DataLockDuplicateEliminator(IPaymentLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<DataLockEventModel> RemoveDuplicates(List<DataLockEventModel> datalockEvents)
        {
            logger.LogDebug($"Removing duplicates from batch. Batch size: {datalockEvents.Count}");
            var deDuplicatedEvents = datalockEvents
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
            if (deDuplicatedEvents.Count != datalockEvents.Count)
                logger.LogInfo($"Removed '{datalockEvents.Count - deDuplicatedEvents.Count}' duplicates from the batch.");
            else
                logger.LogDebug("Found no duplicates in the batch.");
            return deDuplicatedEvents;
        }
    }
}
