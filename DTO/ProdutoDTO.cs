namespace BdAulaBackend.DTO
{
    public class ProdutoDTO
    {
        // Dados Tabela Item
        public int IdCategoria { get; set; }

        public string? Item { get; set; }

        public string? DescricaoItem { get; set; }

        public decimal? Preco { get; set; }

        public string? TipoItem { get; set; }

        public string? AtivoProdutoServico { get; set; }

        // Dados tabela produto

        public decimal? PrecoVenda { get; set; }

        public string? Unidade { get; set; }

        public int CodigoPreco { get; set; }  
    }
}
