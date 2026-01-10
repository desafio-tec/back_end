using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public record RegisterDto(
        [Required(ErrorMessage = "O nome é obrigatório")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+(\s[a-zA-ZÀ-ÿ]+)+$", ErrorMessage = "Informe seu nome completo (nome e sobrenome)")]
        string Name,

        [Required(ErrorMessage = "O login é obrigatório")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "O login deve ter entre 4 e 20 caracteres")]
        string Login,

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "A senha deve conter maiúscula, minúscula e número")]
        string Password
    );

    public record LoginDto(
        [Required(ErrorMessage = "O login é obrigatório")]
        string Login,
        [Required(ErrorMessage = "A senha é obrigatória")]
        string Password
    );
}