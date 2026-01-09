using Microsoft.AspNetCore.Mvc;
using AuthApi.DTOs;
using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserRepository repository, TokenService tokenService, ILogger<AuthController> logger)
        {
            _repository = repository;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterDto dto)
        {
            if (await _repository.UserExistsAsync(dto.Login))
                return BadRequest("Este login já está em uso.");

            // Cria o usuário com senha criptografada
            var user = new User
            {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };

            await _repository.CreateAsync(user);

            // Gera o token para já logar direto se quiser
            var token = _tokenService.GenerateToken(user);

            _logger.LogInformation($"Novo usuário registrado: {user.Login}");

            return CreatedAtAction(nameof(Login), new { user.Id, user.Name, user.Login, user.IsActive, Token = token });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(LoginDto dto)
        {
            var user = await _repository.GetByLoginAsync(dto.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Login ou senha inválidos.");

            if (!user.IsActive)
                return Unauthorized("Usuário inativo.");

            var token = _tokenService.GenerateToken(user);

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                IsActive = user.IsActive,
                Token = token
            });
        }
    }
}