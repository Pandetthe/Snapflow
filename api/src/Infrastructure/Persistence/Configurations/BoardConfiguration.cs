using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Boards;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasOne(b => b.CreatedBy as AppUser)
            .WithMany()
            .HasForeignKey(b => b.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(b => b.UpdatedBy as AppUser)
            .WithMany()
            .HasForeignKey(b => b.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.DeletedBy as AppUser)
            .WithMany()
            .HasForeignKey(b => b.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(b => b.Title)
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(BoardOptions.MaxTitleLength);
        builder.Property(b => b.Description)
            .IsRequired()
            .HasDefaultValue("")
            .HasMaxLength(BoardOptions.MaxDescriptionLength);
    }
}
