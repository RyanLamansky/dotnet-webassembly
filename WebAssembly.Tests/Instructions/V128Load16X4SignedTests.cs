using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128Load16X4Signed"/> instruction.</summary>
[TestClass]
public class V128Load16X4SignedTests
{
    /// <summary>Export for V128Load16X4Signed test.</summary>
    public abstract class V128Load16X4SignedExport
    {
        /// <summary>Returns the i32 lane value after v128.load16x4_s.</summary>
        public abstract int GetLane(int lane);
    }

    private static Module BuildModule(short[] values)
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128Load16X4SignedExport.GetLane) });

        var code = new System.Collections.Generic.List<Instruction>();
        for (var i = 0; i < 4; i++)
        {
            code.Add(new Int32Constant(i * 2));
            code.Add(new Int32Constant(values[i]));
            code.Add(new Int32Store16());
        }
        code.Add(new Int32Constant(64));
        code.Add(new Int32Constant(0));
        code.Add(new V128Load16X4Signed());
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

    /// <summary>Verifies that v128.load16x4_s sign-extends i16 to i32.</summary>
    [TestMethod]
    public void V128Load16X4Signed_SignExtends()
    {
        var values = new short[] { 1, -1, 32767, -32768 };
        var compiled = BuildModule(values).ToInstance<V128Load16X4SignedExport>();
        for (var i = 0; i < 4; i++)
            Assert.AreEqual((int)values[i], compiled.Exports.GetLane(i), $"Lane {i}");
    }
}
