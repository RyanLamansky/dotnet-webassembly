using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load8Splat"/> instruction.</summary>
[TestClass]
public class V128Load8SplatTests
{
    /// <summary>Export for V128Load8Splat test.</summary>
    public abstract class V128Load8SplatExport
    {
        /// <summary>Splats address 0 byte to v128, returns byte at given offset.</summary>
        public abstract int GetByte(int offset);
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
        module.Exports.Add(new Export { Name = nameof(V128Load8SplatExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store 0xAB at address 0
                new Int32Constant(0),
                new Int32Constant(0xAB),
                new Int32Store8(),
                // splat address 0, store v128 at address 32
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Load8Splat(),
                new V128Store(),
                // return byte at 32+offset
                new Int32Constant(32),
                new LocalGet(0),
                new Int32Add(),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.load8_splat broadcasts a byte to all 16 lanes.</summary>
    [TestMethod]
    public void V128Load8Splat_BroadcastsToAllLanes()
    {
        var compiled = BuildModule().ToInstance<V128Load8SplatExport>();
        for (var i = 0; i < 16; i++)
            Assert.AreEqual(0xAB, compiled.Exports.GetByte(i), $"Lane {i}");
    }
}
