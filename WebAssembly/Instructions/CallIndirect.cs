using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Call function indirectly.
/// </summary>
public class CallIndirect : Instruction, IEquatable<CallIndirect>
{
    /// <summary>
    /// Always <see cref="OpCode.CallIndirect"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.CallIndirect;

    /// <summary>
    /// The index of the type representing the function signature.
    /// </summary>
    public uint Type { get; set; }

    /// <summary>
    /// The index of the table from which the indirect call selects its target, encoded as a <c>varuint32</c>.
    /// Before WebAssembly 2.0 reference types this was a reserved zero byte; non-zero values (multiple tables)
    /// are honored by the compiler.
    /// </summary>
    public uint Table { get; set; }

    /// <summary>
    /// Obsolete alias for <see cref="Table"/>, retained for source compatibility from when this was a reserved
    /// zero byte (before WebAssembly 2.0).
    /// </summary>
    [Obsolete("Use Table; this field is the table index.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public byte Reserved
    {
        get => (byte)this.Table;
        set => this.Table = value;
    }

    /// <summary>
    /// Creates a new  <see cref="CallIndirect"/> instance.
    /// </summary>
    public CallIndirect()
    {
    }

    /// <summary>
    /// Creates a new  <see cref="CallIndirect"/> instance.
    /// </summary>
    /// <param name="type">The index of the type representing the function signature.</param>
    public CallIndirect(uint type)
    {
        this.Type = type;
    }

    internal CallIndirect(Reader reader)
    {
        Type = reader.ReadVarUInt32();
        Table = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.CallIndirect);
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Table);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as CallIndirect);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as CallIndirect);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(CallIndirect? other) =>
        other != null
        && other.Type == this.Type
        && other.Table == this.Table
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.CallIndirect, (int)this.Type, (int)this.Table);

    internal sealed override void Compile(CompilationContext context)
    {
        var signature = context.CheckedTypes[this.Type];
        var paramTypes = signature.RawParameterTypes;
        var returnTypes = signature.RawReturnTypes;

        // call_indirect requires a table whose element type is in the function hierarchy (a subtype of funcref);
        // an external-reference table is a "type mismatch".
        if (!context.IsFuncTable(this.Table))
            throw new ModuleLoadException($"call_indirect requires a function-reference table; table {this.Table} holds {context.TableElementValType(this.Table).ToNarrow()}.", 0);

        var stack = context.Stack;

        // The element index on top of the stack is i64 for a table64 target, i32 otherwise.
        var isTable64 = context.IsTable64(this.Table);
        context.PopStackNoReturn(OpCode.CallIndirect, paramTypes.Select(t => (ValType?)t).Reverse().Prepend(context.TableIndexType(this.Table)), paramTypes.Length + 1);

        for (var i = 0; i < returnTypes.Length; i++)
            stack.Push(returnTypes[i]);

        context.EmitLoadThis();

        // The second immediate is the table index (WASM 2.0); remappers are cached per (type, table) since
        // they load from a specific table field.
        if (!context.DelegateRemappersByType.TryGetValue((signature.TypeIndex, this.Table), out var remapper))
        {
            var parms = signature.ParameterTypes;
            var returns = signature.ReturnTypes;

            if (!context.DelegateInvokersByTypeIndex.TryGetValue(signature.TypeIndex, out var invoker))
            {
                // Two-or-more results map onto a delegate with a single (ValueTuple) return.
                var del = context.Configuration.GetDelegateForType(parms.Length, returns.Length > 1 ? 1 : returns.Length) ??
                    throw new CompilerException($"Failed to get a delegate for type {signature}.");
                if (del.IsGenericType)
                    del = del.MakeGenericType(MultiValueHelper.DelegateTypeArgs(parms, returns));
                context.DelegateInvokersByTypeIndex.Add(signature.TypeIndex, invoker = del.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke))!);
            }

            context.DelegateRemappersByType.Add((signature.TypeIndex, this.Table), remapper = context.CheckedExportsBuilder.DefineMethod(
                $"🔁 {signature.TypeIndex}_{this.Table}",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                MultiValueHelper.ClrReturnType(returns),
                // table64: the index argument is a u64, resolved with the trapping Get64 helper.
                [.. parms, isTable64 ? typeof(ulong) : typeof(uint), context.CheckedExportsBuilder]
                ));

            var il = remapper.GetILGenerator();
            il.EmitLoadArg(parms.Length + 1);
            il.Emit(OpCodes.Ldfld, context.GetTable(this.Table));
            il.EmitLoadArg(parms.Length);
            il.Emit(OpCodes.Call, isTable64 ? FunctionTable.Get64Method : FunctionTable.IndexGetter);

            // Guard the arity castclass with the canonical function-type check. Two canonically-distinct func types
            // can share a CLR delegate shape (differently-shaped rec groups, or (ref $a)/(ref $b) params both lowering
            // to object), so the castclass alone would wrongly succeed; the guard traps ("indirect call type
            // mismatch") when the selected function's registered canonical type is not a subtype of this call's
            // declared type. A null or unregistered (host) delegate is left to the castclass and the subsequent
            // invoke. The expected type is a token interned once at instantiation and loaded from the exports
            // instance's FuncTypeTokens field, so no descriptor is re-interned per call.
            if (context.CanonicalTypes is { } map && this.Type < (uint)map.Descriptor.Length)
            {
                var tokenIndex = context.GetFuncTypeTokenIndex(map.Descriptor[this.Type]);
                il.Emit(OpCodes.Dup);
                il.EmitLoadArg(parms.Length + 1); // the exports instance
                il.Emit(OpCodes.Ldfld, context.FuncTypeTokens!);
                il.EmitLoadConstant(tokenIndex);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Call, context.Configuration.NeutralizeType(typeof(WebAssembly.Runtime.WebAssemblyGC))
                    .GetMethod(nameof(WebAssembly.Runtime.WebAssemblyGC.CheckIndirectCallType))!);
            }

            il.Emit(OpCodes.Castclass, invoker.DeclaringType!);

            for (var k = 0; k < parms.Length; k++)
                il.EmitLoadArg(k);

            il.Emit(OpCodes.Call, invoker);
            il.Emit(OpCodes.Ret);
        }

        context.Emit(OpCodes.Call, remapper);

        // Multi-value results arrive packed in a ValueTuple; spread them back onto the stack.
        if (returnTypes.Length > 1)
            MultiValueHelper.EmitTupleUnpack(context, signature.ReturnTypes);
    }
}
