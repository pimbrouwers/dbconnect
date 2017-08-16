using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public class DbConnect
    {
        public static void Execute(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var conn = new SqlConnection(connStr))
            {
                var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                        .SetConnection(conn)
                                                        .WithParameters(parameters)
                                                        .SetCommandType(commandType);

                conn.Execute(cmdBuilder);
            }                
        }

        public static async Task ExecuteAsync(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var conn = new SqlConnection(connStr))
            {
                var cmdBuilder = new SqlCommandBuilder().CreateCommand(query)
                                                        .SetConnection(conn)
                                                        .WithParameters(parameters)
                                                        .SetCommandType(commandType);

                await conn.ExecuteAsync(cmdBuilder);
            }
        }

        public static IDbEnumerator<T> Enumerate<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {            
            var dbReader = DbConnect.Reader(connStr, query, parameters, commandType);

            return new DbEnumerator<T>(dbReader);
        }

        public static async Task<IDbEnumerator<T>> EnumerateAsync<T>(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var dbReader = await DbConnect.ReaderAsync(connStr, query, parameters, commandType);

            return new DbEnumerator<T>(dbReader);
        }

        public static IDbReader Reader(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var conn = new SqlConnection(connStr);
            var cmd = new SqlCommandBuilder().CreateCommand(query)
                                             .SetConnection(conn)
                                             .WithParameters(parameters)
                                             .SetCommandType(commandType)
                                             .Build()
                                             .OpenConnection();

            var rd = cmd.GetReader();

            return new DbReader(cmd, rd, conn);
        }

        public static async Task<IDbReader> ReaderAsync(string connStr, string query, object parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var conn = new SqlConnection(connStr);
            var cmd = new SqlCommandBuilder().CreateCommand(query)
                                             .SetConnection(conn)
                                             .WithParameters(parameters)
                                             .SetCommandType(commandType)
                                             .Build();

            await cmd.Connection.OpenConnectionAsync();

            var rd = await cmd.GetReaderAsync();

            return new DbReader(cmd, rd, conn);
        }
    }
}
