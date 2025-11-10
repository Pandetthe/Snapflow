using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Swimlanes;
using Snapflow.Infrastructure.Behaviours;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class SwimlaneConfiguration : IEntityTypeConfiguration<Swimlane>
{
    public void Configure(EntityTypeBuilder<Swimlane> builder)
    {
        builder.HasOne(s => s.CreatedBy as AppUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(s => s.UpdatedBy as AppUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.DeletedBy as AppUser)
            .WithMany()
            .HasForeignKey(s => s.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(SwimlaneOptions.MaxTitleLength);
        builder.Property(s => s.Rank)
            .IsRequired()
            .HasMaxLength(LexoRankService.Length);
        builder.HasIndex(s => new { s.BoardId, s.Rank })
            .IsUnique();
    }
}
