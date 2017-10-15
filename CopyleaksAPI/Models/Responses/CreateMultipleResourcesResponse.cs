using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copyleaks.SDK.API.Models.Responses
{
	public class CreateMultipleResourcesResponse
	{
		public CreateMultipleResourcesResponse() { }

		public CreateResourceError[] Errors { get; set; }

		public CopyleaksProcess[] Success { get; set; }
	}

	public class InnerCreateMultipleResourcesResponse
	{
		public InnerCreateMultipleResourcesResponse() { }

		public CreateResourceError[] Errors { get; set; }

		public CreateResourceResponse[] Success { get; set; }
	}

	public class CreateResourceError
	{
		public int ErrorCode { get; set; }

		public string ErrorMessage { get; set; }

		public string Filename { get; set; }
	}
}
