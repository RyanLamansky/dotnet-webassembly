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
        other is TableGrow tg && tg.TableIndex == TableIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode, (int)MiscellaneousOpCode, (int)TableIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        var (table, isFunc) = context.ResolveTable(TableIndex);
        var is64 = context.IsTable64(TableIndex);
        var indexType = context.TableIndexType(TableIndex);

        // Stack: initValue delta → old-size. table64: delta and the returned old size (or -1) are i64.
        context.PopStackNoReturn(this.OpCode, indexType); // delta
        context.PopStackNoReturn(this.OpCode, isFunc ? WebAssemblyValueType.FuncRef : WebAssemblyValueType.ExternRef); // initValue
        context.Stack.Push(indexType);

        // IL stack at entry: [..., initValue, delta]
        var delta = context.DeclareLocal(is64 ? typeof(ulong) : typeof(uint));
        if (isFunc)
        {
            // FunctionTable.Grow(Delegate? initValue, uint delta) / Grow64(Delegate? initValue, ulong delta)
            var initValue = context.DeclareLocal(typeof(Delegate));
            context.Emit(OpCodes.Stloc, delta);
            context.Emit(OpCodes.Castclass, typeof(Delegate));
            context.Emit(OpCodes.Stloc, initValue);

            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, table);
            context.Emit(OpCodes.Ldloc, initValue);
            context.Emit(OpCodes.Ldloc, delta);
            context.Emit(OpCodes.Call, is64 ? FunctionTable.Grow64Method : FunctionTable.GrowWithValueMethod);
        }
        else
        {
            // ExternRefTable.Grow(uint delta, object? value) / Grow64(ulong delta, object? value)
            var value = context.DeclareLocal(typeof(object));
            context.Emit(OpCodes.Stloc, delta);
            context.Emit(OpCodes.Stloc, value);

            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, table);
            context.Emit(OpCodes.Ldloc, delta);
            context.Emit(OpCodes.Ldloc, value);
            context.Emit(OpCodes.Call, is64 ? ExternRefTable.Grow64Method : ExternRefTable.GrowMethod);
        }

        // The 64-bit helper already returns a signed i64; the 32-bit one returns a uint reinterpreted as i32.
        if (!is64)
            context.Emit(OpCodes.Conv_I4);
    }
}
