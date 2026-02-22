using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Members;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Common;
using Snapflow.Infrastructure.Identity.Entities;
using System.Reflection.Emit;

namespace Snapflow.Infrastructure.Persistence;

public sealed class AppDbContext(
        DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, int>(options), IAppDbContext
{
    IQueryable<IUser> IAppDbContext.Users => Set<AppUser>().AsQueryable().Cast<IUser>();
    public DbSet<Board> Boards { get; private set; }
    public DbSet<Swimlane> Swimlanes { get; private set; }
    public DbSet<List> Lists { get; private set; }
    public DbSet<Member> Members { get; private set; }
    public DbSet<Tag> Tags { get; private set; }
    public DbSet<Card> Cards { get; private set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.HasDefaultSchema(Schemas.Default);

        var entityTypes = builder.Model.GetEntityTypes()
            .Where(t => typeof(IEntity).IsAssignableFrom(t.ClrType));

        foreach (var entityType in entityTypes)
        {
            builder.Entity(entityType.ClrType).Ignore(nameof(IEntity.DomainEvents));
        }
    }
}
