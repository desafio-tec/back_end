using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int AccessFailedCount { get; set; } = 0; // Necessário para a trava
    }
}