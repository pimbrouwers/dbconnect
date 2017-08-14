using System;
using System.Collections.Generic;
using System.Data;

namespace Cinch.DbConnect
{
    public interface IDbParams
    {
        IList<IDbParam> Parameters { get; set; }

        void Add(string name, object value, SqlDbType? dbType = null);
        void AddOutput(string name, SqlDbType dbType, int? size = null);
        void Add(IDbParam p);
    }

    public class DbParams : IDbParams
    {
        public IList<IDbParam> Parameters { get; set; }

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
        
        public void Add(IDbParam p)
        {
            if (Parameters == null)
                Parameters = new List<IDbParam>();

            Parameters.Add(p);
        }
    }

    public interface IDbParam
    {
        string Name { get; set; }
        object Value { get; set; }
        SqlDbType? DbType { get; set; }
        ParameterDirection ParameterDirection { get; set; }
        int? Size { get; set; }
    }

    public class DbParam : IDbParam
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
