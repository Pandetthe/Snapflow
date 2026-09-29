using FluentValidation;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.ArchitectureTests;

public sealed class ValidationTests : Base
{
    private static readonly HashSet<string> CommandsWithoutUserInput =
    [
        "AddLoginCommand",
        "AddTagToCardCommand",
        "ChangeOwnerCommand",
        "CreatePasskeyOptionsCommand",
        "DeleteAccountCommand",
        "DeleteBoardCommand",
        "DeleteCardCommand",
        "DeleteListCommand",
        "DeleteSwimlaneCommand",
        "DeleteTagCommand",
        "ExternalSignInCommand",
        "PasskeySignInOptionsCommand",
        "RefreshCommand",
        "RemoveMemberCommand",
        "RemoveTagFromCardCommand",
        "SetupAuthenticatorCommand",
        "SignOutCommand"
    ];

    [Fact]
    public void Commands_Should_Have_Validators()
    {
        var validatedTypes = ApplicationAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var failing = Commands()
            .Where(command => !CommandsWithoutUserInput.Contains(command.Name) && !validatedTypes.Contains(command))
            .Select(command => command.Name)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.True(failing.Count == 0, $"every command taking user input should have a validator {string.Join(", ", failing)}");
    }

    [Fact]
    public void ValidatorExemptions_Should_NotOutliveTheirCommands()
    {
        var commandNames = Commands().Select(command => command.Name).ToHashSet(StringComparer.Ordinal);

        Assert.True(!CommandsWithoutUserInput.Where(name => !commandNames.Contains(name)) .Any(), $"exemptions for commands that no longer exist should be dropped {string.Join(", ", CommandsWithoutUserInput.Where(name => !commandNames.Contains(name)) )}");
    }

    private static IEnumerable<Type> Commands() =>
        ApplicationAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.GetInterfaces().Any(i =>
                i == typeof(ICommand) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))));
}
