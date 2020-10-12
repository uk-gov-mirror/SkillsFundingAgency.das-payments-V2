using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.Monitoring.Metrics.Application.Submission;
using SFA.DAS.Payments.Monitoring.Metrics.Data;

[assembly: FunctionsStartup(typeof(SFA.DAS.Payments.Monitoring.Metrics.Functions.Startup))]

namespace SFA.DAS.Payments.Monitoring.Metrics.Functions
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IMetricsPersistenceDataContext>((s) =>
            {
                var config = s.GetService<IConfiguration>();
                var cnn = config["PaymentsConnectionString"];
                return new MetricsPersistenceDataContext(cnn);
            });
            builder.Services.AddSingleton<ISubmissionWindowSummaryFactory, SubmissionWindowSummaryFactory>();
            builder.Services.AddTransient<ISubmissionWindowSummaryService, SubmissionWindowSummaryService>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), true)
                .AddEnvironmentVariables();
        }
    }
}
