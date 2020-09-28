using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 32-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat32Signed : MiscellaneousInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed"/>.
        /// </summary>
        public override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat32Signed"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat32Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed, 1, 0);

            var type = stack.Pop();
            if (type != WebAssemblyValueType.Float32)
                throw new StackTypeInvalidException(MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed, WebAssemblyValueType.Float32, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64TruncateSaturateFloat32Signed, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int64TruncateSaturateFloat32Signed",
                    CompilationContext.HelperMethodAttributes,
                    typeof(long),
                    new[] { typeof(float) }
                );

                var il = builder.GetILGenerator();

                var label1 = il.DefineLabel();
                var label2 = il.DefineLabel();
                var label3 = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R4, 9.223372E+18f);
                il.Emit(OpCodes.Blt_Un_S, label1);
                il.Emit(OpCodes.Ldc_I8, 9223372036854775807);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R4, -9.223372E+18f);
                il.Emit(OpCodes.Bgt_Un_S, label2);
                il.Emit(OpCodes.Ldc_I8, -9223372036854775808);
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
                il.Emit(OpCodes.Conv_I8);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}