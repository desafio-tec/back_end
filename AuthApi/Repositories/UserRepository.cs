using AuthApi.Data;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetByLoginAsync(string login) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}