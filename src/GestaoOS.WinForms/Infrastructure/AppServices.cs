using GestaoOS.Application.Abstractions;
using GestaoOS.Application.Services;

namespace GestaoOS.WinForms.Infrastructure
{
    public class AppServices
    {
        public ClienteService Clientes { get; set; }
        public ServicoService Servicos { get; set; }
        public OrdemServicoService OrdensServico { get; set; }
        public RelatorioService Relatorios { get; set; }
        public ILogger Logger { get; set; }
    }
}
