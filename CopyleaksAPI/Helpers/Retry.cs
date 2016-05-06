/********************************************************************************
 The MIT License(MIT)
 
 Copyright(c) 2016 Copyleaks LTD (https://copyleaks.com)
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
********************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Copyleaks.SDK.API.Extentions;

namespace Copyleaks.SDK.API.Helpers
{
	/// <summary>
	/// Based on code from: http://stackoverflow.com/questions/1563191/c-sharp-cleanest-way-to-write-retry-logic
	/// </summary>
	public static class Retry
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
