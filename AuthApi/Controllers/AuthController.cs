using AuthApi.DTOs;
using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public AuthController(IUserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterDto dto)
        {
            // 1. Validação de Login Único
            var userExists = await _userRepository.GetByLoginAsync(dto.Login);
            if (userExists != null)
                return BadRequest(new { message = "Este login já está em uso por outro usuário." });

            var user = new User
            {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);

            return Ok(new UserResponseDto 
            { 
                Id = user.Id, 
                Name = user.Name, 
                Login = user.Login 
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByLoginAsync(dto.Login);
            
            if (user == null) 
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            // 2. Trava de Segurança (Bloqueio após 3 erros)
            if (user.AccessFailedCount >= 3)
                return BadRequest(new { message = "Conta bloqueada por excesso de tentativas. Procure o administrador." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                await _userRepository.UpdateAsync(user);
                
                int tentativasRestantes = 3 - user.AccessFailedCount;
                return Unauthorized(new { 
                    message = tentativasRestantes > 0 
                        ? $"Senha incorreta. Você tem mais {tentativasRestantes} tentativa(s) antes do bloqueio." 
                        : "Limite de tentativas excedido. Conta bloqueada." 
                });
            }

            // 3. Sucesso: Reseta o contador de erros
            user.AccessFailedCount = 0;
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}