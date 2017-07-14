using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cinch
{
    public class DbConnect : IDisposable
    {

        SqlConnection conn;
        SqlCommand cmd;
        SqlTransaction trans;
        //SqlDataReader dr;

        #region Constructors                
        public DbConnect(string connStr, string query = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new ArgumentNullException("connStr must be provided");
            }

            SetSqlConnection(connStr);
            SetSqlCommand(query, commandType, commandTimeout);
        }
        #endregion

        #region Query Execution
        /// <summary>
        /// Fills a SQLDataReader with query results
        /// </summary>
        /// <returns></returns>
        public async Task<SqlDataReader> FillSqlDataReader()
        {
            Open();

            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            return dr;
        }

        /// <summary>
        /// Returns a list of objects of the given Type, with propeties set based on how they match up to the fields returned in the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IList<T>> FillList<T>() where T : class, new()
        {
            List<T> lst = new List<T>();

            using (SqlDataReader dr = await FillSqlDataReader())
            {
                while (dr.Read())
                {
                    lst.Add(dr.ConvertToObject<T>());
                }
            }
            
            return lst;
        }

        /// <summary>
        /// Populates a single object of the given Type, with propeties set based on how they match up to the fields returned in the first row of the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FillObject<T>() where T : class, new()
        {
            T t = default(T);
            using (SqlDataReader dr = await FillSqlDataReader())
            {
                if (dr.Read())
                {
                    t = dr.ConvertToObject<T>(); ;
                }
            }

            return t;
        }

        public async Task<IList<Dictionary<string, object>>> FillDynamicList()
        {
            var lst = new List<Dictionary<string, object>>();
            using (SqlDataReader dr = await FillSqlDataReader())
            {
                while (dr.Read())
                {
                    lst.Add(dr.ConvertToDictionary());
                }
            }

            return lst;
        }

        /// <summary>
        /// Runs the query.
        /// </summary>
        public async Task ExecuteNonQuery()
        {
            Open();

            // run the query
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Execute the command and return an object
        /// </summary>
        /// <returns>An object</returns>
        public async Task<object> ExecuteScalar()
        {
            Open();

            // run the query and return the value
            object rv = new object();
            rv = await cmd.ExecuteScalarAsync();

            return rv;
        }

        public async Task<T> ExecuteScalarCast<T>()
        {
            var resp = typeof(T);
            object obj = await ExecuteScalar();

            if (obj is T)
            {
                return (T)obj;
            }

            return default(T);
        }
        #endregion

        #region Bulk Copy
        public async Task BulkInsert<T>(SqlDataReader dr, string destinationTableName, int batchSize = 5000, int? bulkCopyTimeout = null, IEnumerable<string> ignoreCols = null) where T : class, new()
        {
            using (var bcp = new SqlBulkCopy(conn))
            using (var dataReader = ObjectReader.Create(dr.AsEnumerable<T>()))
            {
                Type type = typeof(T);
                var accessor = TypeAccessor.Create(type);
                var members = accessor.GetMembers();

                foreach (var member in members)
                {
                    if (ignoreCols != null && ignoreCols.Contains(member.Name))
                        continue;

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(member.Name, member.Name));
                }

                Open();

                bcp.DestinationTableName = destinationTableName;
                bcp.BatchSize = batchSize;

                if (bulkCopyTimeout.HasValue)
                    bcp.BulkCopyTimeout = bulkCopyTimeout.Value;

                await bcp.WriteToServerAsync(dataReader);
            }
        }
        #endregion

        #region SqlCommand Parameters
        public void AddParameter(string id, object value)
        {
            if (value == null)
                throw new ArgumentNullException("SqlDbType must be provided if value null");

            AddParameter(id, GetSqlDbType(value), value);
        }

        /// <summary>
        /// Sets up a parameter for the query
        /// </summary>
        /// <param name="id">The ID of the parameter</param>
        /// <param name="type">The Sql type of the parameter</param>
        /// <param name="Value">The value of the parameter</param>
        public void AddParameter(string id, SqlDbType type, object Value)
        {
            // add the parameter to the command
            cmd.Parameters.Add(id, type);

            // set the value of the parameter
            if (Value == null)
            {
                cmd.Parameters[id].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters[id].Value = Value;
            }
        }

        public void AddOutputParameter(string id, object value)
        {
            AddOutputParameter(id, GetSqlDbType(value), value);
        }

        /// <summary>
        /// Adds an input/output parameter to the command
        /// This parameter can accept and return values
        /// </summary>
        /// <param name="id">ID of the parameter to be created</param>
        /// <param name="type">SqlDbType of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public void AddOutputParameter(string id, SqlDbType type, object value)
        {
            SqlParameter param = cmd.Parameters.Add(id, type);
            param.Direction = ParameterDirection.Output;
            param.Value = value;
        }

        /// <summary>
        /// Used to clear out all of the parameters
        /// </summary>
        public void ClearParameters()
        {
            if (cmd != null && cmd.Parameters != null && cmd.Parameters.Count > 0)
            {
                cmd.Parameters.Clear();
            }
        }
        #endregion

        #region SqlCommand Transaction
        /// <summary>
        /// This starts a transaction, can manually rollback or commit and on close with automatically commit.
        /// </summary>
        public void BeginTransaction()
        {
            Open();
            trans = conn.BeginTransaction();
            cmd.Transaction = trans;
        }

        /// <summary>
        /// This commits the transaction if the object exists.
        /// </summary>
        public void CommitTransaction()
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Commit();
            }
        }

        /// <summary>
        /// This allows the user to rollback all the way to the beginning.
        /// </summary>
        public void RollbackTransaction()
        {
            RollbackTransaction("");
        }

        /// <summary>
        /// This allows the user to rollback to the last save point.
        /// </summary>
        public void RollbackTransaction(string rollBackName)
        {
            if (cmd.Transaction != null)
            {
                if (rollBackName != "")
                {
                    cmd.Transaction.Rollback(rollBackName);
                }
                else
                {
                    cmd.Transaction.Rollback();
                }
            }
        }

        /// <summary>
        /// This allows the user to save a specific rollback marker.
        /// </summary>
        public void SaveTransaction(string savePointName)
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Save(savePointName);
            }
        }
        #endregion

        #region Internal Functionality
        /// <summary>
        /// Sets internal SqlConnection
        /// </summary>
        /// <param name="connStr"></param>
        private void SetSqlConnection(string connStr)
        {
            conn = new SqlConnection(connStr);
        }

        /// <summary>
        /// Sets internal SqlCommand
        /// </summary>
        /// <param name="query"></param>
        /// <param name="commandType"></param>
        public void SetSqlCommand(string query, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = commandType;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    cmd.CommandText = query;
                }
            }
            else
            {
                cmd.CommandText = query;
                cmd.CommandType = commandType;
            }

            /*
             * timeout
             */
            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        private void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        private void Close()
        {
            if (conn != null)
            {
                conn.Close();
            }

            if (cmd != null)
            {
                cmd.Dispose();
            }
        }

        private SqlDbType GetSqlDbType(object paramValue)
        {
            Type type = paramValue.GetType();
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int64:
                    return SqlDbType.BigInt;

                case TypeCode.Boolean:
                    return SqlDbType.Bit;

                case TypeCode.Char:
                    return SqlDbType.NChar;

                case TypeCode.String:
                    return SqlDbType.NVarChar;

                case TypeCode.DateTime:
                    return SqlDbType.DateTime;

                case TypeCode.Double:
                    return SqlDbType.Float;

                case TypeCode.Decimal:
                    return SqlDbType.Decimal;

                case TypeCode.Int16:
                case TypeCode.Int32:
                    return SqlDbType.Int;

                case TypeCode.Byte:
                    return SqlDbType.TinyInt;

                default:
                    throw new ArgumentOutOfRangeException("clr type");
            }
        }

        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
