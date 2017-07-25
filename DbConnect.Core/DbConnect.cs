using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cinch
{        
    public class DbConnect : IDisposable
    {
        string connStr;
        SqlConnection conn;
        SqlCommand cmd;
        SqlTransaction trans;

        public DbConnect(string connStr)
        {
            this.connStr = connStr;
        }

        public void Dispose()
        {
            conn?.Dispose();
            cmd?.Dispose();
        }

        public void Execute(string query, object parameters = null, DbParams dbParams = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (conn = BuildConnection(this.connStr))
            using (cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams))
            {
                cmd.ExecuteNonQuery();
            }
        }
        
        public IEnumerable<T> Execute<T>(string query, object parameters = null, DbParams dbParams = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (conn = BuildConnection(this.connStr))
            using (cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams))
            using (var rd = cmd.FillDataReader())
            {
                while (rd.Read())
                    yield return rd.ConvertTo<T>();
            }
        }        

        public SqlDataReader Multiple(string query, object parameters = null, DbParams dbParams = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30) {
            conn = BuildConnection(this.connStr);
            cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams);

            return cmd.FillDataReader();
        }

        public void Bulk<T>(string destinationTableName, int batchSize = 5000, int? bulkCopyTimeout = null, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            using (conn = BuildConnection(this.connStr))
            using (var bcp = new SqlBulkCopy(conn))
            using (var rd = cmd.FillDataReader())
            {
                bcp.MapColumns<T>(ignoreCols);

                bcp.DestinationTableName = destinationTableName;
                bcp.BatchSize = batchSize;

                if (bulkCopyTimeout.HasValue)
                    bcp.BulkCopyTimeout = bulkCopyTimeout.Value;

                bcp.WriteToServer(rd);
            }
        }

        private SqlCommand BuildCommand(SqlConnection conn, string query, CommandType commandType, int commandTimeout, object parameters = null, DbParams dbParams = null)
        {
            var command = new SqlCommand(query, conn) { CommandTimeout = commandTimeout, CommandType = commandType };

            if (parameters != null)
                command.MapParameters(parameters);
            else if (dbParams != null)
                command.AddDbParams(dbParams);

            return command;
        }

        private SqlConnection BuildConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        #region SqlCommand Transaction
        /// <summary>
        /// This starts a transaction, can manually rollback or commit and on close with automatically commit.
        /// </summary>
        public void BeginTransaction()
        {
            trans = conn.BeginTransaction();
            cmd.Transaction = trans;
        }

        /// <summary>
        /// This commits the transaction if the object exists.
        /// </summary>
        public void CommitTransaction()
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Commit();
            }
        }

        /// <summary>
        /// This allows the user to rollback all the way to the beginning.
        /// </summary>
        public void RollbackTransaction()
        {
            RollbackTransaction("");
        }

        /// <summary>
        /// This allows the user to rollback to the last save point.
        /// </summary>
        public void RollbackTransaction(string rollBackName)
        {
            if (cmd.Transaction != null)
            {
                if (rollBackName != "")
                {
                    cmd.Transaction.Rollback(rollBackName);
                }
                else
                {
                    cmd.Transaction.Rollback();
                }
            }
        }

        /// <summary>
        /// This allows the user to save a specific rollback marker.
        /// </summary>
        public void SaveTransaction(string savePointName)
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Save(savePointName);
            }
        }
        #endregion
    }
    
}
