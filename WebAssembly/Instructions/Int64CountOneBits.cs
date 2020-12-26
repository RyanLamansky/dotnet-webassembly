using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic count number of one bits.
    /// </summary>
    public class Int64CountOneBits : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64CountOneBits"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64CountOneBits;

        /// <summary>
        /// Creates a new  <see cref="Int64CountOneBits"/> instance.
        /// </summary>
        public Int64CountOneBits()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be Int64.
            context.ValidateStack(OpCode.Int64CountOneBits, WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64CountOneBits, CreateHelper]);
        }

        internal static MethodBuilder CreateHelper(HelperMethod helper, CompilationContext context)
        {
            var result = context.CheckedExportsBuilder.DefineMethod(
                "☣ Int64CountOneBits",
                CompilationContext.HelperMethodAttributes,
                typeof(ulong),
                new[] { typeof(ulong)
                });

            //All modern CPUs have a fast instruction specifically for this process, but there's no way to use it from .NET.
            //This algorithm is from https://stackoverflow.com/questions/2709430/count-number-of-bits-in-a-64-bit-long-big-integer
            var il = result.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ldc_I8, 0x5555555555555555);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Starg_S, 0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I8, 0x3333333333333333);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ldc_I8, 0x3333333333333333);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Starg_S, 0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldc_I8, 0x0f0f0f0f0f0f0f0f);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldc_I8, 0x0101010101010101);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Ldc_I4_S, 56);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ret);

            return result;
        }
    }
}