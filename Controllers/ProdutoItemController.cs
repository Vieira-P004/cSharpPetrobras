using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BdAulaBackend.DTO;
using BdAulaBackend.Models;

namespace BdAulaBackend.Controllers
{
    // Definindo anotações para EF identificar o que é uma controller
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoItemController : ControllerBase
    {
        // Definindo um atributo global do tipo BdAulaBackendContext
        private readonly BdAulaBackendContext _context;

        // Criando um método construtor da classe
        public ProdutoItemController(BdAulaBackendContext context)
        {
            // Instânciando o atributo _context
            _context = context;
        }

        // Listar todos os produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var produtos = await _context.Produtos
                // expressão lambda
                .Include(p => p.IditemNavigation)
                .ToListAsync();

            return Ok(produtos);
        }

        // Listar apenas um produto
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .FirstOrDefaultAsync(p => p.Iditem == id);

            if (produto == null)
            {
                return NotFound(
                    new
                    {
                        mensagem = "Produto não Localizado!"
                    });
            }

            return Ok(produto);
        }

        // Insert dados nas tabelas item e produto
        [HttpPost]
        public async Task<ActionResult> PostProduto(ProdutoDTO produtoDTO)
        {
            // Abrindo uma transação no banco de dados
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Dados Item
                var item = new Item
                {
                    Idcategoria = produtoDTO.IdCategoria,
                    Item1 = produtoDTO.Item,
                    DescricaoItem = produtoDTO.DescricaoItem,
                    Preco = (decimal) produtoDTO.Preco,
                    TipoItem = produtoDTO.TipoItem,
                    AtivoProdutoServico = produtoDTO.AtivoProdutoServico
                };
                // Prepara os dados para o banco
                _context.Items.Add(item);

                // Salva os dados no banco
                await _context.SaveChangesAsync();

                // Pega o id gerado
                int idItemGerado = item.Iditem;

                // Dados produto
                var produto = new Produto
                {
                    Iditem = idItemGerado,
                    CodigoPreco = produtoDTO.CodigoPreco,
                    PrecoVenda = (decimal)produtoDTO.PrecoVenda,
                    Unidade = produtoDTO.Unidade
                };

                // Prepara os dados para o banco
                _context.Produtos.Add(produto);

                // Salva os dados no banco
                await _context.SaveChangesAsync();

                // Confirme a transação
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetProduto),
                    new { id = produto.Iditem },
                    produto
                );

            }
            catch (Exception ex)
            {
                // Caso haja algum erro executa o rollback nas tabelas
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro no cadastro!",
                    erro = ex.Message
                });
            }

        }

        // Alterar dados nas tabelas item e produto
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, ProdutoDTO dados)
        {
            // Abrindo uma transação no banco de dados
            using var transaction = await _context.Database.BeginTransactionAsync();    

            try
            {
                // Busca o Produto junto com o Item
                var produtoExistente = await _context.Produtos
                    .Include(p => p.IditemNavigation)
                    .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produtoExistente == null) 
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                // Dados da tabela produto 
                produtoExistente.CodigoPreco = dados.CodigoPreco;
                produtoExistente.PrecoVenda = (decimal)dados.PrecoVenda;
                produtoExistente.Unidade = dados.Unidade;

                // Dados da tabela item

                produtoExistente.IditemNavigation.Idcategoria = dados.IdCategoria;
                produtoExistente.IditemNavigation.Item1 = dados.Item;
                produtoExistente.IditemNavigation.DescricaoItem = dados.DescricaoItem;
                produtoExistente.IditemNavigation.Preco = (decimal) dados.Preco;
                produtoExistente.IditemNavigation.TipoItem = dados.TipoItem;
                produtoExistente.IditemNavigation.AtivoProdutoServico = dados.AtivoProdutoServico;

                // Salva as duas alterações
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                // Caso haja algum erro executa o rollback nas tabelas
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro no alterar",
                    erro = ex.Message
                });
            }

            return NoContent();
        }

        //EXCLUIR DADO NAS TABELAS ESTOQUE, PRODUTO E ITEM
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var produto = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produto == null)
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                // Busca os registros de estoque do produto
                var estoques = await _context.Estoques
                    .Where(e => e.Iditem == id)
                    .ToListAsync();

                // Remove os registros de estoque primeiro
                if (estoques.Any())
                {
                    _context.Estoques.RemoveRange(estoques);
                    await _context.SaveChangesAsync();
                }

                // Remove o produto
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                // Busca e remove o item correspondente
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Iditem == id);

                if (item != null)
                {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro ao excluir produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });

            }

        }

    }// Fim classe

}// Fim namespace