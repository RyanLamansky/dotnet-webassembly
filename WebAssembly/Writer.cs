using System;
using System.IO;
using System.Text;

namespace WebAssembly
{
    internal sealed class Writer : IDisposable
    {
        private readonly UTF8Encoding utf8 = new(false, false);
        private BinaryWriter? writer;

        public Writer(Stream output)
        {
            this.writer = new BinaryWriter(output, utf8, true);
        }

        private BinaryWriter CheckedWriter => this.writer ?? throw new ObjectDisposedException(nameof(Writer));

        public void Write(uint value) => CheckedWriter.Write(value);

        public void WriteVar(uint value)
        {
            var writer = CheckedWriter;

            var remaining = value >> 7;
            while (remaining != 0)
            {
                writer.Write((byte)((value & 0x7f) | 0x80));
                value = remaining;
                remaining >>= 7;
            }
            writer.Write((byte)(value & 0x7f));
        }

        public void WriteVar(ulong value)
        {
            var writer = CheckedWriter;

            var remaining = value >> 7;
            while (remaining != 0)
            {
                writer.Write((byte)((value & 0x7f) | 0x80));
                value = remaining;
                remaining >>= 7;
            }
            writer.Write((byte)(value & 0x7f));
        }

        public void WriteVar(int value)
        {
            var writer = CheckedWriter;

            var remaining = value >> 7;
            var end = ((value & int.MinValue) == 0) ? 0 : -1;
            bool hasMore;
            do
            {
                hasMore = (remaining != end) || ((remaining & 1) != ((value >> 6) & 1));
                writer.Write((byte)((value & 0x7f) | (hasMore ? 0x80 : 0)));
                value = remaining;
                remaining >>= 7;
            } while (hasMore);
        }

        public void WriteVar(long value)
        {
            var writer = CheckedWriter;

            var remaining = value >> 7;
            var end = ((value & long.MinValue) == 0) ? 0 : -1;
            bool hasMore;
            do
            {
                hasMore = (remaining != end) || ((remaining & 1) != ((value >> 6) & 1));
                writer.Write((byte)(((byte)value & 0x7f) | (hasMore ? 0x80 : 0)));
                value = remaining;
                remaining >>= 7;
            } while (hasMore);
        }

        public void Write(float value) => CheckedWriter.Write(value);

        public void Write(double value) => CheckedWriter.Write(value);

        public void Write(string value)
        {
            var bytes = this.utf8.GetBytes(value);
            this.WriteVar((uint)bytes.Length);
            this.Write(bytes);
        }

        public void Write(byte[] value) => CheckedWriter.Write(value);

        public void Write(byte[] buffer, int index, int count) => CheckedWriter.Write(buffer, index, count);

        public void Write(byte value) => CheckedWriter.Write(value);

        #region IDisposable Support
        void Dispose(bool disposing)
        {
            if (this.writer == null || !disposing)
                return;

            try //Tolerate bad dispose implementations.
            {
                this.writer.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }

            this.writer = null;
        }

        ~Writer() => Dispose(false);

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