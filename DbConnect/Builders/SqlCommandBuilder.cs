using System;
using System.Data;
using System.Data.SqlClient;

namespace Cinch
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
        
        // CONVERSION OPERATOR
        public static implicit operator SqlCommand(SqlCommandBuilder bld)
        {
            var command = new SqlCommand(bld.query, bld.conn) {
                CommandTimeout = bld.commandTimeout,
                CommandType = bld.commandType
            };

            if (bld.parameters != null)
                command.MapParameters(bld.parameters);
            else if (bld.dbParams != null)
                command.AddDbParams(bld.dbParams);

            if (bld.transaction != null)
            {
                command.Transaction = bld.transaction;
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
