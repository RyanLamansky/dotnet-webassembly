using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="V128Const"/> instruction.
/// </summary>
[TestClass]
public class V128ConstTests
{
    /// <summary>Export that returns the first byte of a v128 const via memory.store + load.</summary>
    public abstract class V128ConstExport
    {
        /// <summary>Stores the v128 constant to memory address 0 and returns byte at the given offset.</summary>
        public abstract int GetByte(int byteOffset);
    }

    private static Module BuildModule(byte[] constBytes)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));

        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128ConstExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store v128 at address 0
                new Int32Constant(0),
                new V128Const { Value = constBytes },
                new V128Store(),
                // load byte at byteOffset
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>
    /// Verifies that v128.const stores all 16 bytes correctly.
    /// </summary>
    [TestMethod]
    public void V128Const_AllBytesRoundTrip()
    {
        var bytes = new byte[16];
        for (var i = 0; i < 16; i++) bytes[i] = (byte)(i + 1);

        var compiled = BuildModule(bytes).ToInstance<V128ConstExport>();
        for (var i = 0; i < 16; i++)
            Assert.AreEqual(i + 1, compiled.Exports.GetByte(i));
    }
}
