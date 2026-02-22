using FluentAssertions;
using NetArchTest.Rules;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.ArchitectureTests;

public sealed class VerticalSliceTests : Base
{
    [Fact]
    public void VerticalSlices_ShouldNot_Depend_On_Other_Slices()
    {
        var slices = new[] { "Boards", "Cards", "Lists", "Swimlanes", "Users", "Auth", "Members", "Ranking" };

        var failingSlices = new List<string>();

        foreach (var slice in slices)
        {
            var otherSlices = slices.Where(s => s != slice).Select(s => $"Snapflow.Application.{s}").ToArray();

            var result = Types.InAssembly(ApplicationAssembly)
                .That().ResideInNamespace($"Snapflow.Application.{slice}")
                .Should().NotHaveDependencyOnAny(otherSlices)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failingSlices.Add(slice);
            }
        }

        failingSlices.Should().BeEmpty("Vertical Slices in Application layer should be isolated from each other. Use Domain Events for cross-slice communication.");
    }
}
