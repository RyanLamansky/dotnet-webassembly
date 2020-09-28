using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 32-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat32Unsigned : MiscellaneousInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned"/>.
        /// </summary>
        public override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat32Unsigned"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned, 1, 0);

            var type = stack.Pop();
            if (type != WebAssemblyValueType.Float32)
                throw new StackTypeInvalidException(MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned, WebAssemblyValueType.Float32, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64TruncateSaturateFloat32Unsigned, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int64TruncateSaturateFloat32Unsigned",
                    CompilationContext.HelperMethodAttributes,
                    typeof(ulong),
                    new[] { typeof(float) }
                );

                var il = builder.GetILGenerator();

                var label1 = il.DefineLabel();
                var label2 = il.DefineLabel();
                var label3 = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R4, 1.8446744E+19f);
                il.Emit(OpCodes.Blt_Un_S, label1);
                il.Emit(OpCodes.Ldc_I4_M1);
                il.Emit(OpCodes.Conv_I8);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R4, 0.0f);
                il.Emit(OpCodes.Bgt_Un_S, label2);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_I8);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label2);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, typeof(float).GetMethod("IsNaN")!, null);
                il.Emit(OpCodes.Brfalse_S, label3);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_I8);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label3);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Conv_U8);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}