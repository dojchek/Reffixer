using System.Diagnostics;

namespace SolutionParser
{
	[DebuggerDisplay("{ProjectName}, {RelativePath}, {ProjectGuid}")]
	public class SolutionProject
	{
		public string ParentProjectGuid;
		public string ProjectName;
		public string RelativePath;
		public string ProjectGuid;

		public string AsSlnString()
		{
			return "Project(\"" + ParentProjectGuid + "\") = \"" + ProjectName + "\", \"" + RelativePath + "\", \"" + ProjectGuid + "\"";
		}
	}
}