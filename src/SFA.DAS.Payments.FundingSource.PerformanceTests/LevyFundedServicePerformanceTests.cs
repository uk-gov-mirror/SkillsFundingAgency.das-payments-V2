using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Features;
using NUnit.Framework;
using SFA.DAS.Payments.AcceptanceTests.Core.Infrastructure;
using SFA.DAS.Payments.FundingSource.Application.Data;
using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.FundingSource.PerformanceTests
{
    [TestFixture]
    public class LevyFundedServicePerformanceTests
    {
        private static EndpointConfiguration endpointConfiguration;
        private static IEndpointInstance endpointInstance;
        private static Config config;
        private FundingSourceDataContext dataContext;
        private static readonly int jobId = 9990999;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            //var config = new TestsConfiguration();

            config = new Config();
            BuildConfiguration().Bind(config);

            endpointConfiguration = new EndpointConfiguration(config.AppSettings.TestsEndpointName);
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());


            endpointConfiguration.UsePersistence<AzureStoragePersistence>()
                .ConnectionString(config.ConnectionStrings.StorageConnectionString);
            endpointConfiguration.DisableFeature<TimeoutManager>();


            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport
                .ConnectionString(config.ConnectionStrings.ServiceBusConnectionString)
                .Transactions(TransportTransactionMode.ReceiveOnly)
                .RuleNameShortener(ruleName => ruleName.Split('.').LastOrDefault() ?? ruleName);

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(CalculatedRequiredLevyAmount), config.AppSettings.LevyEndPoint);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendOnly();
            endpointInstance = await Endpoint.Start(endpointConfiguration);
        }

        [SetUp]
        public async Task SetUp()
        {
            dataContext = new FundingSourceDataContext(config.ConnectionStrings.PaymentsConnectionString);
            await dataContext.Database.ExecuteSqlCommandAsync($"Delete from [Payments2].[FundingSourceLevyTransaction] where JobId = {jobId}");
        }

        [TearDown]
        public void ClearDown()
        {
            dataContext?.Dispose();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(TestContext.CurrentContext.TestDirectory);
            builder.AddJsonFile("appSettings.json", false);
            //builder.AddJsonFile("appSettings.debug.json",true);
            return builder.Build();
        }

        [TestCase(100, 10, 1)]
        [TestCase(100, 10, 2)]
        [TestCase(100, 10, 3)]
        [TestCase(100, 10, 4)]
        [TestCase(100, 10, 5)]
        [TestCase(100, 10, 6)]
        [TestCase(100, 10, 7)]
        [TestCase(100, 10, 8)]
        [TestCase(100, 10, 9)]
        [TestCase(100, 10, 10)]
        [TestCase(500, 30, 1)]
        [TestCase(500, 30, 2)]
        [TestCase(500, 30, 3)]
        [TestCase(500, 30, 4)]
        [TestCase(500, 30, 5)]
        [TestCase(500, 30, 6)]
        [TestCase(500, 30, 7)]
        [TestCase(500, 30, 8)]
        [TestCase(500, 30, 9)]
        [TestCase(500, 30, 10)]
        [TestCase(1000, 60, 1)]
        [TestCase(1000, 60, 2)]
        [TestCase(1000, 60, 3)]
        [TestCase(1000, 60, 4)]
        [TestCase(1000, 60, 5)]
        [TestCase(1000, 60, 6)]
        [TestCase(1000, 60, 7)]
        [TestCase(1000, 60, 8)]
        [TestCase(1000, 60, 9)]
        [TestCase(1000, 60, 10)]
        //[TestCase(10000, 120, 1)]
        public async Task Batch_For_Same_Employer(int batchSize, int delayInSeconds, int testIndex)
        {
            Console.WriteLine($"Test: #{testIndex}, batch size: {batchSize}");
            var visibleTime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            await SendMessages(batchSize, visibleTime).ConfigureAwait(false);
            var endTime = visibleTime.Add(config.AppSettings.TimeToWait);
            Console.WriteLine($"Waiting until {endTime:G} for Levy Service To finish storing transactions.");
            while (DateTime.Now < endTime)
            {
                var storedCount = await GetStoredCount().ConfigureAwait(false);
                Console.WriteLine($"Stored Count {storedCount}");
                if (storedCount == batchSize)
                {
                    var creationTimeParam = new SqlParameter("@creationDate", SqlDbType.DateTimeOffset)
                    {
                        Direction = ParameterDirection.Output
                    };
                    await dataContext.Database.ExecuteSqlCommandAsync($"Select @creationDate = Max(CreationDate) from  [Payments2].[FundingSourceLevyTransaction]with (NOLOCK) where JobId = {jobId}", creationTimeParam);
                    var creationTime = (DateTimeOffset)creationTimeParam.Value;
                    Console.WriteLine($"Last creation time was {creationTime:G}");
                    var executionTime = creationTime - visibleTime;
                    Console.WriteLine($"Took: {executionTime.TotalSeconds} seconds to store {batchSize} levy transactions");
                    Assert.Pass();
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            Assert.Fail("Failed to store all messages.");
        }

        [TestCase(100, 10, 1)]
        [TestCase(100, 10, 2)]
        [TestCase(100, 10, 3)]
        [TestCase(100, 10, 4)]
        [TestCase(100, 10, 5)]
        //[TestCase(100, 10, 6)]
        //[TestCase(100, 10, 7)]
        //[TestCase(100, 10, 8)]
        //[TestCase(100, 10, 9)]
        //[TestCase(100, 10, 10)]
        [TestCase(500, 30, 1)]
        [TestCase(500, 30, 2)]
        [TestCase(500, 30, 3)]
        [TestCase(500, 30, 4)]
        [TestCase(500, 30, 5)]
        //[TestCase(500, 30, 6)]
        //[TestCase(500, 30, 7)]
        //[TestCase(500, 30, 8)]
        //[TestCase(500, 30, 9)]
        //[TestCase(500, 30, 10)]
        [TestCase(1000, 60, 1)]
        [TestCase(1000, 60, 2)]
        [TestCase(1000, 60, 3)]
        [TestCase(1000, 60, 4)]
        [TestCase(1000, 60, 5)]
        //[TestCase(1000, 60, 6)]
        //[TestCase(1000, 60, 7)]
        //[TestCase(1000, 60, 8)]
        //[TestCase(1000, 60, 9)]
        //[TestCase(1000, 60, 10)]
        public async Task Time_To_Clear_Queue(int batchSize, int delayInSeconds, int testIndex)
        {
            Console.WriteLine($"Test: #{testIndex}, batch size: {batchSize}");
            var visibleTime = DateTime.UtcNow.AddSeconds(delayInSeconds);
            await SendMessages(batchSize, visibleTime).ConfigureAwait(false);
            var endTime = visibleTime.Add(config.AppSettings.TimeToWait);
            Console.WriteLine($"Waiting until {endTime:G} for Levy Service To finish storing transactions.");

            var connection = new ServiceBusConnection(config.ConnectionStrings.ServiceBusConnectionString);
            var client = new ManagementClient(config.ConnectionStrings.ServiceBusConnectionString);

            while (DateTime.Now < endTime)
            {
                var queueInfo = await client.GetQueueRuntimeInfoAsync(config.AppSettings.LevyEndPoint)
                    .ConfigureAwait(false);
                Console.WriteLine($"Time: {DateTime.Now:G}. Queue count: {queueInfo.MessageCount}, Active messages: {queueInfo.MessageCountDetails.ActiveMessageCount}, Dead letter: {queueInfo.MessageCountDetails.DeadLetterMessageCount}");
                if (DateTime.UtcNow > visibleTime && queueInfo.MessageCount == 0 )
                {
                    var executionTime = DateTime.UtcNow - visibleTime;
                    Console.WriteLine($"Time: {DateTime.Now:G}. Took: {executionTime.TotalSeconds} seconds to clear {batchSize} levy transactions");
                    Assert.Pass();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            Assert.Fail("Failed to process all messages.");
        }

        private async Task SendMessages(int batchSize, DateTimeOffset visibleTime)
        {
            var stopwatch = Stopwatch.StartNew();
            var options = new NServiceBus.SendOptions();
            Console.WriteLine($"Messages visible at {visibleTime:G}");
            options.DoNotDeliverBefore(visibleTime);
            var messages = Enumerable.Range(0, batchSize).Select(i =>
                new CalculatedRequiredLevyAmount
                {
                    AmountDue = 100,
                    ContractType = ContractType.Act1,
                    CollectionPeriod = new CollectionPeriod { Period = 1, AcademicYear = 1920 },
                    DeliveryPeriod = 1,
                    JobId = jobId,
                    Ukprn = 100003915,
                    AccountId = 999,
                    SfaContributionPercentage = .95M,
                    EarningEventId = Guid.NewGuid(),
                    Learner = new Learner { Uln = 99999 },
                    OnProgrammeEarningType = OnProgrammeEarningType.Learning,
                    TransferSenderAccountId = 999
                }).ToList();
            foreach (var calculatedRequiredLevyAmount in messages)
            {
                await endpointInstance.Send(calculatedRequiredLevyAmount, options).ConfigureAwait(false);
            }
            Console.WriteLine($"Sent {batchSize} messages at {DateTime.Now:G}.  Took: {stopwatch.ElapsedMilliseconds}ms");

        }

        private async Task<int> GetStoredCount()
        {
            using var tx = dataContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                return dataContext.LevyTransactions.Count(levyTransaction => levyTransaction.JobId == jobId);
            }
            catch (Exception)
            {
                await Task.Delay(500).ConfigureAwait(false);
                return dataContext.LevyTransactions.Count(levyTransaction => levyTransaction.JobId == jobId);
            }
        }



        [OneTimeTearDown]
        public async Task OneTimeCleanUp()
        {
            if (endpointInstance != null)
                await endpointInstance.Stop();
        }
    }
}