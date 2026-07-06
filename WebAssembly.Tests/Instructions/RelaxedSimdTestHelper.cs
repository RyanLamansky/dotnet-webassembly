using System;

namespace WebAssembly.Instructions;

/// <summary>Helpers for packing typed lanes into the 16 bytes of a v128 literal in relaxed-SIMD tests.</summary>
internal static class RelaxedSimdTestHelper
{
    public static byte[] Bytes(params sbyte[] lanes)
    {
        var result = new byte[16];
        for (var i = 0; i < lanes.Length; i++)
            result[i] = unchecked((byte)lanes[i]);
        return result;
    }

    public static byte[] Bytes(params short[] lanes)
    {
        var result = new byte[16];
        for (var i = 0; i < lanes.Length; i++)
            BitConverter.GetBytes(lanes[i]).CopyTo(result, i * 2);
        return result;
    }

    public static byte[] Bytes(params int[] lanes)
    {
        var result = new byte[16];
        for (var i = 0; i < lanes.Length; i++)
            BitConverter.GetBytes(lanes[i]).CopyTo(result, i * 4);
        return result;
    }

    public static byte[] Bytes(params float[] lanes)
    {
        var result = new byte[16];
        for (var i = 0; i < lanes.Length; i++)
            BitConverter.GetBytes(lanes[i]).CopyTo(result, i * 4);
        return result;
    }

    public static byte[] Bytes(params double[] lanes)
    {
        var result = new byte[16];
        for (var i = 0; i < lanes.Length; i++)
            BitConverter.GetBytes(lanes[i]).CopyTo(result, i * 8);
        return result;
    }
}
