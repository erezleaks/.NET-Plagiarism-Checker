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
using System.Linq;
using System.Net.Http;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Extentions;
using Copyleaks.SDK.API.Helpers;
using Copyleaks.SDK.API.Models;
using Copyleaks.SDK.API.Models.Responses;
using Copyleaks.SDK.API.Properties;
using Newtonsoft.Json;

namespace Copyleaks.SDK.API
{
	/// <summary>
	/// Process on Copyleaks servers
	/// </summary>
	public class CopyleaksProcess
	{
		#region Members & Properties
		/// <summary>
		/// The process Id 
		/// </summary>
		public Guid PID { get; set; }

		/// <summary>
		/// The process creation time
		/// </summary>
		public DateTime CreationTimeUtc { get; set; }

		/// <summary>
		/// Login Token to use while accessing the API services
		/// </summary>
		private LoginToken SecurityToken { get; set; }

		public Dictionary<string, string> CustomFields { get; private set; }

		private bool ListProcesses_IsCompleted { get; set; } = false;

		public eProduct Product { get; private set; }

		#endregion

		public CopyleaksProcess(LoginToken authorizationToken, eProduct product, ProcessInList rawProcess)
		{
			this.PID = rawProcess.ProcessId;
			this.CreationTimeUtc = rawProcess.CreationTimeUTC;
			this.SecurityToken = authorizationToken;
			this.CustomFields = rawProcess.CustomFields;
			this.ListProcesses_IsCompleted = rawProcess.Status.ToLower() == "finished";
			this.Product = product;
		}

		public CopyleaksProcess(LoginToken authorizationToken, eProduct product, CreateResourceResponse response, Dictionary<string, string> customFields)
		{
			this.PID = response.ProcessId;
			this.CreationTimeUtc = response.CreationTimeUTC;
			this.SecurityToken = authorizationToken;
			this.CustomFields = customFields;
			this.Product = product;
		}

		/// <summary>
		/// Checks if the operation has been completed
		/// </summary>
		/// <returns>Returns true if the operation has been completed</returns>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		public bool IsCompleted(out ushort currentProgress)
		{
			if (this.ListProcesses_IsCompleted)
			{
				currentProgress = 100;
				return true;
			}

			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'.

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg;
				msg = Retry.Http<HttpResponseMessage>(
					() => client.GetAsync(string.Format("{0}/{1}/{2}/status", Resources.ServiceVersion, this.Product.ToName(), this.PID)).Result,
					TimeSpan.FromSeconds(3),
					3);

				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				string json = msg.Content.ReadAsStringAsync().Result;

				CheckStatusResponse response = JsonConvert.DeserializeObject<CheckStatusResponse>(json);
				currentProgress = response.ProgressPercents;
				return response.Status == "Finished";
			}
		}

		/// <summary>
		/// Get the scan resutls from server.
		/// </summary>
		/// <returns>Scan results</returns>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		public ResultRecord[] GetResults()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'

			string json;
			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.GetAsync(string.Format("{0}/{1}/{2}/result", Resources.ServiceVersion, this.Product.ToName(), this.PID)).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				json = msg.Content.ReadAsStringAsync().Result;
			}
			ResultRecord[] results = JsonConvert.DeserializeObject<ResultRecord[]>(json);
			results = results.OrderByDescending(result => result.Percents).ToArray();
			return results;
		}

		public Stream DownloadSourceText()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.GetAsync(string.Format("{0}/downloads/source-text?pid={1}", Resources.ServiceVersion, this.PID)).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				return msg.Content.ReadAsStreamAsync().Result;
			}
		}

		public Stream DownloadResultText(ResultRecord result)
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.GetAsync(result.CachedVersion).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				return msg.Content.ReadAsStreamAsync().Result;
			}
		}

		public ComparisonReport DownloadResultComparison(ResultRecord result)
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'
			string json;
			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.GetAsync(result.ComparisonReport).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);

				json = msg.Content.ReadAsStringAsync().Result;
			}
			return JsonConvert.DeserializeObject<ComparisonReport>(json);
		}

		/// <summary>
		/// Deletes the process once it has finished running
		/// </summary>
		/// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
		public void Delete()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.DeleteAsync(string.Format("{0}/{1}/{2}/delete", Resources.ServiceVersion, this.Product.ToName(), this.PID)).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg);
			}
		}

		public override string ToString()
		{
			return this.PID.ToString();
		}
	}
}
