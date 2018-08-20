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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;

namespace Copyleaks.SDK.API.Extentions
{
	internal static class HttpClientHelper
	{
		public static void SetCopyleaksClient(this HttpClient client, string contentType)
		{
			client.BaseAddress = new Uri(Consts.ServiceEntryPoint);

			client.Timeout = TimeSpan.FromMilliseconds(Consts.RequestsTimeout);

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Copyleaks-.NET-SDK/1.0");

			// SDK Language Localization
			// ----------------------
			// The Copyleaks SDK allows language localization for the informative and error messages.  
			// To customize the language, change the value of "CurrentThread.CurrentCulture" to the wanted language code. The changes you made will affect the next
			// requests to Copyleks cloud, with English ("en-US") being the default language if the requested language is not supported.
			//client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(Thread.CurrentThread.CurrentCulture.Name));
		}
		public static void SetCopyleaksClient(this HttpClient client, string contentType, LoginToken SecurityToken)
		{
			client.SetCopyleaksClient(contentType);
			client.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", "Bearer", SecurityToken.Token));
		}
	}
}
