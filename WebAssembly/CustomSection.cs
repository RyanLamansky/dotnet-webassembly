using System.Collections.Generic;
using System.Diagnostics;

namespace WebAssembly
{
    /// <summary>
    /// Contains arbitrary bytes that may have meaning in certain environments.
    /// </summary>
    public class CustomSection
    {
        /// <summary>
        /// Creates a new custom section.
        /// </summary>
        public CustomSection()
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private Section precedingSection;

        /// <summary>
        /// This custom section is to be written after the indicated preceding section.  Defaults to <see cref="Section.None"/>, causing it to be in front of all other sections.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Value must be one of the <see cref="Section"/> values.</exception>
        public Section PrecedingSection
        {
            get => this.precedingSection;
            set
            {
                if (!value.IsValid())
                    throw new System.ArgumentOutOfRangeException(nameof(value), value, "Value must be one of the Section values.");

                this.precedingSection = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private string? name;

        /// <summary>
        /// The name of the custom section; nulls are converted to <see cref="string.Empty"/>.
        /// </summary>
        public string Name
        {
            get => this.name ??= string.Empty;
            set => this.name = value ?? string.Empty;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private IList<byte>? content;

        /// <summary>
        /// The content of the custom section; nulls are converted to an empty modifiable collection.
        /// </summary>
        public IList<byte> Content
        {
            get => this.content ??= new List<byte>();
            set => this.content = value ?? new List<byte>();
        }

        internal void WriteTo(Writer writer)
        {
            writer.Write(this.Name);
            if (this.content is byte[] bytes)
                writer.Write(bytes);
            else
                foreach (var b in this.Content)
                    writer.Write(b);
        }
    }
}