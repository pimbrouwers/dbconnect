using FastMember;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Cinch
{
    public static class SqlCommandExtensions
    {
        public static SqlDataReader FillDataReader(this SqlCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }

            return cmd?.ExecuteReader();
        }

        public static void AddDbParams(this SqlCommand cmd, DbParams dbParams)
        {
            if (dbParams != null &&
                dbParams.Parameters != null)
            {
                foreach (var dbParam in dbParams.Parameters)
                {
                    if(dbParam.ParameterDirection == ParameterDirection.Output)
                    {
                        cmd.AddOutputParameter(dbParam.Name, (SqlDbType)dbParam.DbType);
                    }
                    else if(dbParam.DbType != null)
                    {
                        cmd.AddParameter(dbParam.Name, dbParam.Value, (SqlDbType)dbParam.DbType);
                    }
                    else
                    {
                        cmd.AddParameter(dbParam.Name, dbParam.Value);
                    }
                }                
            }
        }

        public static void AddOutputParameter(this SqlCommand cmd, string name, SqlDbType dbType)
        {
            var param = cmd.Parameters.Add(name, dbType);
            param.Direction = ParameterDirection.Output;
        }

        public static void AddParameter(this SqlCommand cmd, string name, object value)
        {
            if (value == null)
                value = DBNull.Value;

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

        private static SqlDbType GetSqlDbType(object paramValue)
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

    }


}
