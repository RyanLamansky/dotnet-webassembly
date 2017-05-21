# .NET WebAssembly

An library able to read, modify, and write WebAssembly binary files (WASM) from .NET-based applications.
The long term goal is to enable JIT compilation and execution of WASM files that don't have JavaScript dependencies.

Publishing via NuGet is planned at the time of API stability and robust JIT compilation support.
Beta versions may be published if there is interest.

## Current Status

Integration with the .NET JIT has begun.
This is being accomplished via run-time CIL generation which is then compiled via the standard .NET JIT.

## Development Plan

This will evolve as the project takes shape.

- [x] Read WebAssembly binary files (WASM)
- [x] Write WebAssembly binary files (WASM)
- [x]  JIT compile `i32.const`, `end`
- [x] Invoke exported functions via .NET (Hello World!)
- [ ] 🔜 Implement more WebAssembly instructions, particularly `block`, `loop`, `if`, and `br_table`
- [x] Add support for automatic implementation of abstract classes as a means to avoid inefficient dynamic invocation
- [ ] Compiler passes all tests at https://github.com/WebAssembly/spec/tree/master/test/core
- [ ] > 90% automated test code coverage
- [ ] API Cleanup (last breaking changes)
- [ ] Publish 1.0 on NuGet
- [ ] Initial WAT/WAST parser (i32.const, end, exported functions)
- [ ] Add support for automatic implementation of interfaces as an alternative to existing abstract class code
- [ ] 🛑 Save generated assemblies to files, blocked by https://github.com/dotnet/corefx/issues/4491