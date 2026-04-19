using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Grow a table by a given delta, returning the old size or -1 on failure. Stack: [ref] [delta:i32] → [i32]
/// </summary>
public class TableGrow : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableGrow"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableGrow;

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableGrow"/> instance.</summary>
    public TableGrow() { }

    internal TableGrow(Reader reader)
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
        other is TableGrow tg && tg.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: ref delta → old-size
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // delta
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.FuncRef); // ref (any ref type)
        context.Stack.Push(WebAssemblyValueType.Int32);

        if (TableIndex != 0 || context.FunctionTable == null)
            throw new NotSupportedException("table.grow only supports table index 0.");

        // Pop delta into local; discard ref; load table; call Grow; cast to int.
        var delta = context.DeclareLocal(typeof(uint));
        context.Emit(OpCodes.Stloc, delta); // store delta
        context.Emit(OpCodes.Pop);          // discard ref
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.FunctionTable);
        context.Emit(OpCodes.Ldloc, delta);
        context.Emit(OpCodes.Call, FunctionTable.GrowMethod);
        context.Emit(OpCodes.Conv_I4);
    }
}
