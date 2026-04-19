using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load32X2Unsigned"/> instruction.</summary>
[TestClass]
public class V128Load32X2UnsignedTests
{
    /// <summary>Export for V128Load32X2Unsigned test.</summary>
    public abstract class V128Load32X2UnsignedExport
    {
        /// <summary>Returns the low 32 bits of the i64 lane after v128.load32x2_u.</summary>
        public abstract int GetLaneLo(int lane);
        /// <summary>Returns the high 32 bits of the i64 lane (should always be 0).</summary>
        public abstract int GetLaneHi(int lane);
    }

    private static Module BuildModule(uint[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load32X2UnsignedExport.GetLaneLo) });
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = nameof(V128Load32X2UnsignedExport.GetLaneHi), Index = 1 });

        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 2; i++)
        {
            code.Add(new Int32Constant(i * 4));
            code.Add(new Int32Constant((int)values[i]));
            code.Add(new Int32Store());
        }
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load32X2Unsigned());
        code.Add(new V128Store());
        code.Add(new Int32Constant(64));
        code.Add(new LocalGet(0));
        code.Add(new Int32Constant(8));
        code.Add(new Int32Multiply());
        code.Add(new Int32Add());
        code.Add(new Int32Load());
        code.Add(new End());

        module.Codes.Add(new FunctionBody { Code = code });

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

    /// <summary>Verifies that v128.load32x2_u zero-extends i32 to i64.</summary>
    [TestMethod]
    public void V128Load32X2Unsigned_ZeroExtends()
    {
        var values = new uint[] { 0xFFFF_FFFFu, 42u };
        var compiled = BuildModule(values).ToInstance<V128Load32X2UnsignedExport>();
        Assert.AreEqual(unchecked((int)0xFFFF_FFFFu), compiled.Exports.GetLaneLo(0), "lane0 lo");
        Assert.AreEqual(0, compiled.Exports.GetLaneHi(0), "lane0 hi (zero extension)");
        Assert.AreEqual(42, compiled.Exports.GetLaneLo(1), "lane1 lo");
        Assert.AreEqual(0, compiled.Exports.GetLaneHi(1), "lane1 hi");
    }
}
