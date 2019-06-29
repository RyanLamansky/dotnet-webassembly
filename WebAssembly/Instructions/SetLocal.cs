using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Set the current value of a local variable.
    /// </summary>
    public class SetLocal : VariableAccessInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.SetLocal"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.SetLocal;

        /// <summary>
        /// Creates a new  <see cref="SetLocal"/> instance.
        /// </summary>
        public SetLocal()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SetLocal"/> for the provided variable index.
        /// </summary>
        /// <param name="index">The index of the variable to access.</param>
        public SetLocal(uint index)
            : base(index)
        {
        }

        internal SetLocal(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 1)
                throw new StackTooSmallException(OpCode.SetLocal, 1, stack.Count);

            var setType = stack.Pop();
            if (setType != context.Locals[this.Index])
                throw new StackTypeInvalidException(OpCode.SetLocal, context.Locals[this.Index], setType);

            var localIndex = this.Index - context.Signature.ParameterTypes.Length;
            if (localIndex < 0)
            {
                //Referring to a parameter.
                if (this.Index <= byte.MaxValue)
                    context.Emit(OpCodes.Starg_S, checked((byte)this.Index));
                else
                    context.Emit(OpCodes.Starg, checked((ushort)this.Index));
            }
            else
            {
                //Referring to a local.
                switch (localIndex)
                {
                    default:
                        if (localIndex > 65534) // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc
                            throw new CompilerException($"Implementation limit exceeded: maximum accessible local index is 65534, tried to access {localIndex}.");

                        if (localIndex <= byte.MaxValue)
                            context.Emit(OpCodes.Stloc_S, (byte)localIndex);
                        else
                            context.Emit(OpCodes.Stloc, checked((ushort)localIndex));
                        break;

                    case 0: context.Emit(OpCodes.Stloc_0); break;
                    case 1: context.Emit(OpCodes.Stloc_1); break;
                    case 2: context.Emit(OpCodes.Stloc_2); break;
                    case 3: context.Emit(OpCodes.Stloc_3); break;
                }
            }
        }
    }
}