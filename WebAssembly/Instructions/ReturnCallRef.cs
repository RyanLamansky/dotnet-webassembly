using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Tail-call a function through a typed function reference on the stack, replacing the current call frame
/// (WebAssembly 3.0 typed function references + tail calls).
/// </summary>
public class ReturnCallRef : Instruction, IEquatable<ReturnCallRef>
{
    /// <summary>
    /// Always <see cref="OpCode.ReturnCallRef"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.ReturnCallRef;

    /// <summary>
    /// The index of the function type the referenced function conforms to.
    /// </summary>
    public uint Type { get; set; }

    /// <summary>
    /// Creates a new <see cref="ReturnCallRef"/> instance.
    /// </summary>
    public ReturnCallRef()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ReturnCallRef"/> instance for the given function-type index.
    /// </summary>
    /// <param name="type">The index of the function type.</param>
    public ReturnCallRef(uint type) => this.Type = type;

    internal ReturnCallRef(Reader reader) => this.Type = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.ReturnCallRef);
        writer.WriteVar(this.Type);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as ReturnCallRef);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as ReturnCallRef);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(ReturnCallRef? other) => other != null && other.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.ReturnCallRef, (int)this.Type);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Type}";

    internal sealed override void Compile(CompilationContext context)
    {
        if (this.Type >= context.CheckedTypes.Length)
            throw new ModuleLoadException($"unknown type {this.Type}: the module defines only {context.CheckedTypes.Length} type(s).", 0);

        var signature = context.CheckedTypes[this.Type];
        var paramTypes = signature.RawParameterTypes;
        var returnTypes = signature.RawReturnTypes;

        // The callee's results must be subtypes of the enclosing function's results (the tail call returns to the
        // caller's caller). A mismatch is a "type mismatch" validation error.
        var enclosingReturns = context.CheckedSignature.RawReturnTypes;
        if (returnTypes.Length != enclosingReturns.Length
            || returnTypes.Where((t, i) => !context.IsSubtype(t, enclosingReturns[i])).Any())
            throw new ModuleLoadException(
                $"return_call_ref target result types [{string.Join(", ", returnTypes)}] do not match the enclosing function's results [{string.Join(", ", enclosingReturns)}].",
                0);

        var stackCountBefore = context.Stack.Count;

        // The typed function reference sits on top of the arguments; it must be a subtype of (ref null $type).
        context.PopStackNoReturn(this.OpCode,
            paramTypes.Select(t => (ValType?)t).Reverse().Prepend(ValType.RefIndexed(true, this.Type)),
            paramTypes.Length + 1);

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

        // Spill the arguments and the reference; discard any operand-stack values beneath them (an IL tail-call
        // requires exactly the call arguments on the stack). Operand order is [args..., ref], ref on top.
        var operandTypes = new ValType[paramTypes.Length + 1];
        Array.Copy(paramTypes, operandTypes, paramTypes.Length);
        operandTypes[paramTypes.Length] = ValType.RefIndexed(true, this.Type); // Spilled as a Delegate local.

        // Inside a protected region: spill (no beneath-pops — the leave clears the stack) and defer the tail call to a
        // per-callsite trampoline emitted after the body's final ret, outside every region.
        if (context.InProtectedRegion)
        {
            var spilled = TailCallHelper.SpillOperands(context, operandTypes, stackCountBefore, discardBeneath: false);
            var stub = context.DefineLabel();
            context.PendingFunctionStubs.Add(stubContext =>
            {
                stubContext.MarkLabel(stub);
                stubContext.Emit(OpCodes.Ldloc, spilled[paramTypes.Length]);
                stubContext.Emit(OpCodes.Castclass, invoker.DeclaringType!);
                for (var i = 0; i < paramTypes.Length; i++)
                    stubContext.Emit(OpCodes.Ldloc, spilled[i]);
                stubContext.Emit(OpCodes.Tailcall);
                stubContext.Emit(OpCodes.Call, invoker);
                stubContext.Emit(OpCodes.Ret);
            });
            context.Emit(OpCodes.Leave, stub);
            context.MarkUnreachable(functionWide: true);
            return;
        }

        var locals = TailCallHelper.SpillOperands(context, operandTypes, stackCountBefore);
        var referenceLocal = locals[paramTypes.Length];

        // Load the target (a null reference throws NullReferenceException on invoke → "null function reference").
        context.Emit(OpCodes.Ldloc, referenceLocal);
        context.Emit(OpCodes.Castclass, invoker.DeclaringType!);

        // Reload the arguments, then tail-call the delegate's Invoke directly so no intermediate frame survives.
        for (var i = 0; i < paramTypes.Length; i++)
            context.Emit(OpCodes.Ldloc, locals[i]);

        context.Emit(OpCodes.Tailcall);
        context.Emit(OpCodes.Call, invoker);
        context.Emit(OpCodes.Ret);

        // Everything after a tail call is unreachable, exactly like return.
        context.MarkUnreachable(functionWide: true);
    }
}
