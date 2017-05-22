# WebAssembly for .NET
[![NuGet](https://img.shields.io/nuget/v/WebAssembly.svg)](https://www.nuget.org/packages/WebAssembly)

A library able to create, read, modify, write and execute WebAssembly (WASM) files from .NET-based applications.
.NET JIT compiler-based execution capability the focus of current development effort.

A preview is available via NuGet at https://www.nuget.org/packages/WebAssembly .
This preview will be updated after major milestones or urgent bug fixes.
Version 1.0 of the package will be released when the API is stable, full compliance with the spec is achieved, and enough automated tests are in place to ensure high quality.

## Getting Started

The API is unstable--this means the names and structure of everything can change--but the most complete feature right now is `WebAssembly.Module`.

- Read and write WASM binary files via `ReadFromBinary()` and `WriteToBinary()`.
- Create a new WASM binary file from scratch: create a new `WebAssembly.Module` instance and start adding things.
- `WebAssembly.Module` reflects the binary format in a very pure form: nearly anything that can be found in a valid WASM file is covered.
As the binary format is optimized for size efficiency, it can be difficult to use, particularly concepts like index space and labels.
The best resource for understanding how things work is the test code, in this repository under WebAssembly.Tests.

Though not in the NuGet package yet, JIT compilation is in development and is available in the repository as `WebAssembly.Compiler`.
All available features are covered by tests in the WebAssembly.Tests project.

## Development Plan

This will evolve as the project takes shape.

- [x] Read WebAssembly binary files (WASM)
- [x] Write WebAssembly binary files (WASM)
- [x] Compile `i32.const`, `end`
- [x] Invoke exported functions via .NET (Hello World!)
- [x] Add support for automatic implementation of abstract classes as a means to avoid inefficient dynamic invocation
- [x] Compile control flow instructions: `block`, `loop`, `if`, and `br_table`
- [ ] 🔜 Implement linear memory using an array
- [ ] Implement any remaining instructions not yet built to support development of other features
- [ ] Implement less-commonly-used features to achieve full compliance with the specification
- [ ] Compiler passes all tests at https://github.com/WebAssembly/spec/tree/master/test/core
- [ ] Final breaking changes to API
- [ ] Publish 1.0.0 on NuGet
- [ ] Documentation and examples for many scenarios
- [ ] 🛑 Save compiled-to-.NET assemblies to files, blocked by https://github.com/dotnet/corefx/issues/4491

## Potential Future Features

These features are under consideration for development after all the core work is done.

- ☣ Implement linear memory using unmanaged memory (raw pointer), as an option for confident users desiring maximum performance
- 🤔 Add support for automatic implementation of interfaces as an alternative to existing abstract class code
- 🚀 Extensible optimization framework