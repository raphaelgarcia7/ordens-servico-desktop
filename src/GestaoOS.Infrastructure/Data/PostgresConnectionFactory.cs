using System.Configuration;
using Npgsql;

namespace GestaoOS.Infrastructure.Data
{
    public class PostgresConnectionFactory
    {
        private readonly string _connectionString;

        public PostgresConnectionFactory()
            : this(ConfigurationManager.ConnectionStrings["GestaoOsDb"].ConnectionString)
        {
        }

        public PostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection Create()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
