using System;
using GestaoOS.Domain.Enums;

namespace GestaoOS.Application.Reports
{
    public class OrdemServicoReportRow
    {
        public int OrdemServicoId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public DateTime DataAbertura { get; set; }
        public StatusOrdemServico Status { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal TotalImpostos { get; set; }
        public decimal TotalCliente { get; set; }
        public int QuantidadeCliente { get; set; }
        public decimal TotalGeral { get; set; }
        public decimal TotalGeralImpostos { get; set; }
        public int QuantidadeTotal { get; set; }
    }
}
