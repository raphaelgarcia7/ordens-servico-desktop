using System;
using System.Data;
using GestaoOS.Application.Abstractions;
using GestaoOS.Infrastructure.Data;
using Npgsql;

namespace GestaoOS.Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly PostgresConnectionFactory _connectionFactory;

        protected RepositoryBase(PostgresConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        protected NpgsqlConnection CreateOpenConnection()
        {
            var connection = _connectionFactory.Create();
            connection.Open();
            return connection;
        }

        protected NpgsqlCommand CreateCommand(string sql, NpgsqlConnection connection)
        {
            return new NpgsqlCommand(sql, connection);
        }

        protected NpgsqlCommand CreateCommand(string sql, IUnitOfWork unitOfWork)
        {
            var command = new NpgsqlCommand(sql, (NpgsqlConnection)unitOfWork.Connection);
            command.Transaction = (NpgsqlTransaction)unitOfWork.Transaction;
            return command;
        }

        protected static int GetOffset(int page, int pageSize)
        {
            return (Math.Max(page, 1) - 1) * pageSize;
        }

        protected static T ValueOrDefault<T>(IDataRecord reader, string field)
        {
            var ordinal = reader.GetOrdinal(field);
            if (reader.IsDBNull(ordinal))
            {
                return default(T);
            }

            return (T)reader.GetValue(ordinal);
        }
    }
}
