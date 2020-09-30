using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 64-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat64Unsigned : TruncateSaturateInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int64TruncateSaturateFloat64Unsigned"/>.
        /// </summary>
        public sealed override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat64Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat64Unsigned"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat64Unsigned()
        {
        }

        private protected override WebAssemblyValueType InputValueType => WebAssemblyValueType.Float64;
        private protected override WebAssemblyValueType OutputValueType => WebAssemblyValueType.Int64;
        private protected override Type InputType => typeof(double);
        private protected override Type OutputType => typeof(ulong);
        private protected override HelperMethod ConversionHelper => HelperMethod.Int64TruncateSaturateFloat64Unsigned;

        private protected override void EmitLoadFloatMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, (double)ulong.MaxValue);
        }

        private protected override void EmitLoadIntegerMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Conv_I8);
        }

        private protected override void EmitLoadFloatMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, 0.0);
        }

        private protected override void EmitLoadIntegerMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_I8);
        }

        private protected override void EmitLoadIntegerZero(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_I8);
        }

        private protected override void EmitConvert(ILGenerator il)
        {
            il.Emit(OpCodes.Conv_U8);
        }
    }
}