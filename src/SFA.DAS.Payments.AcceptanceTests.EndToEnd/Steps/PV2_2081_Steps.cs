using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.Model.Core.Entities;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

// ReSharper disable UnusedMember.Global

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    [Binding]
    [Scope(Feature = "PV2-2081-Price-episode-removed-from-ILR")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public class PV2_2081_Steps : FM36_ILR_Base_Steps
    {
        private const string PriceEpisodeIdentifier1 = "25-102-05/08/2020";
        private const string PriceEpisodeIdentifier2 = "25-102-01/08/2020";
        private const string CommitmentIdentifier1 = "A-2081";
        private const string CommitmentIdentifier2 = "B-2081";

        public PV2_2081_Steps(FeatureContext context) : base(context)
        {
        }

        [Given(@"Commitment exists for learner in period (.*)")]
        public async Task GivenAProviderHasBeenPreviouslyPaidForOneOrMorePeriods(string collectionPeriod)
        {
            GetFm36LearnerForCollectionPeriod($"{collectionPeriod}/current academic year");

            await SetupTestCommitmentData(CommitmentIdentifier1, PriceEpisodeIdentifier1, CommitmentIdentifier2, PriceEpisodeIdentifier2);
        }

        [When(@"an ILR file is submitted for period (.*)")]
        public async Task WhenAnIlrIsSubmitted(string collectionPeriod)
        {
            GetFm36LearnerForCollectionPeriod($"{collectionPeriod}/current academic year");
            
            //reset learner details in FM36
            var learner = TestSession.FM36Global.Learners.Single();
            learner.ULN = TestSession.Learner.Uln;
            learner.LearnRefNumber = TestSession.Learner.LearnRefNumber;

            var dcHelper = Scope.Resolve<IDcHelper>();
            await dcHelper.SendIlrSubmission(TestSession.FM36Global.Learners,
                TestSession.Provider.Ukprn,
                TestSession.CollectionPeriod.AcademicYear,
                TestSession.CollectionPeriod.Period,
                TestSession.Provider.JobId);
        }
        
        [When("After Period-end following provider payments will be generated in database")]
        [Then("After Period-end following provider payments will be generated in database")]
        public async Task AfterPeriodEndRunPaymentsAreGenerated(Table table)
        {
            await WaitForRequiredPayments(table.RowCount);

            await EmployerMonthEndHelper.SendLevyMonthEndForEmployers(
                TestSession.GenerateId(),
                TestSession.Employers.Select(x => x.AccountId),
                TestSession.CollectionPeriod.AcademicYear,
                TestSession.CollectionPeriod.Period,
                MessageSession);

            var expectedPayments = table.CreateSet<ProviderPayment>().ToList();
            expectedPayments.ForEach(ep => ep.Uln = TestSession.Learner.Uln);

            await WaitForIt(() => AssertExpectedPayments(expectedPayments), "Failed to wait for expected number of payments");
        }

        private bool AssertExpectedPayments(List<ProviderPayment> expectedPayments)
        {
            var actualPayments = Scope.Resolve<IPaymentsHelper>().GetPayments(TestSession.Provider.Ukprn, TestSession.CollectionPeriod);

            if (actualPayments.Count != expectedPayments.Count) return false;

            return expectedPayments.All(ep => actualPayments.Any(ap =>
                ep.Uln == ap.LearnerUln &&
                ep.TransactionType == ap.TransactionType &&
                ep.LevyPayments == ap.Amount &&
                ep.ParsedDeliveryPeriod.Period == ap.DeliveryPeriod &&
                ep.ParsedCollectionPeriod.AcademicYear == ap.CollectionPeriod.AcademicYear &&
                ep.ParsedCollectionPeriod.Period == ap.CollectionPeriod.Period &&
                ep.PriceEpisodeIdentifier == ap.PriceEpisodeIdentifier
            ));
        }
    }
}
