using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Emits one CLR class per canonical WebAssembly 3.0 garbage-collected struct/array type into the compiled
/// module's <see cref="ModuleBuilder"/>, leaning on the .NET GC for lifetime management. Structurally-identical
/// types share a class (they share a canonical id), and a declared-supertype chain becomes a CLR base-class chain,
/// so <c>ref.cast</c>/<c>ref.test</c> to a concrete type can lower to <c>castclass</c>/<c>isinst</c>. All builders
/// are defined during the type-section pass and baked (<see cref="CreateAllTypes"/>) before the exports type.
/// </summary>
internal sealed class GCTypeEmitter
{
    // Not sealed: a declared subtype's emitted class inherits from its supertype's emitted class, so bases must
    // remain open. (Sealing would fault CreateType with "the parent type is sealed" the moment a chain appears.)
    private const TypeAttributes GCClassAttributes =
        TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit;

    private const FieldAttributes GCFieldAttributes = FieldAttributes.Public;

    private const MethodAttributes CtorAttributes =
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

    /// <summary>A single field of an emitted struct/array class (the absolute layout includes inherited fields).</summary>
    internal sealed class GCField
    {
        /// <summary>The backing CLR field.</summary>
        public readonly FieldBuilder Builder;

        /// <summary>The packed storage kind, or <see cref="PackedType.None"/> for a regular field.</summary>
        public readonly PackedType Packed;

        /// <summary>Whether the field may be written after construction.</summary>
        public readonly bool IsMutable;

        /// <summary>The unpacked WebAssembly value type of the field (i32 for a packed field's non-signed view).</summary>
        public readonly ValType ValueType;

        public GCField(FieldBuilder builder, PackedType packed, bool isMutable, ValType valueType)
        {
            this.Builder = builder;
            this.Packed = packed;
            this.IsMutable = isMutable;
            this.ValueType = valueType;
        }

        /// <summary>The WASM value type produced by <c>*.get</c> / consumed by <c>*.set</c> and <c>*.new</c> (packed → i32).</summary>
        public ValType StackType => this.Packed == PackedType.None ? this.ValueType : WebAssemblyValueType.Int32;
    }

    /// <summary>An emitted struct/array class and its baking metadata.</summary>
    internal sealed class EmittedType
    {
        public readonly TypeBuilder Builder;
        public readonly FunctionType Kind;

        // The parameterless constructor (default-initializes every field), used by struct.new_default / array.new*.
        public ConstructorBuilder DefaultConstructor = null!;

        // The absolute field layout, including inherited fields (which reference the ancestor's FieldBuilders).
        public GCField[] Fields = [];

        // For an array class: the backing `T[] Items` field and the element metadata.
        public FieldBuilder? ItemsField;
        public GCField? Element;

        public bool Created;

        public EmittedType(TypeBuilder builder, FunctionType kind)
        {
            this.Builder = builder;
            this.Kind = kind;
        }
    }

    private readonly CanonicalTypeMap map;
    private readonly Dictionary<int, EmittedType> byCanonicalId = [];

    private GCTypeEmitter(CanonicalTypeMap map) => this.map = map;

    /// <summary>
    /// Defines (but does not yet bake) one CLR class per canonical struct/array type. Returns null when the module
    /// declares no struct/array types, keeping the pre-3.0 emission path untouched.
    /// </summary>
    public static GCTypeEmitter? Emit(
        ModuleBuilder module,
        CompilerConfiguration configuration,
        CanonicalTypeMap map,
        IReadOnlyList<WebAssemblyType> types)
    {
        var emitter = new GCTypeEmitter(map);
        var any = false;

        // First occurrence of each canonical id is its representative; define a class for struct/array kinds only.
        var repByCanonicalId = new Dictionary<int, int>();
        for (var i = 0; i < types.Count; i++)
        {
            if (map.Kind[i] is not (FunctionType.Struct or FunctionType.Array))
                continue;
            var id = map.CanonicalId[i];
            if (!repByCanonicalId.TryAdd(id, i))
                continue;
            any = true;
        }

        if (!any)
            return null;

        // Define TypeBuilders (base-first so a subtype's base is the supertype's builder). DefineType accepts an
        // unbaked TypeBuilder as parent, so the whole chain is defined before any bodies compile.
        foreach (var (id, rep) in repByCanonicalId)
            emitter.EnsureDefined(module, configuration, types, repByCanonicalId, id, rep);

        return emitter;
    }

    private EmittedType EnsureDefined(
        ModuleBuilder module,
        CompilerConfiguration configuration,
        IReadOnlyList<WebAssemblyType> types,
        Dictionary<int, int> repByCanonicalId,
        int canonicalId,
        int rep)
    {
        if (this.byCanonicalId.TryGetValue(canonicalId, out var existing))
            return existing;

        var kind = this.map.Kind[rep];
        var superIndex = this.map.Supertype[rep];
        EmittedType? baseEmitted = null;
        Type baseClrType;
        if (superIndex >= 0)
        {
            var baseCanonical = this.map.CanonicalId[superIndex];
            baseEmitted = this.EnsureDefined(module, configuration, types, repByCanonicalId, baseCanonical, repByCanonicalId[baseCanonical]);
            baseClrType = baseEmitted.Builder;
        }
        else
        {
            baseClrType = configuration.NeutralizeType(kind == FunctionType.Array ? typeof(WebAssemblyArrayObject) : typeof(WebAssemblyStructObject));
        }

        var builder = module.DefineType($"☘ GC{canonicalId}", GCClassAttributes, baseClrType);
        var emitted = new EmittedType(builder, kind);
        this.byCanonicalId.Add(canonicalId, emitted);

        var source = types[rep];
        if (kind == FunctionType.Struct)
            DefineStruct(configuration, builder, emitted, baseEmitted, source);
        else
            DefineArray(configuration, builder, emitted, source);

        EmitDefaultConstructor(emitted, baseEmitted, baseClrType);
        return emitted;
    }

    private void DefineStruct(CompilerConfiguration configuration, TypeBuilder builder, EmittedType emitted, EmittedType? baseEmitted, WebAssemblyType source)
    {
        var baseFields = baseEmitted?.Fields ?? [];
        var declared = source.Fields ?? [];
        var fields = new GCField[declared.Count];

        for (var i = 0; i < declared.Count; i++)
        {
            // A subtype's binary field list repeats (possibly refines) the supertype's fields, then appends; an
            // inherited field reuses the ancestor's FieldBuilder (references stay object, scalars stay identical).
            if (i < baseFields.Length)
            {
                fields[i] = baseFields[i];
                continue;
            }

            fields[i] = this.DefineField(configuration, builder, i, declared[i]);
        }

        emitted.Fields = fields;
    }

    private void DefineArray(CompilerConfiguration configuration, TypeBuilder builder, EmittedType emitted, WebAssemblyType source)
    {
        var elementField = (source.Fields is { Count: > 0 } f) ? f[0] : new StructField();
        var element = ElementMetadata(elementField);
        emitted.Element = element;

        var elementClrType = ElementClrType(configuration, elementField, element.ValueType, this.map);
        var itemsField = builder.DefineField("☘ Items", elementClrType.MakeArrayType(), GCFieldAttributes);
        emitted.ItemsField = itemsField;

        // Override the abstract WebAssemblyArrayObject.Length getter to return Items.Length.
        var lengthGetter = builder.DefineMethod(
            "get_Length",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            typeof(int),
            Type.EmptyTypes);
        var il = lengthGetter.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, itemsField);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Ret);
    }

    // Metadata (packed/mutable/value type) for a struct or array field; no FieldBuilder for an array element (it
    // lives inside the Items[] array, addressed by index).
    private static GCField ElementMetadata(StructField field) =>
        new(null!, field.Packed, field.IsMutable, FieldValType(field));

    private GCField DefineField(CompilerConfiguration configuration, TypeBuilder builder, int index, StructField field)
    {
        var clrType = FieldClrType(configuration, field, this.map);
        var fieldBuilder = builder.DefineField($"☘ f{index}", clrType, GCFieldAttributes);
        return new GCField(fieldBuilder, field.Packed, field.IsMutable, FieldValType(field));
    }

    private static void EmitDefaultConstructor(EmittedType emitted, EmittedType? baseEmitted, Type baseClrType)
    {
        var ctor = emitted.Builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, Type.EmptyTypes);
        emitted.DefaultConstructor = ctor;

        var il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        if (baseEmitted != null)
        {
            il.Emit(OpCodes.Call, baseEmitted.DefaultConstructor);
        }
        else
        {
            var baseCtor = baseClrType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                ?? throw new InvalidOperationException($"{baseClrType} has no parameterless constructor.");
            il.Emit(OpCodes.Call, baseCtor);
        }

        il.Emit(OpCodes.Ret);
    }

    private static ValType FieldValType(StructField field) =>
        field.Packed != PackedType.None
            ? WebAssemblyValueType.Int32
            : field.StorageRefType is { } r ? ValType.FromReferenceType(r) : ValType.FromValueType(field.StorageType);

    private static Type FieldClrType(CompilerConfiguration configuration, StructField field, CanonicalTypeMap map) => field.Packed switch
    {
        PackedType.I8 => configuration.NeutralizeType(typeof(byte)),
        PackedType.I16 => configuration.NeutralizeType(typeof(ushort)),
        _ => configuration.NeutralizeType(FieldValType(field).ToSystemType(map.Kind)),
    };

    private static Type ElementClrType(CompilerConfiguration configuration, StructField field, ValType valueType, CanonicalTypeMap map) => field.Packed switch
    {
        PackedType.I8 => configuration.NeutralizeType(typeof(byte)),
        PackedType.I16 => configuration.NeutralizeType(typeof(ushort)),
        _ => configuration.NeutralizeType(valueType.ToSystemType(map.Kind)),
    };

    /// <summary>The emitted CLR class representing the concrete struct/array type at <paramref name="typeIndex"/>.</summary>
    public Type GetClass(uint typeIndex) => this.Resolve(typeIndex).Builder;

    /// <summary>The parameterless (default-initializing) constructor of the type at <paramref name="typeIndex"/>.</summary>
    public ConstructorInfo GetDefaultConstructor(uint typeIndex) => this.Resolve(typeIndex).DefaultConstructor;

    /// <summary>The absolute field layout (including inherited fields) of the struct type at <paramref name="typeIndex"/>.</summary>
    public GCField[] GetFields(uint typeIndex) => this.Resolve(typeIndex).Fields;

    /// <summary>The emitted array metadata (Items field and element) of the array type at <paramref name="typeIndex"/>.</summary>
    public EmittedType GetArray(uint typeIndex) => this.Resolve(typeIndex);

    /// <summary>Whether the type at <paramref name="typeIndex"/> is a struct (as opposed to an array).</summary>
    public bool IsStruct(uint typeIndex) => this.map.Kind[typeIndex] == FunctionType.Struct;

    /// <summary>Whether a CLR class was emitted for <paramref name="typeIndex"/> (a struct or array type).</summary>
    public bool Has(uint typeIndex) =>
        typeIndex < (uint)this.map.CanonicalId.Length && this.byCanonicalId.ContainsKey(this.map.CanonicalId[typeIndex]);

    /// <summary>Validates that <paramref name="typeIndex"/> is a declared struct type, throwing "unknown type" otherwise.</summary>
    public void RequireStruct(uint typeIndex)
    {
        if (!this.Has(typeIndex) || this.map.Kind[typeIndex] != FunctionType.Struct)
            throw new ModuleLoadException($"unknown type {typeIndex}: a struct instruction requires a struct type.", 0);
    }

    /// <summary>Validates that <paramref name="typeIndex"/> is a declared array type, throwing "unknown type" otherwise.</summary>
    public void RequireArray(uint typeIndex)
    {
        if (!this.Has(typeIndex) || this.map.Kind[typeIndex] != FunctionType.Array)
            throw new ModuleLoadException($"unknown type {typeIndex}: an array instruction requires an array type.", 0);
    }

    /// <summary>Resolves the field metadata for <c>struct.get</c>/<c>struct.set</c>, validating both indices.</summary>
    public GCField ResolveStructField(uint typeIndex, uint fieldIndex)
    {
        this.RequireStruct(typeIndex);
        var fields = this.GetFields(typeIndex);
        if (fieldIndex >= (uint)fields.Length)
            throw new ModuleLoadException($"unknown field {fieldIndex}: struct type {typeIndex} has {fields.Length} field(s).", 0);
        return fields[(int)fieldIndex];
    }

    private EmittedType Resolve(uint typeIndex) =>
        this.byCanonicalId.TryGetValue(this.map.CanonicalId[typeIndex], out var emitted)
            ? emitted
            : throw new InvalidOperationException($"No emitted GC class for type index {typeIndex}.");

    /// <summary>Bakes every emitted GC class (base classes first). Called once, before the exports type is baked.</summary>
    public void CreateAllTypes()
    {
        foreach (var emitted in this.byCanonicalId.Values)
            Create(emitted);
    }

    private void Create(EmittedType emitted)
    {
        if (emitted.Created)
            return;
        emitted.Created = true;

        // A subtype's base class must be baked first.
        if (emitted.Builder.BaseType is TypeBuilder)
        {
            foreach (var candidate in this.byCanonicalId.Values)
            {
                if (ReferenceEquals(candidate.Builder, emitted.Builder.BaseType))
                {
                    Create(candidate);
                    break;
                }
            }
        }

        emitted.Builder.CreateType();
    }
}
