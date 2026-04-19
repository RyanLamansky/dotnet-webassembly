using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load32X2Signed"/> instruction.</summary>
[TestClass]
public class V128Load32X2SignedTests
{
    /// <summary>Export for V128Load32X2Signed test.</summary>
    public abstract class V128Load32X2SignedExport
    {
        /// <summary>Returns the low 32 bits of the i64 lane after v128.load32x2_s.</summary>
        public abstract int GetLaneLo(int lane);
        /// <summary>Returns the high 32 bits of the i64 lane after v128.load32x2_s (sign bits).</summary>
        public abstract int GetLaneHi(int lane);
    }

    private static Module BuildModule(int[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load32X2SignedExport.GetLaneLo) });
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = nameof(V128Load32X2SignedExport.GetLaneHi), Index = 1 });

        // Store 2 i32 values at address 0
        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 2; i++)
        {
            code.Add(new Int32Constant(i * 4));
            code.Add(new Int32Constant(values[i]));
            code.Add(new Int32Store());
        }
        // v128.load32x2_s, store at 64
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load32X2Signed());
        code.Add(new V128Store());
        // GetLaneLo: low 4 bytes of lane = 64 + lane*8
        code.Add(new Int32Constant(64));
        code.Add(new LocalGet(0));
        code.Add(new Int32Constant(8));
        code.Add(new Int32Multiply());
        code.Add(new Int32Add());
        code.Add(new Int32Load());
        code.Add(new End());

        module.Codes.Add(new FunctionBody { Code = code });

        // GetLaneHi: high 4 bytes = 68 + lane*8
        var code2 = new System.Collections.Generic.List<Instruction>
        {
            new Int32Constant(68),
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

    /// <summary>Verifies that v128.load32x2_s sign-extends i32 to i64.</summary>
    [TestMethod]
    public void V128Load32X2Signed_SignExtends()
    {
        var values = new int[] { -1, 42 };
        var compiled = BuildModule(values).ToInstance<V128Load32X2SignedExport>();
        // -1 → 0xFFFF_FFFF_FFFF_FFFF; lo=-1, hi=-1
        Assert.AreEqual(-1, compiled.Exports.GetLaneLo(0), "lane0 lo");
        Assert.AreEqual(-1, compiled.Exports.GetLaneHi(0), "lane0 hi (sign extension)");
        // 42 → 0x0000_0000_0000_002A; lo=42, hi=0
        Assert.AreEqual(42, compiled.Exports.GetLaneLo(1), "lane1 lo");
        Assert.AreEqual(0, compiled.Exports.GetLaneHi(1), "lane1 hi");
    }
}
