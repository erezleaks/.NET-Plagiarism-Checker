using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Copyleaks.SDK.API.Extentions;

namespace Copyleaks.SDK.API.Helpers
{
	internal static class Retry
	{
		public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3, params Type[] expectedExceptions)
		{
			var exceptions = new List<Exception>();
			bool isFirst = true;
			for (int retry = 0; retry < retryCount; retry++)
			{
				if (!isFirst)
					Thread.Sleep(retryInterval);

				try
				{
					return action();
				}
				catch (Exception ex)
				{
#if DEBUG
					Console.WriteLine("Retry failed ({0}), Exception Message = '{1}'", retry, ex.Message);
#endif
					isFirst = false;

					if (expectedExceptions.IsExpected(ex))
					{
						exceptions.Add(ex);
						continue;
					}
					else
						throw ex; // Unexpected exception.
				}

			}

			throw new AggregateException(exceptions);
		}

		public static T Http<T>(Func<T> action, TimeSpan? retryInterval = null, int retryCount = 3, params Type[] expectedExceptions)
		{
			List<Type> expected_http = new List<Type>(expectedExceptions);
			expected_http.Add(typeof(WebException));
			expected_http.Add(typeof(IOException));
			expected_http.Add(typeof(AggregateException));

			TimeSpan defaultWait = retryInterval ?? TimeSpan.FromSeconds(10);

			return Do<T>(action, defaultWait, retryCount, expected_http.ToArray());
		}
	}
}
