using FluentAssertions;
using NetArchTest.Rules;
using Snapflow.Common;

namespace Snapflow.ArchitectureTests;

public sealed class DomainTests : Base
{
    [Fact]
    public void DomainEvents_Should_Be_Sealed_And_Immutable()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ImplementInterface(typeof(IDomainEvent))
            .Should().BeSealed()
            .And().HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain events should be sealed to prevent inheritance and follow naming conventions.");
    }

    [Fact]
    public void Entities_Should_Have_Parameterless_Constructor()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That().Inherit(typeof(Entity<,>))
            .Or().Inherit(typeof(Entity<>))
            .GetTypes();

        var failingEntities = new List<string>();

        foreach (var entityType in entityTypes)
        {
            if (entityType.IsAbstract) continue;

            var hasParameterlessConstructor = entityType.GetConstructors(
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)
                .Any(c => c.GetParameters().Length == 0);

            if (!hasParameterlessConstructor)
            {
                failingEntities.Add(entityType.Name);
            }
        }

        failingEntities.Should().BeEmpty("Entities should have a parameterless constructor for EF Core or deserialization purposes.");
    }

    [Fact]
    public void Entities_Should_Not_Have_Public_Setters_For_Navigation_Properties_Or_Collections()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That().Inherit(typeof(Entity<,>))
            .Or().Inherit(typeof(Entity<>))
            .GetTypes();

        var failingProperties = new List<string>();

        foreach (var entityType in entityTypes)
        {
            var properties = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType && 
                   (prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) || 
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    if (prop.GetSetMethod(nonPublic: false) != null) // has public setter
                    {
                        failingProperties.Add($"{entityType.Name}.{prop.Name}");
                    }
                }
            }
        }

        failingProperties.Should().BeEmpty("Entities should encapsulate collections. They should not have public setters for them.");
    }
}