using System.Collections.Generic;

namespace WebAssembly.Instructions;

/// <summary>
/// Shared scaffold for the SIMD per-instruction execution tests that store a <c>v128</c> result to linear
/// memory and read it back a byte at a time.
/// </summary>
public static class SimdTestRunner
{
    /// <summary>Reads individual bytes of a <c>v128</c> result stored at linear-memory address 0.</summary>
    public abstract class ByteReader
    {
        /// <summary>Returns the unsigned byte at the given offset of the stored 16-byte result.</summary>
        /// <param name="offset">The byte offset into the result.</param>
        /// <returns>The unsigned byte value at <paramref name="offset"/>.</returns>
        public abstract int GetByte(int offset);
    }

    /// <summary>
    /// Compiles a single-page-memory module whose exported <see cref="ByteReader.GetByte(int)"/> stores the
    /// <c>v128</c> left on the stack by <paramref name="compute"/> to address 0, then returns the byte at the requested offset.
    /// </summary>
    /// <param name="compute">Instructions that leave a single <c>v128</c> value on the stack.</param>
    /// <returns>The compiled export instance.</returns>
    public static ByteReader Compile(params System.ReadOnlySpan<Instruction> compute)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(ByteReader.GetByte) });
        var code = new List<Instruction>(compute.Length + 5) { new Int32Constant(0) };
        code.AddRange(compute);
        code.Add(new V128Store());
        code.Add(new LocalGet(0));
        code.Add(new Int32Load8Unsigned());
        code.Add(new End());
        module.Codes.Add(new FunctionBody { Code = code });
        return module.ToInstance<ByteReader>().Exports;
    }
}
