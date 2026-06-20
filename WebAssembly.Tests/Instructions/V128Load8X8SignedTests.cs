using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load8X8Signed"/> instruction.</summary>
[TestClass]
public class V128Load8X8SignedTests
{
    /// <summary>Export for V128Load8X8Signed test.</summary>
    public abstract class V128Load8X8SignedExport
    {
        /// <summary>Reads a signed byte from address 0, sign-extends to i16, and returns the lane value.</summary>
        public abstract int GetLane(int lane);
    }

    private static Module BuildModule(sbyte[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load8X8SignedExport.GetLane) });

        // Store 8 bytes at address 0
        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 8; i++)
        {
            code.Add(new Int32Constant(i));
            code.Add(new Int32Constant(values[i]));
            code.Add(new Int32Store8());
        }
        // v128.load8x8_s from address 0, store result at 64
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load8X8Signed());
        code.Add(new V128Store());
        // extract lane: load 64 + lane*2 as i16
        code.Add(new Int32Constant(64));
        code.Add(new LocalGet(0));
        code.Add(new Int32Constant(2));
        code.Add(new Int32Multiply());
        code.Add(new Int32Add());
        code.Add(new Int32Load16Signed());
        code.Add(new End());

        module.Codes.Add(new FunctionBody { Code = code });
        return module;
    }

    /// <summary>Verifies that v128.load8x8_s sign-extends negative bytes.</summary>
    [TestMethod]
    public void V128Load8X8Signed_SignExtends()
    {
        var values = new sbyte[] { 1, -1, 127, -128, 0, 100, -50, 42 };
        var compiled = BuildModule(values).ToInstance<V128Load8X8SignedExport>();
        for (var i = 0; i < 8; i++)
            Assert.AreEqual((int)values[i], compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
