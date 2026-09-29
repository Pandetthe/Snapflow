using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
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
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using Snapflow.Domain.Roles;

namespace Snapflow.Infrastructure.Persistence;

public sealed class AppDbContext(
        DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, int>(options), IAppDbContext, IDataProtectionKeyContext
{
    IQueryable<IUser> IAppDbContext.Users => Set<AppUser>().AsQueryable().Cast<IUser>();
    IQueryable<IRole> IAppDbContext.Roles => Set<AppRole>().AsQueryable().Cast<IRole>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Swimlane> Swimlanes => Set<Swimlane>();
    public DbSet<List> Lists => Set<List>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Card> Cards => Set<Card>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.HasDefaultSchema(Schemas.Default);

        var entityTypes = builder.Model.GetEntityTypes()
            .Where(t => typeof(IEntity).IsAssignableFrom(t.ClrType));

        foreach (IMutableEntityType entityType in entityTypes)
        {
            builder.Entity(entityType.ClrType).Ignore(nameof(IEntity.DomainEvents));
        }

        ApplySoftDeleteFilters(builder);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes()
                     .Where(type => typeof(ISoftDeletable).IsAssignableFrom(type.ClrType)))
        {
            ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "entity");
            MemberExpression property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            UnaryExpression notDeleted = Expression.Not(property);

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(ISoftDeletable.FilterName, Expression.Lambda(notDeleted, parameter));
        }
    }
}
