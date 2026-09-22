using Backend_Link_Vault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend_Link_Vault.Data.Configurations;

public class VideoLinkConfiguration : IEntityTypeConfiguration<VideoLink>
{
    public void Configure(EntityTypeBuilder<VideoLink> builder)
    {
        builder.Property(v => v.Platform)
            //Tells EF Core to store the enum as a string in the database, rather than its numeric value.
            //This makes the database more readable and maintainable.
            .HasConversion<string>();

        //Fluent API configuration for the relationship between VideoLink and User.
        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
