using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BdAulaBackend.Models;
using BdAulaBackend.DTO;

namespace BdAulaBackend.Controllers
{
    [ApiController] //INDICANDO QUE ESSA CLASSE É UM CONTROLER
    [Route("api/[cotroller]")]
    public class AtendimentoController : ControllerBase
    {
        //CRIANDO O ATRIBUTO _CONTEXT
        private readonly BdAulaBackendContext _context;

        //MÉTODO CONSTRUTOR
        public AtendimentoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        //LISTAR ATENDIMENTOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtendimentoListDTo>>> GetAtendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .Select(a => new AtendimentoListDTo
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems
                    .Select(i => new AtendimentoItemListDTO
                    {
                        IdAtendimento = i.Idatendimento,
                        IdItem = i.Iditem,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        Quantidade = i.QuantidadeItens,
                        ValorUnitario = i.ValorunitarioEntrada

                    }).ToList()
                }).ToListAsync();

            return Ok(atendimentos);
        }

        //LISTAR ATENDIMENTOS
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IEnumerable<AtendimentoListDTo>>> GetAtendimentos(int id)
        {
            var atendimentos = await _context.Atendimentos
                .Where(a => a.Idatendimento == id)
                .Select(a => new AtendimentoListDTo
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems
                    .Select(i => new AtendimentoItemListDTO
                    {
                        IdAtendimento = i.Idatendimento,
                        IdItem = i.Iditem,
                        Item = i.IditemNavigation.DescricaoItem,
                        IdFuncionario = i.Idfuncionario,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        Quantidade = i.QuantidadeItens,
                        ValorUnitario = i.ValorunitarioEntrada

                    }).ToList()
                }).FirstOrDefaultAsync();

            if(atendimentos == null)
            {
                return NotFound(new
                {
                    mensagem = "Atendimento não localizado!"
                });
            }

            return Ok(atendimentos);
        }

        //ADICIONAR ATENDIMENTO
        [HttpPost]
        public async Task<ActionResult<AtendimentoListDTo>> PostAtendimento(AtendimentoPostDTO dto)
        {
            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(p => p.Idpessoa == dto.IdPessoa);

            if (pessoa == null)
            {
                return BadRequest(NotFound(new
                {
                    mensagem = "Pessoa não localizada!"
                }));
            }

            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = dto.DataAtendimento,
                StatusAtendimento = dto.StatusAtendimento,
                Atendimentoitems = dto.Itens
                .Select(i => new Atendimentoitem
                {
                    Iditem = i.IdItem,
                    Idfuncionario = i.IdFuncionario,
                    QuantidadeItens = i.Quantidade,
                    ValorunitarioEntrada = i.ValorUnitario
                }).ToList()

            };

            _context.Atendimentos.Add(atendimento);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAtendimentos),
                new { id = atendimento.Idatendimento },
                dto);
        } 
    }
}
