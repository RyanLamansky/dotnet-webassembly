using System.Linq;

namespace WebAssembly;

static class HashCode
{
    public static int Combine(this System.Collections.Generic.IEnumerable<int?> hashCodes)
        => Combine(hashCodes.Select(code => code.GetValueOrDefault()));

    public static int Combine(this System.Collections.Generic.IEnumerable<int> hashCodes)
    {
        var hash1 = (5381 << 16) + 5381;
        var hash2 = hash1;

        var i = 0;
        foreach (var hashCode in hashCodes)
        {
            if (i % 2 == 0)
                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
            else
                hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

            ++i;
        }

        return hash1 + (hash2 * 1566083941);
    }

    public static int Combine(int h1, int h2) => (((h1 << 5) + h1) ^ h2);

    public static int Combine(int h1, int h2, int h3) => Combine(Combine(h1, h2), h3);
}
