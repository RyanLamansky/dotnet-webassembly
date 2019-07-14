using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using WebAssembly.Instructions;
using WebAssembly.Runtime;

namespace WebAssembly
{
    /// <summary>
    /// Tests basic functionality of <see cref="GlobalImport"/> when used with <see cref="Compile"/>.
    /// </summary>
    [TestClass]
    public class GlobalImportTests
    {
        /// <summary>
        /// Always returns 3, used by <see cref="Compile_GlobalImmutableImportExport"/>.
        /// </summary>
        public static int ImportedImmutableGlobalReturns3 => 3;

        /// <summary>
        /// Verifies that imported globals can be exported.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Compile_GlobalImmutableImportExport()
        {
            var module = new Module();
            module.Imports.Add(new Import.Global {
                Module = "Imported",
                Field = "Global",
                ContentType = WebAssemblyValueType.Int32,
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
                Kind = ExternalKind.Global,
            });

            var compiled = module.ToInstance<CompilerTestBaseExportedImmutableGlobal<int>>(
                new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => ImportedImmutableGlobalReturns3) },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var instance = compiled.Exports;

            Assert.AreEqual(ImportedImmutableGlobalReturns3, instance.Test);
        }

        /// <summary>
        /// Used by <see cref="Compile_GlobalMutableImportExport"/>.
        /// </summary>
        public static int MutableGlobal { get; set; }

        /// <summary>
        /// Verifies that imported globals can be exported.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Compile_GlobalMutableImportExport()
        {
            var module = new Module();
            module.Imports.Add(new Import.Global
            {
                Module = "Imported",
                Field = "Global",
                ContentType = WebAssemblyValueType.Int32,
                IsMutable = true,
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
                Kind = ExternalKind.Global,
            });

            var compiled = module.ToInstance<CompilerTestBaseExportedMutableGlobal<int>>(
                new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => MutableGlobal, value => MutableGlobal = value) },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var instance = compiled.Exports;

            Assert.AreEqual(0, instance.Test);

            const int passedThroughImport = 5;
            MutableGlobal = passedThroughImport;
            Assert.AreEqual(passedThroughImport, instance.Test);

            const int passedThroughExport = 7;
            instance.Test = passedThroughExport;
            Assert.AreEqual(passedThroughExport, MutableGlobal);
        }
    }
}