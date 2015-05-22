using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using NSubstitute;
using NUnit.Framework;
using Reffixer.Configuration;
using Reffixer.Log;

// ReSharper disable ObjectCreationAsStatement
namespace Reffixer.UnitTests
{
	[TestFixture]
	public class ProjectManagerTests
	{
		private IConfigProvider _configProvider;
		private ILogger _logger;

		[TestFixtureSetUp]
		public void TestsSetup()
		{
			_configProvider = Substitute.For<IConfigProvider>();
			_logger = Substitute.For<ILogger>();
		}

		#region .Ctor tests

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ConstructorAllNulls()
		{
			new ProjectManager(null, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Constructor_ConfigProvider_Null()
		{
			new ProjectManager("dummy", null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Constructor_Logger_Null()
		{
			new ProjectManager("dummy", _configProvider, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void Constructor_InvalidFileName()
		{
			new ProjectManager(string.Empty, _configProvider, _logger);
		}

		[Test]
		public void Constructor_ValidParams()
		{
			var configuration = new Config
			{
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			new ProjectManager("dummy", _configProvider, _logger);
		}

		[Test]
		public void Constructor_LoadsMultipleReferenceConfigs()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string>(),
				ReferencesConfig = new List<ReferenceConfig>
				{
					new ReferenceConfig {ProjectReference = "test1", AssemblyReference = "test1"},
					new ReferenceConfig {ProjectReference = "test2", AssemblyReference = "test2"}
				}
			};

			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);

			//Act
			var manager = new ProjectManager("dummy", _configProvider, _logger);

			//Assert
			Assert.That(manager.NotFoundReferences.Count == 2);
		}

		#endregion


		#region ListProjects()

		[Test]
		public void ListProjects_NoPaths_ProjectCountZero()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string>(),
				ReferencesConfig = new List<ReferenceConfig>()
			};

			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();
			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var projectCount = manager.ListProjects();

			//Assert
			Assert.AreEqual(0, projectCount.Count);
		}

		[Test]
		public void ListProjects_OnePathOneProject_ProjectCountOne()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string> {"dummyPath"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects(null).ReturnsForAnyArgs(
				new List<Project>
				{
					new Project()
				});

			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(1, prjs.Count);
		}

		[Test]
		public void ListProjects_TwoPatshBothWithOneProject_ProjectCountTwo()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string> {"dummyPath", "dummyPath2"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects(null).ReturnsForAnyArgs(
				new List<Project>
				{
					new Project()
				});

			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(2, prjs.Count);
		}

		[Test]
		public void ListProjects_OnePathNoProjects_ProjectCountZero()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string> {"dummyPath"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects(null).ReturnsForAnyArgs(new List<Project>());

			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(0, prjs.Count);
		}

		[Test]
		public void ListProjects_TwoPathsNoProjects_ProjectCountZero()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string> {"dummyPath", "dummyPath2"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects(null).ReturnsForAnyArgs(new List<Project>());

			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(0, prjs.Count);
		}

		[Test]
		public void ListProjects_TwoPathsOneWithProject_ProjectCountOne()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string> {"dummyPath", "dummyPath2"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects("dummyPath").Returns(new List<Project> {new Project()});
			fileSystem.GetProjects("dummyPath2").Returns(new List<Project>());

			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(1, prjs.Count);
		}

		[Test]
		public void ListProjects_OnePathMultipleProjects_ProjectCountEqualsRandomNumberOfProjects()
		{
			//Arrange
			var random = new Random();
			var numberOfProjects = random.Next(2, 10);
			var projects = Enumerable.Repeat(new Project(), numberOfProjects);

			var configuration = new Config
			{
				Include = new List<string> {"dummyPath"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects("dummyPath").Returns(new List<Project>(projects));
			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(numberOfProjects, prjs.Count);
		}

		[Test]
		public void ListProjects_TwoPathsBothWithMultipleProjects_ProjectCountEqualsNumberOfProjectsInBothPaths()
		{
			//Arrange
			var random = new Random();
			var numberOfProjectsPath1 = random.Next(2, 10);
			var numberOfProjectsPath2 = random.Next(2, 10);
			var totalNumberOfProjects = numberOfProjectsPath1 + numberOfProjectsPath2;
			var projectsPath1 = Enumerable.Repeat(new Project(), numberOfProjectsPath1);
			var projectsPath2 = Enumerable.Repeat(new Project(), numberOfProjectsPath2);

			var configuration = new Config
			{
				Include = new List<string> {"dummyPath", "dummyPath2"},
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);
			var fileSystem = Substitute.For<IFileSystem>();

			fileSystem.GetProjects("dummyPath").Returns(new List<Project>(projectsPath1));
			fileSystem.GetProjects("dummyPath2").Returns(new List<Project>(projectsPath2));
			fileSystem.GetDirectories(null).ReturnsForAnyArgs(Enumerable.Empty<string>());

			var manager = new ProjectManager("dummy", _configProvider, _logger, fileSystem);

			//Act
			var prjs = manager.ListProjects();

			//Assert
			Assert.AreEqual(totalNumberOfProjects, prjs.Count);
		}

		#endregion


		[Test]
		public void FixProject_ProjectIsNull_ReturnsFalse()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string>(),
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);

			var manager = new ProjectManager("dummy", _configProvider, _logger);

			//Act
			var fixProject = manager.FixProject(null);

			//Assert
			Assert.IsFalse(fixProject);
		}

		[Test]
		public void FixProject_ProjectWithNoReferences_ReturnsFalse()
		{
			//Arrange
			var configuration = new Config
			{
				Include = new List<string>(),
				ReferencesConfig = new List<ReferenceConfig>()
			};
			_configProvider.Load<Config>(null).ReturnsForAnyArgs(configuration);

			var manager = new ProjectManager("dummy", _configProvider, _logger);
			var project = new Project();

			//Act
			var fixProject = manager.FixProject(project);

			//Assert
			Assert.IsFalse(fixProject);
		}
	}
}
