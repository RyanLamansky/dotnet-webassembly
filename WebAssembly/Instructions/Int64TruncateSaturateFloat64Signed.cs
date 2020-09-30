using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 64-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat64Signed : TruncateSaturateInstruction
    {
        /// <summary>
        /// Always <see cref="Int64TruncateSaturateFloat64Signed"/>.
        /// </summary>
        public sealed override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat64Signed"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat64Signed()
        {
        }

        private protected override WebAssemblyValueType InputValueType => WebAssemblyValueType.Float64;
        private protected override WebAssemblyValueType OutputValueType => WebAssemblyValueType.Int64;
        private protected override Type InputType => typeof(double);
        private protected override Type OutputType => typeof(long);
        private protected override HelperMethod ConversionHelper => HelperMethod.Int64TruncateSaturateFloat64Signed;

        private protected override void EmitLoadFloatMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, (double)long.MaxValue);
        }

        private protected override void EmitLoadIntegerMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I8, long.MaxValue);
        }

        private protected override void EmitLoadFloatMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, (double)long.MinValue);
        }

        private protected override void EmitLoadIntegerMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I8, long.MinValue);
        }

        private protected override void EmitLoadIntegerZero(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_I8);
        }

        private protected override void EmitConvert(ILGenerator il)
        {
            il.Emit(OpCodes.Conv_I8);
        }
    }
}