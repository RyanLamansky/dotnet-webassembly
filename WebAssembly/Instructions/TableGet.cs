using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read an element from a table by index. Stack: [i32] → [ref]
/// </summary>
public class TableGet : Instruction
{
    /// <summary>Always <see cref="OpCode.TableGet"/>.</summary>
    public sealed override OpCode OpCode => OpCode.TableGet;

    /// <summary>Table index (currently must be 0).</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableGet"/> instance.</summary>
    public TableGet() { }

    /// <summary>Creates a new <see cref="TableGet"/> for the given table index.</summary>
    public TableGet(uint tableIndex) => TableIndex = tableIndex;

    internal TableGet(Reader reader) => TableIndex = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableGet tg && tg.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"Table index {TableIndex} out of range (only {context.Tables.Count} tables defined).", 0);

        var elementType = context.GetTableElementType(TableIndex);
        var table = context.GetTable(TableIndex);
        var stackType = elementType == ElementType.FunctionReference ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef;

        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);
        context.Stack.Push(stackType);

        // IL stack on entry: [..., index:i32]  — store it, load table, reload index.
        var idx = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, idx);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, idx);
        
        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var indexGetter = tableType.GetProperty("Item")?.GetGetMethod() 
            ?? throw new NotSupportedException($"Item property not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, indexGetter);
    }
}
