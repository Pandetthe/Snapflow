using Snapflow.Common;

namespace Snapflow.Api.Infrastructure;

public static class ResultHelper
{
    public static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Code,
            ErrorType.Unauthorized => error.Code,
            ErrorType.Problem => error.Code,
            ErrorType.NotFound => error.Code,
            ErrorType.Conflict => error.Code,
            _ => "Server failure"
        };

    public static string GetDetail(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Description,
            ErrorType.Unauthorized => error.Description,
            ErrorType.Problem => error.Description,
            ErrorType.NotFound => error.Description,
            ErrorType.Conflict => error.Description,
            _ => "An unexpected error occurred"
        };

    public static string GetType(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

    public static int GetStatusCode(Error error) =>
        error.Type switch
        {
            ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    public static Dictionary<string, object?>? GetErrors(Result result)
    {
        if (result.Error is not ValidationError validationError)
            return null;

        return new Dictionary<string, object?>
            {
                { "errors", validationError.Errors }
            };
    }
}
