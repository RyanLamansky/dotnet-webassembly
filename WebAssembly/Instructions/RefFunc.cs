using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce a non-null reference to the function at the given index.
/// </summary>
public class RefFunc : Instruction, IEquatable<RefFunc>
{
    /// <summary>
    /// Always <see cref="OpCode.RefFunc"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefFunc;

    /// <summary>
    /// The index of the function to reference.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="RefFunc"/> instance.
    /// </summary>
    public RefFunc()
    {
    }

    /// <summary>
    /// Creates a new <see cref="RefFunc"/> instance for the given function index.
    /// </summary>
    public RefFunc(uint index) => this.Index = index;

    internal RefFunc(Reader reader) => this.Index = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.RefFunc);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as RefFunc);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as RefFunc);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(RefFunc? other) => other != null && other.Index == this.Index;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.RefFunc, (int)this.Index);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Index}";

    internal sealed override void Compile(CompilationContext context)
    {
        context.Stack.Push(WebAssemblyValueType.FuncRef);

        if (context.FunctionReferences == null)
            throw new CompilerException("ref.func: function references array not initialized.");

        var methods = context.Methods;
        if (methods == null || this.Index >= methods.Length)
            throw new ModuleLoadException($"ref.func: function {this.Index} does not exist.", 0);

        if (context.EnforceDeclaredFunctionReferences && !context.DeclaredFunctionReferences.Contains(this.Index))
            throw new ModuleLoadException("undeclared function reference", 0);

        // Emit: this.FunctionReferences[Index]
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.FunctionReferences);
        context.Emit(OpCodes.Ldc_I4, (int)this.Index);
        context.Emit(OpCodes.Ldelem_Ref);
    }
}
