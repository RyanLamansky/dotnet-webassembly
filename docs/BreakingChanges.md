# Breaking Change Information

## Policy

Breaking changes are avoided whenever possible, but are sometimes needed to achieve correct behavior and deliver highly desired functionality.
* WebAssembly inspection, modification, and creation features have been mostly unchanged for years and this is expected to continue for any *stable* features of the official WebAssembly specification.
  * Changes to functionality associated with final standards of WebAssembly will typically result in a new major version, unless the change is determined to be very low risk.
  * Changes to functionality associated with *draft* versions of WebAssembly can occur in minor releases.
* CLR JIT-based compilation and execution can change in any version to achieve closer compliance to officially specified behavior, with the current release of Google Chrome used as the reference.
  * To minimize risk to your project, test your WASM files in Google Chrome and file bugs for deviance in WebAssembly for .NET's behavior.
* .NET Core 3.1 will be supported *at least* as long as Microsoft supports it.
  * .NET 5 and beyond is capable of loading .NET Core 3.1 libraries without complaint.
  * Microsoft will transition long-term support to newer .NET releases over time, eventually leading to hardship with developing against older versions, which will trigger an update to the next oldest "LTS" release in a major version.
  Specifically, if enabling an unsupported version of .NET causes compatibility problems with the Ubuntu-based build agent or Visual Studio Community-based Windows users, it's time to upgrade.
  * Multi-targeted releases (for example, .NET Core 3.1 and .NET 6) may be created if the extra targets add useful features.
  * Automated testing will cover .NET Core 3.1 and any newer compatible versions that are supported by Microsoft.
* WebAssembly for .NET releases will *never* take a dependency on anything other than .NET itself.
  * This minimizes the chance of "dependency hell".
  * Extension points may be made available so that middleware can be created to combine WebAssembly for .NET with other tools. Extension point requests are tracked via GitHub issues.

* Scope will not creep beyond the following objectives:
  * Provide .NET-based data structures that can represent any binary WASM feature.
  * Binary WASM files can be created from scratch, or read into memory for inspection and modification.
  * Binary WASM files can be executed using the .NET CLR via run-time WASM-to-CIL conversion with Google Chrome as the reference for correctness.
  * Provide a pathway to convert WASM files to .NET DLLs to enable ahead-of-time compilation.

## Breaking Changes

### 1.0.0

* Block-type instructions that returned their type in `.ToString` now more closely match the WAT format; "block (returns i32)" is now "block i32", for example.

## Breaking Changes from Preview Releases

### 0.11.0

This is the first non-preview release; these are the last breaking changes for preview versions.

* `Int32Constant`, `Int64Constant`, `Float32Constant`, `Float64Constant`: several members were moved to a new base class, `Constant`.
  For users of this library working in higher-level languages like C#, this shouldn't break your build, but it's a binary incompatibility for already-built code that used the moved members.
* Some instruction `.ToString()` calls now return additional information in WAT format.
* The parameterless constructor for `Import.Table` sets the `ElementType` property to `FunctionReference` 
instead of an invalid zero value.
* The parameterless constructor for `Import.Global` sets the `ContentType` property to `WebAssemblyValueType.Int32` instead of an invalid zero value.

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
