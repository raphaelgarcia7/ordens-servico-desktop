using System;
using GestaoOS.Domain.Enums;

namespace GestaoOS.Application.Filters
{
    public class OrdemServicoFilter
    {
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? ClienteId { get; set; }
        public StatusOrdemServico? Status { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
