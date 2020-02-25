using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMoqCore;
using Castle.Components.DictionaryAdapter;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.Application.Data;
using SFA.DAS.Payments.Audit.Application.PaymentsEventModelCache;
using SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Application.UnitTests
{
    [TestFixture]
    public class BatchProcessorTests
    {
        private AutoMoqer mocker;
        private Mock<IPaymentsEventModelCache<EarningEventModel>> mockPaymentsCache;
        private IPaymentsEventModelDataTable<EarningEventModel> dataTable;
        private Mock<IPaymentLogger> logger;
        private Mock<IConfigurationHelper> config;
        private int batchSize = 50;

        [SetUp]
        public void SetUp()
        {
         
            string connectionString = "Data Source=KEENEXPS;Initial Catalog=SFA.DAS.Payments.Database;User Id=SFActor;Password=SFActor;Persist Security Info=False;Pooling=True;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";
            mocker = new AutoMoqer();
            config = mocker.GetMock<IConfigurationHelper>();
            config.Setup(ch => ch.GetSetting(It.IsAny<string>(), "PaymentsConnectionString"))
                .Returns(connectionString);
            mockPaymentsCache = mocker.GetMock<IPaymentsEventModelCache<EarningEventModel>>();
            mockPaymentsCache.Setup(mpc => mpc.GetPayments(batchSize, CancellationToken.None))
                .Returns(this.CreateMockPayments());
            dataTable = new EarningEventDataTable();
            logger = mocker.GetMock<IPaymentLogger>();



        }

        private Task<List<EarningEventModel>> CreateMockPayments()
        {
            List<EarningEventModel> modelList = new List<EarningEventModel>();

            for (int i = 0; i < 45; i++)
            {
                modelList.Add(CreateEarningEventModel(i));
            }

            //spanner one record
            modelList[11].Periods[0].PriceEpisodeIdentifier = "This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!This is a string that is far too long!!!!!!!";

            return Task.FromResult(modelList);
        }

        private static EarningEventModel CreateEarningEventModel(int i)
        {

            var eventId = Guid.NewGuid();
            return new EarningEventModel()
            {
                Id = 10000 + i,
                EventTime = DateTimeOffset.Now,
                CollectionPeriod = 7,
                Ukprn = 1000050 + i,
                EventId = eventId,
                ContractType = ContractType.Act1,
                Periods = CreatePeriods(eventId, i),
                PriceEpisodes = CreatePriceEpisodes(eventId, i),
                LearnerReferenceNumber = $"947{i:D}",
                LearningAimReference = $"jd{i:D}",
                AgreementId = $"398{i:D}",
                LearningAimFrameworkCode = 4,
                LearningAimPathwayCode = 3,
                LearningAimStandardCode = 51
            };

        }

        private static List<EarningEventPriceEpisodeModel> CreatePriceEpisodes(Guid eventId, int i)
        {
            return new List<EarningEventPriceEpisodeModel>()
            {


                new EarningEventPriceEpisodeModel()
                {
                    EarningEventId = eventId,
                    AgreedPrice = 101.99m,
                    Id = 10000+i,
                    PriceEpisodeIdentifier = $"id_{i}",
                    CompletionAmount = 100m,
                    InstalmentAmount = 100m,
                    CourseStartDate = DateTime.UtcNow.AddYears(-1),
                    PlannedEndDate = DateTime.UtcNow.AddMonths(-6),
                    TotalNegotiatedPrice1 = 0,
                    TotalNegotiatedPrice2 = 0,
                    TotalNegotiatedPrice3 = 0,
                    TotalNegotiatedPrice4 = 0
                }
            };
        }

        private static List<EarningEventPeriodModel> CreatePeriods(Guid eventId, int i)
        {
            List<EarningEventPeriodModel> returnPeriods = new EditableList<EarningEventPeriodModel>();
            for (int j = 1; j < 7; j++)
            {
                returnPeriods.Add(new EarningEventPeriodModel()
                {
                    Id = 10000+i,
                    Amount = 3000+i,
                    DeliveryPeriod =(byte) j,
                    EarningEventId = eventId,
                    PriceEpisodeIdentifier = $"SomeId_{i}" ,
                    TransactionType = (TransactionType) (j * 2) -1
                });
            }
            return returnPeriods;
        }

        [Test]
        public async Task BatchTransactionRollsBackBatchOnErrorAndCompleteUsingSingle()
        {
            var batchProcessor = new PaymentsEventModelBatchProcessor<EarningEventModel>(mockPaymentsCache.Object, dataTable, config.Object, logger.Object);
            int results =     await batchProcessor.Process(batchSize, CancellationToken.None);
        }
    }
}
