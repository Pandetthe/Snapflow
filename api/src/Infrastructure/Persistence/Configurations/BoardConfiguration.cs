using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Boards;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasKey(b => b.Id);
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
        builder.HasIndex(b => b.Title)
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
    }
}
