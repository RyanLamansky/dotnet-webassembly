using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load64Splat"/> instruction.</summary>
[TestClass]
public class V128Load64SplatTests
{
    /// <summary>Export for V128Load64Splat test.</summary>
    public abstract class V128Load64SplatExport
    {
        /// <summary>Splats a 64-bit value from address 0 to both lanes, returns the low 32 bits of a lane.</summary>
        public abstract int GetLaneLo(int lane);
        /// <summary>Returns the high 32 bits of a lane.</summary>
        public abstract int GetLaneHi(int lane);
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
        module.Exports.Add(new Export { Name = nameof(V128Load64SplatExport.GetLaneLo) });
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = nameof(V128Load64SplatExport.GetLaneHi), Index = 1 });

        // store 0x0102030405060708 at address 0 (little-endian)
        var lo = 0x05060708;
        var hi = 0x01020304;

        var code = new System.Collections.Generic.List<Instruction>
        {
            new Int32Constant(0),
            new Int32Constant(lo),
            new Int32Store(),
            new Int32Constant(4),
            new Int32Constant(hi),
            new Int32Store(),
            new Int32Constant(32),
            new Int32Constant(0),
            new V128Load64Splat(),
            new V128Store(),
            new Int32Constant(32),
            new LocalGet(0),
            new Int32Constant(8),
            new Int32Multiply(),
            new Int32Add(),
            new Int32Load(),
            new End(),
        };
        module.Codes.Add(new FunctionBody { Code = code });

        var code2 = new System.Collections.Generic.List<Instruction>
        {
            new Int32Constant(36),
            new LocalGet(0),
            new Int32Constant(8),
            new Int32Multiply(),
            new Int32Add(),
            new Int32Load(),
            new End(),
        };
        module.Codes.Add(new FunctionBody { Code = code2 });

        return module;
    }

    /// <summary>Verifies that v128.load64_splat broadcasts a 64-bit value to both lanes.</summary>
    [TestMethod]
    public void V128Load64Splat_BroadcastsToBothLanes()
    {
        var compiled = BuildModule().ToInstance<V128Load64SplatExport>();
        var lo = 0x05060708;
        var hi = 0x01020304;
        for (var lane = 0; lane < 2; lane++)
        {
            Assert.AreEqual(lo, compiled.Exports.GetLaneLo(lane), $"Lane {lane} lo");
            Assert.AreEqual(hi, compiled.Exports.GetLaneHi(lane), $"Lane {lane} hi");
        }
    }
}
