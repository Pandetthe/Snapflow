namespace Snapflow.Common;

public interface ISoftDeletable
{
    const string FilterName = "SoftDelete";

    bool IsDeleted { get; }
}
