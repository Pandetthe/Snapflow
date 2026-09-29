using System.Diagnostics.CodeAnalysis;

namespace Snapflow.Application.Abstractions.Ranking;

public interface IRankService
{
    string GenerateInitial();

    bool TryGenerateBetween(string? left, string? right, [NotNullWhen(true)] out string? newRank);

    bool TryGenerateBalanced(int count, [NotNullWhen(true)] out IReadOnlyList<string>? ranks);

    bool TryGenerateBalancedBetween(int count, string left, string right, [NotNullWhen(true)] out IReadOnlyList<string>? ranks);
}
