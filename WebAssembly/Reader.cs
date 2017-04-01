using System;
using System.IO;
using System.Text;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
	internal sealed class Reader : IDisposable
	{
		private readonly UTF8Encoding utf8 = new UTF8Encoding(false, false);
		private BinaryReader reader;
		private long offset;

		public Reader(Stream input)
		{
			//The UTF8 encoding parameter is not actually used; it's just there so that the leaveOpen parameter can be reached.
			this.reader = new BinaryReader(input, utf8, true);
		}

		public long Offset => this.offset;

		public uint ReadUInt32()
		{
			var result = this.reader.ReadUInt32();
			this.offset += 4;
			return result;
		}

		public bool TryReadVarUInt7(out byte result)
		{
			try
			{
				result = this.ReadVarUInt7();
				return true;
			}
			catch (EndOfStreamException)
			{
				result = 0;
				return false;
			}
		}

		public byte ReadVarUInt7() => (byte)(this.ReadVarUInt32() & 0b1111111);

		public uint ReadVarUInt32()
		{
			Assert(this.reader != null);

			var result = 0u;
			var shift = 0;
			while (true)
			{
				uint value = this.ReadByte();
				result |= ((value & 0x7F) << shift);
				if ((value & 0x80) == 0)
					break;
				shift += 7;
			}

			return result;
		}

		public string ReadString(uint length) => utf8.GetString(this.ReadBytes(length));

		public byte[] ReadBytes(uint length)
		{
			Assert(this.reader != null);

			var result = this.reader.ReadBytes(checked((int)length));
			this.offset += length;
			return result;
		}

		private byte ReadByte()
		{
			Assert(this.reader != null);

			var result = this.reader.ReadByte();
			this.offset++;
			return result;
		}

		#region IDisposable Support
		void Dispose(bool disposing)
		{
			if (this.reader == null)
				return;

			try //Tolerate bad dispose implementations.
			{
				this.reader.Dispose();
			}
			catch
			{
			}

			this.reader = null;
		}

		~Reader() => Dispose(false);

		/// <summary>
		/// Releases unmanaged resources associated with this instance.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}