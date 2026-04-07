using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.GetDetails;

public sealed record GetBoardDetailsQuery(int Id) : IQuery<GetBoardDetailsResponse>;
