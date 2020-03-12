using Autofac;
using SFA.DAS.Payments.Application.Infrastructure.Ioc;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Handlers;
using SFA.DAS.Payments.ServiceFabric.Core;

namespace SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Infrastructure.Ioc
{
    public class LevyTransactionStorageServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
                {
                    var appConfig = c.Resolve<IApplicationConfiguration>();
                    var configHelper = c.Resolve<IConfigurationHelper>();
                    return new ServiceBusBatchCommunicationListener(configHelper.GetConnectionString("MonitoringServiceBusConnectionString"),
                        appConfig.EndpointName,
                        appConfig.FailedMessagesQueue,
                        c.Resolve<IPaymentLogger>(),
                        c.Resolve<IContainerScopeFactory>(),
                        c.Resolve<ITelemetry>());
                })
                .As<IServiceBusBatchCommunicationListener>()
                .SingleInstance();

            builder.RegisterType<RecordLevyTransactionBatchHandler>()
                .As<IHandleMessageBatches<RecordLevyTransactionBatchHandler>>()
                .InstancePerLifetimeScope();
        }
    }
}