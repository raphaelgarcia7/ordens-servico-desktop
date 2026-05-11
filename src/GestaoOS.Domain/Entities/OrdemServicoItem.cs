namespace GestaoOS.Domain.Entities
{
    public class OrdemServicoItem
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public int ServicoId { get; set; }
        public string ServicoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal PercentualImpostoAplicado { get; set; }
        public decimal ValorTotalItem { get; set; }

        public void Recalcular()
        {
            var subtotal = Quantidade * ValorUnitario;
            var imposto = subtotal * (PercentualImpostoAplicado / 100m);
            ValorTotalItem = subtotal + imposto;
        }
    }
}
