using System;
using System.Web.Script.Serialization;
using GestaoOS.Application.Abstractions;
using GestaoOS.Application.Common;
using GestaoOS.Application.Filters;
using GestaoOS.Application.Repositories;
using GestaoOS.Application.Validation;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Exceptions;

namespace GestaoOS.Application.Services
{
    public class OrdemServicoService
    {
        private readonly IOrdemServicoRepository _ordemRepository;
        private readonly IAuditoriaRepository _auditoriaRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public OrdemServicoService(
            IOrdemServicoRepository ordemRepository,
            IAuditoriaRepository auditoriaRepository,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _ordemRepository = ordemRepository;
            _auditoriaRepository = auditoriaRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public PagedResult<OrdemServico> Search(OrdemServicoFilter filter)
        {
            NormalizePaging(filter);
            return _ordemRepository.Search(filter);
        }

        public OrdemServico GetById(int id)
        {
            return _ordemRepository.GetById(id, true);
        }

        public int Save(OrdemServico ordem, string usuario)
        {
            ordem.RecalcularTotal();
            OrdemServicoValidator.ValidateForSave(ordem);

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                unitOfWork.Begin();

                try
                {
                    var id = SaveInternal(ordem, usuario, unitOfWork);
                    unitOfWork.Commit();
                    return id;
                }
                catch
                {
                    unitOfWork.Rollback();
                    throw;
                }
            }
        }

        private int SaveInternal(OrdemServico ordem, string usuario, IUnitOfWork unitOfWork)
        {
            if (ordem.Id == 0)
            {
                ordem.Id = _ordemRepository.Insert(ordem, unitOfWork);
                _ordemRepository.ReplaceItems(ordem, unitOfWork);
                _ordemRepository.InsertHistorico(ordem.Id, ordem.Status, ordem.Status, usuario, unitOfWork);
                Audit("OrdemServico", ordem.Id, "INSERT", usuario, ordem, unitOfWork);
                return ordem.Id;
            }

            var atual = _ordemRepository.GetById(ordem.Id, true);
            if (atual == null)
            {
                throw new ValidationException("Ordem de serviço não encontrada.");
            }

            OrdemServicoValidator.EnsureItemsCanBeEdited(atual);

            var updated = _ordemRepository.Update(ordem, ordem.Versao, unitOfWork);
            if (!updated)
            {
                throw new ConcurrencyException("A ordem de serviço foi alterada por outro usuário. Recarregue o registro antes de salvar.");
            }

            _ordemRepository.ReplaceItems(ordem, unitOfWork);

            if (atual.Status != ordem.Status)
            {
                _ordemRepository.InsertHistorico(ordem.Id, atual.Status, ordem.Status, usuario, unitOfWork);
                Audit("HistoricoStatus", ordem.Id, "INSERT", usuario, ordem, unitOfWork);
            }

            if (atual.ValorTotal != ordem.ValorTotal)
            {
                Audit("OrdemServico", ordem.Id, "UPDATE_VALOR_TOTAL", usuario, ordem, unitOfWork);
            }

            Audit("OrdemServicoItem", ordem.Id, "UPDATE_ITENS", usuario, ordem.Itens, unitOfWork);
            return ordem.Id;
        }

        private void Audit(string entidade, int idRegistro, string operacao, string usuario, object snapshot, IUnitOfWork unitOfWork)
        {
            var serializer = new JavaScriptSerializer();
            var auditoria = new AuditoriaRegistro
            {
                Entidade = entidade,
                IdRegistro = idRegistro,
                Operacao = operacao,
                DataHora = DateTime.Now,
                Usuario = string.IsNullOrWhiteSpace(usuario) ? Environment.UserName : usuario,
                SnapshotJson = serializer.Serialize(snapshot)
            };

            _auditoriaRepository.Insert(auditoria, unitOfWork);
        }

        private static void NormalizePaging(OrdemServicoFilter filter)
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
