using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using WebAssembly.Runtime;

namespace WebAssembly
{
    static class ModuleExtensions
    {
        public static Module BinaryRoundTrip(this Module module)
        {
            Assert.IsNotNull(module);

            using var memory = new MemoryStream();
            module.WriteToBinary(memory);
            Assert.AreNotEqual(0, memory.Position);

            memory.Position = 0;
            var result = Module.ReadFromBinary(memory);
            Assert.IsNotNull(result);
            return result;
        }

        /// <summary>
        /// Writes a  <see cref="Module"/> to an in-memory stream with no expectation of success.
        /// </summary>
        /// <param name="module">The module to write.</param>
        public static void WriteToBinaryNoOutput(this Module module)
        {
            Assert.IsNotNull(module);

            using var memory = new MemoryStream();
            module.WriteToBinary(memory);
        }

        private sealed class ForwardReadOnlyStream : Stream
        {
            private readonly byte[] data;
            private int position;

            public ForwardReadOnlyStream(byte[] data)
            {
                this.data = data;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                count = Math.Min(count, this.data.Length - this.position);
                if (count == 0)
                    return 0;

                Array.Copy(this.data, this.position, buffer, offset, count);
                this.position += count;
                return count;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        public static Instance<TExports> ToInstance<TExports>(this Module module)
        where TExports : class
            => module.ToInstance<TExports>(new ImportDictionary());

        public static Instance<TExports> ToInstance<TExports>(this Module module, IDictionary<string, IDictionary<string, RuntimeImport>> imports)
        where TExports : class
        {
            Instance<TExports> compiled;
            {
                byte[] bytes;
                using (var memory = new MemoryStream())
                {
                    module.WriteToBinary(memory);
                    Assert.AreNotEqual(0, memory.Position);
                    Assert.AreNotEqual(0, memory.Length);
                    bytes = memory.ToArray();
                }

                InstanceCreator<TExports> maker;
                using (var readOnly = new ForwardReadOnlyStream(bytes))
                {
                    maker = Compile.FromBinary<TExports>(readOnly);
                }
                Assert.IsNotNull(maker);
                compiled = maker(imports);
            }

            Assert.IsNotNull(compiled);
            Assert.IsNotNull(compiled.Exports);
            return compiled;
        }
    }
}