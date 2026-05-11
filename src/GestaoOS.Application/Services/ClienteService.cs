using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Application.Validation;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Exceptions;

namespace GestaoOS.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public PagedResult<Cliente> Search(ClienteFilter filter)
        {
            NormalizePaging(filter);
            return _clienteRepository.Search(filter);
        }

        public Cliente GetById(int id)
        {
            return _clienteRepository.GetById(id);
        }

        public int Save(Cliente cliente)
        {
            ClienteValidator.Validate(cliente);

            if (cliente.Id == 0)
            {
                return _clienteRepository.Insert(cliente);
            }

            _clienteRepository.Update(cliente);
            return cliente.Id;
        }

        public void Delete(int id)
        {
            if (_clienteRepository.HasOrdensServico(id))
            {
                throw new ValidationException("Não é permitido excluir cliente com ordem de serviço vinculada.");
            }

            _clienteRepository.Delete(id);
        }

        private static void NormalizePaging(ClienteFilter filter)
        {
            if (filter.Page <= 0)
            {
                filter.Page = 1;
            }

            if (filter.PageSize <= 0)
            {
                filter.PageSize = 20;
            }
        }
    }
}
