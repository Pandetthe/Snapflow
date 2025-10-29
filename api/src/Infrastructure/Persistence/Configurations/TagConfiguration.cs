using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Tags;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasOne(t => t.CreatedBy as AppUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(t => t.UpdatedBy as AppUser)
            .WithMany()
            .HasForeignKey(t => t.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.DeletedBy as AppUser)
            .WithMany()
            .HasForeignKey(t => t.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TagOptions.MaxTitleLength);
    }
}
