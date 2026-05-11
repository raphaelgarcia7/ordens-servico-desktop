using System.Collections.Generic;
using System.Text;
using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.Infrastructure.Data;
using Npgsql;

namespace GestaoOS.Infrastructure.Repositories
{
    public class ClienteRepository : RepositoryBase, IClienteRepository
    {
        public ClienteRepository(PostgresConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public PagedResult<Cliente> Search(ClienteFilter filter)
        {
            var where = new StringBuilder(" WHERE 1 = 1 ");
            var result = new PagedResult<Cliente> { Page = filter.Page, PageSize = filter.PageSize };

            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(string.Empty, connection))
            {
                ApplyFilters(filter, where, command);
                command.CommandText = "SELECT COUNT(1) FROM clientes" + where;
                result.Total = System.Convert.ToInt32(command.ExecuteScalar());

                command.CommandText = @"SELECT id, nome, documento, tipo, email, telefone, data_cadastro, ativo
                    FROM clientes " + where + " ORDER BY nome LIMIT @limit OFFSET @offset";
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

        public Cliente GetById(int id)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"SELECT id, nome, documento, tipo, email, telefone, data_cadastro, ativo
                FROM clientes WHERE id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Insert(Cliente cliente)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"INSERT INTO clientes (nome, documento, tipo, email, telefone, data_cadastro, ativo)
                VALUES (@nome, @documento, @tipo, @email, @telefone, NOW(), @ativo) RETURNING id", connection))
            {
                AddParameters(command, cliente);
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(Cliente cliente)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"UPDATE clientes SET nome = @nome, documento = @documento, tipo = @tipo,
                email = @email, telefone = @telefone, ativo = @ativo WHERE id = @id", connection))
            {
                AddParameters(command, cliente);
                command.Parameters.AddWithValue("@id", cliente.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand("DELETE FROM clientes WHERE id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public bool HasOrdensServico(int clienteId)
        {
            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand("SELECT EXISTS(SELECT 1 FROM ordens_servico WHERE cliente_id = @cliente_id)", connection))
            {
                command.Parameters.AddWithValue("@cliente_id", clienteId);
                return (bool)command.ExecuteScalar();
            }
        }

        private static void ApplyFilters(ClienteFilter filter, StringBuilder where, NpgsqlCommand command)
        {
            if (!string.IsNullOrWhiteSpace(filter.Nome))
            {
                where.Append(" AND nome ILIKE @nome ");
                command.Parameters.AddWithValue("@nome", "%" + filter.Nome.Trim() + "%");
            }

            if (!string.IsNullOrWhiteSpace(filter.Documento))
            {
                where.Append(" AND documento ILIKE @documento ");
                command.Parameters.AddWithValue("@documento", "%" + filter.Documento.Trim() + "%");
            }

            if (filter.Ativo.HasValue)
            {
                where.Append(" AND ativo = @ativo ");
                command.Parameters.AddWithValue("@ativo", filter.Ativo.Value);
            }
        }

        private static void AddParameters(NpgsqlCommand command, Cliente cliente)
        {
            command.Parameters.AddWithValue("@nome", cliente.Nome.Trim());
            command.Parameters.AddWithValue("@documento", cliente.Documento.Trim());
            command.Parameters.AddWithValue("@tipo", (int)cliente.Tipo);
            command.Parameters.AddWithValue("@email", (object)cliente.Email ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@telefone", (object)cliente.Telefone ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@ativo", cliente.Ativo);
        }

        private static Cliente Map(System.Data.IDataRecord reader)
        {
            return new Cliente
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nome = reader.GetString(reader.GetOrdinal("nome")),
                Documento = reader.GetString(reader.GetOrdinal("documento")),
                Tipo = (TipoCliente)reader.GetInt32(reader.GetOrdinal("tipo")),
                Email = ValueOrDefault<string>(reader, "email"),
                Telefone = ValueOrDefault<string>(reader, "telefone"),
                DataCadastro = reader.GetDateTime(reader.GetOrdinal("data_cadastro")),
                Ativo = reader.GetBoolean(reader.GetOrdinal("ativo"))
            };
        }
    }
}
