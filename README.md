# .NET WebAssembly

An experimental library with the near-term objective of providing the ability to read, modify, and write WebAssembly binary files (WASM) from .NET-based applications.
A longer term goal is to enable JIT compilation and execution of WASM files that don't have JavaScript dependencies.

## Current Status

I have a very rough prototype in a private repository.
I'm refactoring and improving quality, uploading to this repository as significant milestones are reached.

## Development Plan

This will evolve as the project takes shape.

- [x] Unpublished prototype
- [ ] 🔜 Parse all aspects of the MVP spec of Web Assembly
- [ ] Write WASM files
- [ ] Initial WAST parser (i32.const, end, exported functions)
- [ ] JIT compile i32.const, end
- [ ] Invoke exported functions via .NET (Hello World!)
- [ ] Review official WAST test cases, update this plan (many steps will be added)
- [ ] ❓
- [ ] Full JIT support for all spects of the MVP spec of Web Assembly
- [ ] API Cleanup (last breaking changes)
- [ ] Publish 1.0 on NuGet