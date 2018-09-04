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
using System.Net.Http;
using System.Text;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Extentions;
using Newtonsoft.Json;

namespace Copyleaks.SDK.API
{
	public static class UsersAuthentication
	{
		private static readonly string LOGIN_PAGE = string.Format("{0}/{1}/login-api", Consts.ServiceVersion, Consts.AccountPage);

		/// <summary>
		/// Login to Copyleaks authentication server.
		/// </summary>
		/// <param name="email">Email Address</param>
		/// <param name="apiKey">Api Key</param>
		/// <returns>Login Token to use while accessing the API services</returns>
		/// <exception cref="ArgumentException">Email or password is missing</exception>
		/// <exception cref="JsonException">Unexpected response from the server</exception>
		public static LoginToken Login(string email, string apiKey)
		{
			if (string.IsNullOrEmpty(email))
				throw new ArgumentException("Email is mandatory.", nameof(email));
			else if (string.IsNullOrEmpty(apiKey))
				throw new ArgumentException("ApiKey is mandatory.", nameof(apiKey));

			LoginToken token;
			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json);
				HttpResponseMessage msg = client.PostAsync(LOGIN_PAGE, new StringContent(JsonConvert.SerializeObject(new
				{
					email = email,
					apikey = apiKey
				}), Encoding.UTF8, HttpContentTypes.Json)).Result;

				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				string json = msg.Content.ReadAsStringAsync().Result;

				if (string.IsNullOrEmpty(json))
					throw new JsonException("This request could not be processed.");

				token = JsonConvert.DeserializeObject<LoginToken>(json);
				if (token == null)
					throw new JsonException("Unable to process server response.");
			}

			return token;
		}
	}
}
