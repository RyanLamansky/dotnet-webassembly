using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="CallIndirect"/> instruction.
    /// </summary>
    [TestClass]
    public class CallIndirectTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="CallIndirect"/> instruction.
        /// </summary>
        [TestMethod]
        public void CallIndirect_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[]
                {
                    WebAssemblyValueType.Int32,
                },
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new WebAssemblyValueType[]
                {
                },
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Functions.Add(new Function
            {
                Type = 1,
            });
            module.Functions.Add(new Function
            {
                Type = 1,
            });
            module.Exports.Add(new Export
            {
                Name = nameof(CompilerTestBase<int>.Test)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new CallIndirect(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new Int32Constant(5),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                 {
                    new Int32Constant(6),
                    new End(),
                 },
            });
            module.Tables.Add(new Table(2, 2));
            module.Elements.Add(new Element(0, 1, 2));

            var compiled = module.ToInstance<CompilerTestBase<int>>();

            var exports = compiled.Exports;
            Assert.AreEqual(5, exports.Test(0));
            Assert.AreEqual(6, exports.Test(1));
        }

        /// <summary>
        /// Tests the <see cref="CallIndirect.Equals(Instruction)"/> and <see cref="CallIndirect.GetHashCode()"/> methods.
        /// </summary>
        [TestMethod]
        public void CallIndirect_Equals()
        {
            TestUtility.CreateInstances<CallIndirect>(out var a, out var b);

            a.Type = 1;
            TestUtility.AreNotEqual(a, b);
            b.Type = 1;
            TestUtility.AreEqual(a, b);

            a.Reserved = 1;
            TestUtility.AreNotEqual(a, b);
            b.Reserved = 1;
            TestUtility.AreEqual(a, b);
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="CallIndirect"/> instruction with an element referencing an imported function.
        /// </summary>
        [TestMethod]
        public void CallIndirect_Compiled_ElementReferencesImportedFunction()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[]
                {
                    WebAssemblyValueType.Int32,
                },
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Imports.Add(new Import.Function { Module = "Test", Field = "Function", });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = nameof(CompilerTestBase<int>.Test),
                Index = 1,
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new Int32Constant(0),
                    new CallIndirect(0),
                    new End(),
                },
            });
            module.Tables.Add(new Table(1));
            module.Elements.Add(new Element(0, 0));

            var calls = 0;
            var compiled = module.ToInstance<CompilerTestBase<int>>(new ImportDictionary
            {
                { "Test", "Function", new FunctionImport(new Func<int, int>(value => { calls++; return value; })) },
            });

            var exports = compiled.Exports;
            Assert.AreEqual(0, exports.Test(0));
            Assert.AreEqual(1, calls);
            Assert.AreEqual(1, exports.Test(1));
            Assert.AreEqual(2, calls);
        }
    }
}