using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Interfaces;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobsProxyService.Handlers
{
    public class RecordPeriodEndRunJobHandler : IHandleMessages<RecordPeriodEndRunJob>
    {
        public IJobService JobService { get; set; }

        public async Task Handle(RecordPeriodEndRunJob message, IMessageHandlerContext context)
        {
            
        }
    }
}