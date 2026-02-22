namespace Snapflow.Domain.Ranking;

public interface IRankable
{
    int Id { get; }
    string Rank { get; set; }
    bool IsDeleted { get; }
}
