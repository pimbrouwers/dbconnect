using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Cinch
{
    public class SqlBulkCopyBuilder
    {
        SqlConnection conn;
        string destinationTableName;
        int batchSize;
        int bulkCopyTimeout;
        SqlBulkCopyOptions sqlBulkCopyOptions;
        SqlTransaction transaction;

        // CONVERSION OPERATOR
        public static implicit operator SqlBulkCopy(SqlBulkCopyBuilder bld)
        {
            var bcp = new SqlBulkCopy(bld.conn, bld.sqlBulkCopyOptions, bld.transaction);

            bcp.DestinationTableName = bld.destinationTableName;
            bcp.BatchSize = bld.batchSize;
            bcp.BulkCopyTimeout = bld.bulkCopyTimeout;

            return bcp;
        }

        public SqlBulkCopyBuilder CreateBcp(string destinationTableName)
        {
            this.destinationTableName = destinationTableName;

            return this;
        }

        public SqlBulkCopyBuilder SetConnection(SqlConnection conn)
        {
            if (this.conn == null)
                this.conn = conn;

            return this;
        }

        public SqlBulkCopyBuilder SetBatchSize(int batchSize)
        {
            this.batchSize = batchSize;
            return this;
        }

        public SqlBulkCopyBuilder SetTimeout(int bulkCopyTimeout)
        {
            this.bulkCopyTimeout = bulkCopyTimeout;
            return this;
        }

        public SqlBulkCopyBuilder WithOptions(SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            this.sqlBulkCopyOptions = sqlBulkCopyOptions;
            return this;
        }

        public SqlBulkCopyBuilder UsingTransaction(SqlTransaction transaction)
        {
            this.transaction = transaction;
            return this;
        }
    }
}
