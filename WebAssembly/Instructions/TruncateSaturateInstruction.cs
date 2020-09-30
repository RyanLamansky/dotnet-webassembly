using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Provides shared functionality for instructions that do truncation with saturation.
    /// </summary>
    public abstract class TruncateSaturateInstruction : MiscellaneousInstruction
    {
        private protected TruncateSaturateInstruction()
            : base()
        {
        }

        private protected abstract WebAssemblyValueType InputValueType { get; }
        private protected abstract WebAssemblyValueType OutputValueType { get; }
        private protected abstract HelperMethod ConversionHelper { get; }
        private protected abstract Type InputType { get; }
        private protected abstract Type OutputType { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(MiscellaneousOpCode, 1, 0);

            var type = stack.Pop();
            if (type != InputValueType)
                throw new StackTypeInvalidException(MiscellaneousOpCode, InputValueType, type);

            context.Emit(OpCodes.Call, context[ConversionHelper, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    $"☣ {ConversionHelper}",
                    CompilationContext.HelperMethodAttributes,
                    OutputType,
                    new[] { InputType }
                );

                // The following code is an IL version of this C#,
                // with "float" and "int" replaced by "double" and "uint"
                // as appropriate for the overriding class:
                //
                // public static int Int32TruncateSaturateFloat32Signed(float f)
                // {
                //     if (f >= int.MaxValue)
                //         return int.MaxValue;
                // 
                //     if (f <= int.MinValue)
                //         return int.MinValue;
                // 
                //     if (float.IsNaN(f))
                //         return 0;
                // 
                //     return (int)f;
                // }

                var il = builder.GetILGenerator();

                var notAboveRangeLabel = il.DefineLabel();
                var notBelowRangeLabel = il.DefineLabel();
                var notNaNLabel = il.DefineLabel();

                // if (f >= int.MaxValue)
                il.Emit(OpCodes.Ldarg_0);
                EmitLoadFloatMaxValue(il);
                il.Emit(OpCodes.Blt_Un_S, notAboveRangeLabel);

                // return int.MaxValue
                EmitLoadIntegerMaxValue(il);
                il.Emit(OpCodes.Ret);

                // if (f <= int.MinValue)
                il.MarkLabel(notAboveRangeLabel);
                il.Emit(OpCodes.Ldarg_0);
                EmitLoadFloatMinValue(il);
                il.Emit(OpCodes.Bgt_Un_S, notBelowRangeLabel);

                // return int.MinValue
                EmitLoadIntegerMinValue(il);
                il.Emit(OpCodes.Ret);

                // if (float.IsNaN(f))
                il.MarkLabel(notBelowRangeLabel);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, InputType.GetMethod(nameof(float.IsNaN))!, null);
                il.Emit(OpCodes.Brfalse_S, notNaNLabel);

                // return 0
                EmitLoadIntegerZero(il);
                il.Emit(OpCodes.Ret);

                // return (int)f
                il.MarkLabel(notNaNLabel);
                il.Emit(OpCodes.Ldarg_0);
                EmitConvert(il);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);

            stack.Push(OutputValueType);
        }

        private protected abstract void EmitLoadFloatMaxValue(ILGenerator il);
        private protected abstract void EmitLoadIntegerMaxValue(ILGenerator il);
        private protected abstract void EmitLoadFloatMinValue(ILGenerator il);
        private protected abstract void EmitLoadIntegerMinValue(ILGenerator il);
        private protected abstract void EmitLoadIntegerZero(ILGenerator il);
        private protected abstract void EmitConvert(ILGenerator il);
    }
}
