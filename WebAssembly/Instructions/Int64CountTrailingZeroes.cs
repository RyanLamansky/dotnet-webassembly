using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

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
            //Assuming validation passes, the remaining type will be Int64.
            context.ValidateStack(OpCode.Int64CountTrailingZeroes, WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64CountTrailingZeroes, (helper, c) =>
            {
                var result = context.CheckedExportsBuilder.DefineMethod(
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