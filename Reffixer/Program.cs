using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Reffixer.Configuration;
using Reffixer.Log;

namespace Reffixer
{
	internal class Program
	{
		private const string LogFileName = "Reffixer.log";
		private const string ChangeLogFileName = "ChangeLog.txt";

		public static void Main(String[] args)
		{
			var configFile = GetConfigFile(args);
			var logPath = GetLogPath(args);
			var logger = new ConsoleLogger(logPath + LogFileName);

			try
			{
				var manager = new ProjectManager(configFile, new JsonConfigProvider(), logger);

				logger.Info("DETECTING PROJECTS...\n");
				var projects = manager.ListProjects();

				logger.Info("\nPROCESSING PROJECTS...\n");
				foreach (var project in projects.Where(manager.FixProject))
				{
					logger.Info(string.Format("\tFixed references for {0}", Path.GetFileName(project.FullPath)));
				}

				logger.Info("\nDETECTING SOLUTION FILES...\n");
				var solutions = manager.ListSolutions();

				logger.Info("\nPROCESSING SOLUTION FILES...\n");
				foreach (var solution in solutions.Where(manager.FixSolution))
				{
					logger.Info(string.Format("\tFixed solution {0}", Path.GetFileName(solution.SolutionFileName)));
				}

				LogFinishMessage(manager, logger, logPath + ChangeLogFileName);
			}
			catch (Exception ex)
			{
				logger.Error("Tool finished with an error", ex);
			}

			Console.Write("Detailed log saved in \"{0}\", Press any key to continue...", logPath + LogFileName);
			Console.ReadKey();
		}

		private static string GetConfigFile(IList<string> args)
		{
			if (args.Any()) return args[0];

			var currentDirectory = Directory.GetCurrentDirectory();
			var jsonFiles = Directory.EnumerateFiles(currentDirectory, "*.json");
			return jsonFiles.First();
		}

		private static string GetLogPath(IList<string> args)
		{
			var logPath = Directory.GetCurrentDirectory() + "\\";

			if (args.Count() > 1 && Directory.Exists(args[1]))
			{
				logPath = args[1];
			}

			return logPath;
		}

		private static void LogFinishMessage(ProjectManager manager, ILogger logger, string filePath)
		{
			var builder = new StringBuilder();

			if (manager.ChangedProjects.Any()) builder.AppendFormat("Projects:\n{0}\n", string.Join("\n", manager.ChangedProjects));
			if (manager.ChangedSolutions.Any()) builder.AppendFormat("\nSolution Files:\n{0}", string.Join("\n", manager.ChangedSolutions));

			if (builder.Length > 0)
			{
				using (var stream = File.CreateText(filePath))
				{
					stream.Write(builder.ToString());
				}
				logger.Info(string.Format("\nComplete list of changed projects and solutions saved in \"{0}\"\n", filePath));
			}

			if (manager.NotFoundReferences.Any())
			{
				var message = string.Join("\n\t", manager.NotFoundReferences);
				logger.Warning(string.Format("Following references were not found in any of the processed projects:\n\t{0}", message));
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\nSUCCESS - Modified {0} projects and {1} solutions\n", manager.ChangedProjects.Count,
				manager.ChangedSolutions.Count);
			Console.ResetColor();
		}
	}
}
