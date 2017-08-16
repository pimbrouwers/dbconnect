using System;
using System.Data;
using System.Data.SqlClient;

namespace Cinch.DbConnect
{
    public interface ISqlCommandBuilder {
        SqlCommand Build();
        ISqlCommandBuilder CreateCommand(string query);
        ISqlCommandBuilder SetConnection(SqlConnection conn);
        ISqlCommandBuilder SetCommandType(CommandType commandType);
        ISqlCommandBuilder SetCommandTimeout(int commandTimeout);
        ISqlCommandBuilder WithParameters(object parameters);
        ISqlCommandBuilder WithDbParams(IDbParams dbParams);
        ISqlCommandBuilder UsingTransaction(SqlTransaction transaction);
    }

    public class SqlCommandBuilder : ISqlCommandBuilder
    {
        SqlConnection conn;
        string query;        
        int commandTimeout;
        CommandType commandType = CommandType.StoredProcedure;
        object parameters;
        IDbParams dbParams;
        SqlTransaction transaction;
        
        public SqlCommand Build()
        {
            var command = new SqlCommand(this.query, this.conn) {
                CommandTimeout = this.commandTimeout,
                CommandType = this.commandType
            };

            if (this.parameters != null)
                command.MapParameters(this.parameters);
            else if (this.dbParams != null)
                command.AddDbParams(this.dbParams);

            if (this.transaction != null)
            {
                command.Transaction = this.transaction;
            }

            return command;
        }
        
        public ISqlCommandBuilder CreateCommand(string query)
        {
            this.query = query;
            return this;
        }

        public ISqlCommandBuilder SetConnection(SqlConnection conn)
        {
            if(this.conn == null)
                this.conn = conn;

            return this;
        }
        
        public ISqlCommandBuilder SetCommandType(CommandType commandType)
        {
            this.commandType = commandType;
            return this;
        }

        public ISqlCommandBuilder SetCommandTimeout(int commandTimeout)
        {
            this.commandTimeout = commandTimeout;
            return this;
        }
        
        public ISqlCommandBuilder WithParameters(object parameters)
        {
            this.parameters = parameters;
            return this;
        }

        public ISqlCommandBuilder WithDbParams(IDbParams dbParams)
        {
            this.dbParams = dbParams;
            return this;
        }

        public ISqlCommandBuilder UsingTransaction(SqlTransaction transaction)
        {
            this.transaction = transaction;
            return this;
        }
    }
}
