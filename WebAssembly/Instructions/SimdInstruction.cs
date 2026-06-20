using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// Base class for WASM SIMD instructions (prefix byte 0xFD).
/// </summary>
public abstract class SimdInstruction : Instruction
{
    private protected SimdInstruction()
    {
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> executeMethods = new();

    /// <summary>
    /// Resolves the <c>public static Execute</c> method that a compiled SIMD instruction calls.
    /// The method must be public so the emitted assembly (which is neither a subclass nor a friend) can call it.
    /// </summary>
    internal static MethodInfo ExecuteMethod(Type instructionType) =>
        executeMethods.GetOrAdd(instructionType, static type =>
            type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"{type} is missing a public static Execute method."));

    /// <summary>
    /// Always <see cref="OpCode.SimdOperationPrefix"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.SimdOperationPrefix;

    /// <summary>
    /// Gets the <see cref="SimdOpCode"/> that identifies this instruction.
    /// </summary>
    public abstract SimdOpCode SimdOpCode { get; }

    internal override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        writer.WriteVar((uint)this.SimdOpCode);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    public override bool Equals(Instruction? other) =>
        other is SimdInstruction instruction
        && instruction.SimdOpCode == this.SimdOpCode;

    /// <summary>
    /// Returns a hash code based on the SIMD opcode.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.SimdOpCode);

    /// <summary>
    /// Returns the native WebAssembly name of this instruction.
    /// </summary>
    public sealed override string ToString() => this.SimdOpCode.ToNativeName();
}
