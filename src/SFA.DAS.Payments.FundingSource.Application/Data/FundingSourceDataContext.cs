using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.FundingSource.Application.Data.Configurations;
using SFA.DAS.Payments.FundingSource.Model;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.FundingSource.Application.Data
{
    public interface IFundingSourceDataContext
    {
        DbSet<LevyAccountModel> LevyAccounts { get; }
        DbSet<EmployerProviderPriorityModel> EmployerProviderPriorities { get; }
        DbSet<LevyTransactionModel> LevyTransactions { get; }
        Task<int> SaveChanges(CancellationToken cancellationToken);
    }

    public class FundingSourceDataContext : DbContext, IFundingSourceDataContext
    {
        protected readonly string connectionString;
        public virtual DbSet<LevyAccountModel> LevyAccounts { get; protected set; }
        public virtual DbSet<EmployerProviderPriorityModel> EmployerProviderPriorities { get; protected set; }
        public virtual DbSet<LevyTransactionModel> LevyTransactions { get; protected set; }
        public async Task<int> SaveChanges(CancellationToken cancellationToken)
        {
            return await SaveChangesAsync(cancellationToken);
        }

        public FundingSourceDataContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Payments2");
            modelBuilder.ApplyConfiguration(new LevyAccountModelConfiguration());
            modelBuilder.ApplyConfiguration(new EmployerProviderPriorityModelConfiguration());
            modelBuilder.ApplyConfiguration(new LevyTransactionModelConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connectionString != null)
                optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
