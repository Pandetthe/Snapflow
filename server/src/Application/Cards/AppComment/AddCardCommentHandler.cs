using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Boards.GetById;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Application.Cards.AddComment;

internal sealed class AddCardCommentCommandHandler(
    IAppDbContext context) : ICommandHandler<AddCardCommentCommand, GetBoardByIdResponse.CardCommentDto> // <--- ZMIANA 1: Zwracamy DTO zamiast int
{
    public async Task<Result<GetBoardByIdResponse.CardCommentDto>> Handle(AddCardCommentCommand request, CancellationToken cancellationToken = default)
    {
        var card = await context.Cards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && !c.IsDeleted, cancellationToken);

        if (card is null)
        {
            return Result.Failure<GetBoardByIdResponse.CardCommentDto>(CardErrors.NotFound(request.CardId));
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetBoardByIdResponse.CardCommentDto>(new Error("User.NotFound", "User not found", ErrorType.NotFound));
        }

        var comment = CardComment.Create(request.CardId, request.UserId, request.Content);

        context.CardComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        var dto = new GetBoardByIdResponse.CardCommentDto(
            comment.Id,
            user.Id,
            user.UserName ?? "Unknown",
            comment.Content,
            comment.CreatedAt
        );

        return dto;
    }
}