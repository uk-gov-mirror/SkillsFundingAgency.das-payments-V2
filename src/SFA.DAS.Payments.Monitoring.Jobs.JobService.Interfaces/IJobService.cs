using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobService.Interfaces
{
    public interface IJobService
    {
        Task RecordEarningsJob(RecordEarningsJob message, CancellationToken cancellationToken);
        Task RecordEarningsJobAdditionalMessages(RecordEarningsJobAdditionalMessages message, CancellationToken cancellationToken);
        Task RecordJobMessageProcessingStatus(RecordJobMessageProcessingStatus message, CancellationToken cancellationToken);
        Task RecordJobMessageProcessingStartedStatus(RecordStartedProcessingJobMessages message,
            CancellationToken cancellationToken);
    }
}