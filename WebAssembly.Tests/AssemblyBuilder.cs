using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly
{
	using Compiled;
	/// <summary>
	/// Aids in development of test cases by allowing rapid construction and compilation of simple WebAssembly files.
	/// </summary>
	static class AssemblyBuilder
	{
		public static TExport CreateInstance<TExport>(string name, ValueType? @return, params Instruction[] code)
		{
			Assert.IsNotNull(name);
			Assert.IsNotNull(code);

			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = @return.HasValue == false
				? new ValueType[0]
				: new[]
				{
					@return.GetValueOrDefault()
				},
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = name
			});
			module.Codes.Add(new FunctionBody
			{
				Code = code
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				var maker = Compiler.FromBinary<dynamic>(memory);
				Assert.IsNotNull(maker);
				compiled = maker();
			}

			Assert.IsNotNull(compiled);
			Assert.IsNotNull(compiled.Exports);

			return compiled.Exports;
		}
	}
}