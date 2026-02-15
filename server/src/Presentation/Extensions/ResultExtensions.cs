using Snapflow.Common;

namespace Snapflow.Presentation.Extensions;

public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    public static TOut Match<TOut>(
        this Result result,
        TOut onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess : onFailure(result);
    }

    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        TOut onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure;
    }

    public static TOut Match<TOut>(
        this Result result,
        TOut onSuccess,
        TOut onFailure)
    {
        return result.IsSuccess ? onSuccess : onFailure;
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        TOut onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess : onFailure(result);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        TOut onSuccess,
        TOut onFailure)
    {
        return result.IsSuccess ? onSuccess : onFailure;
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<Result<TIn>, TOut> onSuccess,
        TOut onFailure)
    {
        return result.IsSuccess ? onSuccess(result) : onFailure;
    }
}
