using System.Data;
using GestaoOS.Application.Abstractions;
using Npgsql;

namespace GestaoOS.Infrastructure.Data
{
    public class PostgresUnitOfWork : IUnitOfWork
    {
        private readonly NpgsqlConnection _connection;

        public PostgresUnitOfWork(PostgresConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.Create();
        }

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        public IDbTransaction Transaction { get; private set; }

        public void Begin()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            Transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (Transaction == null)
            {
                return;
            }

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public void Rollback()
        {
            if (Transaction == null)
            {
                return;
            }

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }

            _connection.Dispose();
        }
    }
}
