using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic count leading zero bits.  All zero bits are considered leading if the value is zero.
    /// </summary>
    public class Int32CountLeadingZeroes : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32CountLeadingZeroes"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32CountLeadingZeroes;

        /// <summary>
        /// Creates a new  <see cref="Int32CountLeadingZeroes"/> instance.
        /// </summary>
        public Int32CountLeadingZeroes()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be Int32.
            context.ValidateStack(OpCode.Int32CountLeadingZeroes, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32CountLeadingZeroes, (helper, c) =>
            {
                var result = context.CheckedExportsBuilder.DefineMethod(
                    "☣ Int32CountLeadingZeroes",
                    CompilationContext.HelperMethodAttributes,
                    typeof(uint),
                    new[] { typeof(uint)
                    });

                //All modern CPUs have a fast instruction specifically for this process, but there's no way to use it from .NET.
                //This algorithm is from https://stackoverflow.com/questions/10439242/count-leading-zeroes-in-an-int32
                var il = result.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Starg_S, 0);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Starg_S, 0);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_4);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Starg_S, 0);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_8);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Starg_S, 0);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_S, 16);
                il.Emit(OpCodes.Shr_Un);
                il.Emit(OpCodes.Or);
                il.Emit(OpCodes.Starg_S, 0);

                il.Emit(OpCodes.Ldc_I4_S, 32);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, c[HelperMethod.Int32CountOneBits, Int32CountOneBits.CreateHelper]);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Ret);

                return result;
            }
            ]);
        }
    }
}