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
        if (TableIndex != 0 || context.FunctionTable == null)
            throw new NotSupportedException("table.get only supports table index 0.");

        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);
        context.Stack.Push(WebAssemblyValueType.FuncRef);

        // IL stack on entry: [..., index:i32]  — store it, load table, reload index.
        var idx = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, idx);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.FunctionTable);
        context.Emit(OpCodes.Ldloc, idx);
        context.Emit(OpCodes.Call, FunctionTable.IndexGetter);
    }
}
