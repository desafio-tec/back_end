using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Services;
using AuthApi.DTOs;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly TokenService _tokenService;

        public AuthController(IUserRepository repo, TokenService tokenService)
        {
            _repo = repo;
            _tokenService = tokenService;
        }

        // Endpoint de Verificação em tempo real (Corrige o travamento no Front)
        [HttpGet("check-login")]
        public async Task<IActionResult> CheckLogin([FromQuery] string login)
        {
            // Se o campo for muito curto, não bloqueia ainda
            if (string.IsNullOrEmpty(login) || login.Length < 3)
                return Ok(new { available = true });

            var user = await _repo.GetByLoginAsync(login);
            
            // Retorna EXATAMENTE o que o seu Front-end espera: { available: bool }
            return Ok(new { available = user == null });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Validação de segurança de 8 caracteres no Back-end
            if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 8)
                return BadRequest(new { message = "A senha deve ter pelo menos 8 caracteres." });

            var existingUser = await _repo.GetByLoginAsync(dto.Login);
            if (existingUser != null)
                return BadRequest(new { message = "Este login já está em uso." });

            var user = new User
            {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                AccessFailedCount = 0 // Inicializa a coluna criada no Neon
            };

            await _repo.AddAsync(user);
            return Ok(new { message = "Usuário cadastrado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _repo.GetByLoginAsync(dto.Login);
            
            if (user == null)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            // Trava de segurança usando a coluna AccessFailedCount do Neon
            if (user.AccessFailedCount >= 3)
                return BadRequest(new { message = "Conta bloqueada por excesso de tentativas." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                await _repo.UpdateAsync(user);
                
                int tentativasRestantes = 3 - user.AccessFailedCount;
                return Unauthorized(new { 
                    message = $"Senha incorreta. Restam {tentativasRestantes} tentativas." 
                });
            }

            // Reseta falhas ao acertar a senha
            user.AccessFailedCount = 0;
            await _repo.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);
            return Ok(new { 
                token = token, 
                user = new { user.Id, user.Name, user.Login } 
            });
        }
    }
}