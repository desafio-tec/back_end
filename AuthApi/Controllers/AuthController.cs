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
            // Verifica se o login já existe (ignora maiúsculas/minúsculas)
            var userExists = await _userRepository.GetByLoginAsync(dto.Login);
            if (userExists != null)
                return BadRequest("Este login já está em uso.");

            // Cria o novo usuário com a senha criptografada
            var user = new User
            {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);

            // Retorna os dados do usuário usando o DTO de resposta
            return Ok(new UserResponseDto(user.Id, user.Name, user.Login));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByLoginAsync(dto.Login);
            
            // Verifica se o usuário existe
            if (user == null) 
                return Unauthorized("Usuário ou senha inválidos.");

            // Verifica se a conta está bloqueada (3 ou mais erros)
            if (user.AccessFailedCount >= 3)
                return BadRequest("Conta bloqueada por excesso de tentativas falhas. Procure o suporte.");

            // Verifica a senha
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                // Incrementa o erro e salva no banco
                user.AccessFailedCount++;
                await _userRepository.UpdateAsync(user);

                if (user.AccessFailedCount >= 3)
                    return BadRequest("Você excedeu o limite de tentativas. Conta bloqueada.");

                return Unauthorized($"Senha incorreta. Tentativa {user.AccessFailedCount} de 3.");
            }

            // Se acertou a senha, reseta o contador de erros para zero
            user.AccessFailedCount = 0;
            await _userRepository.UpdateAsync(user);

            // Gera o Token JWT
            var token = _tokenService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}