using Newtonsoft.Json;

namespace Copyleaks.SDK.API.Models
{
	public class ComparisonReport
	{
		[JsonProperty(PropertyName = "Identical")]
		public MatchSection[] IdenticalSections { get; set; }

		[JsonProperty(PropertyName = "Similar")]
		public MatchSection[] Similar { get; set; }

		[JsonProperty(PropertyName = "RelatedMeaning")]
		public MatchSection[] RelatedMeaning { get; set; }

		public int IdenticalCopiedWords { get; set; }

		public int TotalWords { get; set; }
	}
}
