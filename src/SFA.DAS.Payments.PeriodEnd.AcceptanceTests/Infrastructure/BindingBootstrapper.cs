﻿using Autofac;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.Messages.Core.Events;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.PeriodEnd.AcceptanceTests.Infrastructure
{
    [Binding]
    public class BindingBootstrapper : BindingsBase
    {
        [BeforeTestRun(Order = 51)]
        public static void AddRoutingConfig()
        {
            var endpointConfiguration = Container.Resolve<EndpointConfiguration>();
            endpointConfiguration.Conventions().DefiningEventsAs(type => type.IsAssignableTo<PeriodEndEvent>());
            var transportConfig = Container.Resolve<TransportExtensions<AzureServiceBusTransport>>();
            //var routing = transportConfig.Routing();
        }

        [BeforeTestRun(Order = 2)]
        public static void AddDcConfig()
        {
            DcHelper.AddDcConfig(Builder);
        }
    }
}
