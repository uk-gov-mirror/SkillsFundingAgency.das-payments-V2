using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
using SFA.DAS.Payments.FundingSource.Model;

namespace SFA.DAS.Payments.FundingSource.Application.Data
{
    public interface IFundingSourceBulkInsert
    {
        Task Insert(List<LevyTransactionModel> models);
    }

    public class FundingSourceBulkInsert : IFundingSourceBulkInsert
    {
        private readonly string connectionString;

        public FundingSourceBulkInsert(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task Insert(List<LevyTransactionModel> models)
        {
            var columnMap = typeof(LevyTransactionModel).GetProperties().ToDictionary(p => p.Name, p => p.Name);
            using (var bcp = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.TableLock))
            {
                foreach (var keyValuePair in columnMap)
                {
                    bcp.ColumnMappings.Add(keyValuePair.Key, keyValuePair.Value);
                }

                using (var reader = ObjectReader.Create(models))
                {
                    bcp.BatchSize = models.Count;
                    bcp.BulkCopyTimeout = 0;
                    bcp.DestinationTableName = "Payments2.FundingSourceLevyTransaction";
                    await bcp.WriteToServerAsync(reader).ConfigureAwait(false);
                }
            }
        }

    }
}