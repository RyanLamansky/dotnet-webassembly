# .NET WebAssembly

An library able to read, modify, and write WebAssembly binary files (WASM) from .NET-based applications.
The long term goal is to enable JIT compilation and execution of WASM files that don't have JavaScript dependencies.

Publishing via NuGet is planned at the time of API stability and robust JIT compilation support.
Beta versions may be published if there is interest.

## Current Status

Integration with the .NET JIT has begun.
This will be accomplished via run-time CIL generation which is then compiled via the standard .NET JIT.

## Development Plan

This will evolve as the project takes shape.

- [x] Read WebAssembly binary files (WASM)
- [x] Write WebAssembly binary files (WASM)
- [ ] 🔜 JIT compile `i32.const`, `end`
- [ ] Invoke exported functions via .NET (Hello World!)
- [ ] Compiler passes all tests at https://github.com/WebAssembly/spec/tree/master/test/core
- [ ] > 90% automated test code coverage
- [ ] API Cleanup (last breaking changes)
- [ ] Publish 1.0 on NuGet
- [ ] Initial WAT/WAST parser (i32.const, end, exported functions)