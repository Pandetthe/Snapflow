using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Infrastructure.Authorization;
using System.Diagnostics.CodeAnalysis;

internal sealed class BoardPermissionAuthorizationHandler(
    PermissionProvider permissionProvider,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BoardPermissionAuthorizationHandler> logger)
    : AuthorizationHandler<BoardPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BoardPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;
        if (!TryGetBoardId(context, out int boardId))
        {
            logger.LogWarning("Authorization failed: Missing or invalid boardId.");
            return;
        }

        int userId = userContext.UserId;
        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId, boardId);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }

    private bool TryGetBoardId(AuthorizationHandlerContext context, out int boardId)
    {
        boardId = 0;
        HttpContext? httpContext;

        if (context.Resource is HubInvocationContext hubContext)
        {
            httpContext = hubContext.Context.GetHttpContext();
        }
        else if (context.Resource is HttpContext resourceHttpContext)
        {
            httpContext = resourceHttpContext;
        }
        else
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        if (httpContext == null) 
            return false;

        return httpContext.Request.RouteValues.TryGetValue("boardId", out var routeVal) &&
            int.TryParse(routeVal?.ToString(), out boardId);
    }
}