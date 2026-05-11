using System;
using System.Collections.Generic;
using System.Text;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Application.Reports;
using GestaoOS.Domain.Enums;
using GestaoOS.Infrastructure.Data;
using Npgsql;

namespace GestaoOS.Infrastructure.Repositories
{
    public class RelatorioRepository : RepositoryBase, IRelatorioRepository
    {
        public RelatorioRepository(PostgresConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public IList<OrdemServicoReportRow> SearchOrdensServico(OrdemServicoFilter filter)
        {
            var rows = new List<OrdemServicoReportRow>();
            var where = new StringBuilder(" WHERE 1 = 1 ");

            using (var connection = CreateOpenConnection())
            using (var command = CreateCommand(string.Empty, connection))
            {
                ApplyFilters(filter, where, command);
                command.CommandText = @"WITH base AS (
                    SELECT os.id AS ordem_servico_id, os.cliente_id, c.nome AS cliente_nome,
                    os.data_abertura, os.status, os.valor_total,
                    COALESCE(SUM((i.quantidade * i.valor_unitario) * (i.percentual_imposto_aplicado / 100)), 0) AS total_impostos
                    FROM ordens_servico os
                    INNER JOIN clientes c ON c.id = os.cliente_id
                    LEFT JOIN ordem_servico_itens i ON i.ordem_servico_id = os.id " + where + @"
                    GROUP BY os.id, os.cliente_id, c.nome, os.data_abertura, os.status, os.valor_total)
                    SELECT *,
                    SUM(valor_total) OVER(PARTITION BY cliente_id) AS total_cliente,
                    COUNT(1) OVER(PARTITION BY cliente_id) AS quantidade_cliente,
                    SUM(valor_total) OVER() AS total_geral,
                    SUM(total_impostos) OVER() AS total_geral_impostos,
                    COUNT(1) OVER() AS quantidade_total
                    FROM base
                    ORDER BY cliente_nome, data_abertura";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new OrdemServicoReportRow
                        {
                            OrdemServicoId = reader.GetInt32(reader.GetOrdinal("ordem_servico_id")),
                            ClienteId = reader.GetInt32(reader.GetOrdinal("cliente_id")),
                            ClienteNome = reader.GetString(reader.GetOrdinal("cliente_nome")),
                            DataAbertura = reader.GetDateTime(reader.GetOrdinal("data_abertura")),
                            Status = (StatusOrdemServico)reader.GetInt32(reader.GetOrdinal("status")),
                            ValorTotal = reader.GetDecimal(reader.GetOrdinal("valor_total")),
                            TotalImpostos = reader.GetDecimal(reader.GetOrdinal("total_impostos")),
                            TotalCliente = reader.GetDecimal(reader.GetOrdinal("total_cliente")),
                            QuantidadeCliente = reader.GetInt32(reader.GetOrdinal("quantidade_cliente")),
                            TotalGeral = reader.GetDecimal(reader.GetOrdinal("total_geral")),
                            TotalGeralImpostos = reader.GetDecimal(reader.GetOrdinal("total_geral_impostos")),
                            QuantidadeTotal = reader.GetInt32(reader.GetOrdinal("quantidade_total"))
                        });
                    }
                }
            }

            return rows;
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
    }
}
