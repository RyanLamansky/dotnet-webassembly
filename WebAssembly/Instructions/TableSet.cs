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

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableSet"/> instance.</summary>
    public TableSet() { }

    /// <summary>Creates a new <see cref="TableSet"/> for the given table index.</summary>
    public TableSet(uint tableIndex) => this.TableIndex = tableIndex;

    internal TableSet(Reader reader) => this.TableIndex = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.WriteVar(this.TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableSet ts && ts.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        var (table, isFunc) = context.ResolveTable(TableIndex);

        // Tracked stack: [..., index:i32, ref] — pop ref (top) then index.
        context.PopStackNoReturn(OpCode, isFunc ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef);
        context.PopStackNoReturn(OpCode, WebAssemblyValueType.Int32);

        // IL eval stack at entry: [..., index:i32, ref]  (ref is on top).
        var val = context.DeclareLocal(typeof(object));
        var idx = context.DeclareLocal(typeof(int));

        context.Emit(OpCodes.Stloc, val);
        context.Emit(OpCodes.Stloc, idx);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, idx);
        context.Emit(OpCodes.Ldloc, val);

        if (isFunc)
            context.Emit(OpCodes.Castclass, typeof(System.Delegate));

        context.Emit(OpCodes.Call, isFunc ? FunctionTable.IndexSetter : ExternRefTable.IndexSetter);
    }
}
