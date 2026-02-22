using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;
using Snapflow.Application.Abstractions.Messaging;
using System.Reflection;

namespace Snapflow.ArchitectureTests;

public sealed class ValidationTests : Base
{
    [Fact]
    public void Commands_Should_Have_Validators()
    {
        var commandTypes = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .GetTypes();

        var validatorTypes = Types.InAssembly(ApplicationAssembly)
            .That().Inherit(typeof(AbstractValidator<>))
            .GetTypes();

        var failingCommands = new List<string>();

        foreach (var commandType in commandTypes)
        {
            var hasValidator = validatorTypes.Any(v => 
                v.BaseType != null && 
                v.BaseType.IsGenericType && 
                v.BaseType.GetGenericArguments().Contains(commandType));

            if (!hasValidator)
            {
                failingCommands.Add(commandType.Name);
            }
        }

        failingCommands.Should().BeEmpty("All Commands should have a corresponding AbstractValidator<T> to ensure data integrity before processing.");
    }
}
