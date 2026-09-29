namespace Snapflow.Common;

public interface ICascadeSoftDeletable : ISoftDeletable
{
    int? DeletedById { get; }

    DateTimeOffset? DeletedAt { get; }

    bool DeletedByCascade { get; }
}
