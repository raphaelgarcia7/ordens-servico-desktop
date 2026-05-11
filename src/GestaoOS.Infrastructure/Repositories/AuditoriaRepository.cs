using GestaoOS.Application.Abstractions;
using GestaoOS.Application.Repositories;
using GestaoOS.Domain.Entities;
using GestaoOS.Infrastructure.Data;

namespace GestaoOS.Infrastructure.Repositories
{
    public class AuditoriaRepository : RepositoryBase, IAuditoriaRepository
    {
        public AuditoriaRepository(PostgresConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public void Insert(AuditoriaRegistro auditoria, IUnitOfWork unitOfWork)
        {
            using (var command = CreateCommand(@"INSERT INTO auditoria
                (entidade, id_registro, operacao, data_hora, usuario, snapshot_json)
                VALUES (@entidade, @id_registro, @operacao, @data_hora, @usuario, CAST(@snapshot_json AS jsonb))", unitOfWork))
            {
                command.Parameters.AddWithValue("@entidade", auditoria.Entidade);
                command.Parameters.AddWithValue("@id_registro", auditoria.IdRegistro);
                command.Parameters.AddWithValue("@operacao", auditoria.Operacao);
                command.Parameters.AddWithValue("@data_hora", auditoria.DataHora);
                command.Parameters.AddWithValue("@usuario", auditoria.Usuario);
                command.Parameters.AddWithValue("@snapshot_json", auditoria.SnapshotJson);
                command.ExecuteNonQuery();
            }
        }
    }
}
