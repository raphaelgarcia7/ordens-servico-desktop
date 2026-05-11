using System;
using GestaoOS.Domain.Enums;

namespace GestaoOS.Domain.Entities
{
    public class HistoricoStatus
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public StatusOrdemServico StatusAnterior { get; set; }
        public StatusOrdemServico StatusNovo { get; set; }
        public DateTime DataHora { get; set; }
        public string Usuario { get; set; }
    }
}
