using System.Runtime.Serialization;

namespace Reffixer.Configuration
{
	[DataContract]
	internal class ReferenceConfig
	{
		[DataMember(IsRequired = true)]
		public string ProjectReference { get; set; }

		[DataMember(IsRequired = true)]
		public string AssemblyReference { get; set; }

		[DataMember]
		public string RequiredTargetFramework { get; set; }

		[DataMember]
		public string HintPath { get; set; }

		[DataMember]
		public bool? SpecificVersion { get; set; }
	}
}