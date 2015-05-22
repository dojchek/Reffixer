using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Reffixer.Configuration;
using Reffixer.Log;
using SolutionParser;

namespace Reffixer
{
	internal class ProjectManager
	{
		#region Fields and .Ctor

		private readonly ILogger _logger;
		private readonly IFileSystem _fileSystem;
		private readonly Config _config;
		private readonly IDictionary<ReferenceConfig, bool> _processedReferences;
		private static readonly object lockObject = new object();
		private readonly List<string> _changedProjects;
		private readonly List<string> _changedSolutions;

		public ProjectManager(string configFile, IConfigProvider configProvider, ILogger logger, IFileSystem fileSystem)
		{
			if (configFile == null) throw new ArgumentNullException("configFile");
			if (configProvider == null) throw new ArgumentNullException("configProvider");
			if (logger == null) throw new ArgumentNullException("logger");
			if (fileSystem == null) throw new ArgumentNullException("fileSystem");
			if (string.IsNullOrEmpty(configFile)) throw new ArgumentException("configFile must be defined");

			_logger = logger;
			_fileSystem = fileSystem;
			_config = configProvider.Load<Config>(configFile);
			_changedProjects = new List<string>();
			_changedSolutions = new List<string>();

			_processedReferences = new Dictionary<ReferenceConfig, bool>(_config.ReferencesConfig.Count);
			foreach (var referenceConfig in ReferencesConfig)
			{
				_processedReferences.Add(referenceConfig, false);
			}
		}

		public ProjectManager(string configFile, IConfigProvider configProvider, ILogger logger)
			:this(configFile,configProvider,logger,new FileSystem())
		{ }

		#endregion

		#region Properties

		public IList<string> NotFoundReferences
		{
			get { return _processedReferences.Where(r => r.Value == false).Select(c => c.Key.ProjectReference).ToList(); }
		}

		public List<string> ChangedProjects
		{
			get { return _changedProjects; }
		}

		public List<string> ChangedSolutions
		{
			get { return _changedSolutions; }
		}

		private IEnumerable<string> ExcludePaths
		{
			get { return _config.Exclude ?? Enumerable.Empty<string>().ToList(); }
		}

		private IList<string> Paths
		{
			get { return _config.Include; }
		}

		private IList<ReferenceConfig> ReferencesConfig
		{
			get { return _config.ReferencesConfig; }
		}

		#endregion

		#region Public Methods

		public IList<Project> ListProjects()
		{
			var projects = new List<Project>(Paths.Count);
			foreach (var path in Paths)
			{
				var loadedProjects = ListProjects(path);
				projects.AddRange(loadedProjects);
				_logger.Info(string.Format("\tFound {0} projects in \t\"{1}\"", loadedProjects.Count, path));
			}

			return projects;
		}

		public bool FixProject(Project project)
		{
			if (project == null) return false;
			var referencesToRemove = new Dictionary<ProjectItem, ReferenceConfig>(ReferencesConfig.Count);

			foreach (var reference in project.GetItems(ProjectStrings.ProjectReference))
			{
				var projectFolder = _fileSystem.GetDirectoryName(project.FullPath);
				if (projectFolder == null) continue;

				var fullPath = Path.GetFullPath(Path.Combine(projectFolder, reference.EvaluatedInclude));

				var projectName = Path.GetFileName(fullPath);

				if (ReferencesConfig.All(r => projectName != r.ProjectReference)) continue;

				referencesToRemove.Add(reference, ReferencesConfig.First(r => projectName == r.ProjectReference));
			}

			if (!referencesToRemove.Any()) return false;

			FixReferences(project, referencesToRemove);

			referencesToRemove.Clear();
			project.Save(project.FullPath);

			_changedProjects.Add(project.FullPath);
			return true;
		}

		public IList<Solution> ListSolutions()
		{
			var solutions = new List<Solution>(Paths.Count);
			foreach (var path in Paths)
			{
				var slnList = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories)
					.Select(sl =>
					{
						try { return new Solution(sl); }
						catch { return null; }
					}).Where(s => s != null).ToList();

				solutions.AddRange(slnList);
				_logger.Info(string.Format("\tFound {0} solutions in \t\"{1}\"", solutions.Count, path));
			}

			return solutions;
		}

		public bool FixSolution(Solution solution)
		{
			var projectNames = _processedReferences.Select(c => c.Key.ProjectReference).ToList();
			if (!projectNames.Any()) return false;

			var projectsToRemove = projectNames.Select(projectName => solution.GetProjects().FirstOrDefault(prj => prj.ProjectName == Path.GetFileNameWithoutExtension(projectName)))
				.Where(p => p != null).ToList();

			if (!projectsToRemove.Any()) return false;

			solution.RemoveProjects(projectsToRemove);
			solution.SaveAs(solution.SolutionFileName);
			_changedSolutions.Add(solution.SolutionFileName);
			return true;
		}

		#endregion

		#region Private Methods

		private IList<Project> ListProjects(string path)
		{
			var files = _fileSystem.GetProjects(path);
			var dirs = _fileSystem.GetDirectories(path).Except(ExcludePaths).ToList();

			Parallel.ForEach(dirs, dir =>
			{
				var newFiles = ListProjects(dir);
				lock (lockObject)
				{
					files.AddRange(newFiles);
				}
			});

			return files;
		}

		private void FixReferences(Project project, Dictionary<ProjectItem, ReferenceConfig> referencesToRemove)
		{
			foreach (var referenceConfig in referencesToRemove.Values)
			{
				_processedReferences[referenceConfig] = true;
			}

			project.RemoveItems(referencesToRemove.Keys);
			foreach (var referenceToRemove in referencesToRemove.Values)
			{
				project.AddItem(ProjectStrings.AssemblyReference, referenceToRemove.AssemblyReference,
					referenceToRemove.CreateMetaData(project.FullPath));
			}
		}

		#endregion
	}
}