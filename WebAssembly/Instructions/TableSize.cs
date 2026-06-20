using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Return the current size of a table. Stack: [] → [i32]
/// </summary>
public class TableSize : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableSize"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableSize;

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableSize"/> instance.</summary>
    public TableSize() { }

    internal TableSize(Reader reader)
    {
        this.TableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(this.TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableSize ts && ts.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        var (table, isFunc) = context.ResolveTable(TableIndex);

        context.Stack.Push(WebAssemblyValueType.Int32);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Call, isFunc ? FunctionTable.LengthGetter : ExternRefTable.LengthGetter);
        context.Emit(OpCodes.Conv_I4);
    }
}
