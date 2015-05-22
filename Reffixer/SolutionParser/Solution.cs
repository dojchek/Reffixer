using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SolutionParser
{
	/// <summary>
	/// .sln loaded into class.
	/// </summary>
	public class Solution
	{
		private readonly string _solutionFileName;
		// List of either String (line format is not interesting to us), or SolutionProject.
		private readonly List<object> _slnLines;

		/// <summary>
		/// Loads visual studio .sln solution
		/// </summary>
		/// <param name="solutionFileName"></param>
		/// <exception cref="System.IO.FileNotFoundException">The file specified in path was not found.</exception>
		public Solution(string solutionFileName)
		{
			_solutionFileName = solutionFileName;
			_slnLines = new List<object>();
			var slnTxt = File.ReadAllText(solutionFileName);
			//Match string like: Project("{66666666-7777-8888-9999-AAAAAAAAAAAA}") = "ProjectName", "projectpath.csproj", "{11111111-2222-3333-4444-555555555555}"
			var projMatcher = new Regex("Project\\(\"(?<ParentProjectGuid>{[A-F0-9-]+})\"\\) = \"(?<ProjectName>.*?)\", \"(?<RelativePath>.*?)\", \"(?<ProjectGuid>{[A-F0-9-]+})");

			Regex.Replace(slnTxt, "^(.*?)[\n\r]*$", m =>
					{
						String line = m.Groups[1].Value;
						Match m2 = projMatcher.Match(line);
						if (m2.Groups.Count < 2)
						{
							_slnLines.Add(line);
							return "";
						}

						var s = new SolutionProject();
						// "0" - RegEx special kind of group
						foreach (String g in projMatcher.GetGroupNames().Where(x => x != "0"))
							s.GetType().GetField(g).SetValue(s, m2.Groups[g].ToString());

						_slnLines.Add(s);
						return "";
					},
					RegexOptions.Multiline);
		}

		/// <summary> Gets Solution's Path </summary>
		public string SolutionFileName
		{
			get { return _solutionFileName; }
		}

		/// <summary>
		/// Gets list of sub-projects in solution.
		/// </summary>
		/// <param name="bGetAlsoFolders">true if get also sub-folders.</param>
		public List<SolutionProject> GetProjects(bool bGetAlsoFolders = false)
		{
			var q = _slnLines.OfType<SolutionProject>();

			// Filter away folder names in solution.
			if (!bGetAlsoFolders)
				q = q.Where(x => x.RelativePath != x.ProjectName);

			return q.ToList();
		}

		/// <summary>
		/// Removes a project from a solution
		/// </summary>
		/// <param name="project">Project to remove</param>
		public void RemoveProject(SolutionProject project)
		{
			if (!_slnLines.Contains(project)) return;

			var guid = project.ProjectGuid;
			var index = _slnLines.FindIndex(p => p == project);
			_slnLines.RemoveRange(index, 2);

			_slnLines.RemoveAll(l =>
			{
				var line = l as string;
				return !string.IsNullOrEmpty(line) && line.Contains(guid);
			});
		}

		/// <summary>
		/// Saves solution as file.
		/// </summary>
		public void SaveAs(String asFilename)
		{
			var s = new StringBuilder();

			for (int i = 0; i < _slnLines.Count; i++)
			{
				if (_slnLines[i] is String)
					s.Append(_slnLines[i]);
				else
				{
					var solutionProject = _slnLines[i] as SolutionProject;
					if (solutionProject != null) s.Append(solutionProject.AsSlnString());
				}

				if (i != _slnLines.Count)
					s.AppendLine();
			}

			s.Remove(s.Length - 1, 1);
			File.WriteAllText(asFilename, s.ToString());
		}

		/// <summary>
		/// Removes a collection of specified projects from a solution
		/// </summary>
		/// <param name="projectsToRemove">Collection of projects to remove</param>
		public void RemoveProjects(IEnumerable<SolutionProject> projectsToRemove)
		{
			foreach (var solutionProject in projectsToRemove)
			{
				RemoveProject(solutionProject);
			}
		}
	}
}