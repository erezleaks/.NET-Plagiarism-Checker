using System;

namespace Copyleaks.SDK.API.Models.Responses
{
	public class ProcessInList
	{
		public Guid ProcessId { get; set; }

		public DateTime CreationTimeUTC { get; set; }

		public String Status { get; set; }
	}
}
