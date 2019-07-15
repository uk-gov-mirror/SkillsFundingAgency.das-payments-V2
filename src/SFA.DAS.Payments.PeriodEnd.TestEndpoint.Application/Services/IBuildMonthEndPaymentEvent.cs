using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.DataLocks.Messages.Internal;
using SFA.DAS.Payments.FundingSource.Messages.Internal.Commands;
using SFA.DAS.Payments.ProviderPayments.Messages.Internal.Commands;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Services
{
    public interface IBuildMonthEndPaymentEvent
    {
        Task<CollectionStartedEvent> CreateCollectionStartedEvent(long ukprn, short academicYear);

        ProcessProviderMonthEndCommand CreateProcessProviderMonthEndCommand(long ukprn, short academicYear, byte period, long jobId);
        Task<List<ProcessLevyPaymentsOnMonthEndCommand>> CreateProcessLevyPaymentsOnMonthEndCommand(long ukprn, short academicYear, byte period, long jobId);
        Task<List<ResetCacheCommand>> CreateDataLockResetCommand();
        long GenerateId(int maxValue = 1000000000);

        Task CreateMonitoringJob(long ukprn,
            short academicYear,
            byte period,
            long jobId,
            List<ProcessLevyPaymentsOnMonthEndCommand> processLevyPaymentsOnMonthEndCommands,
            ProcessProviderMonthEndCommand processProviderMonthEndCommand);
    }
}
