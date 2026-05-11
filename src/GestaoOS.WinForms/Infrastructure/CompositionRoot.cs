using GestaoOS.Application.Services;
using GestaoOS.Infrastructure.Data;
using GestaoOS.Infrastructure.Logging;
using GestaoOS.Infrastructure.Repositories;

namespace GestaoOS.WinForms.Infrastructure
{
    public static class CompositionRoot
    {
        public static AppServices Build()
        {
            var connectionFactory = new PostgresConnectionFactory();
            var unitOfWorkFactory = new PostgresUnitOfWorkFactory(connectionFactory);
            var clienteRepository = new ClienteRepository(connectionFactory);
            var servicoRepository = new ServicoRepository(connectionFactory);
            var ordemRepository = new OrdemServicoRepository(connectionFactory);
            var auditoriaRepository = new AuditoriaRepository(connectionFactory);
            var relatorioRepository = new RelatorioRepository(connectionFactory);

            return new AppServices
            {
                Clientes = new ClienteService(clienteRepository),
                Servicos = new ServicoService(servicoRepository),
                OrdensServico = new OrdemServicoService(ordemRepository, auditoriaRepository, unitOfWorkFactory),
                Relatorios = new RelatorioService(relatorioRepository),
                Logger = new FileLogger()
            };
        }
    }
}
