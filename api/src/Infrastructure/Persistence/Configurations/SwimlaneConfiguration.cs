using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class SwimlaneConfiguration : IEntityTypeConfiguration<Swimlane>
{
    public void Configure(EntityTypeBuilder<Swimlane> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Board)
            .WithMany(b => b.Swimlanes)
            .HasForeignKey(s => s.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.UpdatedBy)
            .WithMany()
            .HasForeignKey(s => s.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.DeletedBy)
            .WithMany()
            .HasForeignKey(s => s.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Lists)
            .WithOne(l => l.Swimlane)
            .HasForeignKey(l => l.SwimlaneId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Cards)
            .WithOne(c => c.Swimlane)
            .HasForeignKey(c => c.SwimlaneId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

    }
}
