using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Domain.Entities;

namespace GestaoOS.Application.Repositories
{
    public interface IClienteRepository
    {
        PagedResult<Cliente> Search(ClienteFilter filter);
        Cliente GetById(int id);
        int Insert(Cliente cliente);
        void Update(Cliente cliente);
        void Delete(int id);
        bool HasOrdensServico(int clienteId);
    }
}
