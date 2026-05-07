using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Copy from a passive element segment into a table. Stack: [dst:i32] [src:i32] [len:i32] → []
/// </summary>
public class TableInit : MiscellaneousInstruction
{
    /// <summary>Always <see cref="MiscellaneousOpCode.TableInit"/>.</summary>
    public sealed override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.TableInit;

    /// <summary>Element segment index.</summary>
    public uint SegmentIndex { get; set; }

    /// <summary>Table index.</summary>
    public uint TableIndex { get; set; }

    /// <summary>Creates a new <see cref="TableInit"/> instance.</summary>
    public TableInit() { }

    internal TableInit(Reader reader)
    {
        SegmentIndex = reader.ReadVarUInt32();
        TableIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode);
        writer.Write((byte)MiscellaneousOpCode);
        writer.WriteVar(SegmentIndex);
        writer.WriteVar(TableIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TableInit ti && ti.SegmentIndex == SegmentIndex && ti.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode), HashCode.Combine((int)SegmentIndex, (int)TableIndex));

    internal sealed override void Compile(CompilationContext context)
    {
        // Stack: dst src len → (nothing)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // len
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // src
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dst

        if (!context.ElementSegments.TryGetValue(SegmentIndex, out var segField))
            throw new ModuleLoadException($"table.init: element segment {SegmentIndex} is not a passive segment or does not exist.", 0);

        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"table.init: table index {TableIndex} out of range.", 0);

        var table = context.GetTable(TableIndex);
        var tableElemType = context.GetTableElementType(TableIndex);
        
        if (context.ElementSegmentTypes.TryGetValue(SegmentIndex, out var segElemType) && segElemType != tableElemType)
            throw new ModuleLoadException($"table.init: type mismatch between element segment {SegmentIndex} ({segElemType}) and table {TableIndex} ({tableElemType}).", 0);

        var len = context.DeclareLocal(typeof(uint));
        var src = context.DeclareLocal(typeof(uint));
        var dst = context.DeclareLocal(typeof(uint));

        context.Emit(OpCodes.Stloc, len);
        context.Emit(OpCodes.Stloc, src);
        context.Emit(OpCodes.Stloc, dst);

        // this.table.InitFromSegment(dst, this.segField, src, len)
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        context.Emit(OpCodes.Ldloc, dst);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, segField);
        context.Emit(OpCodes.Ldloc, src);
        context.Emit(OpCodes.Ldloc, len);
        
        var tableType = tableElemType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var initMethod = tableType.GetMethod("InitFromSegment") 
            ?? throw new CompilerException($"InitFromSegment method not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, initMethod);
    }
}
