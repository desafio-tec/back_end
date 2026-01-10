using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Services;
using AuthApi.DTOs;
using BCrypt.Net;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Verifica se o login já existe para evitar erro de banco
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

            // Trava de segurança usando a coluna manual do Neon
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

            // Sucesso no login: reseta o contador de falhas
            user.AccessFailedCount = 0;
            await _repo.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token = token, user = new { user.Id, user.Name, user.Login } });
        }

        // NOVO: Endpoint para o Front-end verificar disponibilidade em tempo real
        [HttpGet("check-login")]
        public async Task<IActionResult> CheckLogin([FromQuery] string login)
        {
            if (string.IsNullOrEmpty(login) || login.Length < 3)
                return Ok(new { available = true });

            var user = await _repo.GetByLoginAsync(login);
            // Se user for nulo, o login está disponível
            return Ok(new { available = user == null });
        }
    }
}