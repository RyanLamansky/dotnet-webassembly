using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Tail-call a function directly (WASM 3.0): the callee's results replace the caller's frame, so its result
/// types must exactly match the enclosing function's. Emitted as an IL <c>tail.</c>-prefixed call followed by
/// <c>ret</c>, which CoreCLR turns into a real tail call so deep mutual/self recursion does not grow the stack.
/// </summary>
public class ReturnCall : Instruction, IEquatable<ReturnCall>
{
    /// <summary>
    /// Always <see cref="OpCode.ReturnCall"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.ReturnCall;

    /// <summary>
    /// The index of the function to tail-call.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="ReturnCall"/> instance.
    /// </summary>
    public ReturnCall()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ReturnCall"/> instance to tail-call the function at the specified index.
    /// </summary>
    /// <param name="index">The index of the function to tail-call.</param>
    public ReturnCall(uint index)
    {
        this.Index = index;
    }

    internal ReturnCall(Reader reader)
    {
        Index = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.ReturnCall);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as ReturnCall);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as ReturnCall);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(ReturnCall? other) =>
        other != null
        && other.Index == this.Index
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

    internal sealed override void Compile(CompilationContext context)
    {
        if (this.Index >= context.CheckedFunctionSignatures.Length)
            throw new ModuleLoadException($"unknown function {this.Index}: the module defines only {context.CheckedFunctionSignatures.Length} function(s).", 0);

        var signature = context.CheckedFunctionSignatures[this.Index];
        var paramTypes = signature.RawParameterTypes;
        var returnTypes = signature.RawReturnTypes;

        // The callee's results must be subtypes of the enclosing function's results, since the tail call returns
        // straight to the caller's caller. A mismatch is a "type mismatch" validation error.
        var enclosingReturns = context.CheckedSignature.RawReturnTypes;
        if (returnTypes.Length != enclosingReturns.Length
            || returnTypes.Where((t, i) => !context.IsSubtype(t, enclosingReturns[i])).Any())
            throw new ModuleLoadException(
                $"return_call target result types [{string.Join(", ", returnTypes)}] do not match the enclosing function's results [{string.Join(", ", enclosingReturns)}].",
                0);

        var stackCountBefore = context.Stack.Count;
        context.PopStackNoReturn(this.OpCode, paramTypes.Select(t => (ValType?)t).Reverse(), paramTypes.Length);

        var target = context.CheckedMethods[this.Index];

        // Inside a protected region a `tail.` is illegal IL, and the frame must be replaced before the call so the
        // callee's throw escapes the enclosing try_table. Spill the arguments (no beneath-pops — the leave clears the
        // stack), then `leave` to a per-callsite trampoline emitted after the body's final ret, outside every region,
        // where the unchanged tail-call sequence is legal.
        if (context.InProtectedRegion)
        {
            var spilled = TailCallHelper.SpillOperands(context, paramTypes, stackCountBefore, discardBeneath: false);
            var stub = context.DefineLabel();
            context.PendingFunctionStubs.Add(stubContext =>
            {
                stubContext.MarkLabel(stub);
                foreach (var local in spilled)
                    stubContext.Emit(OpCodes.Ldloc, local);
                if (target is MethodBuilder)
                    stubContext.EmitLoadThis();
                stubContext.Emit(OpCodes.Tailcall);
                stubContext.Emit(OpCodes.Call, target);
                stubContext.Emit(OpCodes.Ret);
            });
            context.Emit(OpCodes.Leave, stub);
            context.MarkUnreachable(functionWide: true);
            return;
        }

        // Clear any operand-stack values below the arguments so only the arguments remain (an IL tail-call requirement),
        // then reload the arguments in call order.
        var argLocals = TailCallHelper.SpillOperands(context, paramTypes, stackCountBefore);
        foreach (var local in argLocals)
            context.Emit(OpCodes.Ldloc, local);

        // Dynamically generated (module-defined) functions take the exports instance as their trailing argument.
        if (target is MethodBuilder)
            context.EmitLoadThis();

        context.Emit(OpCodes.Tailcall);
        context.Emit(OpCodes.Call, target);
        context.Emit(OpCodes.Ret);

        // Everything after a tail call is unreachable, exactly like return.
        context.MarkUnreachable(functionWide: true);
    }
}
