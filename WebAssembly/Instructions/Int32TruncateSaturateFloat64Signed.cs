using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 64-bit float to a signed 32-bit integer.
    /// </summary>
    public class Int32TruncateSaturateFloat64Signed : TruncateSaturateInstruction
    {
        /// <summary>
        /// Always <see cref="Int32TruncateSaturateFloat64Signed"/>.
        /// </summary>
        public sealed override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int32TruncateSaturateFloat64Signed;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateSaturateFloat64Signed"/> instance.
        /// </summary>
        public Int32TruncateSaturateFloat64Signed()
        {
        }

        private protected override WebAssemblyValueType InputValueType => WebAssemblyValueType.Float64;
        private protected override WebAssemblyValueType OutputValueType => WebAssemblyValueType.Int32;
        private protected override Type InputType => typeof(double);
        private protected override Type OutputType => typeof(int);
        private protected override HelperMethod ConversionHelper => HelperMethod.Int32TruncateSaturateFloat64Signed;

        private protected override void EmitLoadFloatMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, (double)int.MaxValue);
        }

        private protected override void EmitLoadIntegerMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4, int.MaxValue);
        }

        private protected override void EmitLoadFloatMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, (double)int.MinValue);
        }

        private protected override void EmitLoadIntegerMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4, int.MinValue);
        }

        private protected override void EmitLoadIntegerZero(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_0);
        }

        private protected override void EmitConvert(ILGenerator il)
        {
            il.Emit(OpCodes.Conv_I4);
        }
    }
}