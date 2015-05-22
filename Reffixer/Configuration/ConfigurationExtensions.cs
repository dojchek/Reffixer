using System;
using System.Collections.Generic;
using System.IO;

namespace Reffixer.Configuration
{
	internal static class ConfigurationExtensions
	{
		public static IEnumerable<KeyValuePair<string, string>> CreateMetaData(this ReferenceConfig obj, string fullPath)
		{
			var metaData = new Dictionary<string, string>(3);

			if (!string.IsNullOrEmpty(obj.HintPath))
			{
				metaData.Add(ProjectStrings.HintPath, string.Join("\\", new[] { MakeRelativePath(fullPath, obj.HintPath), obj.AssemblyReference }));
			}
			if (!string.IsNullOrEmpty(obj.RequiredTargetFramework))
			{
				metaData.Add(ProjectStrings.RequiredTargetFramework, obj.RequiredTargetFramework);
			}
			if (obj.SpecificVersion.HasValue)
			{
				metaData.Add(ProjectStrings.SpecificVersion, obj.SpecificVersion.Value.ToString());
			}

			return metaData;
		}

		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private static String MakeRelativePath(String fromPath, String toPath)
		{
			if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			try
			{
				var fromUri = new Uri(fromPath);
				var toUri = new Uri(toPath);

				if (fromUri.Scheme != toUri.Scheme)
				{
					return toPath;
				} // path can't be made relative.

				Uri relativeUri = fromUri.MakeRelativeUri(toUri);
				String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

				if (toUri.Scheme.ToUpperInvariant() == "FILE")
				{
					relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				}

				return relativePath;
			}
			catch (UriFormatException)
			{
				//TODO: Improve this check - We want to return toPath, only if it failed because of environment variable
				if(toPath.Contains("$(")) return toPath;
				throw;
			}
		}
	}
}
