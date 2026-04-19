using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load32Splat"/> instruction.</summary>
[TestClass]
public class V128Load32SplatTests
{
    /// <summary>Export for V128Load32Splat test.</summary>
    public abstract class V128Load32SplatExport
    {
        /// <summary>Splats a 32-bit value from address 0 to all 4 lanes, returns the lane at the given index.</summary>
        public abstract int GetLane(int lane);
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
        module.Exports.Add(new Export { Name = nameof(V128Load32SplatExport.GetLane) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0x1234_5678),
                new Int32Store(),
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Load32Splat(),
                new V128Store(),
                new Int32Constant(32),
                new LocalGet(0),
                new Int32Constant(4),
                new Int32Multiply(),
                new Int32Add(),
                new Int32Load(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.load32_splat broadcasts a 32-bit value to all 4 lanes.</summary>
    [TestMethod]
    public void V128Load32Splat_BroadcastsToAllLanes()
    {
        var compiled = BuildModule().ToInstance<V128Load32SplatExport>();
        for (var i = 0; i < 4; i++)
            Assert.AreEqual(0x1234_5678, compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
