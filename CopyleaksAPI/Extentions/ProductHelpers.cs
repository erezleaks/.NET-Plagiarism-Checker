using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Copyleaks.SDK.API.Properties;

namespace Copyleaks.SDK.API.Extentions
{
	public static class ProductHelpers
	{
		public static string ToName(this eProduct product)
		{
			switch (product)
			{
				case eProduct.Businesses:
					return Resources.BusinessesServicePage;
				case eProduct.Academic:
					return Resources.AcademicServicePage;
				case eProduct.Websites:
					return Resources.WebsitesServicePage;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
