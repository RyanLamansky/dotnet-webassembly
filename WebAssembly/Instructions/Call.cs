using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Call function directly.
    /// </summary>
    public class Call : Instruction, IEquatable<Call>
    {
        /// <summary>
        /// Always <see cref="OpCode.Call"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Call;

        /// <summary>
        /// The location within the function index to call.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// Creates a new  <see cref="Call"/> instance.
        /// </summary>
        public Call()
        {
        }

        /// <summary>
        /// Creates a new  <see cref="Call"/> instance to invoke the function at the specified index.
        /// </summary>
        /// <param name="index">The location within the function index to call.</param>
        public Call(uint index)
        {
            this.Index = index;
        }

        internal Call(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Index = reader.ReadVarUInt32();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write((byte)OpCode.Call);
            writer.WriteVar(this.Index);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) => this.Equals(other as Call);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(Call? other) =>
            other != null
            && other.Index == this.Index
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

        internal sealed override void Compile(CompilationContext context)
        {
            if (this.Index >= context.CheckedMethods.Length)
                throw new CompilerException($"Function for index {this.Index} requseted, the assembly contains only {context.CheckedMethods.Length}.");

            var signature = context.CheckedFunctionSignatures[this.Index];
            var paramTypes = signature.RawParameterTypes;
            var returnTypes = signature.RawReturnTypes;

            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, paramTypes.Cast<WebAssemblyValueType?>().Reverse(), paramTypes.Length);

            for (var i = 0; i < returnTypes.Length; i++)
                stack.Push(returnTypes[i]);

            var target = context.CheckedMethods[this.Index];
            if (target is MethodBuilder) //Indicates a dynamically generated method.
                context.EmitLoadThis();
            context.Emit(OpCodes.Call, target);
        }
    }
}