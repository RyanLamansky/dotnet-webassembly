using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 64-bit float to an unsigned 32-bit integer.
    /// </summary>
    public class Int32TruncateSaturateFloat64Unsigned : MiscellaneousInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int32TruncateSaturateFloat64Unsigned"/>.
        /// </summary>
        public override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int32TruncateSaturateFloat64Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateSaturateFloat64Unsigned"/> instance.
        /// </summary>
        public Int32TruncateSaturateFloat64Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(MiscellaneousOpCode.Int32TruncateSaturateFloat64Unsigned, 1, 0);

            var type = stack.Pop();
            if (type != WebAssemblyValueType.Float64)
                throw new StackTypeInvalidException(MiscellaneousOpCode.Int32TruncateSaturateFloat64Unsigned, WebAssemblyValueType.Float64, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32TruncateSaturateFloat64Unsigned, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int32TruncateSaturateFloat64Unsigned",
                    CompilationContext.HelperMethodAttributes,
                    typeof(uint),
                    new[] { typeof(double) }
                );

                var il = builder.GetILGenerator();

                var label1 = il.DefineLabel();
                var label2 = il.DefineLabel();
                var label3 = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R8, 4294967295.0);
                il.Emit(OpCodes.Blt_Un_S, label1);
                il.Emit(OpCodes.Ldc_I4_M1);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R8, 0.0);
                il.Emit(OpCodes.Bgt_Un_S, label2);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label2);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, typeof(double).GetMethod("IsNaN")!, null);
                il.Emit(OpCodes.Brfalse_S, label3);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label3);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Conv_U4);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);

            stack.Push(WebAssemblyValueType.Int32);
        }
    }
}