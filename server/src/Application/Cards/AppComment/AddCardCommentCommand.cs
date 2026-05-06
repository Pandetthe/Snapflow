using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetById;

namespace Snapflow.Application.Cards.AddComment;

public sealed record AddCardCommentCommand(int CardId, int UserId, string Content)
    : ICommand<GetBoardByIdResponse.CardCommentDto>;