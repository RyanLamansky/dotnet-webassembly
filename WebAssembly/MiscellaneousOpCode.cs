using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebAssembly
{
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
    }

    static class MiscellaneousOpCodeExtensions
    {
        private static readonly RegeneratingWeakReference<Dictionary<MiscellaneousOpCode, string>> opCodeNativeNamesByOpCode = new RegeneratingWeakReference<Dictionary<MiscellaneousOpCode, string>>(
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
}