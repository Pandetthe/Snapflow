using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Persistence;

public sealed record UniqueConflict(string ConstraintName, Error Error);

public static class SaveChangesExtensions
{
    extension(IAppDbContext dbContext)
    {
        public Task<Result> TrySaveChangesAsync(
            IReadOnlyList<UniqueConflict> conflicts,
            CancellationToken cancellationToken) =>
            SaveAsync(dbContext, conflicts, concurrencyConflict: null, cancellationToken);

        public Task<Result> TrySaveChangesAsync(
            IReadOnlyList<UniqueConflict> conflicts,
            Error concurrencyConflict,
            CancellationToken cancellationToken) =>
            SaveAsync(dbContext, conflicts, concurrencyConflict, cancellationToken);
    }

    private static async Task<Result> SaveAsync(
        IAppDbContext dbContext,
        IReadOnlyList<UniqueConflict> conflicts,
        Error? concurrencyConflict,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (UniqueConstraintException exception) when (Match(conflicts, exception) is { } expected)
        {
            return expected.Error;
        }
        catch (DbUpdateConcurrencyException) when (concurrencyConflict is not null)
        {
            return concurrencyConflict;
        }
    }

    private static UniqueConflict? Match(
        IReadOnlyList<UniqueConflict> conflicts, UniqueConstraintException exception)
    {
        foreach (UniqueConflict conflict in conflicts)
        {
            if (string.Equals(conflict.ConstraintName, exception.ConstraintName, StringComparison.Ordinal))
                return conflict;
        }

        return null;
    }
}
