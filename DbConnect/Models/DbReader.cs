using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public class DbReader : IDisposable
    {
        SqlCommand cmd;
        SqlDataReader rd;

        public DbReader(SqlCommand cmd, SqlDataReader rd)
        {
            this.cmd = cmd;
            this.rd = rd;
        }

        public SqlDataReader Get()
        {
            return this.rd;
        }

        public void Dispose()
        {
            cmd?.Dispose();
            rd?.Dispose();
        }
    }

}
