using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Application.Validation;
using GestaoOS.Domain.Entities;

namespace GestaoOS.Application.Services
{
    public class ServicoService
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoService(IServicoRepository servicoRepository)
        {
            _servicoRepository = servicoRepository;
        }

        public PagedResult<Servico> Search(ServicoFilter filter)
        {
            NormalizePaging(filter);
            return _servicoRepository.Search(filter);
        }

        public Servico GetById(int id)
        {
            return _servicoRepository.GetById(id);
        }

        public int Save(Servico servico)
        {
            ServicoValidator.Validate(servico);

            if (servico.Id == 0)
            {
                return _servicoRepository.Insert(servico);
            }

            _servicoRepository.Update(servico);
            return servico.Id;
        }

        private static void NormalizePaging(ServicoFilter filter)
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
