using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copyleaks.SDK.API
{
	public static class Consts
	{
		public static string AccountPage = "account";
		public static string BusinessesServicePage = "businesses";
		public static string EducationServicePage = "education";
		public static string MiscellaneousServicePage = "miscellaneous";
		public static int RequestsTimeout = 60000; // In Milliseconds. Wait up to 60 seconds to response.
		public static string ServiceEntryPoint = "https://api.copyleaks.com/";
		public static string ServiceVersion = "v1";
		public static string WebsitesServicePage = "websites";
	}
}
