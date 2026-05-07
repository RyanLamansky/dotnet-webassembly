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
        other is TableFill tf && tf.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"Table index {TableIndex} out of range (only {context.Tables.Count} tables defined).", 0);

        var elementType = context.GetTableElementType(TableIndex);
        var table = context.GetTable(TableIndex);

        // Stack: [i, val, n] -> []
        // Pop count (n), value (val), start index (i)
        context.Stack.Pop(); // count (n)
        context.Stack.Pop(); // value (val)
        context.Stack.Pop(); // start (i)

        // Emit code to call FunctionTable.Fill(start, value, count)
        // Stack at runtime: start, value, count
        // We need to rearrange to: table, start, value, count
        
        var countLocal = context.DeclareLocal(typeof(int));
        var valueLocal = context.DeclareLocal(typeof(Delegate));
        var startLocal = context.DeclareLocal(typeof(int));
        
        // Pop in reverse order
        context.Emit(OpCodes.Stloc, countLocal);
        context.Emit(OpCodes.Stloc, valueLocal);
        context.Emit(OpCodes.Stloc, startLocal);
        
        // Load table
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, table);
        
        // Push arguments
        context.Emit(OpCodes.Ldloc, startLocal);
        context.Emit(OpCodes.Ldloc, valueLocal);
        context.Emit(OpCodes.Ldloc, countLocal);
        
        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        var fillMethod = tableType.GetMethod(nameof(FunctionTable.Fill)) 
            ?? throw new NotSupportedException($"Fill method not found on {tableType.Name}");
        
        context.Emit(OpCodes.Call, fillMethod);
    }
}
