using Microsoft.AspNetCore.Mvc;
using Snapflow.Common;

namespace Snapflow.Presentation.Contracts;

public sealed class ValidationProblemDetails : ProblemDetails
{
    public PropertyValidationError[] Errors { get; set; } = Array.Empty<PropertyValidationError>();
}
