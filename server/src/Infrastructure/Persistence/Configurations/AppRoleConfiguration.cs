using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Roles;
using Snapflow.Infrastructure.Auth.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("roles");

        // Ids and concurrency stamps are fixed so the seed does not change between migrations.
        builder.HasData(
            new AppRole
            {
                Id = 1,
                Name = SystemRoles.Admin,
                NormalizedName = SystemRoles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "5b0f4f7e-3c1d-4b8a-9a6e-2f1c7d9e8a01"
            });
    }
}
