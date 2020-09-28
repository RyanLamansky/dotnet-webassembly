using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate (with saturation) a 64-bit float to a signed 64-bit integer.
    /// </summary>
    public class Int64TruncateSaturateFloat64Signed : MiscellaneousInstruction
    {
        /// <summary>
        /// Always <see cref="MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed"/>.
        /// </summary>
        public override MiscellaneousOpCode MiscellaneousOpCode =>
            MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateSaturateFloat64Signed"/> instance.
        /// </summary>
        public Int64TruncateSaturateFloat64Signed()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed, 1, 0);

            var type = stack.Pop();
            if (type != WebAssemblyValueType.Float64)
                throw new StackTypeInvalidException(MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed, WebAssemblyValueType.Float64, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64TruncateSaturateFloat64Signed, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int64TruncateSaturateFloat64Signed",
                    CompilationContext.HelperMethodAttributes,
                    typeof(long),
                    new[] { typeof(double) }
                );

                var il = builder.GetILGenerator();

                var label1 = il.DefineLabel();
                var label2 = il.DefineLabel();
                var label3 = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R8, 9.223372036854776E+18);
                il.Emit(OpCodes.Blt_Un_S, label1);
                il.Emit(OpCodes.Ldc_I8, 9223372036854775807);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_R8, -9.223372036854776E+18);
                il.Emit(OpCodes.Bgt_Un_S, label2);
                il.Emit(OpCodes.Ldc_I8, -9223372036854775808);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(label2);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, typeof(double).GetMethod("IsNaN")!, null);
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