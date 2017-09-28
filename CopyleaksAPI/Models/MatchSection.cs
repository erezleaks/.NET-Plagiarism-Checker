using Newtonsoft.Json;

namespace Copyleaks.SDK.API.Models
{
	public class MatchSection
	{
		[JsonProperty(PropertyName ="WC")]
		public int WordsCount { get; set; }

		[JsonProperty(PropertyName = "SoS")]
		public int SourceStartPos { get; set; }

		[JsonProperty(PropertyName = "SoE")]
		public int SourceEndPos { get; set; }

		[JsonProperty(PropertyName = "SuS")]
		public int SuspectedStartPos { get; set; }

		[JsonProperty(PropertyName = "SuE")]
		public int SuspectedEndPos { get; set; }
	}
}
