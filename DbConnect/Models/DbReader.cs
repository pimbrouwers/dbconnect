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
    }

    public class DbReader : IDbReader
    {
        IDbConnection conn;
        IDbCommand cmd;
        IDataReader rd;

        public DbReader(IDbCommand cmd, IDataReader rd)
        {            
            this.cmd = cmd;
            this.rd = rd;
        }

        public DbReader(IDbCommand cmd, IDataReader rd, IDbConnection conn)
        {
            this.cmd = cmd;
            this.rd = rd;
            this.conn = conn;
        }
        
        public void Dispose()
        {
            conn?.Dispose();
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
        
    }
}
