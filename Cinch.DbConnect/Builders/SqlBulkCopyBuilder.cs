using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Cinch.DbConnect
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
        public SqlBulkCopy Build ()
        {
            var bcp = new SqlBulkCopy(this.conn, this.sqlBulkCopyOptions, this.transaction);

            bcp.DestinationTableName = this.destinationTableName;
            bcp.BatchSize = this.batchSize;
            bcp.BulkCopyTimeout = this.bulkCopyTimeout;

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
