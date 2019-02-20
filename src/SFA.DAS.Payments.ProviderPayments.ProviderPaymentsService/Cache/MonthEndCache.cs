﻿using Microsoft.ServiceFabric.Data.Collections;
using SFA.DAS.Payments.Audit.Application.ServiceFabric.Infrastructure;
using SFA.DAS.Payments.ProviderPayments.Application.Services;
using SFA.DAS.Payments.ProviderPayments.Model;
using SFA.DAS.Payments.ServiceFabric.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.ProviderPayments.ProviderPaymentsService.Cache
{
    public class MonthEndCache : IMonthEndCache
    {
        private readonly IReliableStateManagerTransactionProvider transactionProvider;
        private readonly IReliableStateManagerProvider stateManagerProvider;
        private IReliableDictionary2<string, MonthEndDetails> reliableDictionary;
        private static readonly object lockObject = new object();

        public MonthEndCache(IReliableStateManagerProvider stateManagerProvider, IReliableStateManagerTransactionProvider transactionProvider)
        {
            this.transactionProvider = transactionProvider ?? throw new ArgumentNullException(nameof(transactionProvider));
            this.stateManagerProvider = stateManagerProvider ?? throw new ArgumentNullException(nameof(stateManagerProvider));
        }

        public async Task AddOrReplace(long ukprn, short academicYear, byte collectionPeriod, long monthEndJobId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = CreateKey(ukprn, academicYear, collectionPeriod);
            var entity = new MonthEndDetails { Ukprn = ukprn, AcademicYear = academicYear, CollectionPeriod = collectionPeriod, JobId = monthEndJobId };
            var state = await GetState();

            await state.AddOrUpdateAsync(transactionProvider.Current, key, entity, (newKey, monthEnd) => monthEnd, TimeSpan.FromSeconds(4), cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Exists(long ukprn, short academicYear, byte collectionPeriod, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = CreateKey(ukprn, academicYear, collectionPeriod);
            var state = await GetState();

            return await state
                .ContainsKeyAsync(transactionProvider.Current, key, TimeSpan.FromSeconds(2), cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<MonthEndDetails> GetMonthEndDetails(long ukprn, short academicYear, byte collectionPeriod, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = CreateKey(ukprn, academicYear, collectionPeriod);
            var state = await GetState();
            var value = await state.TryGetValueAsync(transactionProvider.Current, key, TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            return value.Value;
        }

        private async Task<IReliableDictionary2<string, MonthEndDetails>> GetState()
        {
            if (reliableDictionary != null) return reliableDictionary;

            if (Monitor.TryEnter(lockObject, TimeSpan.FromSeconds(2)))
            {
                try
                {
                        reliableDictionary = await stateManagerProvider
                            .Current
                            .GetOrAddAsync<IReliableDictionary2<string, MonthEndDetails>>(transactionProvider.Current, "MonthEndCache")
                            .ConfigureAwait(false);
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
            }
            else
            {
                throw new Exception("Unable obtain State Manager Provider ReliableDictionary");
            }

            return reliableDictionary;
        }

        private static string CreateKey(long ukprn, short academicYear, byte collectionPeriod)
        {
            return $"{ukprn}-{academicYear}-{collectionPeriod}";
        }
    }
}