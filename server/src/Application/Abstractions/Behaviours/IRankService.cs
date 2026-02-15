using System.Diagnostics.CodeAnalysis;

namespace Snapflow.Application.Abstractions.Behaviours;

public interface IRankService
{
    string Minimum { get; }

    string Maximum { get; }

    string GenerateInitial();

    bool TryGenerateBetween(string? left, string? right, [NotNullWhen(true)] out string? newRank);

    List<string> GenerateBalanced(int count);

    List<string> GenerateBalancedBetween(int count, string? left, string? right);
}
