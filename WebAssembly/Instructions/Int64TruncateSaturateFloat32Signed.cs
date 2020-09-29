using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 32-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat32Signed : TruncateSaturateInstruction
    {
        /// <summary>
        /// Always <see cref="Int64TruncateSaturateFloat32Signed"/>.
        /// </summary>
        public sealed override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat32Signed"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat32Signed()
        {
        }

        private protected override WebAssemblyValueType InputValueType => WebAssemblyValueType.Float32;
        private protected override WebAssemblyValueType OutputValueType => WebAssemblyValueType.Int64;
        private protected override Type InputType => typeof(float);
        private protected override Type OutputType => typeof(long);
        private protected override HelperMethod ConversionHelper => HelperMethod.Int64TruncateSaturateFloat32Signed;

        private protected override void EmitLoadFloatMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R4, (float)long.MaxValue);
        }

        private protected override void EmitLoadIntegerMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I8, long.MaxValue);
        }

        private protected override void EmitLoadFloatMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R4, (float)long.MinValue);
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