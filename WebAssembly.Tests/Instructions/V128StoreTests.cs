using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="V128Store"/> instruction.
/// </summary>
[TestClass]
public class V128StoreTests
{
    /// <summary>Export: store a v128 constant, read a byte back.</summary>
    public abstract class V128StoreExport
    {
        /// <summary>Stores a 16-byte vector [10,20,...,160] at address 0 and returns the byte at byteOffset.</summary>
        public abstract int GetByte(int byteOffset);
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
        module.Exports.Add(new Export { Name = nameof(V128StoreExport.GetByte) });

        var constBytes = new byte[16];
        for (var i = 0; i < 16; i++) constBytes[i] = (byte)((i + 1) * 10);

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = constBytes },
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>
    /// Verifies that v128.store writes all 16 bytes correctly.
    /// </summary>
    [TestMethod]
    public void V128Store_WritesAllBytes()
    {
        var compiled = BuildModule().ToInstance<V128StoreExport>();
        for (var i = 0; i < 16; i++)
            Assert.AreEqual((i + 1) * 10, compiled.Exports.GetByte(i));
    }
}
