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
        SqlConnection conn;
        SqlCommand cmd;
        SqlTransaction trans;
        
        public DbConnect(string connStr)
        {
            conn = BuildConnection(connStr);
        }

        public void Dispose()
        {
            conn?.Dispose();
            cmd?.Dispose();
            trans?.Dispose();
        }
        
        public void Execute(string query, 
                            object parameters = null, 
                            DbParams dbParams = null, 
                            CommandType commandType = CommandType.StoredProcedure, 
                            int commandTimeout = 30, 
                            SqlTransaction transaction = null, 
                            Action<SqlCommand> afterExecution = null)
        {
            Open();

            using (cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams, transaction))
            {
                cmd.ExecuteNonQuery();

                afterExecution?.Invoke(cmd);
            }
        }

        public IEnumerable<T> Execute<T>(string query, object parameters = null, DbParams dbParams = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30, SqlTransaction transaction = null)
        {
            Open();
            
            using (cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams, transaction))
            using (var rd = cmd.GetReader())
            {
                while (rd.Read())
                    yield return rd.ConvertTo<T>();
            }
        }

        public SqlDataReader Reader(string query, object parameters = null, DbParams dbParams = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30, SqlTransaction transaction = null)
        {
            Open();
            
            cmd = BuildCommand(conn, query, commandType, commandTimeout, parameters, dbParams, transaction);

            return cmd.GetReader();
        }
        
        public void Bulk<T>(SqlDataReader rd, string destinationTableName, int batchSize = 5000, int bulkCopyTimeout = 30, IEnumerable<string> ignoreCols = null, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, SqlTransaction transaction = null) where T : class, new()
        {
            Open();

            using (var bcp = BuildBulkCopy(conn, sqlBulkCopyOptions, transaction))
            using (var dataReader = ObjectReader.Create(rd.Read<T>()))
            {                
                bcp.MapColumns<T>(ignoreCols);

                bcp.DestinationTableName = destinationTableName;
                bcp.BatchSize = batchSize;
                bcp.BulkCopyTimeout = bulkCopyTimeout;
                
                bcp.WriteToServer(dataReader);
            }
        }

        public SqlTransaction BeginTransaction()
        {
            Open();

            trans = conn.BeginTransaction();
            return trans;
        }

        private SqlBulkCopy BuildBulkCopy(SqlConnection conn, SqlBulkCopyOptions sqlBulkCopyOptions, SqlTransaction transaction = null)
        {
            return new SqlBulkCopy(conn, sqlBulkCopyOptions, transaction);
        }

        private SqlCommand BuildCommand(SqlConnection conn, string query, CommandType commandType, int commandTimeout, object parameters = null, DbParams dbParams = null, SqlTransaction transaction = null)
        {
            var command = new SqlCommand(query, conn) { CommandTimeout = commandTimeout, CommandType = commandType };

            if (parameters != null)
                command.MapParameters(parameters);
            else if (dbParams != null)
                command.AddDbParams(dbParams);

            if(transaction != null)
            {
                command.Transaction = transaction;
            }            

            return command;
        }
        
        private SqlConnection BuildConnection(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            
            return conn;
        }

        private void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }        
    }
    
}
