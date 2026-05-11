using System.Collections.Generic;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Application.Reports;

namespace GestaoOS.Application.Services
{
    public class RelatorioService
    {
        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public IList<OrdemServicoReportRow> GerarOrdensServico(OrdemServicoFilter filter)
        {
            return _relatorioRepository.SearchOrdensServico(filter);
        }
    }
}
