using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class UserPasskeyConfiguration : IEntityTypeConfiguration<IdentityUserPasskey<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserPasskey<int>> builder)
    {
        builder.ToTable("user_passkeys");
    }
}
