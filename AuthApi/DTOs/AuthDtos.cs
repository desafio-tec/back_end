using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+(\s[a-zA-ZÀ-ÿ]+)+$", ErrorMessage = "Informe nome e sobrenome")]
        public string Name { get; set; } = "Lucas Silva";

        [Required]
        public string Login { get; set; } = "lucas123";

        [Required]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
        public string Password { get; set; } = "Senha123!";
    }

    public class LoginDto
    {
        [Required] public string Login { get; set; } = "lucas123";
        [Required] public string Password { get; set; } = "Senha123!";
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }
}