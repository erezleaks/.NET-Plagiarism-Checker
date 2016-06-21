using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Copyleaks.SDK.API.Properties;

namespace Copyleaks.SDK.API.Extentions
{
	internal static class ProductHelpers
	{
		public static string ToName(this eProduct product)
		{
			switch (product)
			{
				case eProduct.Publishers:
					return Resources.PublisherServicePage;
				case eProduct.Academic:
					return Resources.AcademicServicePage;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
