using System.Text;
using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Domain.Entities;
using GestaoOS.Infrastructure.Data;
using Npgsql;

namespace GestaoOS.Infrastructure.Repositories
{
    public class ServicoRepository : RepositoryBase, IServicoRepository
    {
        public ServicoRepository(PostgresConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public PagedResult<Servico> Search(ServicoFilter filter)
        {
            var where = new StringBuilder(" WHERE 1 = 1 ");
            var result = new PagedResult<Servico> { Page = filter.Page, PageSize = filter.PageSize };

            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(string.Empty, connection))
            {
                ApplyFilters(filter, where, command);
                command.CommandText = "SELECT COUNT(1) FROM servicos" + where;
                result.Total = System.Convert.ToInt32(command.ExecuteScalar());

                command.CommandText = @"SELECT id, nome, valor_base, percentual_imposto, ativo
                    FROM servicos " + where + " ORDER BY nome LIMIT @limit OFFSET @offset";
                command.Parameters.AddWithValue("@limit", filter.PageSize);
                command.Parameters.AddWithValue("@offset", GetOffset(filter.Page, filter.PageSize));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(Map(reader));
                    }
                }
            }

            return result;
        }

        public Servico GetById(int id)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand("SELECT id, nome, valor_base, percentual_imposto, ativo FROM servicos WHERE id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Insert(Servico servico)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"INSERT INTO servicos (nome, valor_base, percentual_imposto, ativo)
                VALUES (@nome, @valor_base, @percentual_imposto, @ativo) RETURNING id", connection))
            {
                AddParameters(command, servico);
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(Servico servico)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"UPDATE servicos SET nome = @nome, valor_base = @valor_base,
                percentual_imposto = @percentual_imposto, ativo = @ativo WHERE id = @id", connection))
            {
                AddParameters(command, servico);
                command.Parameters.AddWithValue("@id", servico.Id);
                command.ExecuteNonQuery();
            }
        }

        private static void ApplyFilters(ServicoFilter filter, StringBuilder where, NpgsqlCommand command)
        {
            if (!string.IsNullOrWhiteSpace(filter.Nome))
            {
                where.Append(" AND nome ILIKE @nome ");
                command.Parameters.AddWithValue("@nome", "%" + filter.Nome.Trim() + "%");
            }

            if (filter.Ativo.HasValue)
            {
                where.Append(" AND ativo = @ativo ");
                command.Parameters.AddWithValue("@ativo", filter.Ativo.Value);
            }
        }

        private static void AddParameters(NpgsqlCommand command, Servico servico)
        {
            command.Parameters.AddWithValue("@nome", servico.Nome.Trim());
            command.Parameters.AddWithValue("@valor_base", servico.ValorBase);
            command.Parameters.AddWithValue("@percentual_imposto", servico.PercentualImposto);
            command.Parameters.AddWithValue("@ativo", servico.Ativo);
        }

        private static Servico Map(System.Data.IDataRecord reader)
        {
            return new Servico
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nome = reader.GetString(reader.GetOrdinal("nome")),
                ValorBase = reader.GetDecimal(reader.GetOrdinal("valor_base")),
                PercentualImposto = reader.GetDecimal(reader.GetOrdinal("percentual_imposto")),
                Ativo = reader.GetBoolean(reader.GetOrdinal("ativo"))
            };
        }
    }
}
