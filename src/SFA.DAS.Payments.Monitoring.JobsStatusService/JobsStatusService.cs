using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ServiceFabric.Services.Runtime;
using SFA.DAS.Payments.Application.Infrastructure.Ioc;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Application;

namespace SFA.DAS.Payments.Monitoring.JobsStatusService
{
    

    public class JobsStatusService : StatelessService
    {
        private readonly IPaymentLogger logger;
        private readonly TimeSpan interval;

        public JobsStatusService(StatelessServiceContext context, IPaymentLogger logger, IConfigurationHelper configurationHelper)
            : base(context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var intervalInSeconds = int.Parse(configurationHelper.GetSetting("IntervalInSeconds"));
            interval = TimeSpan.FromSeconds(intervalInSeconds);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogVerbose($"Starting status update process.");
                    using (var scope = ContainerFactory.Container.BeginLifetimeScope())
                    {
                        var telemetry = scope.Resolve<ITelemetry>();
                        using (var operation = telemetry.StartOperation("CompletedJobsProcessing"))
                        {
                            var completedJobsService = scope.Resolve<ICompletedJobsService>();
                            
                            await completedJobsService.UpdateCompletedJobs(cancellationToken);
                            telemetry.StopOperation(operation);
                        }
                    }
                    logger.LogDebug("Finished status update process.");
                    await Task.Delay(interval, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogFatal($"Fatal error running JobStatus Service. Error: {ex}", ex);
                throw;
            }
        }
    }
}
