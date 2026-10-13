using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.UserName).HasMaxLength(32).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(64);
        builder.Property(u => u.Bio).HasMaxLength(500);
        builder.Property(u => u.AvatarUrl).HasMaxLength(512);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
