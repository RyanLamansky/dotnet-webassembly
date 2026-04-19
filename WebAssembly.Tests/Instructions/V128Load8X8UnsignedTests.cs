using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load8X8Unsigned"/> instruction.</summary>
[TestClass]
public class V128Load8X8UnsignedTests
{
    /// <summary>Export for V128Load8X8Unsigned test.</summary>
    public abstract class V128Load8X8UnsignedExport
    {
        /// <summary>Reads a byte from address 0, zero-extends to i16, and returns the lane value.</summary>
        public abstract int GetLane(int lane);
    }

    private static Module BuildModule(byte[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load8X8UnsignedExport.GetLane) });

        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 8; i++)
        {
            code.Add(new Int32Constant(i));
            code.Add(new Int32Constant(values[i]));
            code.Add(new Int32Store8());
        }
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load8X8Unsigned());
        code.Add(new V128Store());
        code.Add(new Int32Constant(64));
        code.Add(new LocalGet(0));
        code.Add(new Int32Constant(2));
        code.Add(new Int32Multiply());
        code.Add(new Int32Add());
        code.Add(new Int32Load16Unsigned());
        code.Add(new End());

        module.Codes.Add(new FunctionBody { Code = code });
        return module;
    }

    /// <summary>Verifies that v128.load8x8_u zero-extends bytes (0xFF stays 255).</summary>
    [TestMethod]
    public void V128Load8X8Unsigned_ZeroExtends()
    {
        var values = new byte[] { 0, 1, 127, 128, 200, 255, 42, 100 };
        var compiled = BuildModule(values).ToInstance<V128Load8X8UnsignedExport>();
        for (var i = 0; i < 8; i++)
            Assert.AreEqual((int)values[i], compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
