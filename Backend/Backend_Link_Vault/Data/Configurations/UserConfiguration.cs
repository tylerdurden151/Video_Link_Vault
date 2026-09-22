using Backend_Link_Vault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend_Link_Vault.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        //Fluent API configuration for the User entity
        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}
