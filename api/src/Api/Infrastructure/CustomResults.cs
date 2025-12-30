using Snapflow.Common;

namespace Snapflow.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot create a problem result for a successful operation.");

        return Results.Problem(
            title: ResultHelper.GetTitle(result.Error),
            detail: ResultHelper.GetDetail(result.Error),
            type: ResultHelper.GetType(result.Error),
            statusCode: ResultHelper.GetStatusCode(result.Error),
            extensions: ResultHelper.GetErrors(result));
    }

    public static IResult OkWithId<T>(T id)
    {
        return Results.Ok(new IdResponse<T>(id));
    }

    public static IResult OkWithRank(string rank)
    {
        return Results.Ok(new RankResponse(rank));
    }
}
