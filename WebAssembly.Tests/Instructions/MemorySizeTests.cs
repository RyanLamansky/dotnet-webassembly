using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="MemorySize"/> instruction.
    /// </summary>
    [TestClass]
    public class MemorySizeTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="MemorySize"/> instruction.
        /// </summary>
        [TestMethod]
        public void CurrentMemory_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                },
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new MemorySize(),
                    new End(),
                }
            });
            module.Memories.Add(new Memory(1, 1));

            var compiled = module.ToInstance<dynamic>();

            var exports = compiled.Exports;

            Assert.AreEqual<int>(1, exports.Test());
        }
    }
}