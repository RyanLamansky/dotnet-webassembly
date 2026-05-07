using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Write an element to a table by index. Stack: [i32] [ref] → []
/// </summary>
public class TableSet : Instruction
{
    /// <summary>Always <see cref="OpCode.TableSet"/>.</summary>
    public sealed override OpCode OpCode => OpCode.TableSet;

    /// <summary>Table index (currently must be 0).</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableSet"/> instance.</summary>
    public TableSet() { }

    /// <summary>Creates a new <see cref="TableSet"/> for the given table index.</summary>
    public TableSet(uint tableIndex) => TableIndex = tableIndex;

    internal TableSet(Reader reader) => TableIndex = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableSet ts && ts.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"Table index {TableIndex} out of range (only {context.Tables.Count} tables defined).", 0);

        var elementType = context.GetTableElementType(TableIndex);
        var table = context.GetTable(TableIndex);
        var stackType = elementType == ElementType.FunctionReference ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef;

        // Tracked stack: [..., index:i32, ref:object] — pop ref first (top), then index.
        context.PopStackNoReturn(OpCode, stackType);
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);

        // IL eval stack at entry: [..., index:i32, ref:object]  (ref is on top)
        // Target call: table.set_Item(int index, Delegate? value)
        var val = context.DeclareLocal(typeof(object));
        var idx = context.DeclareLocal(typeof(int));

        context.Emit(OpCodes.Stloc, val);  // pop ref → val
        context.Emit(OpCodes.Stloc, idx);  // pop index → idx
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, idx);
        context.Emit(OpCodes.Ldloc, val);
        
        if (elementType == ElementType.FunctionReference)
            context.Emit(OpCodes.Castclass, typeof(System.Delegate));
        
        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var indexSetter = tableType.GetProperty("Item")?.GetSetMethod() 
            ?? throw new NotSupportedException($"Item property not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, indexSetter);
    }
}
