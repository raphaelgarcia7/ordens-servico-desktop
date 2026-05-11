using GestaoOS.Application.Abstractions;

namespace GestaoOS.Infrastructure.Data
{
    public class PostgresUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly PostgresConnectionFactory _connectionFactory;

        public PostgresUnitOfWorkFactory(PostgresConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IUnitOfWork Create()
        {
            return new PostgresUnitOfWork(_connectionFactory);
        }
    }
}
