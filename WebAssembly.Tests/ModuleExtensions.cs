using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace WebAssembly
{
	static class ModuleExtensions
	{
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
			=> module.ToInstance<TExports>(null);

		public static Instance<TExports> ToInstance<TExports>(this Module module, IEnumerable<RuntimeImport> imports)
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

				Func<Instance<TExports>> maker;
				using (var readOnly = new ForwardReadOnlyStream(bytes))
				{
					maker = Compile.FromBinary<TExports>(readOnly, imports);
				}
				Assert.IsNotNull(maker);
				compiled = maker();
			}

			Assert.IsNotNull(compiled);
			return compiled;
		}
	}
}