using Snapflow.Application.Abstractions.Ranking;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Snapflow.Infrastructure.Common;

public class LexoRankService : IRankService
{
    public const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
    public static readonly int Base = Alphabet.Length;
    public const int Length = 12;

    private static string Minimum => Pad(BigInteger.Zero);
    private static string Maximum => Pad(BigInteger.Pow(Base, Length) - 1);

    public string GenerateInitial()
    {
        var mid = BigInteger.Pow(Base, Length) / 2;
        return Pad(mid);
    }

    public bool TryGenerateBetween(string? left, string? right, [NotNullWhen(true)] out string? newRank)
    {
        if (string.IsNullOrEmpty(left)) left = Minimum;
        if (string.IsNullOrEmpty(right)) right = Maximum;

        newRank = null;

        var leftInt = Parse(left);
        var rightInt = Parse(right);

        if (leftInt >= rightInt)
            return false;

        var mid = (leftInt + rightInt) / 2;
        if (mid == leftInt || mid == rightInt)
            return false;

        newRank = Pad(mid);
        return true;
    }

    public bool TryGenerateBalanced(int count, [NotNullWhen(true)] out IReadOnlyList<string>? ranks) =>
        TrySpread(count, Parse(Minimum), Parse(Maximum), out ranks);

    public bool TryGenerateBalancedBetween(
        int count, string left, string right, [NotNullWhen(true)] out IReadOnlyList<string>? ranks) =>
        TrySpread(count, Parse(left), Parse(right), out ranks);

    private static bool TrySpread(
        int count, BigInteger from, BigInteger to, [NotNullWhen(true)] out IReadOnlyList<string>? ranks)
    {
        ranks = null;
        if (count < 0 || from >= to)
            return false;

        var span = to - from;
        if (span <= count)
            return false;

        var step = span / (count + 1);
        var results = new List<string>(count);
        for (int i = 0; i < count; i++)
            results.Add(Pad(from + step * (i + 1)));

        ranks = results;
        return true;
    }

    private static BigInteger Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            return BigInteger.Zero;
        BigInteger result = 0;
        foreach (var c in s)
        {
            var idx = Alphabet.IndexOf(c);
            if (idx < 0)
                throw new FormatException($"Invalid rank character '{c}'.");
            result = result * Base + idx;
        }
        return result;
    }

    private static string Pad(BigInteger value)
    {
        var sb = new StringBuilder();
        var v = value;
        while (v > 0)
        {
            var rem = (int)(v % Base);
            sb.Insert(0, Alphabet[rem]);
            v /= Base;
        }
        while (sb.Length < Length)
            sb.Insert(0, Alphabet[0]);
        if (sb.Length > Length)
            throw new InvalidOperationException("Value overflows rank length");
        return sb.ToString();
    }
}