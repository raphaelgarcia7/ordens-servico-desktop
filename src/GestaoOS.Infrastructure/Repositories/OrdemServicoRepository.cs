using System;
using System.Text;
using GestaoOS.Application.Abstractions;
using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.Infrastructure.Data;
using Npgsql;

namespace GestaoOS.Infrastructure.Repositories
{
    public class OrdemServicoRepository : RepositoryBase, IOrdemServicoRepository
    {
        public OrdemServicoRepository(PostgresConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public PagedResult<OrdemServico> Search(OrdemServicoFilter filter)
        {
            var where = new StringBuilder(" WHERE 1 = 1 ");
            var result = new PagedResult<OrdemServico> { Page = filter.Page, PageSize = filter.PageSize };

            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(string.Empty, connection))
            {
                ApplyFilters(filter, where, command);
                command.CommandText = "SELECT COUNT(1) FROM ordens_servico os" + where;
                result.Total = Convert.ToInt32(command.ExecuteScalar());

                command.CommandText = @"SELECT os.id, os.cliente_id, c.nome AS cliente_nome, os.data_abertura, os.data_conclusao,
                    os.status, os.observacao, os.valor_total, os.versao
                    FROM ordens_servico os
                    INNER JOIN clientes c ON c.id = os.cliente_id " + where + @"
                    ORDER BY os.data_abertura DESC, os.id DESC LIMIT @limit OFFSET @offset";
                command.Parameters.AddWithValue("@limit", filter.PageSize);
                command.Parameters.AddWithValue("@offset", GetOffset(filter.Page, filter.PageSize));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(MapOrdem(reader));
                    }
                }
            }

            return result;
        }

        public OrdemServico GetById(int id, bool includeItems)
        {
            OrdemServico ordem;

            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(@"SELECT os.id, os.cliente_id, c.nome AS cliente_nome, os.data_abertura,
                os.data_conclusao, os.status, os.observacao, os.valor_total, os.versao
                FROM ordens_servico os
                INNER JOIN clientes c ON c.id = os.cliente_id
                WHERE os.id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    ordem = reader.Read() ? MapOrdem(reader) : null;
                }

                if (ordem == null || !includeItems)
                {
                    return ordem;
                }

                LoadItems(ordem, connection);
                return ordem;
            }
        }

        public int Insert(OrdemServico ordem, IUnitOfWork unitOfWork)
        {
            using (var command = CreateCommand(@"INSERT INTO ordens_servico
                (cliente_id, data_abertura, data_conclusao, status, observacao, valor_total, versao)
                VALUES (@cliente_id, @data_abertura, @data_conclusao, @status, @observacao, @valor_total, 1)
                RETURNING id", unitOfWork))
            {
                AddOrdemParameters(command, ordem);
                return (int)command.ExecuteScalar();
            }
        }

        public bool Update(OrdemServico ordem, int expectedVersion, IUnitOfWork unitOfWork)
        {
            using (var command = CreateCommand(@"UPDATE ordens_servico SET
                cliente_id = @cliente_id,
                data_abertura = @data_abertura,
                data_conclusao = @data_conclusao,
                status = @status,
                observacao = @observacao,
                valor_total = @valor_total,
                versao = versao + 1
                WHERE id = @id AND versao = @versao", unitOfWork))
            {
                AddOrdemParameters(command, ordem);
                command.Parameters.AddWithValue("@id", ordem.Id);
                command.Parameters.AddWithValue("@versao", expectedVersion);
                return command.ExecuteNonQuery() == 1;
            }
        }

        public void ReplaceItems(OrdemServico ordem, IUnitOfWork unitOfWork)
        {
            using (var delete = CreateCommand("DELETE FROM ordem_servico_itens WHERE ordem_servico_id = @ordem_servico_id", unitOfWork))
            {
                delete.Parameters.AddWithValue("@ordem_servico_id", ordem.Id);
                delete.ExecuteNonQuery();
            }

            foreach (var item in ordem.Itens)
            {
                using (var insert = CreateCommand(@"INSERT INTO ordem_servico_itens
                    (ordem_servico_id, servico_id, quantidade, valor_unitario, percentual_imposto_aplicado, valor_total_item)
                    VALUES (@ordem_servico_id, @servico_id, @quantidade, @valor_unitario, @percentual_imposto_aplicado, @valor_total_item)", unitOfWork))
                {
                    insert.Parameters.AddWithValue("@ordem_servico_id", ordem.Id);
                    insert.Parameters.AddWithValue("@servico_id", item.ServicoId);
                    insert.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    insert.Parameters.AddWithValue("@valor_unitario", item.ValorUnitario);
                    insert.Parameters.AddWithValue("@percentual_imposto_aplicado", item.PercentualImpostoAplicado);
                    insert.Parameters.AddWithValue("@valor_total_item", item.ValorTotalItem);
                    insert.ExecuteNonQuery();
                }
            }
        }

        public void InsertHistorico(int ordemServicoId, StatusOrdemServico statusAnterior, StatusOrdemServico statusNovo, string usuario, IUnitOfWork unitOfWork)
        {
            using (var command = CreateCommand(@"INSERT INTO historico_status
                (ordem_servico_id, status_anterior, status_novo, data_hora, usuario)
                VALUES (@ordem_servico_id, @status_anterior, @status_novo, NOW(), @usuario)", unitOfWork))
            {
                command.Parameters.AddWithValue("@ordem_servico_id", ordemServicoId);
                command.Parameters.AddWithValue("@status_anterior", (int)statusAnterior);
                command.Parameters.AddWithValue("@status_novo", (int)statusNovo);
                command.Parameters.AddWithValue("@usuario", string.IsNullOrWhiteSpace(usuario) ? Environment.UserName : usuario);
                command.ExecuteNonQuery();
            }
        }

        private void LoadItems(OrdemServico ordem, NpgsqlConnection connection)
        {
            using (var command = CreateCommand(@"SELECT i.id, i.ordem_servico_id, i.servico_id, s.nome AS servico_nome,
                i.quantidade, i.valor_unitario, i.percentual_imposto_aplicado, i.valor_total_item
                FROM ordem_servico_itens i
                INNER JOIN servicos s ON s.id = i.servico_id
                WHERE i.ordem_servico_id = @ordem_servico_id
                ORDER BY i.id", connection))
            {
                command.Parameters.AddWithValue("@ordem_servico_id", ordem.Id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ordem.Itens.Add(MapItem(reader));
                    }
                }
            }
        }

        private static void ApplyFilters(OrdemServicoFilter filter, StringBuilder where, NpgsqlCommand command)
        {
            if (filter.DataInicial.HasValue)
            {
                where.Append(" AND os.data_abertura >= @data_inicial ");
                command.Parameters.AddWithValue("@data_inicial", filter.DataInicial.Value.Date);
            }

            if (filter.DataFinal.HasValue)
            {
                where.Append(" AND os.data_abertura < @data_final ");
                command.Parameters.AddWithValue("@data_final", filter.DataFinal.Value.Date.AddDays(1));
            }

            if (filter.ClienteId.HasValue)
            {
                where.Append(" AND os.cliente_id = @cliente_id ");
                command.Parameters.AddWithValue("@cliente_id", filter.ClienteId.Value);
            }

            if (filter.Status.HasValue)
            {
                where.Append(" AND os.status = @status ");
                command.Parameters.AddWithValue("@status", (int)filter.Status.Value);
            }
        }

        private static void AddOrdemParameters(NpgsqlCommand command, OrdemServico ordem)
        {
            command.Parameters.AddWithValue("@cliente_id", ordem.ClienteId);
            command.Parameters.AddWithValue("@data_abertura", ordem.DataAbertura);
            command.Parameters.AddWithValue("@data_conclusao", (object)ordem.DataConclusao ?? DBNull.Value);
            command.Parameters.AddWithValue("@status", (int)ordem.Status);
            command.Parameters.AddWithValue("@observacao", (object)ordem.Observacao ?? DBNull.Value);
            command.Parameters.AddWithValue("@valor_total", ordem.ValorTotal);
        }

        private static OrdemServico MapOrdem(System.Data.IDataRecord reader)
        {
            return new OrdemServico
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                ClienteId = reader.GetInt32(reader.GetOrdinal("cliente_id")),
                ClienteNome = reader.GetString(reader.GetOrdinal("cliente_nome")),
                DataAbertura = reader.GetDateTime(reader.GetOrdinal("data_abertura")),
                DataConclusao = ValueOrDefault<DateTime?>(reader, "data_conclusao"),
                Status = (StatusOrdemServico)reader.GetInt32(reader.GetOrdinal("status")),
                Observacao = ValueOrDefault<string>(reader, "observacao"),
                ValorTotal = reader.GetDecimal(reader.GetOrdinal("valor_total")),
                Versao = reader.GetInt32(reader.GetOrdinal("versao"))
            };
        }

        private static OrdemServicoItem MapItem(System.Data.IDataRecord reader)
        {
            return new OrdemServicoItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                OrdemServicoId = reader.GetInt32(reader.GetOrdinal("ordem_servico_id")),
                ServicoId = reader.GetInt32(reader.GetOrdinal("servico_id")),
                ServicoNome = reader.GetString(reader.GetOrdinal("servico_nome")),
                Quantidade = reader.GetInt32(reader.GetOrdinal("quantidade")),
                ValorUnitario = reader.GetDecimal(reader.GetOrdinal("valor_unitario")),
                PercentualImpostoAplicado = reader.GetDecimal(reader.GetOrdinal("percentual_imposto_aplicado")),
                ValorTotalItem = reader.GetDecimal(reader.GetOrdinal("valor_total_item"))
            };
        }
    }
}
