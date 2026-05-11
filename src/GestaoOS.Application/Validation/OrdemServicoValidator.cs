using System.Linq;
using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Enums;
using GestaoOS.Domain.Exceptions;

namespace GestaoOS.Application.Validation
{
    public static class OrdemServicoValidator
    {
        public static void ValidateForSave(OrdemServico ordem)
        {
            if (ordem == null)
            {
                throw new ValidationException("Ordem de serviço inválida.");
            }

            if (ordem.ClienteId <= 0)
            {
                throw new ValidationException("Informe o cliente da ordem de serviço.");
            }

            if (ordem.Status == StatusOrdemServico.Concluida && ordem.ValorTotal <= 0)
            {
                throw new ValidationException("Não é permitido concluir ordem de serviço com valor total igual a zero.");
            }

            if (!ordem.Itens.Any())
            {
                return;
            }

            foreach (var item in ordem.Itens)
            {
                if (item.ServicoId <= 0)
                {
                    throw new ValidationException("Informe o serviço de todos os itens.");
                }

                if (item.Quantidade <= 0)
                {
                    throw new ValidationException("A quantidade dos itens deve ser maior que zero.");
                }

                if (item.ValorUnitario <= 0)
                {
                    throw new ValidationException("O valor unitário dos itens deve ser maior que zero.");
                }

                if (item.PercentualImpostoAplicado < 0 || item.PercentualImpostoAplicado > 100)
                {
                    throw new ValidationException("O imposto aplicado nos itens deve estar entre 0 e 100.");
                }
            }
        }

        public static void EnsureItemsCanBeEdited(OrdemServico ordem)
        {
            if (ordem.Status != StatusOrdemServico.Concluida && ordem.Status != StatusOrdemServico.Cancelada)
            {
                return;
            }

            throw new ValidationException("Não é permitido editar itens de uma OS concluída ou cancelada.");
        }
    }
}
