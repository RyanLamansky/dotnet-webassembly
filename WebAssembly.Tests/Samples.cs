using System;
using System.Linq;
using System.Text;

namespace WebAssembly;

/// <summary>
/// Provides diverse sample data values.
/// </summary>
static class Samples
{
    public static int[] Int32 =>
    [
            -1,
            0,
            1,
            0x00,
            0x0F,
            0xF0,
            0xFF,
            byte.MaxValue,
            short.MinValue,
            short.MaxValue,
            ushort.MaxValue,
            int.MinValue,
            int.MaxValue,
    ];

    public static uint[] UInt32 =>
    [
            0,
            1,
            0x00,
            0x0F,
            0xF0,
            0xFF,
            byte.MaxValue,
            ushort.MaxValue,
            int.MaxValue,
            uint.MaxValue,
    ];

    public static long[] Int64 =>
    [
            -1,
            0,
            1,
            0x00,
            0x0F,
            0xF0,
            0xFF,
            byte.MaxValue,
            short.MinValue,
            short.MaxValue,
            ushort.MaxValue,
            int.MinValue,
            int.MaxValue,
            uint.MaxValue,
            long.MinValue,
            long.MaxValue,
    ];

    public static ulong[] UInt64 =>
    [
            0,
            1,
            0x00,
            0x0F,
            0xF0,
            0xFF,
            byte.MaxValue,
            ushort.MaxValue,
            int.MaxValue,
            uint.MaxValue,
            long.MaxValue,
            ulong.MaxValue,
    ];

    public static float[] Single =>
    [
            0.0f,
            1.0f,
            -1.0f,
            -(float)Math.PI,
            (float)Math.PI,
            float.NaN,
            float.NegativeInfinity,
            float.PositiveInfinity,
            float.Epsilon,
            -float.Epsilon,
    ];

    public static double[] Double =>
    [
            0.0,
            1.0,
            -1.0,
            -(float)Math.PI,
            (float)Math.PI,
            float.Epsilon,
            -float.Epsilon,
            -Math.PI,
            Math.PI,
            double.NaN,
            double.NegativeInfinity,
            double.PositiveInfinity,
            double.Epsilon,
            -double.Epsilon,
        ];

    public static byte[] Memory => [254, 2, 3, 4, 5, 6, 7, 8, .. Encoding.Unicode.GetBytes("🐩")]
        ;
}
