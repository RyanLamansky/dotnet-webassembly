using System.Collections.Generic;

namespace WebAssembly
{
    /// <summary>
    /// Types suitable when a value is expected.
    /// </summary>
    public enum ValueType : sbyte
    {
        /// <summary>
        /// 32-bit integer value-type, equivalent to .NET's <see cref="int"/> and <see cref="uint"/>.
        /// </summary>
        Int32 = -0x01,
        /// <summary>
        /// 64-bit integer value-type, equivalent to .NET's <see cref="long"/> and <see cref="ulong"/>.
        /// </summary>
        Int64 = -0x02,
        /// <summary>
        /// 32-bit floating point value-type, equivalent to .NET's <see cref="float"/>.
        /// </summary>
        Float32 = -0x03,
        /// <summary>
        /// 64-bit floating point value-type, equivalent to .NET's <see cref="double"/>.
        /// </summary>
        Float64 = -0x04,
    }

    static class ValueTypeExtensions
    {
        public static System.Type ToSystemType(this ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Int32: return typeof(int);
                case ValueType.Int64: return typeof(long);
                case ValueType.Float32: return typeof(float);
                case ValueType.Float64: return typeof(double);
                default: throw new System.ArgumentOutOfRangeException(nameof(valueType), $"{nameof(ValueType)} {valueType} not recognized.");
            }
        }

        private static readonly RegeneratingWeakReference<Dictionary<System.Type, ValueType>> systemTypeToValueType
            = new RegeneratingWeakReference<Dictionary<System.Type, ValueType>>(() => new Dictionary<System.Type, ValueType>
            {
                { typeof(int), ValueType.Int32 },
                { typeof(long), ValueType.Int64 },
                { typeof(float), ValueType.Float32 },
                { typeof(double), ValueType.Float64 },
            });

        public static bool TryConvertToValueType(this System.Type type, out ValueType value) => systemTypeToValueType.Reference.TryGetValue(type, out value);

        public static bool IsSupported(this System.Type type) => systemTypeToValueType.Reference.ContainsKey(type);
    }
}