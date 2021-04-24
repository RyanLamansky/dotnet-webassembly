// This example originates from https://github.com/RyanLamansky/dotnet-webassembly/issues/7
using System;
using System.IO;
using WebAssembly.Runtime; // Acquire from https://www.nuget.org/packages/WebAssembly

using var stream = File.OpenRead("HelloWorld.wasm");

var imports = new ImportDictionary
{
    { "env", "sayc", new FunctionImport(new Action<int>(raw => Console.Write((char)raw))) },
};
var compiled = Compile.FromBinary<dynamic>(stream)(imports);
compiled.Exports.main();
