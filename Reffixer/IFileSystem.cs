using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace Reffixer
{
	internal interface IFileSystem
	{
		List<Project> GetProjects(string path);
		string[] GetDirectories(string path);
		string GetDirectoryName(string path);
	}

	internal class FileSystem : IFileSystem
	{
		public List<Project> GetProjects(string path)
		{
			return Directory.GetFiles(path, "*.csproj")
				.Select(x =>
				{
					try
					{
						return new Project(x);
					}
					catch
					{
						return null;
					}
				})
				.Where(x => x != null).ToList();
		}

		public string[] GetDirectories(string path)
		{
			return Directory.GetDirectories(path);
		}

		public string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}
	}
}