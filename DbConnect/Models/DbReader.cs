using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public class DbReader : IDisposable
    {
        public SqlCommand cmd;
        public SqlDataReader rd;

        public DbReader(SqlCommand cmd, SqlDataReader rd)
        {
            this.cmd = cmd;
            this.rd = rd;
        }

        public void Dispose()
        {
            cmd?.Dispose();
            rd?.Dispose();
        }
    }

    public static class DbReaderExtensions
    {
        public static IEnumerable<T> Read<T>(this DbReader dbReader)
        {
            return dbReader.rd.Read<T>();
        }

        public static async Task<IEnumerable<T>> ReadAsync<T>(this DbReader dbReader)
        {
            return await dbReader.rd.ReadAsync<T>();
        }
    }
}
