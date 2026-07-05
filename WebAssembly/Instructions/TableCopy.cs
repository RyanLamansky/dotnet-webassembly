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
        var dst64 = context.IsTable64(DestinationTableIndex);
        var src64 = context.IsTable64(SourceTableIndex);
        // When either table is 64-bit the whole operation runs through the u64 (table64) path; a 32-bit side's
        // operand is widened to u64. The length operand's type is the narrower of the two (per the spec): i64 only
        // when both are 64-bit, else i32.
        var useUlong = dst64 || src64;
        var dstType = context.TableIndexType(DestinationTableIndex);
        var srcType = context.TableIndexType(SourceTableIndex);
        var lenType = dst64 && src64 ? WebAssemblyValueType.Int64 : WebAssemblyValueType.Int32;

        // Stack: dst src len → ()
        context.PopStackNoReturn(this.OpCode, lenType); // len
        context.PopStackNoReturn(this.OpCode, srcType); // src
        context.PopStackNoReturn(this.OpCode, dstType); // dst

        var (dstTable, dstIsFunc) = context.ResolveTable(DestinationTableIndex);
        var (srcTable, srcIsFunc) = context.ResolveTable(SourceTableIndex);
        if (dstIsFunc != srcIsFunc)
            throw new ModuleLoadException("table.copy: source and destination tables have different element types.", 0);

        var isFunc = dstIsFunc;

        // Widen any i32 operand to u64 (zero-extending) so the table64 overloads' bounds tests see the true value.
        var localType = useUlong ? typeof(ulong) : typeof(uint);
        var len = context.DeclareLocal(localType);
        var src = context.DeclareLocal(localType);
        var dst = context.DeclareLocal(localType);

        if (useUlong && lenType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, len);
        if (useUlong && srcType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, src);
        if (useUlong && dstType == WebAssemblyValueType.Int32)
            context.Emit(OpCodes.Conv_U8);
        context.Emit(OpCodes.Stloc, dst);

        // this.dstTable.Copy(this.srcTable, dst, src, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, dstTable);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, srcTable);
        context.Emit(OpCodes.Ldloc, dst);
        context.Emit(OpCodes.Ldloc, src);
        context.Emit(OpCodes.Ldloc, len);
        context.Emit(OpCodes.Call, useUlong
            ? (isFunc ? FunctionTable.CopyBetween64Method : ExternRefTable.CopyBetween64Method)
            : (isFunc ? FunctionTable.CopyBetweenMethod : ExternRefTable.CopyBetweenMethod));
    }
}
