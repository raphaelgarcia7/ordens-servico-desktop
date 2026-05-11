using GestaoOS.Application.Abstractions;
using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;

namespace GestaoOS.Application.Repositories
{
    public interface IOrdemServicoRepository
    {
        PagedResult<OrdemServico> Search(OrdemServicoFilter filter);
        OrdemServico GetById(int id, bool includeItems);
        int Insert(OrdemServico ordem, IUnitOfWork unitOfWork);
        bool Update(OrdemServico ordem, int expectedVersion, IUnitOfWork unitOfWork);
        void ReplaceItems(OrdemServico ordem, IUnitOfWork unitOfWork);
        void InsertHistorico(int ordemServicoId, StatusOrdemServico statusAnterior, StatusOrdemServico statusNovo, string usuario, IUnitOfWork unitOfWork);
    }
}
