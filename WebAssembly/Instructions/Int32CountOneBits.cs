using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic count number of one bits.
    /// </summary>
    public class Int32CountOneBits : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32CountOneBits"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32CountOneBits;

        /// <summary>
        /// Creates a new  <see cref="Int32CountOneBits"/> instance.
        /// </summary>
        public Int32CountOneBits()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.Int32CountOneBits, 1, stack.Count);

            var type = stack.Peek(); //Assuming validation passes, the remaining type will be this.

            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Int32CountOneBits, ValueType.Int32, type);

            context.Emit(OpCodes.Call, context[HelperMethod.Int32CountOneBits, CreateHelper]);
        }

        internal static MethodBuilder CreateHelper(HelperMethod helper, CompilationContext context)
        {
            Assert(context != null);

            var result = context.ExportsBuilder.DefineMethod(
                "☣ Int32CountOneBits",
                CompilationContext.HelperMethodAttributes,
                typeof(uint),
                new[] { typeof(uint)
                });

            //All modern CPUs have a fast instruction specifically for this process, but there's no way to use it from .NET.
            //This algorithm is from https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
            var il = result.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ldc_I4, 0x55555555);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Starg_S, 0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, 0x33333333);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ldc_I4, 0x33333333);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Starg_S, 0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldc_I4, 0x0f0f0f0f);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldc_I4, 0x01010101);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Ldc_I4_S, 24);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Ret);

            return result;
        }
    }
}