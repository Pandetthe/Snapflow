using Microsoft.EntityFrameworkCore;
using Snapflow.Domain.Members;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Snapflow.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DatabaseFacade Database { get; }
    IQueryable<IUser> Users { get; }

    DbSet<Board> Boards { get; }

    DbSet<Swimlane> Swimlanes { get; }

    DbSet<List> Lists { get; }

    DbSet<Member> Members { get; }

    DbSet<Tag> Tags { get; }

    DbSet<Card> Cards { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}