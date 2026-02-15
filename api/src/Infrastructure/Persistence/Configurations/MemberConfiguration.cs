using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Members;
using Snapflow.Infrastructure.Auth.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("board_members");
        builder.HasKey(b => new { b.BoardId, b.UserId });
        builder.HasOne(b => b.User as AppUser)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
