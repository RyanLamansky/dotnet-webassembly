# WebAssembly for .NET
[![NuGet](https://img.shields.io/nuget/v/WebAssembly.svg)](https://www.nuget.org/packages/WebAssembly)

A library able to create, read, modify, write and execute WebAssembly (WASM) files from .NET-based applications.
*Execution does not use an interpreter.*
WASM instructions are mapped to their .NET equivalents and converted to native machine language by the .NET JIT compiler.

A preview is available via NuGet at https://www.nuget.org/packages/WebAssembly .
No further updates to the preview are planned; the next release will be 1.0 with full support for the WebAssembly "MVP".

## Getting Started

The API is unstable--this means the names and structure of everything can change--but the most complete feature right now is `WebAssembly.Module`.

- Read and write WASM binary files via `ReadFromBinary()` and `WriteToBinary()`.
- Create a new WASM binary file from scratch: create a new `WebAssembly.Module` instance and start adding things.
- `WebAssembly.Module` reflects the binary format in a very pure form: nearly anything that can be found in a valid WASM file is covered.
As the binary format is optimized for size efficiency, it can be difficult to use, particularly concepts like index space and labels.
The best resource for understanding how things work is the test code, in this repository under WebAssembly.Tests.
- `WebAssembly.Compile` converts WebAssembly binary files (WASM) to .NET via the run-time code generation features in [System.Reflection.Emit](https://msdn.microsoft.com/en-us/library/system.reflection.emit.aspx).
As it ultimately runs on the same CLR as C#, performance is equivalent.

## Development Status

- Post-"MVP" features of WebAssembly (garbage collection, threads, SIMD, etc) will be added after the 1.0 release of this library.
- Current development focus is addressing the known issues listed below.
- 100% of instructions can be parsed by `WebAssemnbly.Module.ReadFromBinary` and written back out.
- 91% of instructions can be compiled.
- 197 unit tests (at the time of writing) provide strong quality assurance.
Following traditional [test-driven development](https://en.wikipedia.org/wiki/Test-driven_development) practices, the tests are written first and then the library is updated to pass the test.

## Known Issues

Everything on this list will be fixed before 1.0 is published.

* 12 instructions are not supported by the compiler: 
`current_memory`
`grow_memory`
`i32.clz`
`i32.popcnt`
`i32.ctz`
`i32.rotl`
`i32.rotr`
`i64.clz`
`i64.popcnt`
`i64.ctz`
`i64.rotl`
`i64.rotr`
* `block` instructions that yield a value are not supported by the compiler.
* `end` and `ret` instructions that leave leftover values on the stack will cause the .NET CLR to report an [InvalidProgramException](https://msdn.microsoft.com/en-us/library/system.invalidprogramexception.aspx).
* The following section types are not supported by the compiler: Custom, Start, Data.
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
Due to streaming compilation, this section would have to be before the Function section for names to be applied.
- ☣ Option to remove remove range check on linear memory access, for confident users desiring maximum performance.
- 🤔 Add support for automatic implementation of interfaces as an alternative to existing abstract class code.
- 🚀 Extensible optimization framework.
- 🛑 Save compiled-to-.NET assemblies to files; blocked on .NET Core by https://github.com/dotnet/corefx/issues/4491, but should be possible with .NET "Classic".