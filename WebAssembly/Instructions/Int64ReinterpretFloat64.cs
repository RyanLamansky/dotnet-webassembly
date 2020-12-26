using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Reinterpret the bits of a 64-bit float as a 64-bit integer.
    /// </summary>
    public class Int64ReinterpretFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ReinterpretFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ReinterpretFloat64;

        /// <summary>
        /// Creates a new  <see cref="Int64ReinterpretFloat64"/> instance.
        /// </summary>
        public Int64ReinterpretFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int64ReinterpretFloat64, WebAssemblyValueType.Float64);

            stack.Push(WebAssemblyValueType.Int64);

            context.Emit(OpCodes.Call, context[HelperMethod.Int64ReinterpretFloat64, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ Int64ReinterpretFloat64",
                    CompilationContext.HelperMethodAttributes,
                    typeof(long),
                    new[]
                    {
                            typeof(double),
                    }
                    );

                var il = builder.GetILGenerator();
                il.Emit(OpCodes.Ldarga_S, 0);
                il.Emit(OpCodes.Ldind_I8);
                il.Emit(OpCodes.Ret);
                return builder;
            }
            ]);
        }
    }
}