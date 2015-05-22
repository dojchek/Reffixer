using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reffixer.Configuration
{
	[DataContract]
	[KnownType(typeof(ReferenceConfig))]
	internal class Config
	{
		[DataMember(IsRequired = true)]
		public List<string> Include { get; set; }

		[DataMember(IsRequired = true)]
		public List<ReferenceConfig> ReferencesConfig { get; set; }

		[DataMember]
		public List<string> Exclude { get; set; }
	}
}