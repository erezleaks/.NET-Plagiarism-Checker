using System;

namespace Copyleaks.SDK.API.Extentions
{
	internal static class TypesArrayExceptionExtention
	{
		public static bool IsExpected(this Type[] expectedExceptions, Exception ex)
		{
			foreach (Type item in expectedExceptions)
				if (ex.GetType() == item)
					return true;

			return false;
		}
	}
}
