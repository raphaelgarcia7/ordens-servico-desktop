using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;

namespace GestaoOS.Application.Repositories
{
    public interface IServicoRepository
    {
        PagedResult<Servico> Search(ServicoFilter filter);
        Servico GetById(int id);
        int Insert(Servico servico);
        void Update(Servico servico);
    }
}
