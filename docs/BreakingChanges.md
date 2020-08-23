# Breaking Change Information

Details and migration information regarding breaking changes will be provided here after the 1.0 release.
Preview changes will be less detailed due to low usage.

## Preview Breaking Change Summary

### 0.10.0

* Dropped .NET Standard builds, now targeting .NET Core 3.1 only.
  - This means it's no longer possible to use this library on the classic .NET Framework or old .NET Core versions.
  - This was due to .NET Standard being abandoned by Microsoft without getting proper nullable reference type support.
* `WebAssembly.Instruction.ToString()` now returns the native WebAssembly opcode name.
* Improved spec compliance by requiring memory to be either defined internally or imported, but not both.

### 0.9.0

* Out of range import types throw `ModuleLoadException` instead of `IndexOutOfRangeException`.
* `RuntimeException` is now abstract.
* Missing or incorrectly typed imports now throw `ImportException` instead of `ArgumentException`.
* `ImportDictionary`'s only member, an `Add` method, was changed to an extension method, which is binary-breaking but not code-breaking.

### 0.8.0

* `UnmanagedMemory` can no longer be un-disposed by calling its `Grow` method.

### 0.7.0

* Renamed several instructions to match the published WebAssembly specification.
  This project started over a year before the final specification was released so some of the names it used became out of date.
* `ElementType` member `AnyFunction` has been renamed to `FunctionReference` to align with the spec.
* `Function` member `Type` was changed from a public field to a property.
* The `Type` and `ValueType` types were renamed to `WebAssemblyType` and `WebAssemblyValueType`, respectively, to avoid conflicts with the popular `System` namespace.
* .NET Standard 2.0 is now the only build produced.
  This means .NET Framework consumers must be 4.7.2 or higher.
  Infrastructure is still in place to support builds as old as .NET 4.5 and .NET Standard 1.1 if a compelling use case arises, but this may eventually be removed otherwise.

### 0.5.0

* `WebAssembly.Table`'s `Type` property was renamed to `Definition` to better reflect its purpose.
  This will likely be renamed again during the big 1.0 final restructuring.

### 0.4.0

* Most types associated with compilation, including the `Compile` class itself, have been moved to the `WebAssembly.Runtime` namespace.
* Importing was changed from compile time (using raw `MethodInfo`) to instantiation time (using delegates).
* The `ModuleName` and `FieldName` properties of `RuntimeImport` have been removed in favor of a more JavaScript-like dictionary-of-dictionaries-of-imports at instantiation time.
