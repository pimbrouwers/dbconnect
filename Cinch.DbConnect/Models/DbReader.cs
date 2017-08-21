using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public interface IDbReader : IDisposable
    {
        T ConvertTo<T>();
        IEnumerable<T> Enumerate<T>();
        Task<IEnumerable<T>> EnumerateAsync<T>();
    }

    public class DbReader : IDbReader
    {
        SqlCommand cmd;
        SqlDataReader rd;

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

        public T ConvertTo<T>()
        {
            return rd.ConvertTo<T>();
        }

        public IEnumerable<T> Enumerate<T>()
        {
            return rd.Enumerate<T>();
        }

        public async Task<IEnumerable<T>> EnumerateAsync<T>()
        {
            return await rd.EnumerateAsync<T>();
        }

        public SqlDataReader GetSqlDataReader()
        {
            return rd;
        }
    }
}
