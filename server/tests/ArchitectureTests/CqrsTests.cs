using FluentAssertions;
using NetArchTest.Rules;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.ArchitectureTests;

public sealed class CqrsTests : Base
{
    private static readonly string AbstractionsNamespace = $"{ApplicationNamespace}.Abstractions";

    [Fact]
    public void CommandHandlers_Should_Be_Internal_And_EndWith_Handler()
    {
        var failing = SliceHandlers(typeof(ICommandHandler<>), typeof(ICommandHandler<,>))
            .Where(handler => handler.IsPublic || !handler.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Select(handler => handler.FullName ?? handler.Name)
            .ToList();

        failing.Should().BeEmpty("command handlers should be internal and end with 'Handler'");
    }

    [Fact]
    public void QueryHandlers_Should_Be_Internal_And_EndWith_Handler()
    {
        var failing = SliceHandlers(typeof(IQueryHandler<,>))
            .Where(handler => handler.IsPublic || !handler.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Select(handler => handler.FullName ?? handler.Name)
            .ToList();

        failing.Should().BeEmpty("query handlers should be internal and end with 'Handler'");
    }

    [Fact]
    public void EveryCommand_Should_HaveExactlyOneHandler()
    {
        var handledCommands = SliceHandlers(typeof(ICommandHandler<>), typeof(ICommandHandler<,>))
            .SelectMany(handler => handler.GetInterfaces())
            .Where(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                 i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .Select(i => i.GetGenericArguments()[0])
            .ToList();

        var failing = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .GetTypes()
            .Where(command => handledCommands.Count(handled => handled == command) != 1)
            .Select(command => command.Name)
            .ToList();

        failing.Should().BeEmpty("every command should have exactly one handler");
    }

    [Fact]
    public void Handlers_Should_Reside_In_Same_Namespace_As_Messaging_Models()
    {
        var handlerTypes = Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        var failingHandlers = new List<string>();

        foreach (var handler in handlerTypes)
        {
            var handlerInterface = handler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

            if (handlerInterface != null)
            {
                var messagingModelType = handlerInterface.GetGenericArguments()[0];

                if (handler.Namespace != messagingModelType.Namespace)
                {
                    failingHandlers.Add($"{handler.Name} (expected namespace: {messagingModelType.Namespace})");
                }
            }
        }

        failingHandlers.Should().BeEmpty("Handlers should be located in the same namespace as their respective Commands or Queries.");
    }

    [Fact]
    public void Commands_Should_Be_Sealed_Records()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(ICommand))
            .Or().ImplementInterface(typeof(ICommand<>))
            .Should().BeClasses() // Records are classes under the hood
            .And().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Commands should be sealed records to ensure immutability.");
    }

    [Fact]
    public void Queries_Should_Be_Sealed_Records()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(IQuery<>))
            .Should().BeClasses()
            .And().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Queries should be sealed records to ensure immutability.");
    }

    private static IEnumerable<Type> SliceHandlers(params Type[] handlerInterfaces) =>
        ApplicationAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.Namespace?.StartsWith(AbstractionsNamespace, StringComparison.Ordinal) != true)
            .Where(type => type.GetInterfaces().Any(i =>
                i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())));
}
