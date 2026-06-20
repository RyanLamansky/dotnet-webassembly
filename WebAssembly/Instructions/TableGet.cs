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

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableGet"/> instance.</summary>
    public TableGet() { }

    /// <summary>Creates a new <see cref="TableGet"/> for the given table index.</summary>
    public TableGet(uint tableIndex) => this.TableIndex = tableIndex;

    internal TableGet(Reader reader) => this.TableIndex = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.WriteVar(this.TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableGet tg && tg.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        var (table, isFunc) = context.ResolveTable(TableIndex);

        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);
        context.Stack.Push(isFunc ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef);

        // IL stack on entry: [..., index:i32]  — store it, load table, reload index, call the indexer getter.
        var idx = context.DeclareLocal(typeof(int));
        context.Emit(OpCodes.Stloc, idx);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, idx);
        context.Emit(OpCodes.Call, isFunc ? FunctionTable.IndexGetter : ExternRefTable.IndexGetter);
    }
}
