using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public static class SqlConnectionExtensions
    {
        public static void OpenConnection(this SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        public static async Task OpenConnectionAsync(this SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
        }

        public static SqlTransaction BeginTransaction(this SqlConnection conn)
        {
            conn.OpenConnection();

            var trans = conn.BeginTransaction();
            return trans;
        }

        public static async Task<SqlTransaction> BeginTransactionAsync(this SqlConnection conn)
        {
            await conn.OpenConnectionAsync();

            var trans = conn.BeginTransaction();
            return trans;
        }

        #region Helpers
        public static void Execute(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            conn.Execute(cmdBuilder);
        }

        public static async Task ExecuteAsync(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            await conn.ExecuteAsync(cmdBuilder);
        }

        public static IEnumerable<T> Execute<T>(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return conn.Execute<T>(cmdBuilder);
        }

        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return await conn.ExecuteAsync<T>(cmdBuilder);
        }

        public static DbReader Reader(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return conn.Reader(cmdBuilder);
        }

        public static async Task<DbReader> ReaderAsync(this SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return await conn.ReaderAsync(cmdBuilder);
        }

        public static void Bulk<T>(this SqlConnection conn, IEnumerable<T> srcData, string destinationTableName) where T : class, new()
        {
            var bcpBuilder = new SqlBulkCopyBuilder().CreateBcp(destinationTableName);

            conn.Bulk<T>(bcpBuilder, srcData);
        }

        public static async Task BulkAsync<T>(this SqlConnection conn, IEnumerable<T> srcData, string destinationTableName) where T : class, new()
        {
            var bcpBuilder = new SqlBulkCopyBuilder().CreateBcp(destinationTableName);

            await conn.BulkAsync<T>(bcpBuilder, srcData);
        }
        #endregion

        #region Commands
        public static void Execute(this SqlConnection conn, SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null)
        {
            using (var cmd = cmdBuilder.SetConnection(conn).Build())
            {
                cmd.Connection.OpenConnection();
                cmd.ExecuteNonQuery();

                afterExecution?.Invoke(cmd);
            }
        }

        public static async Task ExecuteAsync(this SqlConnection conn, SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null)
        {
            using (var cmd = cmdBuilder.SetConnection(conn).Build())
            {
                await cmd.Connection.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();

                afterExecution?.Invoke(cmd);
            }
        }
        #endregion

        #region Queries
        public static IEnumerable<T> Execute<T>(this SqlConnection conn, SqlCommandBuilder cmdBuilder)
        {
            using (var cmd = cmdBuilder.SetConnection(conn).Build())
            {
                cmd.Connection.OpenConnection();

                using (var rd = cmd.GetReader())
                {
                    return rd.Read<T>();
                }
            }
        }

        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this SqlConnection conn, SqlCommandBuilder cmdBuilder)
        {
            using (var cmd = cmdBuilder.SetConnection(conn).Build())
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
        public static DbReader Reader(this SqlConnection conn, SqlCommandBuilder cmdBuilder)
        {
            var cmd = cmdBuilder.SetConnection(conn).Build();            
            cmd.Connection.OpenConnection();

            var rd = cmd.GetReader();

            return new DbReader(cmd, rd);
        }

        public static async Task<DbReader> ReaderAsync(this SqlConnection conn, SqlCommandBuilder cmdBuilder)
        {
            var cmd = cmdBuilder.SetConnection(conn).Build();
            await cmd.Connection.OpenConnectionAsync();

            var rd = await cmd.GetReaderAsync();

            return new DbReader(cmd, rd);
        }
        #endregion

        #region Bulk Copy
        public static void Bulk<T>(this SqlConnection conn, SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            conn.OpenConnection();

            using (var bcp = bcpBuilder.SetConnection(conn).Build())
            {
                bcp.MapColumns<T>(ignoreCols);

                using (var dataReader = ObjectReader.Create(srcData))
                {
                    bcp.WriteToServer(dataReader);
                }
            }
        }

        public static async Task BulkAsync<T>(this SqlConnection conn, SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            await conn.OpenConnectionAsync();

            using (var bcp = bcpBuilder.SetConnection(conn).Build())
            {
                bcp.MapColumns<T>(ignoreCols);

                using (var dataReader = ObjectReader.Create(srcData))
                {
                    await bcp.WriteToServerAsync(dataReader);
                }
            }
        }
        #endregion
    }
}
