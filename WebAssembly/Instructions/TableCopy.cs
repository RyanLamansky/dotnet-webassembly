using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy entries within or between tables. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class TableCopy : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableCopy"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableCopy;

    /// <summary>Destination table index.</summary>
    public uint DestinationTableIndex { get; set; }

    /// <summary>Source table index.</summary>
    public uint SourceTableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableCopy"/> instance.</summary>
    public TableCopy() { }

    internal TableCopy(Reader reader)
    {
        this.DestinationTableIndex = reader.ReadVarUInt32();
        this.SourceTableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(this.DestinationTableIndex);
        writer.WriteVar(this.SourceTableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableCopy tc && tc.DestinationTableIndex == DestinationTableIndex && tc.SourceTableIndex == SourceTableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)MiscellaneousOpCode, (int)DestinationTableIndex, (int)SourceTableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: dst src len → ()
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        var (dstTable, dstIsFunc) = context.ResolveTable(DestinationTableIndex);
        var (srcTable, srcIsFunc) = context.ResolveTable(SourceTableIndex);
        if (dstIsFunc != srcIsFunc)
            throw new ModuleLoadException("table.copy: source and destination tables have different element types.", 0);

        var isFunc = dstIsFunc;

        var len = context.DeclareLocal(typeof(uint));
        var src = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, src);
        context.Emit(OpCodes.Stloc, dst);

        // this.dstTable.Copy(this.srcTable, dst, src, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, dstTable);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, srcTable);
        context.Emit(OpCodes.Ldloc, dst);
        context.Emit(OpCodes.Ldloc, src);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, isFunc ? FunctionTable.CopyBetweenMethod : ExternRefTable.CopyBetweenMethod);
    }
}
