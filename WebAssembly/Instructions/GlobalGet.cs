using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// (i32 index){T}; Read a global variable.
/// </summary>
public class GlobalGet : VariableAccessInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.GlobalGet"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.GlobalGet;

    /// <summary>
    /// Creates a new  <see cref="GlobalGet"/> instance.
    /// </summary>
    public GlobalGet()
    {
    }

    /// <summary>
    /// Creates a new <see cref="GlobalGet"/> for the provided variable index.
    /// </summary>
    /// <param name="index">The index of the variable to access.</param>
    public GlobalGet(uint index)
        : base(index)
    {
    }

    internal GlobalGet(Reader reader)
        : base(reader)
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        if (context.Globals == null)
            throw new CompilerException("Can't use GetGlobal without a global section or global imports.");

        if (context.ConstantExpression && this.Index >= (uint)context.ImportedGlobals)
        {
            // In a constant (initializer/offset) expression, global.get is in scope only for imported
            // globals — those occupy indices below ImportedGlobals. Referencing a module-defined global
            // (or an out-of-range index) is "unknown global" per the spec's constant-expression context.
            throw new ModuleLoadException(
                $"global.get in a constant expression may only reference an imported global; index {this.Index} is out of that range.",
                0);
        }

        GlobalInfo global;
        try
        {
            global = context.Globals[this.Index];
        }
        catch (System.IndexOutOfRangeException x)
        {
            throw new CompilerException($"Global at index {this.Index} does not exist.", x);
        }

        if (context.ConstantExpression && global.Setter != null)
        {
            // A constant expression's global.get must reference an immutable global; a mutable one
            // (its setter is non-null) is rejected as "constant expression required".
            throw new ModuleLoadException(
                $"global.get in a constant expression must reference an immutable global; index {this.Index} is mutable.",
                0);
        }

        if (global.RequiresInstance)
            context.EmitLoadThis();

        context.Emit(OpCodes.Call, global.Getter);

        context.Stack.Push(global.Type);
    }
}
