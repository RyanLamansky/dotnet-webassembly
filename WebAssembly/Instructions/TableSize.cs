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
        TableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableSize ts && ts.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        context.Stack.Push(WebAssemblyValueType.Int32);

        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"Table index {TableIndex} out of range (only {context.Tables.Count} tables defined).", 0);

        var elementType = context.GetTableElementType(TableIndex);
        var table = context.GetTable(TableIndex);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        
        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var lengthGetter = tableType.GetProperty("Length")?.GetGetMethod() 
            ?? throw new System.NotSupportedException($"Length property not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, lengthGetter);
        context.Emit(OpCodes.Conv_I4);
    }
}
