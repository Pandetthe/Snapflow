namespace Snapflow.Application.Abstractions.Identity;

public interface IAuthenticationSettings
{
    bool PasswordAuthenticationEnabled { get; }

    bool ExternalSignUpEnabled { get; }
}
