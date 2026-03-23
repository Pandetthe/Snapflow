using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.AvatarData)
            .HasColumnType("bytea");

        builder.Property(u => u.AvatarContentType)
            .HasMaxLength(50);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.AvatarType)
            .HasConversion<int>()
            .HasDefaultValue(AvatarType.Generated);

        builder.ToTable("users");
    }
}
