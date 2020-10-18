using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Instructions;
using WebAssembly.Runtime;

namespace WebAssembly
{
    /// <summary>
    /// Tests basic functionality of <see cref="FunctionImport"/> when used with <see cref="Compile"/>.
    /// </summary>
    [TestClass]
    public class FunctionImportTests
    {
        /// <summary>
        /// Verifies that <see cref="FunctionImport"/> when used with <see cref="Compile"/> work properly together.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Compile_FunctionImport()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Returns = new[] { WebAssemblyValueType.Float64 },
                Parameters = new[] { WebAssemblyValueType.Float64, WebAssemblyValueType.Float64, }
            });
            module.Imports.Add(new Import.Function { Module = "Math", Field = "Pow", });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
                Index = 1,
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                new LocalGet(0),
                new LocalGet(1),
                new Call(0),
                new End()
                },
            });

            var compiled = module.ToInstance<CompilerTestBase2<double>>(
                new ImportDictionary {
                    { "Math", "Pow", new FunctionImport(new Func<double, double, double>(Math.Pow)) },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var instance = compiled.Exports;

            Assert.AreEqual(Math.Pow(2, 3), instance.Test(2, 3));
        }

        /// <summary>
        /// Used by <see cref="Compile_FunctionImportNoReturn"/>.
        /// </summary>
        public static class NothingDoer
        {
            /// <summary>
            /// The number of calls to <see cref="DoNothing"/> made.
            /// </summary>
            public static int Calls;

            /// <summary>
            /// Does nothing.
            /// </summary>
            /// <param name="ignored">Ignored.</param>
            public static void DoNothing(double ignored) => System.Threading.Interlocked.Increment(ref Calls);
        }

        /// <summary>
        /// Verifies that <see cref="FunctionImport"/> when used with <see cref="Compile"/> work properly together.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Compile_FunctionImportNoReturn()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[] { WebAssemblyValueType.Float64, }
            });
            module.Imports.Add(new Import.Function { Module = "Do", Field = "Nothing", });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
                Index = 1,
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                new LocalGet(0),
                new Call(0),
                new End()
                },
            });

            var compiled = module.ToInstance<CompilerTestBaseVoid<double>>(
                new ImportDictionary {
                    { "Do", "Nothing", new FunctionImport(new Action<double>(NothingDoer.DoNothing)) },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var instance = compiled.Exports;

            lock (typeof(NothingDoer))
            {
                var start = NothingDoer.Calls;
                instance.Test(2);
                Assert.AreEqual(start + 1, NothingDoer.Calls);
            }
        }

        /// <summary>
        /// Verifies that <see cref="FunctionImport"/> when used with <see cref="Compile"/> work properly together.
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void Compile_FunctionImportDelegateNoReturn()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[] { WebAssemblyValueType.Float64, }
            });
            module.Imports.Add(new Import.Function { Module = "Do", Field = "Nothing", });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = "Test",
                Index = 1,
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                new LocalGet(0),
                new Call(0),
                new End()
                },
            });

            var calls = 0;

            void doNothing(double ignored) => calls++;

            var compiled = module.ToInstance<CompilerTestBaseVoid<double>>(
                new ImportDictionary {
                    { "Do", "Nothing", new FunctionImport(new Action<double>(doNothing)) },
                });

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            var instance = compiled.Exports;

            lock (typeof(NothingDoer))
            {
                var start = calls;
                instance.Test(2);
                Assert.AreEqual(start + 1, calls);
            }
        }

        /// <summary>
        /// Tests function imports with dynamically generated code.
        /// </summary>
        [TestMethod]
        public void Compile_FunctionImportMethodBuilderIsNotBlocked()
        {
            var module = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                   new AssemblyName("CompiledWebAssembly"),
                   AssemblyBuilderAccess.RunAndCollect
                   )
                   .DefineDynamicModule("CompiledWebAssembly")
                   ;

            var dynamicClass = module.DefineType("TestClass");

            var methodBuilder = dynamicClass.DefineMethod(
                "TestMethod",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.Final | MethodAttributes.HideBySig,
                typeof(int),
                Type.EmptyTypes);

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_7);
            il.Emit(OpCodes.Ret);

            var testMethod = dynamicClass.CreateType()?.GetMethod("TestMethod")?.CreateDelegate(typeof(Func<int>));
            Assert.IsNotNull(testMethod);

            new FunctionImport(testMethod!);
        }
    }
}