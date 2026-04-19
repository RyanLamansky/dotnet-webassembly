using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load16Splat"/> instruction.</summary>
[TestClass]
public class V128Load16SplatTests
{
    /// <summary>Export for V128Load16Splat test.</summary>
    public abstract class V128Load16SplatExport
    {
        /// <summary>Splats a 16-bit value from address 0 to all 8 lanes, returns the lane at the given index.</summary>
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
        module.Exports.Add(new Export { Name = nameof(V128Load16SplatExport.GetLane) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // store 0x1234 at address 0
                new Int32Constant(0),
                new Int32Constant(0x1234),
                new Int32Store16(),
                // splat address 0, store at 32
                new Int32Constant(32),
                new Int32Constant(0),
                new V128Load16Splat(),
                new V128Store(),
                // return i16 at 32 + lane*2
                new Int32Constant(32),
                new LocalGet(0),
                new Int32Constant(2),
                new Int32Multiply(),
                new Int32Add(),
                new Int32Load16Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies that v128.load16_splat broadcasts a 16-bit value to all 8 lanes.</summary>
    [TestMethod]
    public void V128Load16Splat_BroadcastsToAllLanes()
    {
        var compiled = BuildModule().ToInstance<V128Load16SplatExport>();
        for (var i = 0; i < 8; i++)
            Assert.AreEqual(0x1234, compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
