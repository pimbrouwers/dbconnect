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
        SqlDataReader dr;

        #region Constructors                
        public DbConnect(string connStr, string query = null, CommandType commType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new ArgumentNullException("connStr must be provided");
            }

            SetSqlConnection(connStr);

            if (!string.IsNullOrWhiteSpace(query))
            {
                SetSqlCommand(query, commType);
            }
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

            dr = await cmd.ExecuteReaderAsync();
            return dr;
        }

        /// <summary>
        /// Returns a list of objects of the given Type, with propeties set based on how they match up to the fields returned in the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IList<T>> FillList<T>() where T : class, new()
        {
            dr = await FillSqlDataReader();

            List<T> lst = new List<T>();

            while (dr.Read())
            {
                lst.Add(ConvertReaderToObject<T>(dr));
            }

            return lst;
        }

        /// <summary>
        /// Populates a single object of the given Type, with propeties set based on how they match up to the fields returned in the first row of the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> Fillobject<T>() where T : class, new()
        {
            dr = await FillSqlDataReader();

            T t = default(T);
            if (dr.Read())
            {
                t = ConvertReaderToObject<T>(dr); ;
            }

            return t;
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

        #region Utils
        public static T ConvertReaderToObject<T>(SqlDataReader rd) where T : class, new()
        {

            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();
            var t = new T();

            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    string fieldName = rd.GetName(i);

                    if (members.Any(m => m.Name == fieldName))
                    {
                        accessor[t, fieldName] = rd.GetValue(i);
                    }
                }
            }

            return t;
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
        /// <param name="value">The value of the parameter</param>
        public void AddParameter(string id, SqlDbType type, object value)
        {
            // add the parameter to the command
            cmd.Parameters.Add(id, type);

            // set the value of the parameter
            if (value == null)
            {
                cmd.Parameters[id].Value = Convert.DBNull;
            }
            else
            {
                cmd.Parameters[id].Value = value;
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
            if (cmd.Parameters != null && cmd.Parameters.Count > 0)
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
        /// <param name="commType"></param>
        public void SetSqlCommand(string query, CommandType commType = CommandType.StoredProcedure)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand(query, conn)
                {
                    CommandType = commType
                };
            }
            else
            {
                cmd.CommandText = query;
                cmd.CommandType = commType;
            }
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
