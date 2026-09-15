using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Logins.GetAll;

public sealed record GetLoginsQuery : IQuery<IReadOnlyList<ExternalLoginDetails>>;
