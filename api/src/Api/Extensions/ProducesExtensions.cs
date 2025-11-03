using Microsoft.AspNetCore.Mvc;
using Snapflow.Api.Infrastructure;

namespace Snapflow.Api.Extensions;

public static class ProducesExtensions
{
    public static RouteHandlerBuilder ProducesIdResponse<T>(
        this RouteHandlerBuilder builder,
        int statusCode = 200)
    {
        return builder.Produces<IdResponse<T>>(statusCode);
    }

    public static RouteHandlerBuilder ProducesIdResponse(
        this RouteHandlerBuilder builder,
        int statusCode = 200)
    {
        return builder.ProducesIdResponse<int>(statusCode);
    }


    public static RouteHandlerBuilder ProducesCustomValidationProblem(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return builder.Produces<Infrastructure.ValidationProblemDetails>(statusCode, "application/problem+json");
    }

    public static RouteHandlerBuilder ProducesInternalServerError(
        this RouteHandlerBuilder builder)
    {
        return builder.ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
