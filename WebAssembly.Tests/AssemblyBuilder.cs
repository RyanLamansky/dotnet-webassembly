using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace WebAssembly
{
    /// <summary>
    /// Aids in development of test cases by allowing rapid construction and compilation of simple WebAssembly files.
    /// </summary>
    static class AssemblyBuilder
    {
        public static TExport CreateInstance<TExport>(string name, ValueType? @return, params Instruction[] code)
        where TExport : class
        {
            Assert.IsNotNull(name);
            Assert.IsNotNull(code);

            var module = new Module();
            module.Types.Add(new Type
            {
                Returns = @return.HasValue == false
                ? new ValueType[0]
                : new[]
                {
                    @return.GetValueOrDefault()
                },
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = name
            });
            module.Codes.Add(new FunctionBody
            {
                Code = code
            });

            var compiled = module.ToInstance<TExport>();

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            return compiled.Exports;
        }

        public static TExport CreateInstance<TExport>(string name, ValueType? @return, IList<ValueType> parameters, params Instruction[] code)
        where TExport : class
        {
            Assert.IsNotNull(name);
            Assert.IsNotNull(parameters);
            Assert.IsNotNull(code);

            var module = new Module();
            module.Types.Add(new Type
            {
                Returns = @return.HasValue == false
                ? new ValueType[0]
                : new[]
                {
                    @return.GetValueOrDefault()
                },
                Parameters = parameters,
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = name
            });
            module.Codes.Add(new FunctionBody
            {
                Code = code
            });

            var compiled = module.ToInstance<TExport>();

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);

            return compiled.Exports;
        }

        private static readonly Dictionary<System.Type, ValueType> map = new Dictionary<System.Type, ValueType>(4)
        {
            { typeof(int), ValueType.Int32 },
            { typeof(long), ValueType.Int64 },
            { typeof(float), ValueType.Float32 },
            { typeof(double), ValueType.Float64 },
        };

        public static ValueType Map(System.Type type) => map[type];
    }
}