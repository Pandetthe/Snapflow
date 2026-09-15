using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<int>> builder)
    {
        builder.Property(l => l.LoginProvider).Metadata.SetMaxLength(null);
        builder.Property(l => l.ProviderKey).Metadata.SetMaxLength(null);

        builder.ToTable("user_logins");
    }
}
