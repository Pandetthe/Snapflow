using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Persistence;

public static class TransactionExtensions
{
    extension(IAppDbContext dbContext)
    {
        public Task<Result> InTransactionAsync(
            Func<Task<Result>> work,
            CancellationToken cancellationToken) =>
            RunAsync(dbContext, work, cancellationToken);

        public Task<Result<TValue>> InTransactionAsync<TValue>(
            Func<Task<Result<TValue>>> work,
            CancellationToken cancellationToken) =>
            RunAsync(dbContext, work, cancellationToken);
    }

    private static Task<TResult> RunAsync<TResult>(
        IAppDbContext dbContext,
        Func<Task<TResult>> work,
        CancellationToken cancellationToken)
        where TResult : Result
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();

            await using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            TResult result = await work();

            if (result.IsSuccess)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);

            return result;
        });
    }
}
