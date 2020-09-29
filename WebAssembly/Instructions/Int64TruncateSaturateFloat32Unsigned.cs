using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 32-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat32Unsigned : TruncateSaturateInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned"/>.
        /// </summary>
        public sealed override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat32Unsigned"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat32Unsigned()
        {
        }

        private protected override WebAssemblyValueType InputValueType => WebAssemblyValueType.Float32;
        private protected override WebAssemblyValueType OutputValueType => WebAssemblyValueType.Int64;
        private protected override Type InputType => typeof(float);
        private protected override Type OutputType => typeof(ulong);
        private protected override HelperMethod ConversionHelper => HelperMethod.Int64TruncateSaturateFloat32Unsigned;

        private protected override void EmitLoadFloatMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R4, (float)ulong.MaxValue);
        }

        private protected override void EmitLoadIntegerMaxValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Conv_I8);
        }

        private protected override void EmitLoadFloatMinValue(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R4, 0.0f);
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