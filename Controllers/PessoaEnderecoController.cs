using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BdAulaBackend.DTO;
using BdAulaBackend.Models;

namespace BdAulaBackend.Controllers
{
    // Anotações de identificação de controller
    [ApiController]
    [Route("api/[controller]")]
    // ControllerBase transforma os métodos em APIs
    public class PessoaEnderecoController : ControllerBase
    {
        // Declaração do atributo de contexto
        private readonly BdAulaBackendContext _context;

        // Declaração do método construtor
        public PessoaEnderecoController(BdAulaBackendContext context)
        {
            // Instância do atributo construtor
            _context = context;
        }

        // Declaração do método para listar todas as pessoas e seus endereços
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaDTO>>> GetAll()
        {
            var pessoas = await _context.Pessoas
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = (long)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento,
                    }).ToList(),
                }).ToListAsync();
                

            return Ok(pessoas);

        } // Fim do método GetAll

        // Declaração do método para listar uma pessoa e seus endereços pelo id
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaDTO>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoas
                .Where(p => p.Idpessoa == id)
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento,
                    }).ToList(),
                }).ToListAsync();


            return Ok(pessoa);
        } // Fim do método GetPessoa


        // Método para cadastrar uma pessoa
        [HttpPost]
        public async Task<ActionResult<PessoaDTO>> Post(PessoaDTO dto)
        {
            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Cpf = dto.CPF,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                Email = dto.Email,
                Telefone = dto.Telefone,
                AtivoPessoa = dto.AtivoPessoa,

                Enderecos = dto.Enderecos.Select(e => new Endereco
                {
                    Cep = e.CEP,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()
            };
            _context.Pessoas.Add(pessoa);

            await _context.SaveChangesAsync();

            var resposta = new PessoaDTO
            {
                IdPessoa = pessoa.Idpessoa,
                Nome = pessoa.Nome,
                CPF = pessoa.Cpf,
                DataNascimento = pessoa.DataNascimento,
                Sexo = pessoa.Sexo,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone,
                AtivoPessoa = pessoa.AtivoPessoa,

                Enderecos = pessoa.Enderecos.Select(e => new EnderecoDTO
                {
                    CEP = e.Cep,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()
            };



            return CreatedAtAction(
                nameof(GetPessoa),
                new {id = pessoa.Idpessoa},
                resposta);
        } // Fim do método Post

        // Método para alterar uma pessoa
        [HttpPut("{id}")]
        public async Task<ActionResult<ActionResult<Pessoa>>> Put(int id, PessoaDTO dto)
        {
            // Busca a pessoa e seus endereços e atribui a variável Pessoa
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // Verifica se a pessoa existe 
            if (pessoa == null)
            {
                return NotFound();
            }

            // Projeta os dados do PessoaDTO para o modelo Pessoa oriunda do banco de dados
            pessoa.Nome = dto.Nome;
            pessoa.Cpf = dto.CPF;
            pessoa.DataNascimento = dto.DataNascimento;
            pessoa.Sexo = dto.Sexo;
            pessoa.Email = dto.Email;
            pessoa.Telefone = dto.Telefone;
            pessoa.AtivoPessoa = dto.AtivoPessoa;

            // Percorre os endereços da Pessoa
            foreach (var endPessoa in dto.Enderecos)
            {
                // Verifica se o id ja existe com a pessoa
                if (endPessoa.IdPessoa > 0)
                {
                    // Busca o endereço da pessoa no atributo do modelo Pessoa
                    var endereco = pessoa.Enderecos
                        .FirstOrDefault(e => e.Idpessoa == endPessoa.IdPessoa);
                    
                    // Caso não encontre emite uma mensagem de que o endereço não pertence a essa pessoa
                    if (endereco != null)
                    {
                        return BadRequest($"O endereço não pertence a essa pessoa.");
                    }

                    // Ao localizar projeta no modelo Endereco os dados vindo do atributo endereços
                    // Do modelo Pessoa que veio do PessoaDTO
                    endereco.Numero = endPessoa.Numero;
                    endereco.Cep = endPessoa.CEP;
                    endereco.Complemento = endPessoa.Complemento;      
                }
                else
                {
                    // Caso o id não seja maior que 0 (zero) é um novo endereço a ser adicionado a pessoa
                    pessoa.Enderecos.Add(new Endereco
                    {
                        Numero = endPessoa.Numero,
                        Cep = endPessoa.CEP,
                        Complemento = endPessoa.Complemento
                    });
                }

            }
            
            // Salva no banco de dados
            await _context.SaveChangesAsync();

            return NoContent();

        } // Fim do método Put

        // Método para excluir uma pessoa
        [HttpDelete("{id}")]
        public async Task<ActionResult<Pessoa>> Delete(int id)
        {
            // Busca a pessoa e seus endereços e atribui a variável pessoa
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // Verifca se a pessoa existe
            if (pessoa == null)
            {
                return NotFound();
            }

            // Prepara a instrução para executar no banco de dados
            _context.Pessoas.Remove(pessoa);

            // Salva no banco de dados
            await _context.SaveChangesAsync();

            return NoContent();
        } // Fim do método delete


    } // Fim da classe Controller PessoaEnrecoController

} // Fim do NameSpace
