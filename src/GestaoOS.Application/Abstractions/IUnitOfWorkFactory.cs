namespace GestaoOS.Application.Abstractions
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
