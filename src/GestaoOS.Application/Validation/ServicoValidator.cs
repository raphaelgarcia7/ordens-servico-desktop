using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Exceptions;

namespace GestaoOS.Application.Validation
{
    public static class ServicoValidator
    {
        public static void Validate(Servico servico)
        {
            if (servico == null)
            {
                throw new ValidationException("Serviço inválido.");
            }

            if (string.IsNullOrWhiteSpace(servico.Nome))
            {
                throw new ValidationException("Informe o nome do serviço.");
            }

            if (servico.ValorBase <= 0)
            {
                throw new ValidationException("O valor base deve ser maior que zero.");
            }

            if (servico.PercentualImposto < 0 || servico.PercentualImposto > 100)
            {
                throw new ValidationException("O percentual de imposto deve estar entre 0 e 100.");
            }
        }
    }
}
