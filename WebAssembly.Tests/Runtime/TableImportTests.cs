using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Tests the <see cref="FunctionTable"/> class.
    /// </summary>
    [TestClass]
    public class TableImportTests
    {
        /// <summary>
        /// Tests adding a function delegate to an imported table.
        /// </summary>
        [TestMethod]
        public void Compile_TableImport_AddFunction()
        {
            var module = new Module();
            module.Types.Add(new Type
            {
                Returns = new[] { ValueType.Int32 },
                Parameters = new[] { ValueType.Int32 }
            });
            module.Imports.Add(new Import.Table
            {
                Module = "Test",
                Field = "Test",
                Definition = new Table
                {
                    ElementType = ElementType.AnyFunction,
                    ResizableLimits = new ResizableLimits(1)
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
            });
            module.Elements.Add(new Element
            {
                Elements = new uint[] { 0 },
                InitializerExpression = new Instruction[]
                {
                    new Int32Constant(0),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                new GetLocal(0),
                new End()
                },
            });

            var table = new FunctionTable(1);
            Assert.IsNull(table[0]);

            var compiled = module.ToInstance<CompilerTestBase<int>>(
                new ImportDictionary {
                    { "Test", "Test", table },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var rawDelegate = table[0];
            Assert.IsNotNull(rawDelegate);
            Assert.IsInstanceOfType(rawDelegate, typeof(Func<int, int>));
            var nativeDelegate = (Func<int, int>)rawDelegate;
            Assert.AreEqual(0, nativeDelegate(0));
            Assert.AreEqual(5, nativeDelegate(5));
        }
    }
}
