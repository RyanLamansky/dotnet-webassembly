# WebAssembly for .NET
[![NuGet](https://img.shields.io/nuget/v/WebAssembly.svg)](https://www.nuget.org/packages/WebAssembly)

A library able to create, read, modify, write and execute WebAssembly (WASM) files from .NET-based applications.
*Execution does not use an interpreter.*
WASM instructions are mapped to their .NET equivalents and converted to native machine language by the .NET JIT compiler.

A preview is available via NuGet at https://www.nuget.org/packages/WebAssembly .
No further updates to the preview are planned; the next release will be 1.0 with full support for the WebAssembly "MVP".

## Overview

The API is unstable--this means the names and structure of everything can change--but the most complete feature right now is `WebAssembly.Module`.

- Read and write WASM binary files via `ReadFromBinary()` and `WriteToBinary()`.
- Create a new WASM binary file from scratch: create a new `WebAssembly.Module` instance and start adding things.
- `WebAssembly.Module` reflects the binary format in a very pure form: nearly anything that can be found in a valid WASM file is covered.
As the binary format is optimized for size efficiency, it can be difficult to use, particularly concepts like index space and labels.
The best resource for understanding how things work is the test code, in this repository under WebAssembly.Tests.
- `WebAssembly.Compile` converts WebAssembly binary files (WASM) to .NET via the run-time code generation features in [System.Reflection.Emit](https://msdn.microsoft.com/en-us/library/system.reflection.emit.aspx).
As it ultimately runs on the same CLR as C#, performance is equivalent.

## Sample

The sample below shows how to create and execute a WebAssembly file in memory.

``` C#
using WebAssembly; //Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;
using System;

//We need this later to call the code we're generating.
public abstract class Sample
{
	//Sometimes you can use C# dynamic instead of building an abstract class like this.
	public abstract int Demo(int value);
}

static class Program
{
	static void Main()
	{
		//Module can be used to create, read, modify, and write WebAssembly files.
		var module = new Module(); //In this case, we're creating a new one.

		//Types are function signatures: the list of parameters and returns.
		module.Types.Add(new WebAssembly.Type //The first added type gets index 0.
		{
			Parameters = new[]
			{
				WebAssembly.ValueType.Int32, //This sample takes a single Int32 as input.
				//Complex types can be passed by sending them in pieces.
			},
			Returns = new[]
			{
				//Multiple returns are supported by the binary format.
				//Standard currently allows a count of 0 or 1, though.
				WebAssembly.ValueType.Int32,
			},
		});
		//Types can be re-used for multiple functions to reduce WASM size.

		//The function list associates a function index to a type index.
		module.Functions.Add(new Function //The first added function gets index 0.
		{
			Type = 0, //The index for the "type" value added above.
		});

		//Code must be passed in the exact same order as the Functions above.
		module.Codes.Add(new FunctionBody
		{
			Code = new Instruction[]
			{
				new GetLocal(0), //The parameters are the first locals, in order.
				//We defined the first parameter as Int32, so now an Int32 is at the top of the stack.
				new Int32CountOneBits(), //Returns the number of binary bits set to 1.
				//It takes the Int32 from the top of the stack, and pushes the return value.
				//So, in the end, there is still a single Int32 on the stack, but it's now the result of Int32CountOneBits.
				new End(), //All functions must end with "End".
				//The final "End" also delivers the returned value, if the type says that a value is returned.
			},
		});

		//Exports enable features to be accessed by external code.
		//Typically this means JavaScript, but this library adds .NET execution capability, too.
		module.Exports.Add(new Export
		{
			Kind = ExternalKind.Function,
			Index = 0, //This should match the function index from above.
			Name = "Demo", //Anything legal in Unicode is legal in an export name.
		});

		//We now have enough for a usable WASM file, which we could save with module.WriteToBinary().
		//Below, we show how the Compile feature can be used for .NET-based execution.

		//We don't need to keep the below memory stream open for the instance creator.
		Func<Instance<Sample>> instanceCreator;
		using (var memory = new System.IO.MemoryStream())
		{
			//Can write to any stream, including files or ASP.NET responses, for example.
			module.WriteToBinary(memory);
			memory.Position = 0; //Going to read it back...
			instanceCreator = Compile.FromBinary<Sample>(memory);
		} //Automatically release the memory stream here.

		//Instances should be wrapped in a "using" block for automatic disposal.
		using (var instance = instanceCreator())
		{
			//FYI, instanceCreator can be used multiple times to create independant instances.
			Console.WriteLine(instance.Exports.Demo(0)); //Binary 0, result 0
			Console.WriteLine(instance.Exports.Demo(1)); //Binary 1, result 1,
			Console.WriteLine(instance.Exports.Demo(42));  //Binary 101010, result 3
		} //Automatically release the WebAssembly instance here.
	}
}
```

## Development Status

- Post-"MVP" features of WebAssembly (garbage collection, threads, SIMD, etc) will be added after the 1.0 release of this library.
- The current development focus is fixing the known issues listed below.
- 100% of instructions can be parsed by `WebAssembly.Module.ReadFromBinary` and written back out.
- 100% of instructions can be compiled to native code via the .NET CLR.
- Over 220 code tests provide strong quality assurance.
Following traditional [test-driven development](https://en.wikipedia.org/wiki/Test-driven_development) practices, the tests are written first and then the library is updated to pass the test.

## Known Issues

Everything on this list will be fixed before 1.0 is published.

* The following section types are not supported by the compiler: Data.
* The following export types are not supported by the compiler: Table, Global.
* The following import types are not supported: Global, Memory, Table.
* Offsets reported in exceptions are mostly wrong, reflecting the position of the reader at the time of the exception rather than the start of the bad bytes.
* `WebAssembly.Module` will let you write WASM files that it can't read back: specifically, a function or initializer expression that's not terminated with an `end` instruction.
* Function exports that expose an import are not supported by the compiler.
* Passing a MethodBuilder as an imported function will cause the compiler to generate incorrect code.
* Thorough documentation is needed.

## API Issues

Feedback on these (via GitHub issue) is welcome.
The API will be frozen with the 1.0 release.

* Some WebAssembly class names collide with entries in System (`Type`, `ValueType`).
This probably won't be changed because this is why we have namespaces, and these names are appropriate for WebAssembly.
* The `WebAssembly.Instructions` namespace has a high number of classes--nearly 200, one per instruction plus some helper base classes.
Additional namespaces will be created for new post-"MVP" WebAssembly instructions.
* Can't directly compile a loaded `WebAssembly.Module` instance.
This is intentional because the compiler is designed for streaming raw binary, but it might be convenient for some use cases.
* Instruction names in the library have little resemblance to their native name.
This is partially forced by the names themselves, which include characters like `/` and `.` that are not legal in C#.
The other reason is to conform with [.NET Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces), which discourage the use of acronyms and abbreviations.
* Consistent with the native encoding of WebAssembly, most integers are unsigned.
This can make interaction with .NET Framework  classes less convenient, as they mostly use signed integers even for values that are never negative, such as the count of entries in a list.
* The `Import` class has specific types as nested classes, which can be awkward to use.
* The `Import` class used by the parser can easily be confused with the `RuntimeImport` class used by the compiler.

## Potential Future Features

These features are under consideration for development after all the core work is done.

- Use the known custom section "name" to provide human-readable names to the generated functions.
Since this section is required to be at the end of the file by the WebAssembly standard, its use will be disabled by default for more efficient streaming compilation.
- ☣ Option to remove remove range check on linear memory access, for confident users desiring maximum performance.
- 🤔 Add support for automatic implementation of interfaces as an alternative to existing abstract class code.
- 🚀 Extensible optimization framework.
- 🛑 Save compiled-to-.NET assemblies to files; blocked on .NET Core by https://github.com/dotnet/corefx/issues/4491, but should be possible with .NET "Classic".