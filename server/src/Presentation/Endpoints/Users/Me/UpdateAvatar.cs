using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.UpdateAvatar;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Avatars;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class UpdateAvatar : IEndpoint
{
    public sealed class UpdateAvatarRequest
    {
        public AvatarType AvatarType { get; init; }
        public IFormFile? File { get; init; }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("me/avatar", async (
            [FromForm] UpdateAvatarRequest request,
            IOptions<AvatarOptions> avatarOptions,
            ICommandHandler<UpdateAvatarCommand> handler,
            CancellationToken cancellationToken) =>
        {
            byte[]? avatarData = null;
            string? contentType = null;

            if (request.AvatarType == AvatarType.Uploaded)
            {
                if (request.File == null)
                    return Results.Problem(Result.Failure(UserErrors.AvatarFileRequired));

                var maxBytes = avatarOptions.Value.MaxFileSize * 1024 * 1024;
                if (request.File.Length > maxBytes)
                    return Results.Problem(Result.Failure(UserErrors.AvatarFileTooLarge(avatarOptions.Value.MaxFileSize)));

                var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (!avatarOptions.Value.AllowedExtensions.Contains(ext))
                    return Results.Problem(Result.Failure(UserErrors.AvatarInvalidFileType));

                contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                using var ms = new MemoryStream();
                await request.File.CopyToAsync(ms, cancellationToken);
                avatarData = ms.ToArray();
            }

            var command = new UpdateAvatarCommand(request.AvatarType, avatarData, contentType);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem();
    }
}
