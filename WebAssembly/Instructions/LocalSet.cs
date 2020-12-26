using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Set the current value of a local variable.
    /// </summary>
    public class LocalSet : VariableAccessInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.LocalSet"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.LocalSet;

        /// <summary>
        /// Creates a new  <see cref="LocalSet"/> instance.
        /// </summary>
        public LocalSet()
        {
        }

        /// <summary>
        /// Creates a new <see cref="LocalSet"/> for the provided variable index.
        /// </summary>
        /// <param name="index">The index of the variable to access.</param>
        public LocalSet(uint index)
            : base(index)
        {
        }

        internal LocalSet(Reader reader)
            : base(reader)
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(OpCode.LocalSet, context.CheckedLocals[this.Index]);

            var localIndex = this.Index - context.CheckedSignature.ParameterTypes.Length;
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