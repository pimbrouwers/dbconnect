using FastMember;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    public static class SqlCommandExtensions
    {
        public static SqlDataReader GetReader(this SqlCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.OpenConnection();
            }

            return cmd?.ExecuteReader();
        }

        public static async Task<SqlDataReader> GetReaderAsync(this SqlCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                await cmd.Connection.OpenConnectionAsync();
            }

            return await cmd?.ExecuteReaderAsync();
        }

        public static void AddDbParams(this SqlCommand cmd, IDbParams dbParams)
        {
            if (dbParams != null &&
                dbParams.Parameters != null)
            {
                foreach (var dbParam in dbParams.Parameters)
                {
                    if(dbParam.ParameterDirection == ParameterDirection.Output)
                    {
                        cmd.AddOutputParameter(dbParam.Name, dbParam.Value, dbParam.Size, dbParam.DbType.Value);
                    }
                    else if(dbParam.DbType != null)
                    {
                        cmd.AddParameter(dbParam.Name, dbParam.Value, dbParam.DbType.Value);
                    }
                    else
                    {
                        cmd.AddParameter(dbParam.Name, dbParam.Value);
                    }
                }                
            }
        }

        public static void AddOutputParameter(this SqlCommand cmd, string name, object value, int? size, SqlDbType dbType)
        {
            var param = cmd.Parameters.Add(name, dbType);
            param.Direction = ParameterDirection.Output;            
            param.Value = value;

            if (size.HasValue)
                param.Size = size.Value;
            else
                param.Size = -1;
        }

        public static void AddParameter(this SqlCommand cmd, string name, object value)
        {
            if (value == null)
                throw new ArgumentNullException("SqlDbType cannot be inferred from a null value. Is this a nullable type? If so, the SqlDbType must be provided when adding the DbParam.");

            cmd.AddParameter(name, value, GetSqlDbType(value));
        }

        public static void AddParameter(this SqlCommand cmd, string name, object value, SqlDbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            if (value == null)
                value = DBNull.Value;

            // add the parameter to the command            
            var param = cmd.Parameters.Add(name, dbType);

            // set the value of the parameter
            if (value == null)
            {
                param.Value = DBNull.Value;
            }
            else
            {
                param.Value = value;
            }
        }

        public static T GetOuputValue<T>(this SqlCommand cmd, string name)
        {
            object val = null;

            if(cmd.Parameters.Contains(name))
            {
                val = cmd.Parameters[name].Value;
            }

            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new Exception("Attempting to cast a DBNull to a non nullable type.");
                }
                return default(T);
            }
            return (T)val;
        }

        public static void MapParameters(this SqlCommand cmd, object parameters)
        {
            var accessor = TypeAccessor.Create(parameters.GetType());
            var members = accessor.GetMembers();
            var objAccessor = ObjectAccessor.Create(parameters);

            foreach (var member in members)
            {
                cmd.AddParameter(member.Name, objAccessor[member.Name]);
            }
        }

        public static SqlCommand OpenConnection(this SqlCommand cmd)
        {
            cmd.Connection?.OpenConnection();

            return cmd;
        }

        public static async Task<SqlCommand> OpenConnectionAsync(this SqlCommand cmd)
        {
            await cmd.Connection?.OpenConnectionAsync();

            return cmd;
        }

        private static SqlDbType GetSqlDbType(object paramValue)
        {            
            TypeCode typeCode = Convert.GetTypeCode(paramValue);

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
                    return SqlDbType.SmallInt;

                case TypeCode.Int32:
                    return SqlDbType.Int;

                case TypeCode.Byte:
                    return SqlDbType.TinyInt;

                default:
                    throw new ArgumentOutOfRangeException("clr type");
            }
        }

    }


}
