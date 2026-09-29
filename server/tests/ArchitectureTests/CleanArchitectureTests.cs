using NetArchTest.Rules;
using System.Reflection;

namespace Snapflow.ArchitectureTests;

public sealed class CleanArchitectureTests : Base
{
    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void EachLayer_Should_BindToItsOwnAssemblyAndNamespace()
    {
        var layers = new[]
        {
            (ExpectedAssembly: "Domain", Assembly: DomainAssembly, Namespace: (string?)null),
            (ExpectedAssembly: "Application", Assembly: ApplicationAssembly, Namespace: ApplicationNamespace),
            (ExpectedAssembly: "Infrastructure", Assembly: InfrastructureAssembly, Namespace: InfrastructureNamespace),
            (ExpectedAssembly: "Presentation", Assembly: PresentationAssembly, Namespace: PresentationNamespace)
        };

        foreach ((string expectedAssembly, Assembly assembly, string? layerNamespace) in layers)
        {
            Assert.Equal(expectedAssembly, assembly.GetName().Name);

            if (layerNamespace is not null)
                Assert.Equal($"Snapflow.{expectedAssembly}", layerNamespace);
        }
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

        Assert.True(failingInterfaces.Count == 0, $"All interfaces in the solution should start with the letter 'I' followed by an uppercase letter. {string.Join(", ", failingInterfaces)}");
    }

    private static string Describe(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
