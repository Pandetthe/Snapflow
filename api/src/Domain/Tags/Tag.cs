using Snapflow.Common;

namespace Snapflow.Domain.Tags;

public class Tag : Entity<int>
{
    public required string Title { get; set; }
}
