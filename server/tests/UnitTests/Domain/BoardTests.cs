using FluentAssertions;
using Snapflow.Domain.Boards;

namespace Snapflow.UnitTests.Domain;

public sealed class BoardTests
{
    [Fact]
    public void Board_Should_InitializeWithEmptyCollections()
    {
        var board = new Board
        {
            Title = "Test Board",
            CreatedAt = DateTime.Now,
            CreatedById = 1
        };
        
        board.Members.Should().BeEmpty();
        board.Swimlanes.Should().BeEmpty();
    }
}
