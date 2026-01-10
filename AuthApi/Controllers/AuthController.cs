using AuthApi.DTOs;
using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController] [Route("api/[controller]")]
    public class AuthController(IUserRepository repo, TokenService tokenService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await repo.GetByLoginAsync(dto.Login) != null) 
                return BadRequest(new { message = "Login já existe" });

            var user = new User { 
                Name = dto.Name, Login = dto.Login, 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) 
            };
            
            await repo.AddAsync(user);
            return Ok(new UserResponseDto { Id = user.Id, Name = user.Name, Login = user.Login });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await repo.GetByLoginAsync(dto.Login);
            if (user == null) return Unauthorized(new { message = "Invalido" });

            if (user.AccessFailedCount >= 3) 
                return BadRequest(new { message = "Conta Bloqueada" });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) {
                user.AccessFailedCount++;
                await repo.UpdateAsync(user);
                return Unauthorized(new { message = $"Senha incorreta. Restam {3 - user.AccessFailedCount} tentativas" });
            }

            user.AccessFailedCount = 0;
            await repo.UpdateAsync(user);
            return Ok(new { token = tokenService.GenerateToken(user) });
        }
    }
}