using System;

namespace GestaoOS.Application.Abstractions
{
    public interface ILogger
    {
        void Error(Exception exception);
        void Info(string message);
    }
}
