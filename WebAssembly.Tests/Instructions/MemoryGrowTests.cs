using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemoryGrow"/> instruction.
/// </summary>
[TestClass]
public class MemoryGrowTests
{
    /// <summary>
    /// Assists with the test.
    /// </summary>
    public abstract class Tester
    {
        /// <summary>
        /// Runs the test.
        /// </summary>
        public abstract int Test(int value);

        /// <summary>
        /// The memory associated with the instance.
        /// </summary>
        public abstract Runtime.UnmanagedMemory Memory { get; }
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="MemoryGrow"/> instruction.
    /// </summary>
    [TestMethod]
    public void GrowMemory_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters =
            [
                    WebAssemblyValueType.Int32,
                ],
            Returns =
            [
                    WebAssemblyValueType.Int32,
                ],
        });
        module.Functions.Add(new Function
        {
        });
        module.Exports.Add(new Export
        {
            Name = "Test",
        });
        module.Exports.Add(new Export
        {
            Name = "Memory",
            Kind = ExternalKind.Memory,
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                    new LocalGet(0),
                    new MemoryGrow(),
                    new End(),
            ]
        });
        module.Memories.Add(new Memory(1, 2));

        var compiled = module.ToInstance<Tester>();

        var exports = compiled.Exports;

        Assert.AreEqual(1, exports.Test(0));
        Assert.AreEqual(1, exports.Test(1));
        Assert.AreEqual(2, exports.Test(0));
        Assert.AreEqual(-1, exports.Test(1));
    }
}
