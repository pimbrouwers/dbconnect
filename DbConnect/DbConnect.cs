using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public class DbConnect
    {
        public static void Execute(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                Execute(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static void Execute(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                        .SetConnection(conn)
                                                        .SetCommandTimeout(commandTimeout)
                                                        .WithParameters(parameters)
                                                        .SetCommandType(commandType);

            conn.Execute(cmdBuilder);
        }
        
        public static T Execute<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                return Execute<T>(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static T Execute<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                        .SetConnection(conn)
                                                        .SetCommandTimeout(commandTimeout)
                                                        .WithParameters(parameters)
                                                        .SetCommandType(commandType);

            using (var cmd = cmdBuilder.SetConnection(conn).Build())
            using (var dbReader = cmd.GetReader())
            {
                return dbReader.Enumerate<T>().FirstOrDefault();
            }
        }
        
        public static async Task ExecuteAsync(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {                
                await ExecuteAsync(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static async Task ExecuteAsync(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .SetCommandTimeout(commandTimeout)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            await conn.ExecuteAsync(cmdBuilder);
        }
        
        public static async Task<T> ExecuteAsync<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                return await ExecuteAsync<T>(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static async Task<T> ExecuteAsync<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                        .SetConnection(conn)
                                                        .SetCommandTimeout(commandTimeout)
                                                        .WithParameters(parameters)
                                                        .SetCommandType(commandType);

            using (var cmd = cmdBuilder.SetConnection(conn).Build())
            using (var dbReader = await cmd.GetReaderAsync())
            {
                return dbReader.Enumerate<T>().FirstOrDefault();
            }
        }

        public static IDbEnumerator<T> Enumerate<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var dbReader = DbConnect.Reader(connStr, query, parameters, commandType, commandTimeout);

            return new DbEnumerator<T>(dbReader);
        }
        public static IDbEnumerator<T> Enumerate<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var dbReader = DbConnect.Reader(conn, query, parameters, commandType, commandTimeout);

            return new DbEnumerator<T>(dbReader);
        }

        public static async Task<IDbEnumerator<T>> EnumerateAsync<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var dbReader = await DbConnect.ReaderAsync(connStr, query, parameters, commandType, commandTimeout);

            return new DbEnumerator<T>(dbReader);
        }
        public static async Task<IDbEnumerator<T>> EnumerateAsync<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var dbReader = await DbConnect.ReaderAsync(conn, query, parameters, commandType, commandTimeout);

            return new DbEnumerator<T>(dbReader);
        }

        public static IDbReader Reader(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var conn = new SqlConnection(connStr);
            return Reader(conn, query, parameters, commandType, commandTimeout);
        }
        public static IDbReader Reader(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {            
            var cmd = new SqlCommandBuilder().CreateCommand(query)
                                             .SetConnection(conn)
                                             .SetCommandTimeout(commandTimeout)
                                             .SetCommandType(commandType)
                                             .WithParameters(parameters)
                                             .Build()
                                             .OpenConnection();

            var rd = cmd.GetReader();

            return new DbReader(cmd, rd, conn);
        }

        public static async Task<IDbReader> ReaderAsync(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var conn = new SqlConnection(connStr);
            return await ReaderAsync(conn, query, parameters, commandType, commandTimeout);
        }
        public static async Task<IDbReader> ReaderAsync(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {            
            var cmd = new SqlCommandBuilder().CreateCommand(query)
                                             .SetConnection(conn)
                                             .SetCommandTimeout(commandTimeout)
                                             .SetCommandType(commandType)
                                             .WithParameters(parameters)
                                             .Build();

            await cmd.Connection.OpenConnectionAsync();

            var rd = await cmd.GetReaderAsync();

            return new DbReader(cmd, rd, conn);
        }
    }
}
