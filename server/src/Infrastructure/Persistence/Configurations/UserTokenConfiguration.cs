using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<int>> builder)
    {
        builder.Property(t => t.LoginProvider).Metadata.SetMaxLength(null);
        builder.Property(t => t.Name).Metadata.SetMaxLength(null);

        builder.ToTable("user_tokens");
    }
}
