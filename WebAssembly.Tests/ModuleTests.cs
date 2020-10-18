using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly
{
    /// <summary>
    /// Tests the <see cref="Module"/> class.
    /// </summary>
    [TestClass]
    public class ModuleTests
    {
        /// <summary>
        /// Tests the <see cref="Module.ReadFromBinary(Stream)"/> method.
        /// </summary>
        [TestMethod]
        public void Module_ReadFromBinaryStream()
        {
            Assert.AreEqual("input", Assert.ThrowsException<ArgumentNullException>(() => Module.ReadFromBinary((Stream)null!)).ParamName);

            using (var sample = new MemoryStream())
            {
                var utf8 = new UTF8Encoding(false, false);

                using (var writer = new BinaryWriter(sample, utf8, true))
                {
                    writer.Write(0x6e736100); //Bad magic number.
                }
                sample.Position = 0;
                Assert.IsTrue(Assert.ThrowsException<ModuleLoadException>(() => Module.ReadFromBinary(sample)).Message.ToLowerInvariant().Contains("magic"));
                Assert.IsTrue(sample.CanSeek, "Stream was closed but should have been left open.");

                sample.Position = 0;
                using (var writer = new BinaryWriter(sample, utf8, true))
                {
                    writer.Write(0x6d736100);
                    //Missing version.
                }
                sample.Position = 0;
                Assert.IsInstanceOfType(Assert.ThrowsException<ModuleLoadException>(() => Module.ReadFromBinary(sample)).InnerException, typeof(EndOfStreamException));

                sample.Position = 0;
                using (var writer = new BinaryWriter(sample, utf8, true))
                {
                    writer.Write(0x6d736100);
                    writer.Write(0x0); //Bad version
                }
                sample.Position = 0;
                Assert.IsTrue(Assert.ThrowsException<ModuleLoadException>(() => Module.ReadFromBinary(sample)).Message.ToLowerInvariant().Contains("version"));

                sample.Position = 0;
                using (var writer = new BinaryWriter(sample, utf8, true))
                {
                    //Shouldn't fail, this is the bare minimum WASM binary file.
                    writer.Write(0x6d736100);
                    writer.Write(0x1);
                }
                sample.Position = 0;
                Assert.IsNotNull(Module.ReadFromBinary(sample));

                sample.Position = 0;
                using (var writer = new BinaryWriter(sample, utf8, true))
                {
                    //Shouldn't fail, this is the bare minimum WASM binary file.
                    writer.Write(0x6d736100);
                    writer.Write(0xd); //Pre-release version, binary format is otherwise identical to first release.
                }
                sample.Position = 0;
                Assert.IsNotNull(Module.ReadFromBinary(sample));
            }
        }

        /// <summary>
        /// Ensures that <see cref="CustomSection"/>s are both written and readable.
        /// </summary>
        [TestMethod]
        public void Module_CustomSectionRoundTrip()
        {
            var content = BitConverter.DoubleToInt64Bits(Math.PI);
            var toWrite = new Module();
            toWrite.CustomSections.Add(new CustomSection
            {
                Content = BitConverter.GetBytes(content),
                Name = "Test",
            });

            Module toRead;
            using (var memory = new MemoryStream())
            {
                toWrite.WriteToBinary(memory);
                memory.Position = 0;

                toRead = Module.ReadFromBinary(memory);
            }

            Assert.IsNotNull(toRead);
            Assert.AreNotSame(toWrite, toRead);
            Assert.IsNotNull(toRead.CustomSections);
            Assert.AreEqual(1, toRead.CustomSections.Count);

            var custom = toRead.CustomSections[0];
            Assert.IsNotNull(custom);
            Assert.AreEqual("Test", custom.Name);
            Assert.IsNotNull(custom.Content);
            Assert.AreEqual(8, custom.Content.Count);
            Assert.AreEqual(content, BitConverter.ToInt64(custom.Content.ToArray(), 0));
        }

        /// <summary>
        /// Verifies that the import types (<see cref="ExternalKind.Function"/>, <see cref="ExternalKind.Table"/>, <see cref="ExternalKind.Memory"/>, and <see cref="ExternalKind.Global"/> are both written and readable.
        /// </summary>
        [TestMethod]
        public void Module_ImportRoundTrip()
        {
            var source = new Module
            {
                Imports = new Import[]
                {
                    new Import.Function
                    {
                        Module = "A",
                        Field = "1",
                        TypeIndex = 2,
                    },
                    new Import.Table
                    {
                        Module = "B",
                        Field = "2",
                        Definition = new Table
                        {
                            ElementType = ElementType.FunctionReference,
                            ResizableLimits = new ResizableLimits(1, 2),
                        },
                    },
                    new Import.Memory
                    {
                        Module = "C",
                        Field = "3",
                        Type = new Memory(4, 5),
                    },
                    new Import.Global
                    {
                        Module = "D",
                        Field = "4",
                    },
                }
            };

            Module destination;
            using (var stream = new MemoryStream())
            {
                source.WriteToBinary(stream);
                stream.Position = 0;

                destination = Module.ReadFromBinary(stream);
            }

            Assert.IsNotNull(destination);
            Assert.AreNotSame(source, destination);
            Assert.IsNotNull(destination.Imports);
            var imports = destination.Imports;
            Assert.AreNotSame(source.Imports, imports);
            Assert.AreEqual(4, imports.Count);

            Assert.IsInstanceOfType(imports[0], typeof(Import.Function));
            {
                var function = (Import.Function)imports[0];
                Assert.AreEqual("A", function.Module);
                Assert.AreEqual("1", function.Field);
                Assert.AreEqual(2u, function.TypeIndex);
            }

            Assert.IsInstanceOfType(imports[1], typeof(Import.Table));
            {
                var table = (Import.Table)imports[1];
                Assert.AreEqual("B", table.Module);
                Assert.AreEqual("2", table.Field);
                Assert.IsNotNull(table.Definition);
                Assert.AreEqual(ElementType.FunctionReference, table.Definition!.ElementType);
                Assert.IsNotNull(table.Definition.ResizableLimits);
                Assert.AreEqual(1u, table.Definition.ResizableLimits.Minimum);
                Assert.AreEqual(2u, table.Definition.ResizableLimits.Maximum.GetValueOrDefault());
            }

            Assert.IsInstanceOfType(imports[2], typeof(Import.Memory));
            {
                var memory = (Import.Memory)imports[2];
                Assert.AreEqual("C", memory.Module);
                Assert.AreEqual("3", memory.Field);
                Assert.IsNotNull(memory.Type);
                Assert.IsNotNull(memory.Type!.ResizableLimits);
                Assert.AreEqual(4u, memory.Type.ResizableLimits.Minimum);
                Assert.AreEqual(5u, memory.Type.ResizableLimits.Maximum.GetValueOrDefault());
            }

            Assert.IsInstanceOfType(imports[3], typeof(Import.Global));
            {
                var global = (Import.Global)imports[3];
                Assert.AreEqual("D", global.Module);
                Assert.AreEqual("4", global.Field);
            }
        }

        /// <summary>
        /// Verifies that <see cref="Module.Types"/> contents are round-tripped correctly.
        /// </summary>
        [TestMethod]
        public void Module_TypeRoundTrip()
        {
            var source = new Module
            {
                Types = new WebAssemblyType[]
                {
                    new WebAssemblyType
                    {
                        Parameters = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float32 },
                        Returns = new[] { WebAssemblyValueType.Int64 }
                    }
                }
            }; ;

            Module destination;
            using (var stream = new MemoryStream())
            {
                source.WriteToBinary(stream);
                stream.Position = 0;

                destination = Module.ReadFromBinary(stream);
            }

            Assert.IsNotNull(destination.Types);
            Assert.AreNotSame(source.Types, destination.Types);
            Assert.AreEqual(1, destination.Types.Count);
            Assert.IsTrue(source.Types[0].Equals(destination.Types[0]));
        }

        /// <summary>
        /// Verifies that <see cref="Module.Codes"/> contents are round-tripped correctly.
        /// </summary>
        [TestMethod]
        public void Module_FunctionBodyRoundTrip()
        {
            var source = new Module
            {
                Codes = new[]
                {
                    new FunctionBody
                    {
                        Locals = new[]
                        {
                            new Local
                            {
                                Count  = 2,
                                Type = WebAssemblyValueType.Float64
                            }
                        },
                        Code = new Instruction[]
                        {
                            new Instructions.End()
                        }
                    }
                }
            };

            Module destination;
            using (var stream = new MemoryStream())
            {
                source.WriteToBinary(stream);
                stream.Position = 0;

                destination = Module.ReadFromBinary(stream);
            }

            Assert.IsNotNull(destination.Codes);
            Assert.AreNotSame(source.Codes, destination.Codes);
            TestUtility.AreEqual(source.Codes[0], destination.Codes[0]);
        }

        /// <summary>
        /// Verifies that the module writing process prevents an attempt to write an unreadable assembly that contains an instruction sequence that doesn't end with <see cref="OpCode.End"/>.
        /// </summary>
        [TestMethod]
        public void Module_InstructionSequenceMissingEndValidation()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new Module { Globals = new[] { new Global() } }.WriteToBinaryNoOutput());
            Assert.ThrowsException<InvalidOperationException>(() => new Module { Elements = new[] { new Element() } }.WriteToBinaryNoOutput());
            Assert.ThrowsException<InvalidOperationException>(() => new Module { Codes = new[] { new FunctionBody() } }.WriteToBinaryNoOutput());
            Assert.ThrowsException<InvalidOperationException>(() => new Module { Data = new[] { new Data() } }.WriteToBinaryNoOutput());
        }
    }
}