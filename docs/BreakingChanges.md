# Breaking Change Information

Details and migration information regarding breaking changes will be provided here after the 1.0 release.
Preview changes will be less detailed due to low usage.

## Preview Breaking Change Summary

### 0.5.0

* `WebAssembly.Table`'s `Type` property was renamed to `Definition` to better reflect its purpose.
  This will likely be renamed again during the big 1.0 final restructuring.

### 0.4.0

* Most types associated with compilation, including the `Compile` class itself, have been moved to the `WebAssembly.Runtime` namespace.
* Importing was changed from compile time (using raw `MethodInfo`) to instantiation time (using delegates).
* The `ModuleName` and `FieldName` properties of `RuntimeImport` have been removed in favor of a more JavaScript-like dictionary-of-dictionaries-of-imports at instantiation time.
