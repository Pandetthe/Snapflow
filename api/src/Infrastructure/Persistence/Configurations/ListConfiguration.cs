﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snapflow.Domain.Lists;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence.Configurations;

internal sealed class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.HasOne(l => l.CreatedBy as AppUser)
            .WithMany()
            .HasForeignKey(l => l.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(l => l.UpdatedBy as AppUser)
            .WithMany()
            .HasForeignKey(l => l.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.DeletedBy as AppUser)
            .WithMany()
            .HasForeignKey(l => l.DeletedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(ListOptions.MaxTitleLength);
    }
}
