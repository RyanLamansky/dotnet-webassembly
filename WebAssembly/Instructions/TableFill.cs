using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Fill a range of a table with a reference value. Stack: [dst:i32] [ref] [len:i32] → []
/// </summary>
public class TableFill : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableFill"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableFill;

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableFill"/> instance.</summary>
    public TableFill() { }

    internal TableFill(Reader reader)
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
        other is TableFill tf && tf.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        var (table, isFunc) = context.ResolveTable(TableIndex);

        // Stack: dst val len → ()
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, isFunc ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef); // value
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        // IL stack at entry: [..., dst:i32, value, len:i32]
        var len = context.DeclareLocal(typeof(uint));
        var value = context.DeclareLocal(isFunc ? typeof(Delegate) : typeof(object));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        if (isFunc)
            context.Emit(OpCodes.Castclass, typeof(Delegate));
        context.Emit(OpCodes.Stloc, value);
        context.Emit(OpCodes.Stloc, dst);

        // this.table.Fill(dst, value, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, dst);
        context.Emit(OpCodes.Ldloc, value);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, isFunc ? FunctionTable.FillMethod : ExternRefTable.FillMethod);
    }
}
