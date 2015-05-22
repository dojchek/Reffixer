using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Reffixer.Configuration
{
	/// <summary>
	/// Configuration provider from JSON files
	/// </summary>
	internal class JsonConfigProvider : IConfigProvider
	{
		/// <summary>
		/// Deserializes <see cref="Config"/> settings from JSON file specified with
		/// <paramref name="filePath"/>
		/// </summary>
		/// <exception cref="System.ArgumentException"><paramref name="filePath" /> is
		/// a zero-length string, contains only white space, or contains one or more
		/// invalid characters as defined by
		/// <see cref="System.IO.Path.GetInvalidPathChars" />. </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="filePath" /> is null. </exception>
		/// <exception cref="PathTooLongException">The specified filePath exceed the
		/// system-defined maximum length. For example, on Windows-based platforms,
		/// paths must be less than 248 characters, and file names must be less than
		/// 260 characters. </exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid
		/// (for example, it is on an unmapped drive). </exception>
		/// <exception cref="IOException">An I/O error occurred while opening the
		/// file. </exception>
		/// <exception cref="System.UnauthorizedAccessException"/>
		/// <exception cref="FileNotFoundException">The file specified in
		/// <paramref name="filePath" /> was not found. </exception>
		public T Load<T>(string filePath) where T : class
		{
			if (!Path.HasExtension(filePath) || Path.GetExtension(filePath) != ".json")
			{
				throw new Exception("Invalid configuration file. Should be JSON file (*.json)\"");
			}

			var jsonText = File.ReadAllText(filePath);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText)))
			{
				var serializer = new DataContractJsonSerializer(typeof(T));
				return serializer.ReadObject(stream) as T;
			}
		}
	}
}