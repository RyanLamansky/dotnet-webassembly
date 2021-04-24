using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace WebAssembly
{
    /// <summary>
    /// Aids in development of test cases by allowing rapid construction and compilation of simple WebAssembly files.
    /// </summary>
    static class AssemblyBuilder
    {
        public static TExport CreateInstance<TExport>(string name, WebAssemblyValueType? @return, params Instruction[] code)
        where TExport : class
        {
            Assert.IsNotNull(name);
            Assert.IsNotNull(code);

            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Returns = @return.HasValue == false
                ? Array.Empty<WebAssemblyValueType>()
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

        public static TExport CreateInstance<TExport>(string name, WebAssemblyValueType? @return, IList<WebAssemblyValueType> parameters, params Instruction[] code)
        where TExport : class
        {
            Assert.IsNotNull(name);
            Assert.IsNotNull(parameters);
            Assert.IsNotNull(code);

            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Returns = @return.HasValue == false
                ? Array.Empty<WebAssemblyValueType>()
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

        private static readonly Dictionary<System.Type, WebAssemblyValueType> map = new(4)
        {
            { typeof(int), WebAssemblyValueType.Int32 },
            { typeof(long), WebAssemblyValueType.Int64 },
            { typeof(float), WebAssemblyValueType.Float32 },
            { typeof(double), WebAssemblyValueType.Float64 },
        };

        public static WebAssemblyValueType Map(System.Type type) => map[type];
    }
}