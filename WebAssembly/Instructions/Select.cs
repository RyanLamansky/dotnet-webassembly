using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// A ternary operator with two operands, which have the same type as each other, plus a boolean (i32) condition. Returns the first operand if the condition operand is non-zero, or the second otherwise.
    /// </summary>
    public class Select : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Select"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Select;

        /// <summary>
        /// Creates a new  <see cref="Select"/> instance.
        /// </summary>
        public Select()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            var popped = context.PopStack(OpCode.Select, new WebAssemblyValueType?[] { WebAssemblyValueType.Int32, null, null }, 3).ToArray();
            var typeB = popped[1];
            var typeA = popped[2];

            if (typeA != typeB && typeA.HasValue && typeB.HasValue)
                throw new StackTypeInvalidException(OpCode.Select, typeA.Value, typeB.Value);

            if (!typeA.HasValue)
                stack.Push(typeB.GetValueOrDefault(WebAssemblyValueType.Int32));
            else
                stack.Push(typeA.GetValueOrDefault(WebAssemblyValueType.Int32));

            if (!typeA.HasValue)
            {
                typeA = WebAssemblyValueType.Int32; //And treat it as an int32
            }

            var helper = typeA switch
            {
                WebAssemblyValueType.Int32 => HelperMethod.SelectInt32,
                WebAssemblyValueType.Int64 => HelperMethod.SelectInt64,
                WebAssemblyValueType.Float32 => HelperMethod.SelectFloat32,
                WebAssemblyValueType.Float64 => HelperMethod.SelectFloat64,
                _ => throw new InvalidOperationException(),// Shouldn't be possible.
            };
            context.Emit(OpCodes.Call, context[helper, CreateSelectHelper]);
        }

        static MethodBuilder CreateSelectHelper(HelperMethod helper, CompilationContext context)
        {
            MethodBuilder builder = helper switch
            {
                HelperMethod.SelectInt32 => context.CheckedExportsBuilder.DefineMethod(
                                       "☣ Select Int32",
                                       CompilationContext.HelperMethodAttributes,
                                       typeof(int),
                                       new[]
                                       {
                                typeof(int),
                                typeof(int),
                                typeof(int),
                                       }
                                       ),
                HelperMethod.SelectInt64 => context.CheckedExportsBuilder.DefineMethod(
              "☣ Select Int64",
              CompilationContext.HelperMethodAttributes,
              typeof(long),
              new[]
              {
                                typeof(long),
                                typeof(long),
                                typeof(int),
              }
              ),
                HelperMethod.SelectFloat32 => context.CheckedExportsBuilder.DefineMethod(
              "☣ Select Float32",
              CompilationContext.HelperMethodAttributes,
              typeof(float),
              new[]
              {
                                typeof(float),
                                typeof(float),
                                typeof(int),
              }
              ),
                HelperMethod.SelectFloat64 => context.CheckedExportsBuilder.DefineMethod(
              "☣ Select Float64",
              CompilationContext.HelperMethodAttributes,
              typeof(double),
              new[]
              {
                                typeof(double),
                                typeof(double),
                                typeof(int),
              }
              ),
                _ => throw new InvalidOperationException(),// Shouldn't be possible.
            };
            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_2);
            var @true = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, @true);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(@true);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            return builder;
        }
    }
}