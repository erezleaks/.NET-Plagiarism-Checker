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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Extentions;
using Copyleaks.SDK.API.Models;
using Copyleaks.SDK.API.Models.Requests;
using Copyleaks.SDK.API.Models.Responses;
using Copyleaks.SDK.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Copyleaks.SDK.API
{
	/// <summary>
	/// This class allows you to connect to Copyleaks cloud, 
	/// scan for plagiarism and get your Copyleaks account status. 
	/// </summary>
	public class CopyleaksCloud
	{
		/// <summary>
		/// Connection to Copyleaks cloud.
		/// </summary>
		/// <param name="product">The product for scanning the documents</param>
		public CopyleaksCloud(eProduct product)
		{
			this.Product = product;
		}

		#region Members And Properties

		private LoginToken _Token;
		public LoginToken Token
		{
			get
			{
				return _Token;
			}
			set
			{
				if (value == null)
					throw new UnauthorizedAccessException();
				else
					value.Validate();

				_Token = value;
			}
		}

		/// <summary>
		/// Get your current credit balance
		/// </summary>
		/// <param name="token">Login Token</param>
		/// <returns>Current credit balance</returns>
		public uint Credits
		{
			get
			{
				if (this.Token == null)
					throw new UnauthorizedAccessException("Empty token!");
				else
					this.Token.Validate();

				using (HttpClient client = new HttpClient())
				{
					client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

					HttpResponseMessage msg = client.GetAsync(string.Format("{0}/{1}/count-credits", Resources.ServiceVersion, this.Product.ToName())).Result;
					if (!msg.IsSuccessStatusCode)
						throw new CommandFailedException(msg);

					string json = msg.Content.ReadAsStringAsync().Result;

					if (string.IsNullOrEmpty(json))
						throw new JsonException("This request could not be processed.");

					CountCreditsResponse res = JsonConvert.DeserializeObject<CountCreditsResponse>(json);
					if (res == null)
						throw new JsonException("Unable to process server response.");

					return res.Amount;
				}
			}
		}
		
		/// <summary>
		/// Get your active processes
		/// </summary>
		public CopyleaksProcess[] Processes
		{
			get
			{
				if (this.Token == null)
					throw new UnauthorizedAccessException("Empty token!");
				else
					this.Token.Validate();

				using (HttpClient client = new HttpClient())
				{
					client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

					HttpResponseMessage msg = client.GetAsync(string.Format("{0}/{1}/list", Resources.ServiceVersion, this.Product.ToName())).Result;
					if (!msg.IsSuccessStatusCode)
						throw new CommandFailedException(msg);

					string json = msg.Content.ReadAsStringAsync().Result;

					if (string.IsNullOrEmpty(json))
						throw new JsonException("This request could not be processed.");

					var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
					ProcessInList[] rawProcesses = JsonConvert.DeserializeObject<ProcessInList[]>(json, dateTimeConverter);
					if (rawProcesses == null)
						throw new JsonException("Unable to process server response.");

					CopyleaksProcess[] processes = new CopyleaksProcess[rawProcesses.Length];
					for (int i = 0; i < rawProcesses.Length; i++)
						processes[i] = new CopyleaksProcess(this.Token, this.Product, rawProcesses[i]);

					processes = processes.OrderByDescending(proc => proc.CreationTimeUtc).ToArray();

					return processes;
				}
			}
		}

		public eProduct Product
		{
			get; private set;
		}

		#endregion

		/// <summary>
		/// Login to Copyleaks API
		/// </summary>
		/// <param name="email">Your email address</param>
		/// <param name="APIKey">Copyleaks API key</param>
		public void Login(string email, string APIKey)
		{
			this.Token = UsersAuthentication.Login(email, APIKey);
			// This security token can be used multiple times (it will expire after 48 hours).
		}


		/// <summary>
		/// Submitting URL to plagiarism scan
		/// </summary>
		/// <param name="url">The url containing the content to scan</param>
		/// <param name="options">Process Options: include http callback and add custom fields to the process</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <exception cref="ArgumentOutOfRangeException">The input URL scheme is different than HTTP or HTTPS</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByUrl(Uri url, ProcessOptions options = null)
		{
			if (this.Token == null)
				throw new UnauthorizedAccessException("Empty token!");
			else
				this.Token.Validate();

			if (url.Scheme != "http" && url.Scheme != "https")
				throw new ArgumentOutOfRangeException(nameof(url), "Allowed protocols: HTTP, HTTPS");

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

				CreateCommandRequest req = new CreateCommandRequest() { URL = url.AbsoluteUri };

				HttpResponseMessage msg;
				// Submitting the URL 
				HttpContent content = new StringContent(
					JsonConvert.SerializeObject(req),
					Encoding.UTF8,
					HttpContentTypes.Json);

				if (options != null)
					options.AddHeaders(client);

				msg = client.PostAsync(string.Format("{0}/{1}/{2}", Resources.ServiceVersion, this.Product.ToName(), "create-by-url"), content).Result;

				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				string json = msg.Content.ReadAsStringAsync().Result;

				CreateResourceResponse response;
				try
				{
					var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
					response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				}
				catch (Exception e)
				{
					throw new Exception("JSON=" + json, e);
				}
				if (options == null)
					return new CopyleaksProcess(this.Token, this.Product, response, null);
				else
					return new CopyleaksProcess(this.Token, this.Product, response, options.CustomFields);
			}
		}

		/// <summary>
		/// Submitting local file to plagiarism scan
		/// </summary>
		/// <param name="localfile">Local file containing the content to scan</param>
		/// <param name="options">Process Options: include http callback and add custom fields to the process</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByFile(FileInfo localfile, ProcessOptions options = null)
		{
			if (this.Token == null)
				throw new UnauthorizedAccessException("Empty token!");
			else
				this.Token.Validate();

			if (!localfile.Exists)
				throw new FileNotFoundException("File not found!", localfile.FullName);

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

				client.Timeout = TimeSpan.FromMinutes(10); // Uploading large file may take a while

				HttpResponseMessage msg;
				// Uploading the local file to the server

				if (options != null)
					options.AddHeaders(client);

				using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
				using (FileStream stream = localfile.OpenRead())
				{
					content.Add(new StreamContent(stream, (int)stream.Length), "document", Path.GetFileName(localfile.Name));
					msg = client.PostAsync(string.Format("{0}/{1}/create-by-file", Resources.ServiceVersion, this.Product.ToName()), content).Result;
				}

				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				string json = msg.Content.ReadAsStringAsync().Result;
				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
				CreateResourceResponse response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				if (options == null)
					return new CopyleaksProcess(this.Token, this.Product, response, null);
				else
					return new CopyleaksProcess(this.Token, this.Product, response, options.CustomFields);
			}
		}

		/// <summary>
		/// Submitting picture, containing textual content, to plagiarism scan
		/// </summary>
		/// <param name="localfile">The local picture containing the content to scan</param>
		/// <param name="language">Specify the language of the text</param>
		/// <param name="options">Process Options: include http callback and add custom fields to the process</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByOcr(FileInfo localfile, OcrLanguage language, ProcessOptions options = null)
		{
			if (this.Token == null)
				throw new UnauthorizedAccessException("Empty token!");
			else
				this.Token.Validate();

			if (!localfile.Exists)
				throw new FileNotFoundException("File not found!", localfile.FullName);

			if (language == null)
				throw new ArgumentNullException(nameof(language), "Cannot be null!");

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

				if (options != null)
					options.AddHeaders(client);

				HttpResponseMessage msg;
				// Uploading the local file to the server

				using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
				using (FileStream stream = localfile.OpenRead())
				{
					content.Add(new StreamContent(stream, (int)stream.Length), "document", Path.GetFileName(localfile.Name));
					msg = client.PostAsync(
						string.Format("{0}/{1}/create-by-file-ocr?language={2}", Resources.ServiceVersion, this.Product.ToName(), Uri.EscapeDataString(language.Type.ToString())),
						content).Result;
				}

				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				string json = msg.Content.ReadAsStringAsync().Result;

				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
				CreateResourceResponse response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				if (options == null)
					return new CopyleaksProcess(this.Token, this.Product, response, null);
				else
					return new CopyleaksProcess(this.Token, this.Product, response, options.CustomFields);
			}
		}
	}
}
