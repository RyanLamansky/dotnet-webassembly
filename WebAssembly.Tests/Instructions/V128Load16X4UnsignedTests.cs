using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load16X4Unsigned"/> instruction.</summary>
[TestClass]
public class V128Load16X4UnsignedTests
{
    /// <summary>Export for V128Load16X4Unsigned test.</summary>
    public abstract class V128Load16X4UnsignedExport
    {
        /// <summary>Returns the i32 lane value after v128.load16x4_u.</summary>
        public abstract int GetLane(int lane);
    }

    private static Module BuildModule(ushort[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load16X4UnsignedExport.GetLane) });

        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 4; i++)
        {
            code.Add(new Int32Constant(i * 2));
            code.Add(new Int32Constant(values[i]));
            code.Add(new Int32Store16());
        }
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load16X4Unsigned());
        code.Add(new V128Store());
        code.Add(new Int32Constant(64));
        code.Add(new LocalGet(0));
        code.Add(new Int32Constant(4));
        code.Add(new Int32Multiply());
        code.Add(new Int32Add());
        code.Add(new Int32Load());
        code.Add(new End());

        module.Codes.Add(new FunctionBody { Code = code });
        return module;
    }

    /// <summary>Verifies that v128.load16x4_u zero-extends i16 to i32.</summary>
    [TestMethod]
    public void V128Load16X4Unsigned_ZeroExtends()
    {
        var values = new ushort[] { 0, 1, 32768, 65535 };
        var compiled = BuildModule(values).ToInstance<V128Load16X4UnsignedExport>();
        for (var i = 0; i < 4; i++)
            Assert.AreEqual((int)values[i], compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
