using System;
using System.Collections.Generic;
using System.Data;

namespace Cinch
{
    public class DbParams
    {
        public IList<DbParam> Parameters { get; set; }

        public void Add(string name, object value)
        {
            var p = new DbParam(name, value);
        }

        public void Add (DbParam p)
        {
            if (Parameters == null)
                Parameters = new List<DbParam>();

            Parameters.Add(p);
        }
        
    }

    public class DbParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public SqlDbType? DbType { get; set; }
        public ParameterDirection ParameterDirection { get; set; }

        public DbParam() { }

        public DbParam(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public DbParam(string name, object value, SqlDbType dbType) : this(name, value)
        {
            DbType = dbType;
        }

        public DbParam(string name, object value, SqlDbType dbType, ParameterDirection parameterDirection) : this(name, value, dbType)
        {
            ParameterDirection = parameterDirection;
        }
    }
}
