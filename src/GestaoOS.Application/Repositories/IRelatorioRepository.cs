using System.Collections.Generic;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Reports;

namespace GestaoOS.Application.Repositories
{
    public interface IRelatorioRepository
    {
        IList<OrdemServicoReportRow> SearchOrdensServico(OrdemServicoFilter filter);
    }
}
