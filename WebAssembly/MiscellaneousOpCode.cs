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
    /// Copy from a passive data segment to linear memory
    /// </summary>
    [OpCodeCharacteristics("memory.init")]
    MemoryInit = 0x08,

    /// <summary>
    /// Prevent further use of passive data segment
    /// </summary>
    [OpCodeCharacteristics("data.drop")]
    DataDrop = 0x09,

    /// <summary>
    /// Copy from one region of linear memory to another region
    /// </summary>
    [OpCodeCharacteristics("memory.copy")]
    MemoryCopy = 0x0A,

    /// <summary>
    /// Fill a region of linear memory with a given byte value
    /// </summary>
    [OpCodeCharacteristics("memory.fill")]
    MemoryFill = 0x0B,

    /// <summary>
    /// Copy from a passive element segment to a table
    /// </summary>
    [OpCodeCharacteristics("table.init")]
    TableInit = 0x0C,

    /// <summary>
    /// Prevent further use of a passive element segment
    /// </summary>
    [OpCodeCharacteristics("elem.drop")]
    ElemDrop = 0x0D,

    /// <summary>
    /// Copy from one region of a table to another region
    /// </summary>
    [OpCodeCharacteristics("table.copy")]
    TableCopy = 0x0E,

    /// <summary>
    /// Manipulate the size of a table
    /// </summary>
    [OpCodeCharacteristics("table.grow")]
    TableGrow = 0x0F,

    /// <summary>
    /// Manipulate the size of a table
    /// </summary>
    [OpCodeCharacteristics("table.size")]
    TableSize = 0x10,

    /// <summary>
    /// Fill a range in a table with a value
    /// </summary>
    [OpCodeCharacteristics("table.fill")]
    TableFill = 0x11
}

static class MiscellaneousOpCodeExtensions
{
    private static readonly RegeneratingWeakReference<Dictionary<MiscellaneousOpCode, string>> opCodeNativeNamesByOpCode = new(
        () => typeof(MiscellaneousOpCode)
            .GetFields()
            .Where(field => field.IsStatic)
            .Select(field => new KeyValuePair<MiscellaneousOpCode, string>((MiscellaneousOpCode)field.GetValue(null)!, field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
    );

    public static string ToNativeName(this MiscellaneousOpCode opCode)
    {
        opCodeNativeNamesByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }
}
