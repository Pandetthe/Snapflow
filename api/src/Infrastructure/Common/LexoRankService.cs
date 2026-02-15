using Snapflow.Application.Abstractions.Behaviours;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Snapflow.Infrastructure.Common;

public class LexoRankService : IRankService
{
    public const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
    public static readonly int Base = Alphabet.Length;
    public const int Length = 12;

    public string Minimum => Pad(BigInteger.Zero);
    public string Maximum => Pad(BigInteger.Pow(Base, Length) - 1);

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

    public List<string> GenerateBalanced(int count)
    {
        if (count <= 0) return [];

        var min = Parse(Minimum);
        var max = Parse(Maximum);
        var span = max - min;
        if (span <= count)
            throw new InvalidOperationException("Not enough space to rebalance.");

        var step = span / (count + 1);
        List<string> results = new(count);
        for (int i = 0; i < count; i++)
        {
            var value = min + step * (i + 1);
            results.Add(Pad(value));
        }
        return results;
    }

    public List<string> GenerateBalancedBetween(int count, string? left, string? right)
    {
        if (count <= 0)
            return new List<string>();

        BigInteger leftVal = left != null ? Parse(left) : Parse(Minimum);
        BigInteger rightVal = right != null ? Parse(right) : Parse(Maximum);

        if (leftVal >= rightVal)
            throw new InvalidOperationException("Left bound must be strictly less than right bound.");

        var span = rightVal - leftVal;
        if (span <= count)
            throw new InvalidOperationException("Not enough space to rebalance between bounds.");

        var step = span / (count + 1);

        var results = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            var value = leftVal + step * (i + 1);
            results.Add(Pad(value));
        }

        return results;
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