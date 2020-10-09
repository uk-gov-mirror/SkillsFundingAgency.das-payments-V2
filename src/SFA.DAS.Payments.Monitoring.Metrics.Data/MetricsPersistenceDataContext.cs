using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses;
using SFA.DAS.Payments.Monitoring.Metrics.Data.Configuration;
using SFA.DAS.Payments.Monitoring.Metrics.Model.PeriodEnd;
using SFA.DAS.Payments.Monitoring.Metrics.Model.Submission;

namespace SFA.DAS.Payments.Monitoring.Metrics.Data
{

    public interface IMetricsPersistenceDataContext
    {
        DbSet<SubmissionSummaryModel> SubmissionSummaries { get; }
        Task Save(SubmissionSummaryModel submissionSummary, CancellationToken cancellationToken);
        Task SaveProviderSummaries(List<ProviderPeriodEndSummaryModel> providerSummaries, PeriodEndSummaryModel overallPeriodEndSummary, CancellationToken cancellationToken);
        Task<SubmissionsSummaryModel> GetSubmissionsSummary(short academicYear, byte collectionPeriod);
    }

    public class MetricsPersistenceDataContext: DbContext, IMetricsPersistenceDataContext
    {
        private readonly string connectionString;
        public virtual DbSet<SubmissionSummaryModel> SubmissionSummaries { get; set; }
        public virtual DbSet<PeriodEndSummaryModel> PeriodEndSummaries { get; set; }
        public virtual DbSet<ProviderPeriodEndSummaryModel> ProviderPeriodEndSummaries { get; set; }
        public virtual DbSet<ProviderPaymentTransactionModel> ProviderPaymentTransactions { get; set; }
        public virtual DbSet<ProviderPaymentFundingSourceModel> ProviderPaymentFundingSources { get; set; }
        public virtual DbQuery<SubmissionsSummaryModel> SubmissionsSummaries { get; set; }
        //[DbFunction("GetSubmissionsSummary","Metrics")]
        //public static List<SubmissionsSummaryModel> GetSubmissionsSummary(short academicYear, byte collectionPeriod) { throw new NotImplementedException(); }

        public MetricsPersistenceDataContext(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Metrics");
            modelBuilder.ApplyConfiguration(new SubmissionSummaryModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockedEarningsModelConfiguration());
            modelBuilder.ApplyConfiguration(new EarningsModelConfiguration());
            modelBuilder.ApplyConfiguration(new RequiredPaymentsModelConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderPeriodEndSummaryModelConfiguration());
            modelBuilder.ApplyConfiguration(new PeriodEndSummaryModelConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderPaymentTransactionModelConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderPaymentFundingSourceModelConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public async Task Save(SubmissionSummaryModel submissionSummary, CancellationToken cancellationToken)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await Database.ExecuteSqlCommandAsync($@"
                    Delete 
                        From [Metrics].[SubmissionSummary] 
                    Where 
                        Ukprn = {submissionSummary.Ukprn}
                        And AcademicYear = {submissionSummary.AcademicYear}
                        And CollectionPeriod = {submissionSummary.CollectionPeriod}
                    "
                    , cancellationToken);
                SubmissionSummaries.Add(submissionSummary);
                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task SaveProviderSummaries(List<ProviderPeriodEndSummaryModel> providerSummaries, PeriodEndSummaryModel overallPeriodEndSummary,
            CancellationToken cancellationToken)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await Database.ExecuteSqlCommandAsync($@"
                    Delete 
                        From [Metrics].[PeriodEndSummary] 
                    Where 
                        AcademicYear = {overallPeriodEndSummary.AcademicYear}
                        And CollectionPeriod = {overallPeriodEndSummary.CollectionPeriod}
                    "
                    , cancellationToken);

                await PeriodEndSummaries.AddAsync(overallPeriodEndSummary, cancellationToken);

                await Database.ExecuteSqlCommandAsync($@"
                    Delete 
                        From [Metrics].[ProviderPeriodEndSummary] 
                    Where 
                        AcademicYear = {overallPeriodEndSummary.AcademicYear}
                        And CollectionPeriod = {overallPeriodEndSummary.CollectionPeriod}
                    "
                    , cancellationToken);
                await ProviderPeriodEndSummaries.AddRangeAsync(providerSummaries, cancellationToken);

                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<SubmissionsSummaryModel> GetSubmissionsSummary(short academicYear, byte collectionPeriod)
        {
            return await Query<SubmissionsSummaryModel>().FromSql($"Select * from [Metrics].GetSubmissionsSummary({academicYear},{collectionPeriod})")
                .FirstOrDefaultAsync();
        }
    }
}