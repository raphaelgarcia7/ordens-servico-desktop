using System;

namespace GestaoOS.Domain.Entities
{
    public class AuditoriaRegistro
    {
        public int Id { get; set; }
        public string Entidade { get; set; }
        public int IdRegistro { get; set; }
        public string Operacao { get; set; }
        public DateTime DataHora { get; set; }
        public string Usuario { get; set; }
        public string SnapshotJson { get; set; }
    }
}
