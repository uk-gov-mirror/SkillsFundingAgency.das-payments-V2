using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Interfaces;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobsProxyService.Handlers
{
    public class RecordEarningsJobHandler: IHandleMessages<RecordEarningsJob>
    {
        public IJobService JobService { get; set; }

        public async Task Handle(RecordEarningsJob message, IMessageHandlerContext context)
        {
            await JobService.RecordEarningsJob(message, default);
        }
    }
}