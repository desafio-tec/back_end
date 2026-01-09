using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O login é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O login deve ter entre 3 e 50 caracteres")]
        public string Login { get; set; } = string.Empty;

        // A senha será armazenada como Hash, nunca em texto puro
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Status: True = Ativo, False = Inativo
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}