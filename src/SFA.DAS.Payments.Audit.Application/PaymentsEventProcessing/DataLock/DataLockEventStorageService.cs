using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.Application.Data.DataLock;
using SFA.DAS.Payments.Audit.Application.Mapping.DataLock;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.DataLock
{
    public interface IDataLockEventStorageService
    {
        Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken);
    }

    public class DataLockEventStorageService : IDataLockEventStorageService
    {
        private readonly IDataLockEventMapper mapper;
        private readonly IPaymentLogger logger;
        private readonly IDataLockEventRepository repository;
        private readonly IDataLockDuplicateEliminator duplicateEliminator;

        public DataLockEventStorageService(IDataLockEventMapper mapper, IPaymentLogger logger, IDataLockEventRepository repository,
            IDataLockDuplicateEliminator duplicateEliminator
        )
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.duplicateEliminator = duplicateEliminator ?? throw new ArgumentNullException(nameof(duplicateEliminator));
        }

        public async Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Removing duplicate datalock events. Count: {models.Count}");
            var deDuplicatedmodels = duplicateEliminator.RemoveDuplicates(models);
            logger.LogDebug($"De-duplicated datalock events count: {deDuplicatedmodels.Count}");

            try
            {
                await repository.SaveDataLockEvents(deDuplicatedmodels, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;
                logger.LogInfo($"Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");
                await repository.SaveDataLocksIndividually(deDuplicatedmodels, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}