using GestaoOS.Domain.Entities;
using GestaoOS.Domain.Exceptions;

namespace GestaoOS.Application.Validation
{
    public static class ClienteValidator
    {
        public static void Validate(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ValidationException("Cliente inválido.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Nome))
            {
                throw new ValidationException("Informe o nome do cliente.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Documento))
            {
                throw new ValidationException("Informe o documento do cliente.");
            }
        }
    }
}
