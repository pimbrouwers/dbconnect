using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public class DbConnect : IDisposable
    {
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlTransaction trans;
        private SqlDataReader dr;

        public string _connStr;
        public string ConnStr
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_connStr))
                    return _connStr;

                string appConnStr = ConfigurationManager.ConnectionStrings[0].ConnectionString;

                if (!String.IsNullOrWhiteSpace(appConnStr))
                    _connStr = appConnStr;
                else
                    throw new ApplicationException("No connection strings have been specified");

                return _connStr;
            }
            set
            {
                _connStr = value;
            }
        }

        public DbConnect(string query, CommandType commType)
        {
            SetSqlConnection(ConnStr);
            SetSqlCommand(query, commType);
        }

        public DbConnect(string connStr, string query, CommandType commType)
        {
            ConnStr = connStr;
            SetSqlConnection(ConnStr);
            SetSqlCommand(query, commType);
        }

        /// <summary>
        /// Fills a SQLDataReader with query results
        /// </summary>
        /// <returns></returns>
        public async Task<SqlDataReader> FillSqlDataReader()
        {
            try
            {
                Open();

                dr = await cmd.ExecuteReaderAsync();
                return dr;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DbConnectException dbx = new DbConnectException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Returns a list of objects of the given Type, with propeties set based on how they match up to the fields returned in the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IList<T>> FillList<T>()
        {
            try
            {

                SqlDataReader rd = await FillSqlDataReader();
                List<T> lst = new List<T>();
                while (rd.Read())
                {
                    lst.Add(ConvertReaderToObject<T>(rd));
                }
                rd.Close();

                return lst;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DbConnectException dbx = new DbConnectException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                Close();
            }

        }

        /// <summary>
        /// Populates a single object of the given Type, with propeties set based on how they match up to the fields returned in the first row of the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FillObject<T>()
        {
            try
            {
                SqlDataReader rd = await FillSqlDataReader();
                T t = default(T);
                if (rd.Read())
                {
                    t = ConvertReaderToObject<T>(rd); ;
                }
                rd.Close();

                return t;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DbConnectException dbx = new DbConnectException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Close();
            }

        }

        /// <summary>
        /// Runs the query.
        /// </summary>
        public async Task ExecuteNonQuery()
        {

            try
            {

                //verbose output
                //VerboseOutput();

                Open();
                // run the query
                await cmd.ExecuteNonQueryAsync();

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DbConnectException dbx = new DbConnectException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (cmd.Transaction == null)
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Execute the command and return an object
        /// </summary>
        /// <returns>An Object</returns>
        internal async Task<object> ExecuteScalar()
        {
            object rv = new object();
            try
            {
                //verbose output
                //VerboseOutput();

                Open();

                // run the query and return the value
                rv = await cmd.ExecuteScalarAsync();

                return rv;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DbConnectException dbx = new DbConnectException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch
            {

                throw;
            }
            finally
            {
                if (cmd.Transaction == null)
                {
                    Close();
                }
            }




        }

        #region SqlDataReader Utils
        private T ConvertReaderToObject<T>(SqlDataReader rd)
        {

            Type type = typeof(T);

            if (type.IsPrimitive || type == (typeof(string)))
            {
                //if this is a primitive type, we can't set a property, so simply attemp to use the first value in the row
                return (T)rd[0];

            }

            PropertyInfo[] props;
            ConstructorInfo constr;

            if (!DbConnectCache._objectPropertyCache.ContainsKey(type.Name))
            {
                props = type.GetProperties();
                DbConnectCache._objectPropertyCache.Add(type.Name, props);
            }
            else
            {
                props = DbConnectCache._objectPropertyCache[type.Name];
            }

            if (!DbConnectCache._objectConstructorCache.ContainsKey(type.Name))
            {
                constr = type.GetConstructor(System.Type.EmptyTypes);
                DbConnectCache._objectConstructorCache.Add(type.Name, constr);
            }
            else
            {
                constr = DbConnectCache._objectConstructorCache[type.Name];
            }
            T t = (T)constr.Invoke(new object[0]);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite) continue;

                for (int i = 0; i < rd.FieldCount; i++)
                {
                    string fieldName = rd.GetName(i);
                    if (string.Compare(fieldName, prop.Name, true) == 0)
                    {
                        if (!rd.IsDBNull(i))
                        {

                            prop.SetValue(t, rd.GetValue(i), null);
                        }
                    }

                }
            }
            return t;
        }
        #endregion

        #region SqlCommand Parameters
        /// <summary>
        /// Sets up a parameter for the query
        /// </summary>
        /// <param name="id">The ID of the parameter</param>
        /// <param name="type">The Sql type of the parameter</param>
        /// <param name="Value">The value of the parameter</param>
        public void AddParameter(string id, SqlDbType type, Object Value)
        {
            // add the parameter to the command
            cmd.Parameters.Add(id, type);

            if (Value == null)
            {
                cmd.Parameters[id].Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && type != SqlDbType.VarChar)
            {
                // must convert the empty string to a DBNull
                cmd.Parameters[id].Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && (type == SqlDbType.Float || type == SqlDbType.Int || type == SqlDbType.Money))
            {
                cmd.Parameters[id].Value = 0;
            }
            else
            {
                // set the value of the parameter
                cmd.Parameters[id].Value = Value;
            }
        }

        /// <summary>
        /// Used to clear out all of the parameters
        /// </summary>
        public void ClearParameters()
        {
            cmd.Parameters.Clear();
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
        private void SetSqlCommand(string query, CommandType commType)
        {
            cmd = new SqlCommand(query, conn);
            cmd.CommandType = commType;
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        internal void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        internal void Close()
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
