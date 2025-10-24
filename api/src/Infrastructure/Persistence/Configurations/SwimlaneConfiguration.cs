using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Swimlanes;
using Snapflow.Infrastructure.Identity;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class SwimlaneConfiguration : IEntityTypeConfiguration<Swimlane>
{
    public void Configure(EntityTypeBuilder<Swimlane> builder)
    {
        builder.HasKey(s => s.Id);
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
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

    }
}
