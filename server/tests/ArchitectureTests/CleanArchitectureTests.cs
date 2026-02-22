using FluentAssertions;
using NetArchTest.Rules;

namespace Snapflow.ArchitectureTests;

public sealed class CleanArchitectureTests : Base
{
    [Fact]
    public void DomainLayer_Should_NotHaveDependencyOn_Application()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Interfaces_Should_StartWith_I()
    {
        var assemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };
        var failingInterfaces = new List<string>();

        foreach (var assembly in assemblies)
        {
            var interfaces = Types.InAssembly(assembly).That().AreInterfaces().GetTypes();
            foreach (var type in interfaces)
            {
                if (!type.Name.StartsWith('I') || (type.Name.Length > 1 && !char.IsUpper(type.Name[1])))
                {
                    failingInterfaces.Add(type.Name);
                }
            }
        }

        failingInterfaces.Should().BeEmpty("All interfaces in the solution should start with the letter 'I' followed by an uppercase letter.");
    }
}
