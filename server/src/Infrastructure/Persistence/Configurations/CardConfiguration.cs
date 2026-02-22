using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Cards;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasOne(c => c.CreatedBy as AppUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(c => c.UpdatedBy as AppUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.DeletedBy as AppUser)
            .WithMany()
            .HasForeignKey(c => c.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Rank)
            .IsRequired()
            .HasMaxLength(LexoRankService.Length);

        builder.HasIndex(c => new { c.ListId, c.Rank })
            .IsUnique()
            .HasFilter("is_deleted = false");
    }
}
