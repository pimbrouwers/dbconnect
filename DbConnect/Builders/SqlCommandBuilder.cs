using System;
using System.Data;
using System.Data.SqlClient;

namespace Cinch.DbConnect
{
    public class SqlCommandBuilder
    {
        SqlConnection conn;
        string query;        
        int commandTimeout;
        CommandType commandType;
        object parameters;
        DbParams dbParams;
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

        public SqlCommandBuilder CreateCommand(string query)
        {
            this.query = query;
            return this;
        }

        public SqlCommandBuilder SetConnection(SqlConnection conn)
        {
            if(this.conn == null)
                this.conn = conn;

            return this;
        }

        public SqlCommandBuilder SetCommandType(CommandType commandType)
        {
            this.commandType = commandType;
            return this;
        }

        public SqlCommandBuilder SetCommandTimeout(int commandTimeout)
        {
            this.commandTimeout = commandTimeout;
            return this;
        }
        
        public SqlCommandBuilder WithParameters(object parameters)
        {
            this.parameters = parameters;
            return this;
        }

        public SqlCommandBuilder WithDbParams(DbParams dbParams)
        {
            this.dbParams = dbParams;
            return this;
        }

        public SqlCommandBuilder UsingTransaction(SqlTransaction transaction)
        {
            this.transaction = transaction;
            return this;
        }
    }
}
