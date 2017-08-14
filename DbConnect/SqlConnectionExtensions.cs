using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch
{
    public static class SqlConnectionExtensions
    {

        public static void OpenConnection(this SqlConnection conn)
        {
            if(conn.State != ConnectionState.Open)
                conn.Open();
        }

        public static async Task OpenConnectionAsync(this SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
        }
    }
}
