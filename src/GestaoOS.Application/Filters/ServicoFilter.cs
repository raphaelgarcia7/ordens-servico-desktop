namespace GestaoOS.Application.Filters
{
    public class ServicoFilter
    {
        public string Nome { get; set; }
        public bool? Ativo { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
