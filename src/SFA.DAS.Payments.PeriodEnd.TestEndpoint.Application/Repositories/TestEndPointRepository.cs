using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Remotion.Linq.Clauses;
using SFA.DAS.Payments.Monitoring.Jobs.Data.Model;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Repositories
{

    public interface ITestEndPointDataContext : IPaymentsDataContext
    {
        DbSet<JobModel> Job { get; set; }
    }

    public class JobModelConfiguration : IEntityTypeConfiguration<JobModel>
    {
        public void Configure(EntityTypeBuilder<JobModel> builder)
        {
            builder.ToTable("Job", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(@"JobId").IsRequired();
            builder.Property(x => x.JobType).HasColumnName(@"JobType").IsRequired();
            builder.Property(x => x.StartTime).HasColumnName(@"StartTime").IsRequired();
            builder.Property(x => x.EndTime).HasColumnName(@"EndTime");
            builder.Property(x => x.Status).HasColumnName(@"Status").IsRequired();
            builder.Property(x => x.DcJobId).HasColumnName(@"DcJobId");
            builder.Property(x => x.Ukprn).HasColumnName(@"Ukprn");
            builder.Property(x => x.IlrSubmissionTime).HasColumnName(@"IlrSubmissionTime");
            builder.Property(x => x.LearnerCount).HasColumnName(@"LearnerCount");
            builder.Property(x => x.AcademicYear).HasColumnName(@"AcademicYear").IsRequired();
            builder.Property(x => x.CollectionPeriod).HasColumnName(@"CollectionPeriod").IsRequired();
        }
    }

    public class TestEndPointDataContext : PaymentsDataContext, ITestEndPointDataContext
    {
        public virtual DbSet<JobModel> Job { get; set; }

        public TestEndPointDataContext(string connectionString) : base(connectionString)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new JobModelConfiguration());
        }

    }

    public class TestEndPointRepository : ITestEndPointRepository
    {
        
        private readonly ITestEndPointDataContext paymentsDataContext;

        public TestEndPointRepository(ITestEndPointDataContext paymentsDataContext)
        {
            this.paymentsDataContext = paymentsDataContext;
        }

        public async Task<List<SubmittedLearnerAimModel>> GetProviderLearnerAims(long ukprn, CancellationToken cancellationToken = default(CancellationToken))
        {
            var aims = await paymentsDataContext
                .SubmittedLearnerAim
                .Where(x => x.Ukprn == ukprn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return aims;
        }

        public async Task<List<long>> GetAccountIds( long ukprn,CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountIdTuples = await paymentsDataContext
                .Apprenticeship
                .Where(x => x.Ukprn == ukprn)
                .Select(x => new {x.AccountId, x.TransferSendingEmployerAccountId})
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var accountIds = accountIdTuples.Select(x => x.AccountId).ToList();
            var transferAccountIds = accountIdTuples
                                    .Where(o => o.TransferSendingEmployerAccountId.HasValue)
                                    .Select(o => o.TransferSendingEmployerAccountId.Value)
                                    .ToList();
            accountIds.AddRange(transferAccountIds);

            return accountIds.Distinct().ToList();
        }

        public async Task<List<long>> GetUkprns(CancellationToken cancellationToken = default(CancellationToken))
        {
            var ukprns = await paymentsDataContext
                .Apprenticeship
                .Select(x => x.Ukprn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

           
            return ukprns.Distinct().ToList();
        }

        public async Task CreateMonitoringJob(JobModel job, CancellationToken cancellationToken = default(CancellationToken))
        {

           await paymentsDataContext
                .Job.AddAsync(job, cancellationToken)
                .ConfigureAwait(false);

           await paymentsDataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

    }
}