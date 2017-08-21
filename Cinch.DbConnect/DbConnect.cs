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

        public static T Execute<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                return Execute<T>(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static T Execute<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            return Enumerate<T>(conn, query, parameters, commandType, commandTimeout).FirstOrDefault();
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
            return (await EnumerateAsync<T>(conn, query, parameters, commandType, commandTimeout)).FirstOrDefault();       
        }
        
        public static IEnumerable<T> Enumerate<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                return Enumerate<T>(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static IEnumerable<T> Enumerate<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .SetCommandTimeout(commandTimeout)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return conn.Enumerate<T>(cmdBuilder);
        }
        public static async Task<IEnumerable<T>> EnumerateAsync<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(connStr))
            {
                return await EnumerateAsync<T>(conn, query, parameters, commandType, commandTimeout);
            }
        }
        public static async Task<IEnumerable<T>> EnumerateAsync<T>(SqlConnection conn, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure, int commandTimeout = 30)
        {
            var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                    .SetConnection(conn)
                                                    .SetCommandTimeout(commandTimeout)
                                                    .WithParameters(parameters)
                                                    .SetCommandType(commandType);

            return await conn.EnumerateAsync<T>(cmdBuilder);
        }
        
    }
}
