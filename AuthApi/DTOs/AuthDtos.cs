using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    // O que o front envia para registrar
    public class RegisterDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Login é obrigatório")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    // O que o front envia para logar
    public class LoginDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // O que a API devolve (sem a senha!)
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty; // O JWT vai aqui
    }
}