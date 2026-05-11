namespace GestaoOS.Application.Filters
{
    public class ClienteFilter
    {
        public string Nome { get; set; }
        public string Documento { get; set; }
        public bool? Ativo { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
