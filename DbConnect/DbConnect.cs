using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            conn = new SqlConnection(connStr);
        }

        public void Dispose()
        {
            conn?.Dispose();
            cmd?.Dispose();
            trans?.Dispose();
        }

        #region Helpers
        public void Execute(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            this.Execute(cmdBuilder);
        }

        public async Task ExecuteAsync(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            await this.ExecuteAsync(cmdBuilder);
        }

        public IEnumerable<T> Execute<T>(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return this.Execute<T>(cmdBuilder);
        }

        public async Task<IEnumerable<T>> ExecuteAsync<T>(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return await this.ExecuteAsync<T>(cmdBuilder);
        }

        public SqlDataReader Reader(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return this.Reader(cmdBuilder);
        }

        public async Task<SqlDataReader> ReaderAsync(string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return await this.ReaderAsync(cmdBuilder);
        }

        public void Bulk<T>(IEnumerable<T> srcData, string destinationTableName) where T : class, new()
        {
            var bcpBuilder = new SqlBulkCopyBuilder().CreateBcp(destinationTableName);

            this.Bulk<T>(bcpBuilder, srcData);
        }

        public async Task BulkAsync<T>(IEnumerable<T> srcData, string destinationTableName) where T : class, new()
        {
            var bcpBuilder = new SqlBulkCopyBuilder().CreateBcp(destinationTableName);

            await this.BulkAsync<T>(bcpBuilder, srcData);
        }
        #endregion

        #region Commands
        public void Execute(SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null)
        {
            using (cmd = cmdBuilder.SetConnection(conn))
            {
                cmd.Connection.OpenConnection();
                cmd.ExecuteNonQuery();

                afterExecution?.Invoke(cmd);
            }
        }

        public async Task ExecuteAsync(SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null)
        {
            using (cmd = cmdBuilder.SetConnection(conn))
            {
                await cmd.Connection.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();

                afterExecution?.Invoke(cmd);
            }
        }
        #endregion

        #region Queries
        public IEnumerable<T> Execute<T>(SqlCommandBuilder cmdBuilder)
        {
            using (cmd = cmdBuilder.SetConnection(conn))
            {
                cmd.Connection.OpenConnection();

                using (var rd = cmd.GetReader())
                {
                    return rd.Read<T>();
                }
            }
        }

        public async Task<IEnumerable<T>> ExecuteAsync<T>(SqlCommandBuilder cmdBuilder)
        {
            using (cmd = cmdBuilder.SetConnection(conn))
            {
                await cmd.Connection.OpenConnectionAsync();

                using (var rd = await cmd.GetReaderAsync())
                {
                    return await rd.ReadAsync<T>();
                }
            }
        }
        #endregion

        #region Reader
        public SqlDataReader Reader(SqlCommandBuilder cmdBuilder)
        {
            cmd = cmdBuilder.SetConnection(conn);

            cmd.Connection.OpenConnection();

            return cmd.GetReader();
        }

        public async Task<SqlDataReader> ReaderAsync(SqlCommandBuilder cmdBuilder)
        {
            cmd = cmdBuilder.SetConnection(conn);

            await cmd.Connection.OpenConnectionAsync();

            return await cmd.GetReaderAsync();
        }
        #endregion

        #region Bulk Copy
        public void Bulk<T>(SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            conn.OpenConnection();

            using (SqlBulkCopy bcp = bcpBuilder.SetConnection(conn))
            {
                bcp.MapColumns<T>(ignoreCols);

                using (var dataReader = ObjectReader.Create(srcData))
                {
                    bcp.WriteToServer(dataReader);
                }
            }
        }

        public async Task BulkAsync<T>(SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            await conn.OpenConnectionAsync();

            using (SqlBulkCopy bcp = bcpBuilder.SetConnection(conn))
            {
                bcp.MapColumns<T>(ignoreCols);

                using (var dataReader = ObjectReader.Create(srcData))
                {
                    await bcp.WriteToServerAsync(dataReader);
                }
            }
        }
        #endregion

        public SqlTransaction BeginTransaction()
        {
            conn.OpenConnection();

            trans = conn.BeginTransaction();
            return trans;
        }

        public async Task<SqlTransaction> BeginTransactionAsync()
        {
            await conn.OpenConnectionAsync();

            trans = conn.BeginTransaction();
            return trans;
        }
    }

}
