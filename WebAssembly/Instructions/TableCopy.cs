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
    public uint DstTableIndex { get; set; }

    /// <summary>Source table index.</summary>
    public uint SrcTableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableCopy"/> instance.</summary>
    public TableCopy() { }

    internal TableCopy(Reader reader)
    {
        DstTableIndex = reader.ReadVarUInt32();
        SrcTableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(DstTableIndex);
        writer.WriteVar(SrcTableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableCopy tc && tc.DstTableIndex == DstTableIndex && tc.SrcTableIndex == SrcTableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode), HashCode.Combine((int)DstTableIndex, (int)SrcTableIndex));

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: dst src len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        if (DstTableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"table.copy: dst table index {DstTableIndex} out of range.", 0);
        if (SrcTableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"table.copy: src table index {SrcTableIndex} out of range.", 0);

        var dstTable = context.GetTable(DstTableIndex);
        var srcTable = context.GetTable(SrcTableIndex);
        var dstElemType = context.GetTableElementType(DstTableIndex);
        var srcElemType = context.GetTableElementType(SrcTableIndex);

        if (dstElemType != srcElemType)
            throw new ModuleLoadException($"table.copy: dst and src tables have different element types.", 0);

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
        
        var tableType = dstElemType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var copyMethod = tableType.GetMethod("Copy") 
            ?? throw new CompilerException($"Copy method not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, copyMethod);
    }
}
