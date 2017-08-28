using System.Collections.Generic;

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

		/// <summary>
		/// This custom section is to be written after the indicated preceding section.  Defaults to <see cref="Section.None"/>, causing it to be in front of all other sections.
		/// </summary>
		public Section PrecedingSection { get; set; }

		private string name;

		/// <summary>
		/// The name of the custom section; nulls are converted to <see cref="string.Empty"/>.
		/// </summary>
		public string Name
		{
			get => this.name ?? (this.name = string.Empty);
			set => this.name = value ?? string.Empty;
		}

		private IList<byte> content;

		/// <summary>
		/// The content of the custom section; nulls are converted to an empty modifiable collection.
		/// </summary>
		public IList<byte> Content
		{
			get => this.content ?? (this.content = new List<byte>());
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