using Microsoft.AspNetCore.Mvc;
using Snapflow.Common;

namespace Snapflow.Presentation.Infrastructure;

public class ValidationProblemDetails : ProblemDetails
{
    public PropertyValidationError[] Errors { get; set; } = Array.Empty<PropertyValidationError>();
}
