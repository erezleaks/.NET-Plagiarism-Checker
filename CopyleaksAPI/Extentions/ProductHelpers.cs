using System;

namespace Copyleaks.SDK.API.Extentions
{
	public static class ProductHelpers
	{
		public static string ToName(this eProduct product)
		{
			switch (product)
			{
				case eProduct.Businesses:
					return Consts.BusinessesServicePage;
				case eProduct.Education:
					return Consts.EducationServicePage;
				case eProduct.Websites:
					return Consts.WebsitesServicePage;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
