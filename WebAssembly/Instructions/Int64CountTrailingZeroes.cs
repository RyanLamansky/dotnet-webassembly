using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
    /// </summary>
    public class Int64CountTrailingZeroes : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64CountTrailingZeroes"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64CountTrailingZeroes;

        /// <summary>
        /// Creates a new  <see cref="Int64CountTrailingZeroes"/> instance.
        /// </summary>
        public Int64CountTrailingZeroes()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.Int64CountTrailingZeroes, 1, stack.Count);

            var type = stack.Peek(); //Assuming validation passes, the remaining type will be this.

            if (type != ValueType.Int64)
                throw new StackTypeInvalidException(OpCode.Int64CountTrailingZeroes, ValueType.Int64, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64CountTrailingZeroes, (helper, c) =>
            {
                Assert(c != null);

                var result = context.ExportsBuilder.DefineMethod(
                    "☣ Int64CountTrailingZeroes",
                    CompilationContext.HelperMethodAttributes,
                    typeof(long),
                    new[] { typeof(ulong)
                    });

                //All modern CPUs have a fast instruction specifically for this process, but there's no way to use it from .NET.
                //Based on the algorithm found here: http://aggregate.org/MAGIC/#Trailing%20Zero%20Count
                var il = result.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Neg);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Conv_I8);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Call, c[HelperMethod.Int64CountOneBits, Int64CountOneBits.CreateHelper]);
                il.Emit(OpCodes.Ret);

                return result;
            }
            ]);
        }
    }
}