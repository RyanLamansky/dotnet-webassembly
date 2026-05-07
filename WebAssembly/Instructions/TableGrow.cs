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
        if (TableIndex >= (uint)context.Tables.Count)
            throw new ModuleLoadException($"Table index {TableIndex} out of range (only {context.Tables.Count} tables defined).", 0);

        var elementType = context.GetTableElementType(TableIndex);
        var table = context.GetTable(TableIndex);
        var stackType = elementType == ElementType.FunctionReference ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef;

        // Stack: initValue delta → old-size
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // delta
        context.PopStackNoReturn(this.OpCode, stackType); // initValue (any ref type)
        context.Stack.Push(WebAssemblyValueType.Int32);

        // IL stack: [..., initValue, delta]
        // Need to call: table.Grow(initValue, delta)
        // For FunctionTable: Grow(Delegate? initValue, uint delta)
        // For ExternRefTable: Grow(uint delta, object? value) - signature is backwards!
        
        var tableType = elementType == ElementType.FunctionReference ? typeof(FunctionTable) : typeof(ExternRefTable);
        
        if (elementType == ElementType.FunctionReference)
        {
            // FunctionTable.Grow(Delegate? initValue, uint delta)
            // IL stack: [..., initValue:object, delta:i32]
            // Swap them so delta is on top, then store both
            var delta = context.DeclareLocal(typeof(uint));
            var initValue = context.DeclareLocal(typeof(Delegate));
            
            context.Emit(OpCodes.Stloc, delta);      // store delta
            context.Emit(OpCodes.Castclass, typeof(Delegate)); // cast initValue to Delegate
            context.Emit(OpCodes.Stloc, initValue);  // store initValue
            
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, table);
            context.Emit(OpCodes.Ldloc, initValue);  // load initValue
            context.Emit(OpCodes.Ldloc, delta);      // load delta
            
            var growMethod = typeof(FunctionTable).GetMethod("Grow") 
                ?? throw new NotSupportedException($"Grow method not found on FunctionTable");
            context.Emit(OpCodes.Call, growMethod);
        }
        else
        {
            // ExternRefTable.Grow(uint delta, object? value)
            // IL stack: [..., value:object, delta:i32]
            // Need: table, delta, value
            var delta = context.DeclareLocal(typeof(uint));
            var value = context.DeclareLocal(typeof(object));
            
            context.Emit(OpCodes.Stloc, delta);   // store delta
            context.Emit(OpCodes.Stloc, value);   // store value
            
            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, table);
            context.Emit(OpCodes.Ldloc, delta);   // load delta
            context.Emit(OpCodes.Ldloc, value);   // load value
            
            var growMethod = typeof(ExternRefTable).GetMethod("Grow") 
                ?? throw new NotSupportedException($"Grow method not found on ExternRefTable");
            context.Emit(OpCodes.Call, growMethod);
        }
        
        context.Emit(OpCodes.Conv_I4);
    }
}
