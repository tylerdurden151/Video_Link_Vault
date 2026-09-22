using Backend_Link_Vault.Data;
using Backend_Link_Vault.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Link_Vault.Services;

public class UserStore
{
    private readonly AppDbContext _context;

    public UserStore(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> FindByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == email.ToLower());

    public Task<User?> FindByIdAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public Task<List<User>> GetAllUsersAsync() =>
        _context.Users.ToListAsync();
}
