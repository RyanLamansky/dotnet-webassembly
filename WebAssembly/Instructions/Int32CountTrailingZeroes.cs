using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
    /// </summary>
    public class Int32CountTrailingZeroes : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32CountTrailingZeroes"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32CountTrailingZeroes;

        /// <summary>
        /// Creates a new  <see cref="Int32CountTrailingZeroes"/> instance.
        /// </summary>
        public Int32CountTrailingZeroes()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be this.
            context.ValidateStack(OpCode.Int32CountTrailingZeroes, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32CountTrailingZeroes, (helper, c) =>
            {
                var result = context.CheckedExportsBuilder.DefineMethod(
                    "☣ Int32CountTrailingZeroes",
                    CompilationContext.HelperMethodAttributes,
                    typeof(int),
                    new[] { typeof(uint)
                    });

                //All modern CPUs have a fast instruction specifically for this process, but there's no way to use it from .NET.
                //Based on the algorithm found here: http://aggregate.org/MAGIC/#Trailing%20Zero%20Count
                var il = result.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Neg);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Call, c[HelperMethod.Int32CountOneBits, Int32CountOneBits.CreateHelper]);
                il.Emit(OpCodes.Ret);

                return result;
            }
            ]);
        }
    }
}