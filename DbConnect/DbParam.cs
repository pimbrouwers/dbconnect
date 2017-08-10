using System;
using System.Collections.Generic;
using System.Data;

namespace Cinch
{
    public class DbParams
    {
        public IList<DbParam> Parameters { get; set; }

        public void Add(string name, object value, SqlDbType? dbType = null)
        {
            var p = new DbParam(name, value, dbType);

            Add(p);
        }

        public void AddOutput(string name, SqlDbType dbType, int? size = null)
        {
            var p = new DbParam(name, null, dbType, ParameterDirection.Output, size);

            Add(p);
        }
        
        public void Add(DbParam p)
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
        public int? Size { get; set; }

        public DbParam() { }

        public DbParam(string name, object value, SqlDbType? dbType = null, ParameterDirection parameterDirection = ParameterDirection.Input, int? size = null)
        {
            Name = name;
            Value = value;
            DbType = dbType;
            ParameterDirection = parameterDirection;
            Size = size;
        }
    }      
}
