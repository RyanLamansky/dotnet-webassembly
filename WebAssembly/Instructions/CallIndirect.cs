using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Call function indirectly.
    /// </summary>
    public class CallIndirect : Instruction, IEquatable<CallIndirect>
    {
        /// <summary>
        /// Always <see cref="OpCode.CallIndirect"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.CallIndirect;

        /// <summary>
        /// The index of the type representing the function signature.
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public byte Reserved { get; set; }

        /// <summary>
        /// Creates a new  <see cref="CallIndirect"/> instance.
        /// </summary>
        public CallIndirect()
        {
        }

        /// <summary>
        /// Creates a new  <see cref="CallIndirect"/> instance.
        /// </summary>
        /// <param name="type">The index of the type representing the function signature.</param>
        public CallIndirect(uint type)
        {
            this.Type = type;
        }

        internal CallIndirect(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Type = reader.ReadVarUInt32();
            Reserved = reader.ReadVarUInt1();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.CallIndirect);
            writer.WriteVar(this.Type);
            writer.WriteVar(this.Reserved);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction other) => this.Equals(other as CallIndirect);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(CallIndirect other) =>
            other != null
            && other.Type == this.Type
            && other.Reserved == this.Reserved
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)OpCode.CallIndirect, (int)this.Type, this.Reserved);

        internal sealed override void Compile(CompilationContext context)
        {
            var signature = context.Types[this.Type];
            var paramTypes = signature.RawParameterTypes;
            var returnTypes = signature.RawReturnTypes;

            var stack = context.Stack;
            if (stack.Count < paramTypes.Length + 1)
                throw new StackTooSmallException(OpCode.CallIndirect, paramTypes.Length + 1, stack.Count);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.CallIndirect, ValueType.Int32, type);

            for (var i = paramTypes.Length - 1; i >= 0; i--)
            {
                type = stack.Pop();
                if (type != paramTypes[i])
                    throw new StackTypeInvalidException(OpCode.CallIndirect, paramTypes[i], type);
            }

            for (var i = 0; i < returnTypes.Length; i++)
                stack.Push(returnTypes[i]);

            context.EmitLoadThis();
            context.Emit(OpCodes.Call, context.DelegateRemappersByType[this.Type]);
        }
    }
}