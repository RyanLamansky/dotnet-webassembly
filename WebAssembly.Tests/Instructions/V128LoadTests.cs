using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="V128Load"/> instruction.
/// </summary>
[TestClass]
public class V128LoadTests
{
    /// <summary>Export: write 16 bytes to memory, load as v128, store back at offset 64, return byte.</summary>
    public abstract class V128LoadExport
    {
        /// <summary>
        /// Writes bytes [1..16] to address 0, loads them as v128, stores at address 64, returns byte at 64+byteOffset.
        /// </summary>
        public abstract int RoundTrip(int byteOffset);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));

        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128LoadExport.RoundTrip) });

        // Store bytes [1..16] at address 0 using v128.const + v128.store
        var constBytes = new byte[16];
        for (var i = 0; i < 16; i++) constBytes[i] = (byte)(i + 1);

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store known v128 at address 0
                new Int32Constant(0),
                new V128Const { Value = constBytes },
                new V128Store(),
                // v128.load from address 0, store at address 64
                new Int32Constant(64),
                new Int32Constant(0),
                new V128Load(),
                new V128Store(),
                // return byte at 64+byteOffset
                new Int32Constant(64),
                new LocalGet(0),
                new Int32Add(),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>
    /// Verifies that v128.load reads all 16 bytes correctly.
    /// </summary>
    [TestMethod]
    public void V128Load_ReadsAllBytes()
    {
        var compiled = BuildModule().ToInstance<V128LoadExport>();
        for (var i = 0; i < 16; i++)
            Assert.AreEqual(i + 1, compiled.Exports.RoundTrip(i));
    }
}
