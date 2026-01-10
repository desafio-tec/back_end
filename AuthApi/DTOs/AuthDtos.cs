using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+(\s[a-zA-ZÀ-ÿ]+)+$", ErrorMessage = "Informe nome e sobrenome")]
        public string Name { get; set; } = "Lucas Silva";

        [Required] public string Login { get; set; } = "lucas123";

        [Required] [MinLength(8)]
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