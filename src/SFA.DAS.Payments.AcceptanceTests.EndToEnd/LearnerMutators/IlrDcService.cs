using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ESFA.DC.ILR.TestDataGenerator.Interfaces;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using MoreLinq;
using Polly;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.Core.Services;
using SFA.DAS.Payments.AcceptanceTests.Services;
using SFA.DAS.Payments.AcceptanceTests.Services.Exceptions;
using SFA.DAS.Payments.AcceptanceTests.Services.Intefaces;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Tests.Core;
using SFA.DAS.Payments.Tests.Core.Builders;
using Enums = ESFA.DC.Jobs.Model.Enums;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.LearnerMutators
{
    public class IlrDcService : IIlrService
    {
        private readonly ITdgService tdgService;
        private readonly TestSession testSession;
        private readonly IJobService jobService;
        private readonly IAzureStorageKeyValuePersistenceServiceConfig storageServiceConfig;
        private readonly IStreamableKeyValuePersistenceService storageService;
        private readonly IPaymentsDataContext dataContext;

        private readonly IApprenticeshipEarningsHistoryService appEarnHistoryService;

        public IlrDcService(ITdgService tdgService,
                            TestSession testSession,
                            IJobService jobService,
                            IAzureStorageKeyValuePersistenceServiceConfig storageServiceConfig,
                            IStreamableKeyValuePersistenceService storageService,
                            IPaymentsDataContext dataContext,
                            IApprenticeshipEarningsHistoryService appEarnHistoryService)
        {
            this.tdgService = tdgService;
            this.testSession = testSession;
            this.jobService = jobService;
            this.storageServiceConfig = storageServiceConfig;
            this.storageService = storageService;
            this.dataContext = dataContext;
            this.appEarnHistoryService = appEarnHistoryService;
        }

        public async Task PublishLearnerRequest(List<Training> previousIlr, List<Training> currentIlr, List<Learner> learners, string collectionPeriodText, string featureNumber, Func<Task> clearCache)
        {
            var collectionYear = collectionPeriodText.ToDate().Year;
            var collectionPeriod = new CollectionPeriodBuilder().WithSpecDate(collectionPeriodText).Build().Period;

            if (currentIlr != null && currentIlr.Any() && (learners == null || !learners.Any()))
            {
                foreach (var ilr in currentIlr)
                {
                    Provider provider;

                    if (ilr.Ukprn == default(long))
                    {
                        provider = testSession.GetProviderByIdentifier(null);
                    }
                    else
                    {
                        provider = testSession.Providers.Single(p => p.Ukprn == ilr.Ukprn);
                    }

                    var learner = testSession.GenerateLearner(provider.Ukprn);
                    learner.UpdateFromTrainingType(ilr, null);
                }
            }
            else if (learners != null && learners.Any() && learners.All(l => l.Aims == null || !l.Aims.Any()))
            {
                // Learners provided, but are incomplete
                foreach (var learner in learners)
                {
                    Training ilr;
                    // may need refactoring - not sure if these properties will be available at this point.
                    if (currentIlr.Count() > 1)
                    {
                        ilr = currentIlr.Single(i => i.Ukprn == learner.Ukprn && i.LearnerId == learner.LearnerIdentifier);
                    }
                    else
                    {
                        ilr = currentIlr.Single(i => i.Ukprn == learner.Ukprn);
                    }

                    var preIlr = previousIlr.SingleOrDefault(i => i.LearnerId == learner.LearnerIdentifier);

                    learner.UpdateFromTrainingType(ilr, preIlr);
                }
            }

            var learnerMutator = LearnerMutatorFactory.Create(featureNumber, learners);

            // Assumptions - all learners are for the same provider (per call to this method)
            var ilrFile = await tdgService.GenerateIlrTestData(learnerMutator, (int)learners.First().Ukprn);

            await RefreshTestSessionLearnerFromIlr(ilrFile.Value, learners);

            if (learners.Any(l => l.EarningsHistory != null) && !testSession.AtLeastOneScenarioCompleted)
            {
                await appEarnHistoryService.DeleteHistoryAsync(learners.First().Ukprn);
                await appEarnHistoryService.AddHistoryAsync(learners);
            }

            // this needs to be called here as the LearnRefNumber is updated to match the ILR in RefreshTestSessionLearnerFromIlr above
            await clearCache();

            await StoreAndPublishIlrFile((int)learners.First().Ukprn, ilrFileName: ilrFile.Key, ilrFile: ilrFile.Value, collectionYear: collectionYear, collectionPeriod: collectionPeriod);
        }

        private async Task RefreshTestSessionLearnerFromIlr(string ilrFile, IEnumerable<Learner> learners)
        {
            XNamespace xsdns = tdgService.IlrNamespace;
            var xDoc = XDocument.Parse(ilrFile);
            var learningProvider = xDoc.Descendants(xsdns + "LearningProvider");
            var ukprn = learningProvider.Elements(xsdns + "UKPRN").First().Value;

            var learnerDescendants = xDoc.Descendants(xsdns + "Learner");
            var learnersEnumeration = learners as Learner[] ?? learners.ToArray();

            for (var i = 0; i < learnersEnumeration.Count(); i++)
            {
                var request = learnersEnumeration.Skip(i).Take(1).First();
                var testSessionLearner = testSession.GetLearner(int.Parse(ukprn), request.LearnerIdentifier);
                var originalUln = testSessionLearner.OriginalUln ?? testSessionLearner.Uln;
                var learner = learnerDescendants.Skip(i).Take(1).First();
                testSessionLearner.LearnRefNumber = learner.Elements(xsdns + "LearnRefNumber").First().Value;
                testSessionLearner.Uln = long.Parse(learner.Elements(xsdns + "ULN").First().Value);

                await UpdatePaymentHistoryTables(testSessionLearner.Ukprn, originalUln, testSessionLearner.Uln,
                    testSessionLearner.LearnRefNumber);
            }
        }

        private async Task UpdatePaymentHistoryTables(long ukprn, long originalUln, long newUln, string learnRefNumber)
        {
            var payments = dataContext.Payment.Where(p => p.Ukprn == ukprn && p.LearnerUln == originalUln);
            foreach (var payment in payments)
            {
                payment.LearnerReferenceNumber = learnRefNumber;
                payment.LearnerUln = newUln;
            }

            await dataContext.SaveChangesAsync();
        }

        private async Task StoreAndPublishIlrFile(int ukprn, string ilrFileName, string ilrFile, int collectionYear, int collectionPeriod)
        {
            await StoreIlrFile(ukprn, ilrFileName, ilrFile);

            await PublishIlrFile(ukprn, ilrFileName, ilrFile, collectionYear, collectionPeriod);

        }

        private async Task PublishIlrFile(int ukprn, string ilrFileName, string ilrFile, int collectionYear, int collectionPeriod)
        {
            var submission = new SubmissionModel(EnumJobType.IlrSubmission, ukprn)
            {
                FileName = $"{ukprn}/{ilrFileName}",
                FileSizeBytes = ilrFile.Length,
                CreatedBy = "System",
                CollectionName = $"ILR{ilrFileName.Split('-')[2]}",
                Period = collectionPeriod,
                NotifyEmail = "dcttestemail@gmail.com",
                StorageReference = storageServiceConfig.ContainerName,
                CollectionYear = collectionYear
            };

            var jobId = await jobService.SubmitJob(submission);

            //TODO: Overriding JobId, but better implementation needed. Eg: calling GetProvider with proper Identifier when needed.
            foreach (var provider in testSession.Providers.Where(x => x.Ukprn == ukprn))
            {
                provider.JobId = jobId;
            }

            var retryPolicy = Policy
                .HandleResult<JobStatusType>(r => r != JobStatusType.Waiting)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

            var jobStatusResult = await retryPolicy.ExecuteAndCaptureAsync(async () => await jobService.GetJobStatus(jobId));
            if (jobStatusResult.Outcome == OutcomeType.Failure)
            {
                if (jobStatusResult.FinalHandledResult == JobStatusType.Ready)
                {
                    await jobService.DeleteJob(jobId);
                    throw new JobStatusNotWaitingException($"JobId:{jobId} is not yet in a Waiting Status. Current status: {jobStatusResult.FinalHandledResult}. " +
                                                           $"Ukprn: {ukprn} is probably blocked by an old job that hasn't completed.");
                }

                throw new JobStatusNotWaitingException($"JobId:{jobId} is not yet in a Waiting Status. Current status: {jobStatusResult.FinalHandledResult}");
            }

            await jobService.UpdateJobStatus(jobId, JobStatusType.Ready);

        }

        private async Task StoreIlrFile(int ukPrn, string ilrFileName, string ilrFile)
        {
            var byteArray = Encoding.UTF8.GetBytes(ilrFile);
            var stream = new MemoryStream(byteArray);

            var ilrStoragePathAndFileName = $"{ukPrn}/{ilrFileName}";

            await storageService.SaveAsync(ilrStoragePathAndFileName, stream);
        }
    }
}
