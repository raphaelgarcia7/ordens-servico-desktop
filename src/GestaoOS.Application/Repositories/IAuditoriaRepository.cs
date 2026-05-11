using GestaoOS.Application.Abstractions;
using GestaoOS.Domain.Entities;

namespace GestaoOS.Application.Repositories
{
    public interface IAuditoriaRepository
    {
        void Insert(AuditoriaRegistro auditoria, IUnitOfWork unitOfWork);
    }
}
