using FluentAssertions;
using NetArchTest.Rules;
using Snapflow.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Snapflow.ArchitectureTests;

public sealed class CqrsTests : Base
{
    [Fact]
    public void CommandHandlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("CommandHandler")
            .Should().NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("CommandHandlers should be internal to prevent direct instantiation.");
    }

    [Fact]
    public void QueryHandlers_Should_Be_Internal()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("QueryHandler")
            .Should().NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandlers_Should_Be_Internal_And_EndWith_Handler()
    {
        // Sprawdzamy oba typy ICommandHandler (z rezultatem i bez)
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Should().NotBePublic()
            .And().HaveNameEndingWith("CommandHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Command handlers should be internal to encapsulate the slice logic.");
    }

    [Fact]
    public void QueryHandlers_Should_Be_Internal_And_EndWith_Handler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(IQueryHandler<,>))
            .Should().NotBePublic()
            .And().HaveNameEndingWith("QueryHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
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
}
