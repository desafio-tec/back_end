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
            var userExists = await _userRepository.GetByLoginAsync(dto.Login);
            if (userExists != null) return BadRequest("Este login já está em uso.");

            var user = new User {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);
            return Ok(new UserResponseDto(user.Id, user.Name, user.Login));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByLoginAsync(dto.Login);
            if (user == null) return Unauthorized("Usuário ou senha inválidos.");

            if (user.AccessFailedCount >= 3)
                return BadRequest("Conta bloqueada por excesso de tentativas.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                await _userRepository.UpdateAsync(user);
                return Unauthorized($"Senha incorreta. Tentativa {user.AccessFailedCount} de 3.");
            }

            user.AccessFailedCount = 0;
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}