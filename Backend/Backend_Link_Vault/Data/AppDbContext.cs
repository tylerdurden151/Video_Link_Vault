using Backend_Link_Vault.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Link_Vault.Data;

// This is the bridge between the database and the application.
// It is used to query and save instances of your entities.
// DbContext is a combination of the Unit Of Work and Repository patterns.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    // These are the Tables in the database. Each DbSet corresponds to a table, and each entity corresponds to a row in that table.
    public DbSet<User> Users => Set<User>();
    public DbSet<VideoLink> VideoLinks => Set<VideoLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
