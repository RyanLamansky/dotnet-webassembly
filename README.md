# .NET WebAssembly

An experimental library with the near-term objective of providing the ability to read, modify, and write WebAssembly binary files (WASM) from .NET-based applications.
A longer term goal is to enable JIT compilation and execution of WASM files that don't have JavaScript dependencies.

## Current Status

Full parsing of the MVP WebAssembly spec is done.
This enables deep inspection of any valid WASM file.

Writing WASM is currently being migrated from my unpublished prototype.
Once complete, this library will be able to create new WASMs or modify existing ones.

## Development Plan

This will evolve as the project takes shape.

- [x] Unpublished prototype
- [x]  Parse all aspects of the MVP spec of Web Assembly
- [ ] 🔜 Write WASM files
- [ ] Initial WAST parser (i32.const, end, exported functions)
- [ ] JIT compile i32.const, end
- [ ] Invoke exported functions via .NET (Hello World!)
- [ ] Review official WAST test cases, update this plan (many steps will be added)
- [ ] ❓
- [ ] Full JIT support for all spects of the MVP spec of Web Assembly
- [ ] API Cleanup (last breaking changes)
- [ ] Publish 1.0 on NuGet