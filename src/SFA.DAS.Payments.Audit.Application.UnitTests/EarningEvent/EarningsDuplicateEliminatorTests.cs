using System;
using System.Collections.Generic;
using AutoMoqCore;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents;
using SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.EarningEvent
{
    [TestFixture]
    public class EarningsDuplicateEliminatorTests
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
        }

        private EarningEventModel CreateEarningEventModel(Action<EarningEventModel> action)
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
                Periods = new List<EarningEventPeriodModel>(),
                LearningAimFundingLineType = "Funding line type",
                LearningAimSequenceNumber = 1,
                LearningAimFrameworkCode = 10,
                LearningAimPathwayCode = 11,
                LearningAimReference = "learn ref",
                LearningStartDate = DateTime.Today.AddYears(-1),
                LearningAimProgrammeType = 12,
                LearningAimStandardCode = 13,
                ContractType = ContractType.Act1,
                PriceEpisodes = new List<EarningEventPriceEpisodeModel>(),
                StartDate = DateTime.Today,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Today.AddYears(1),
                InstalmentAmount = 500,
                NumberOfInstalments = 24,
                CompletionAmount = 1500,
                CompletionStatus = 1,
                EventType = typeof(ApprenticeshipContractType1EarningEvent).FullName,
            };

            action?.Invoke(earningEvent);
            
            return earningEvent;
        }

        [Test]
        public void Removes_Duplicates_In_The_Batch()
        {
            var earnings = new List<EarningEventModel>
            {
                CreateEarningEventModel(null),
                CreateEarningEventModel(null),
                CreateEarningEventModel(model => model.Ukprn = 4321),
            };

            var service = moqer.Create<EarningsDuplicateEliminator>();
            
            var deDuplicatedEvents = service.RemoveDuplicates(earnings);
            
            deDuplicatedEvents.Count.Should().Be(2);
        }
    }
}