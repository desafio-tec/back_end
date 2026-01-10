using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // Campo necessário para a trava de 3 tentativas
        public int AccessFailedCount { get; set; } = 0; 
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}