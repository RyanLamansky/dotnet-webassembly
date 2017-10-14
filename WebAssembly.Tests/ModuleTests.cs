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
			Assert.AreEqual("input", ExceptionAssert.Expect<ArgumentNullException>(() => Module.ReadFromBinary((Stream)null)).ParamName);

			using (var sample = new MemoryStream())
			{
				var utf8 = new UTF8Encoding(false, false);

				using (var writer = new BinaryWriter(sample, utf8, true))
				{
					writer.Write(0x6e736100); //Bad magic number.
				}
				sample.Position = 0;
				Assert.IsTrue(ExceptionAssert.Expect<ModuleLoadException>(() => Module.ReadFromBinary(sample)).Message.ToLowerInvariant().Contains("magic"));
				Assert.IsTrue(sample.CanSeek, "Stream was closed but should have been left open.");

				sample.Position = 0;
				using (var writer = new BinaryWriter(sample, utf8, true))
				{
					writer.Write(0x6d736100);
					//Missing version.
				}
				sample.Position = 0;
				Assert.IsInstanceOfType(ExceptionAssert.Expect<ModuleLoadException>(() => Module.ReadFromBinary(sample)).InnerException, typeof(EndOfStreamException));

				sample.Position = 0;
				using (var writer = new BinaryWriter(sample, utf8, true))
				{
					writer.Write(0x6d736100);
					writer.Write(0x0); //Bad version
				}
				sample.Position = 0;
				Assert.IsTrue(ExceptionAssert.Expect<ModuleLoadException>(() => Module.ReadFromBinary(sample)).Message.ToLowerInvariant().Contains("version"));

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
	}
}