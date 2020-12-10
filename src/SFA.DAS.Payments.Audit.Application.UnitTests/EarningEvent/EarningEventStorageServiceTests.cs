﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMoqCore;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Data.EarningEvent;
using SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents;
using SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.EarningEvent
{
    [TestFixture]
    public class EarningEventStorageServiceTests
    {
        private AutoMoqer moqer;

        [SetUp]
        public void SetUp()
        {
            moqer = new AutoMoqer();
            moqer.GetMock<IEarningEventMapper>()
                .Setup(mapper => mapper.Map(It.IsAny<EarningEventModel>()))
                .Returns<EarningEventModel>(earningEvent =>
                    new EarningEventModel { EventId = earningEvent.EventId });
            moqer.GetMock<IEarningsDuplicateEliminator>()
                .Setup(x => x.RemoveDuplicates(It.IsAny<List<EarningEventModel>>()))
                .Returns<List<EarningEventModel>>(items => items);
        }

        [Test]
        public void Test()
        {
            var eventId = Guid.NewGuid();
            var earning = new EarningEventModel
            {
                Id = 1,
                EventId = eventId,
                ContractType = ContractType.Act1,
                JobId = 123,
                PriceEpisodes = new List<EarningEventPriceEpisodeModel> { new EarningEventPriceEpisodeModel { Id = 1, EarningEventId = eventId, CompletionAmount = 3000, InstalmentAmount = 1000 } },
                Periods = new List<EarningEventPeriodModel> { new EarningEventPeriodModel { Id = 1, EarningEventId = eventId, PriceEpisodeIdentifier = "1/1/1234", TransactionType = TransactionType.Learning, Amount = 1000 } }
            };

            var config = new MapperConfiguration(cfg => cfg.AddProfile<EarningEventProfile>());
            IMapper mapper = new Mapper(config);

            var newEarning = mapper.Map<EarningEventModel, EarningEventModel>(earning);
            newEarning.Should().NotBeNull();
            newEarning.Id.Should().Be(0);
            newEarning.EventId.Should().Be(earning.EventId);
            newEarning.ContractType.Should().Be(earning.ContractType);
            newEarning.JobId.Should().Be(earning.JobId);
            newEarning.PriceEpisodes.Should().NotBeNullOrEmpty();
            newEarning.PriceEpisodes.First().Id.Should().Be(0);
            newEarning.PriceEpisodes.First().EarningEventId.Should().Be(earning.EventId);
            newEarning.PriceEpisodes.First().CompletionAmount.Should().Be(3000);
            newEarning.PriceEpisodes.First().InstalmentAmount.Should().Be(1000);
            newEarning.Periods.Should().NotBeNullOrEmpty();
            newEarning.Periods.First().Id.Should().Be(0);
            newEarning.Periods.First().Amount.Should().Be(1000);
            newEarning.Periods.First().EarningEventId.Should().Be(eventId);
            newEarning.Periods.First().PriceEpisodeIdentifier.Should().Be("1/1/1234");
            newEarning.Periods.First().TransactionType.Should().Be(TransactionType.Learning);
        }

        [Test]
        public async Task Saves_Batch_Of_Act1_Earning_Events()
        {
            var earnings = new List<EarningEventModel>
            {
                CreateEarningEvent(null)
            };
            var service = moqer.Create<EarningEventStorageService>();
            await service.StoreEarnings(earnings, CancellationToken.None);
            moqer.GetMock<IEarningEventRepository>()
                .Verify(x => x.SaveEarningEvents(It.Is<List<EarningEventModel>>(lst => lst.All(item => earnings.Any(earning => earning.EventId == item.EventId))), It.IsAny<CancellationToken>()), Times.Once);
        }

        private EarningEventModel CreateEarningEvent(Action<EarningEventModel> action)
        {
            var earningEvent = new EarningEventModel
            {
                JobId = 123,
                CollectionPeriod = 1,
                AcademicYear = 1920,
                Ukprn = 1234,
                EventId = Guid.NewGuid(),
                LearnerUln = 123456,
                LearnerReferenceNumber = "learner ref",
                EventTime = DateTimeOffset.Now,
                IlrSubmissionDateTime = DateTime.Now,
                SfaContributionPercentage = .95M,
                AgreementId = null,
                IlrFileName = "somefile.ilr",
                Periods = new List<EarningEventPeriodModel>
                {
                    new EarningEventPeriodModel
                    {
                        DeliveryPeriod = 1,
                        Amount = 100,
                    }
                },
                PriceEpisodes = new List<EarningEventPriceEpisodeModel>(),
                StartDate = DateTime.Today
            };
            action?.Invoke(earningEvent);
            return earningEvent;
        }

        [Test]
        public async Task Removes_Duplicates_Earning_Events()
        {
            var earnings = new List<EarningEventModel>
            {
                CreateEarningEvent(null),
                CreateEarningEvent(null),
                CreateEarningEvent(model => model.Ukprn = 4321),
            };
            var service = moqer.Create<EarningEventStorageService>();
            await service.StoreEarnings(earnings, CancellationToken.None);
            moqer.GetMock<IEarningsDuplicateEliminator>()
                .Verify(x => x.RemoveDuplicates(It.IsAny<List<EarningEventModel>>()), Times.Once);
        }
    }
}