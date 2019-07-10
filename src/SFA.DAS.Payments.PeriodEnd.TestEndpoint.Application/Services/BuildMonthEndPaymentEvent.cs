using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Payments.DataLocks.Messages.Internal;
using SFA.DAS.Payments.FundingSource.Messages.Internal.Commands;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Repositories;
using SFA.DAS.Payments.ProviderPayments.Messages.Internal.Commands;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Services
{
    public class BuildMonthEndPaymentEvent : IBuildMonthEndPaymentEvent
    {
        private readonly ITestEndPointRepository testEndPointRepository;
        private readonly IApprenticeshipKeyService apprenticeshipKeyService;
        private readonly Random random;

        public BuildMonthEndPaymentEvent(ITestEndPointRepository testEndPointRepository, IApprenticeshipKeyService apprenticeshipKeyService)
        {
            this.testEndPointRepository = testEndPointRepository;
            this.apprenticeshipKeyService = apprenticeshipKeyService;
            random = new Random();
        }

        public async Task<CollectionStartedEvent> CreateCollectionStartedEvent(long ukprn, short academicYear)
        {
            var providerAims = await testEndPointRepository
                .GetProviderLearnerAims(ukprn)
                .ConfigureAwait(false);

            var apprenticeshipKeys = providerAims.Select(aim =>
                    apprenticeshipKeyService.GenerateApprenticeshipKey(
                        ukprn,
                        aim.LearnerReferenceNumber,
                        aim.LearningAimFrameworkCode,
                        aim.LearningAimPathwayCode,
                        aim.LearningAimProgrammeType,
                        aim.LearningAimStandardCode,
                        aim.LearningAimReference,
                        academicYear,
                        aim.ContractType))
                .ToList();

            return new CollectionStartedEvent
            {
                ApprenticeshipKeys = apprenticeshipKeys,
                JobId = GenerateId()
            };
        }

        public ProcessProviderMonthEndCommand CreateProcessProviderMonthEndCommand(long ukprn, short academicYear,
            byte period)
        {
            return new ProcessProviderMonthEndCommand
            {
                Ukprn = ukprn,
                JobId = GenerateId(),
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = academicYear,
                    Period = period,
                },
                SubmissionDate = DateTime.UtcNow,
                RequestTime = DateTimeOffset.UtcNow
            };
        }

        public async Task<List<ProcessLevyPaymentsOnMonthEndCommand>> CreateProcessLevyPaymentsOnMonthEndCommand( short academicYear, byte period)
        {
            var accountIds = await testEndPointRepository
                .GetAccountIds()
                .ConfigureAwait(false);

            var commands = new List<ProcessLevyPaymentsOnMonthEndCommand>();

            foreach (var accountId in accountIds)
            {
                commands.Add(new ProcessLevyPaymentsOnMonthEndCommand
                {
                    CommandId = Guid.NewGuid(),
                    JobId = GenerateId(),
                    CollectionPeriod = new CollectionPeriod
                    {
                        AcademicYear = academicYear,
                        Period = period,
                    },
                    RequestTime = DateTime.UtcNow,
                    SubmissionDate = DateTime.UtcNow,
                    AccountId = accountId
                });
            }

            return commands;
        }

        public async Task<List<ResetCacheCommand>> CreateDataLockResetCommand()
        {
            var ukprns = await testEndPointRepository
                .GetUkprns()
                .ConfigureAwait(false);

            var commands = new List<ResetCacheCommand>();

            foreach (var ukprn in ukprns)
            {
                commands.Add(new ResetCacheCommand
                {
                    Ukprn = ukprn
                });
            }

            return commands;
        }


        private long GenerateId(int maxValue = 1000000000)
        {
            var id = random.Next(maxValue);
            return id;
        }
    }
}