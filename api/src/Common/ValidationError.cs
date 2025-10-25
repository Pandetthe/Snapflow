namespace Snapflow.Common;

public sealed record ValidationError : Error
{
    public ValidationError(PropertyValidationError[] propertyValidationErrors)
        : base(
            "Validation.General",
            "One or more validation errors occurred.",
            ErrorType.Validation)
    {
        Errors = propertyValidationErrors;
    }

    public PropertyValidationError[] Errors { get; }
}

public sealed record PropertyValidationError(string? PropertyName, string Code, string Description);