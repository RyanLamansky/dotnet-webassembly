using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Call a function through a typed function reference on the stack (WebAssembly 3.0 typed function references).
/// </summary>
public class CallRef : Instruction, IEquatable<CallRef>
{
    /// <summary>
    /// Always <see cref="OpCode.CallRef"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.CallRef;

    /// <summary>
    /// The index of the function type the referenced function conforms to.
    /// </summary>
    public uint Type { get; set; }

    /// <summary>
    /// Creates a new <see cref="CallRef"/> instance.
    /// </summary>
    public CallRef()
    {
    }

    /// <summary>
    /// Creates a new <see cref="CallRef"/> instance for the given function-type index.
    /// </summary>
    /// <param name="type">The index of the function type.</param>
    public CallRef(uint type) => this.Type = type;

    internal CallRef(Reader reader) => this.Type = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.CallRef);
        writer.WriteVar(this.Type);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as CallRef);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as CallRef);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(CallRef? other) => other != null && other.Type == this.Type;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.CallRef, (int)this.Type);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Type}";

    internal sealed override void Compile(CompilationContext context)
    {
        if (this.Type >= context.CheckedTypes.Length)
            throw new ModuleLoadException($"unknown type {this.Type}: the module defines only {context.CheckedTypes.Length} type(s).", 0);

        var signature = context.CheckedTypes[this.Type];
        var paramTypes = signature.RawParameterTypes;
        var returnTypes = signature.RawReturnTypes;
        var stack = context.Stack;

        // The typed function reference sits on top of the arguments; it must be a subtype of (ref null $type).
        context.PopStackNoReturn(OpCode.CallRef,
            paramTypes.Select(t => (ValType?)t).Reverse().Prepend(ValType.RefIndexed(true, this.Type)),
            paramTypes.Length + 1);

        for (var i = 0; i < returnTypes.Length; i++)
            stack.Push(returnTypes[i]);

        if (!context.DelegateCallersByTypeIndex.TryGetValue(signature.TypeIndex, out var caller))
        {
            var parms = signature.ParameterTypes;
            var returns = signature.ReturnTypes;

            if (!context.DelegateInvokersByTypeIndex.TryGetValue(signature.TypeIndex, out var invoker))
            {
                var del = context.Configuration.GetDelegateForType(parms.Length, returns.Length > 1 ? 1 : returns.Length) ??
                    throw new CompilerException($"Failed to get a delegate for type {signature}.");
                if (del.IsGenericType)
                    del = del.MakeGenericType(MultiValueHelper.DelegateTypeArgs(parms, returns));
                context.DelegateInvokersByTypeIndex.Add(signature.TypeIndex, invoker = del.GetTypeInfo().GetDeclaredMethod(nameof(Action.Invoke))!);
            }

            // (params…, Delegate target) -> results: cast the target to its concrete delegate type and invoke it.
            context.DelegateCallersByTypeIndex.Add(signature.TypeIndex, caller = context.CheckedExportsBuilder.DefineMethod(
                $"📞 {signature.TypeIndex}",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                MultiValueHelper.ClrReturnType(returns),
                [.. parms, typeof(Delegate)]));

            var il = caller.GetILGenerator();
            il.EmitLoadArg(parms.Length); // The target delegate.
            il.Emit(OpCodes.Castclass, invoker.DeclaringType!); // A null target throws NullReferenceException on invoke.
            for (var k = 0; k < parms.Length; k++)
                il.EmitLoadArg(k);
            il.Emit(OpCodes.Call, invoker);
            il.Emit(OpCodes.Ret);
        }

        context.Emit(OpCodes.Call, caller);

        // Multi-value results arrive packed in a ValueTuple; spread them back onto the stack.
        if (returnTypes.Length > 1)
            MultiValueHelper.EmitTupleUnpack(context, signature.ReturnTypes);
    }
}
