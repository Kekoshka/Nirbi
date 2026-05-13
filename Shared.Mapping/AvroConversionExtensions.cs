using System.Numerics;

namespace Shared.Mapping;

public static class AvroConversionExtensions
{
    // ── DateTime ↔ long ────────────────────────────────────────────────────
    public static long ToUnixMillis(this DateTime dt) =>
       new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds();

    public static DateTime ToDateTime(this long unixMillis) =>
        DateTimeOffset.FromUnixTimeMilliseconds(unixMillis).UtcDateTime;

    // ── decimal ↔ byte[] ───────────────────────────────────────────────────

    public static byte[] ToAvroDecimal(this decimal value, int scale = 10)
    {
        var unscaled = new BigInteger(decimal.Round(value * (decimal)Math.Pow(10, scale)));
        return unscaled.ToByteArray(isBigEndian: true);
    }

    public static decimal ToDecimal(this byte[] bytes, int scale = 10)
    {
        var unscaled = new BigInteger(bytes, isBigEndian: true);
        return (decimal)unscaled / (decimal)Math.Pow(10, scale);
    }
}