using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+(\s[a-zA-ZÀ-ÿ]+)+$", ErrorMessage = "Informe seu nome completo (pelo menos nome e sobrenome)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O login é obrigatório")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "O login deve ter entre 4 e 20 caracteres")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "O login é obrigatório")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}