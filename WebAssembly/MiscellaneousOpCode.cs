using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebAssembly;

/// <summary>
/// Binary miscellaneous operation values.
/// </summary>
public enum MiscellaneousOpCode : byte
{
    /// <summary>
    /// Truncate (with saturation) a 32-bit float to a signed 32-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i32.trunc_sat_f32_s")]
    Int32TruncateSaturateFloat32Signed = 0x00,

    /// <summary>
    /// Truncate (with saturation) a 32-bit float to an unsigned 32-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i32.trunc_sat_f32_u")]
    Int32TruncateSaturateFloat32Unsigned = 0x01,

    /// <summary>
    /// Truncate (with saturation) a 64-bit float to a signed 32-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i32.trunc_sat_f64_s")]
    Int32TruncateSaturateFloat64Signed = 0x02,

    /// <summary>
    /// Truncate (with saturation) a 64-bit float to an unsigned 32-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i32.trunc_sat_f64_u")]
    Int32TruncateSaturateFloat64Unsigned = 0x03,

    /// <summary>
    /// Truncate (with saturation) a 32-bit float to a signed 64-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i64.trunc_sat_f32_s")]
    Int64TruncateSaturateFloat32Signed = 0x04,

    /// <summary>
    /// Truncate (with saturation) a 32-bit float to an unsigned 64-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i64.trunc_sat_f32_u")]
    Int64TruncateSaturateFloat32Unsigned = 0x05,

    /// <summary>
    /// Truncate (with saturation) a 64-bit float to a signed 64-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i64.trunc_sat_f64_s")]
    Int64TruncateSaturateFloat64Signed = 0x06,

    /// <summary>
    /// Truncate (with saturation) a 64-bit float to an unsigned 64-bit integer.
    /// </summary>
    [OpCodeCharacteristics("i64.trunc_sat_f64_u")]
    Int64TruncateSaturateFloat64Unsigned = 0x07,

    /// <summary>
    /// Initialize memory.
    /// </summary>
    [OpCodeCharacteristics("memory.init")]
    MemoryInitialize = 0x08,

    /// <summary>
    /// Drop a data element to release memory.
    /// </summary>
    [OpCodeCharacteristics("data.drop")]
    DataDrop = 0x09,

    /// <summary>
    /// Copy bytes from one area in memory to another.
    /// </summary>
    [OpCodeCharacteristics("memory.copy")]
    MemoryCopy = 0x0A,

    /// <summary>
    /// Fills memory bytes to a specific value.
    /// </summary>
    [OpCodeCharacteristics("memory.fill")]
    MemoryFill = 0x0B,
}

static class MiscellaneousOpCodeExtensions
{
    private static readonly RegeneratingWeakReference<Dictionary<MiscellaneousOpCode, string>> opCodeNativeNamesByOpCode = new(
        () => typeof(MiscellaneousOpCode)
            .GetFields()
            .Where(field => field.IsStatic)
            .ToDictionary(field => (MiscellaneousOpCode)field.GetValue(null)!, field => field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name)
    );

    public static string ToNativeName(this MiscellaneousOpCode opCode)
        => opCodeNativeNamesByOpCode.Reference[opCode];
}
