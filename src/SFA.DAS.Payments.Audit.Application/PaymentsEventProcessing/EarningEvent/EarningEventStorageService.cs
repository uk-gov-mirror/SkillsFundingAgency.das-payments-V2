using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.Application.Data.EarningEvent;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent
{
    public interface IEarningEventStorageService
    {
        Task StoreEarnings(List<EarningEventModel> earningEvents, CancellationToken cancellationToken);
    }

    public class EarningEventStorageService : IEarningEventStorageService
    {
        private readonly IPaymentLogger logger;
        private readonly IEarningEventRepository repository;
        private readonly IEarningsDuplicateEliminator duplicateEliminator;

        public EarningEventStorageService(IPaymentLogger logger, IEarningEventRepository repository, IEarningsDuplicateEliminator duplicateEliminator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.duplicateEliminator = duplicateEliminator ?? throw new ArgumentNullException(nameof(duplicateEliminator));
        }

        public async Task StoreEarnings(List<EarningEventModel> models, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Removing duplicate earning events. Count: {models.Count}");
            var deDuplicatedmodels = duplicateEliminator.RemoveDuplicates(models);
            logger.LogDebug($"De-duplicated earning events count: {deDuplicatedmodels.Count}");

            try
            {
                await repository.SaveEarningEvents(deDuplicatedmodels, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;
                
                if (e.IsDeadLockException())
                    logger.LogInfo("Batch was deadlocked");

                logger.LogInfo("Batch contained a duplicate earning.  Will store each individually and discard duplicate.");
                await repository.SaveEarningsIndividually(deDuplicatedmodels, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}