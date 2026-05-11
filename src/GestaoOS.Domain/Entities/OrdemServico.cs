using System;
using System.Collections.Generic;
using System.Linq;
using GestaoOS.Domain.Enums;

namespace GestaoOS.Domain.Entities
{
    public class OrdemServico
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public DateTime DataAbertura { get; set; }
        public DateTime? DataConclusao { get; set; }
        public StatusOrdemServico Status { get; set; }
        public string Observacao { get; set; }
        public decimal ValorTotal { get; set; }
        public int Versao { get; set; }
        public IList<OrdemServicoItem> Itens { get; private set; }

        public OrdemServico()
        {
            Itens = new List<OrdemServicoItem>();
        }

        public void RecalcularTotal()
        {
            foreach (var item in Itens)
            {
                item.Recalcular();
            }

            ValorTotal = Itens.Sum(item => item.ValorTotalItem);
        }
    }
}
