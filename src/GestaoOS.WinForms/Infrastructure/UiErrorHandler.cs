using System;
using System.Windows.Forms;
using GestaoOS.Application.Abstractions;
using GestaoOS.Domain.Exceptions;
using Npgsql;

namespace GestaoOS.WinForms.Infrastructure
{
    public static class UiErrorHandler
    {
        public static void Handle(Exception exception, ILogger logger)
        {
            logger.Error(exception);

            var message = GetFriendlyMessage(exception);
            MessageBox.Show(message, "Gestão de OS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string GetFriendlyMessage(Exception exception)
        {
            if (exception is DomainException)
            {
                return exception.Message;
            }

            var postgresException = exception as PostgresException;
            if (postgresException == null)
            {
                return "Não foi possível concluir a operação. Consulte o log técnico.";
            }

            if (postgresException.SqlState == "23505")
            {
                return "Já existe um registro cadastrado com os dados informados.";
            }

            if (postgresException.SqlState == "23503")
            {
                return "Não é possível excluir ou alterar o registro porque ele possui vínculos.";
            }

            if (postgresException.SqlState == "23514")
            {
                return "Os dados informados violam uma regra de validação do banco.";
            }

            return "Erro de banco de dados. Consulte o log técnico.";
        }
    }
}
