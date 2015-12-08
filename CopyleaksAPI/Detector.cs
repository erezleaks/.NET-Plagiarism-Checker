using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Extentions;
using Copyleaks.SDK.API.Models.Requests;
using Copyleaks.SDK.API.Models.Responses;
using Copyleaks.SDK.API.Models.Responses.Copyleaks.SDK.API.Models.Responses;
using Copyleaks.SDK.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Copyleaks.SDK.API
{
	public class Detector
	{
		private LoginToken Token { get; set; }

		public Detector(LoginToken token)
		{
			if (token == null)
				throw new UnauthorizedAccessException();
			else
				token.Validate();

			this.Token = token;
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
				this.Token.Validate();

				using (HttpClient client = new HttpClient())
				{
					client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

					HttpResponseMessage msg = client.GetAsync(string.Format("{0}/account/count-credits", Resources.ServiceVersion)).Result;
					if (!msg.IsSuccessStatusCode)
					{
						string errorResponse = msg.Content.ReadAsStringAsync().Result;
						BadLoginResponse response = JsonConvert.DeserializeObject<BadLoginResponse>(errorResponse);
						if (response == null)
							throw new JsonException("Unable to process server response.");
						else
							throw new CommandFailedException(response.Message, msg.StatusCode);
					}

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

		public CopyleaksProcess[] Processes
		{
			get
			{
				this.Token.Validate();

				using (HttpClient client = new HttpClient())
				{
					client.SetCopyleaksClient(HttpContentTypes.Json, this.Token);

					HttpResponseMessage msg = client.GetAsync(string.Format("{0}/detector/list", Resources.ServiceVersion)).Result;
					if (!msg.IsSuccessStatusCode)
					{
						string errorResponse = msg.Content.ReadAsStringAsync().Result;
						BadLoginResponse response = JsonConvert.DeserializeObject<BadLoginResponse>(errorResponse);
						if (response == null)
							throw new JsonException("Unable to process server response.");
						else
							throw new CommandFailedException(response.Message, msg.StatusCode);
					}

					string json = msg.Content.ReadAsStringAsync().Result;

					if (string.IsNullOrEmpty(json))
						throw new JsonException("This request could not be processed.");

					var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
                    ProcessInList[] rawProcesses = JsonConvert.DeserializeObject<ProcessInList[]>(json, dateTimeConverter);
					if (rawProcesses == null)
						throw new JsonException("Unable to process server response.");

					CopyleaksProcess[] processes = new CopyleaksProcess[rawProcesses.Length];
					for (int i = 0; i < rawProcesses.Length; i++)
						processes[i] = new CopyleaksProcess(this.Token, rawProcesses[i]);

					return processes;
				}
			}
		}

		/// <summary>
		/// Submitting specific URL to plagiarism scan
		/// </summary>
		/// <param name="url">The url containing the content to scan</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <exception cref="ArgumentOutOfRangeException">The input URL scheme is different than HTTP or HTTPS</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByUrl(Uri url, Uri httpCallback = null)
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

				if (httpCallback != null)
					client.DefaultRequestHeaders.Add("Http-Callback", httpCallback.AbsoluteUri); // Add HTTP callback to the request header.

				msg = client.PostAsync(Resources.ServiceVersion + "/detector/create-by-url", content).Result;

				if (!msg.IsSuccessStatusCode)
				{
					var errorJson = msg.Content.ReadAsStringAsync().Result;
					var errorObj = JsonConvert.DeserializeObject<BadResponse>(errorJson);
					if (errorObj == null)
						throw new CommandFailedException(msg.StatusCode);
					else
						throw new CommandFailedException(errorObj.Message, msg.StatusCode);
				}

				string json = msg.Content.ReadAsStringAsync().Result;

				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
				CreateResourceResponse response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				return new CopyleaksProcess(this.Token, response);
			}
		}

		/// <summary>
		/// Submitting specific local file to plagiarism scan
		/// </summary>
		/// <param name="localfile">Local file containing the content to scan</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByFile(FileInfo localfile, Uri httpCallback = null)
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

				HttpResponseMessage msg;
				// Uploading the local file to the server

				if (httpCallback != null)
					client.DefaultRequestHeaders.Add("Http-Callback", httpCallback.AbsoluteUri); // Add HTTP callback to the request header.

				using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
				using (FileStream stream = localfile.OpenRead())
				{
					content.Add(new StreamContent(stream, (int)stream.Length), "document", Path.GetFileName(localfile.Name));
					msg = client.PostAsync(Resources.ServiceVersion + "/detector/create-by-file", content).Result;
				}

				if (!msg.IsSuccessStatusCode)
				{
					var errorJson = msg.Content.ReadAsStringAsync().Result;
					var errorObj = JsonConvert.DeserializeObject<BadResponse>(errorJson);
					if (errorObj == null)
						throw new CommandFailedException(msg.StatusCode);
					else
						throw new CommandFailedException(errorObj.Message, msg.StatusCode);
				}

				string json = msg.Content.ReadAsStringAsync().Result;
				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
				CreateResourceResponse response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				return new CopyleaksProcess(this.Token, response);
			}
		}

		/// <summary>
		/// Submitting picture, containing textual content, to plagiarism scan
		/// </summary>
		/// <param name="localfile">The local picture containing the content to scan</param>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		/// <returns>The newly created process</returns>
		public CopyleaksProcess CreateByOcr(FileInfo localfile, Uri httpCallback = null)
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

				CreateCommandRequest req = new CreateCommandRequest()
				{
					URL = localfile.FullName
				};

				if (httpCallback != null)
					client.DefaultRequestHeaders.Add("Http-Callback", httpCallback.AbsoluteUri); // Add HTTP callback to the request header.

				HttpResponseMessage msg;
				// Uploading the local file to the server

				using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
				using (FileStream stream = localfile.OpenRead())
				{
					content.Add(new StreamContent(stream, (int)stream.Length), "document", Path.GetFileName(localfile.Name));
					msg = client.PostAsync(Resources.ServiceVersion + "/detector/create-by-file-ocr", content).Result;
				}

				if (!msg.IsSuccessStatusCode)
				{
					var errorJson = msg.Content.ReadAsStringAsync().Result;
					var errorObj = JsonConvert.DeserializeObject<BadResponse>(errorJson);
					if (errorObj == null)
						throw new CommandFailedException(msg.StatusCode);
					else
						throw new CommandFailedException(errorObj.Message, msg.StatusCode);
				}

				string json = msg.Content.ReadAsStringAsync().Result;

				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy HH:mm:ss" };
				CreateResourceResponse response = JsonConvert.DeserializeObject<CreateResourceResponse>(json, dateTimeConverter);
				return new CopyleaksProcess(this.Token, response);
			}
		}
	}
}
