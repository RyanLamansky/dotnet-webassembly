using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace WebAssembly
{
	/// <summary>
	/// Verifies proper functionality when handling small externally-generate WASM sources.
	/// </summary>
	[TestClass]
	public class SampleTests
	{
		/// <summary>
		/// The number of times <see cref="Receive(int)"/> was called.
		/// </summary>
		private static int receiveCalls;

		/// <summary>
		/// Used with <see cref="Sample_Issue7"/> to verify a call out from a WebAssembly file.
		/// </summary>
		/// <param name="value">The input.</param>
		public static void Receive(int value)
		{
			System.Threading.Interlocked.Increment(ref receiveCalls);
		}

		/// <summary>
		/// Verifies proper functionality of the sample provided via https://github.com/RyanLamansky/dotnet-webassembly/issues/7 .
		/// This sample was produced via a very simple program built with https://webassembly.studio/ .
		/// </summary>
		[TestMethod]
		public void Sample_Issue7()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.Issue7.wasm"))
			{
				var module = Module.ReadFromBinary(stream);
				Assert.IsNotNull(module);
			}

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebAssembly.Samples.Issue7.wasm"))
			{
				var compiled = Compile.FromBinary<dynamic>(stream,
					new RuntimeImport[] {
					new FunctionImport("env", "sayc", typeof(SampleTests).GetTypeInfo().GetMethod(nameof(Receive)))
					})();
				compiled.Exports.main();
			}
		}
	}
}