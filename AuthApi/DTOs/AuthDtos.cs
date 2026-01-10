using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+(\s[a-zA-ZÀ-ÿ]+)+$", ErrorMessage = "Informe nome e sobrenome")]
        // Definindo um valor padrão curto para o Swagger
        public string Name { get; set; } = "Lucas Silva";

        [Required(ErrorMessage = "O login é obrigatório")]
        [StringLength(20, MinimumLength = 4)]
        public string Login { get; set; } = "lucas123";

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Senha deve ter maiúscula, minúscula e número")]
        public string Password { get; set; } = "Senha123!";
    }

    public class LoginDto
    {
        [Required]
        public string Login { get; set; } = "lucas123";
        [Required]
        public string Password { get; set; } = "Senha123!";
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }
}