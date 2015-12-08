using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Extentions;
using Copyleaks.SDK.API.Helpers;
using Copyleaks.SDK.API.Models;
using Copyleaks.SDK.API.Models.Responses;
using Copyleaks.SDK.API.Properties;
using Newtonsoft.Json;

namespace Copyleaks.SDK.API
{
	public class CopyleaksProcess
	{
		#region Members & Properties

		public Guid PID { get; set; }

		public DateTime CreationTimeUtc { get; set; }

		private LoginToken SecurityToken { get; set; }

		#endregion

		internal CopyleaksProcess(LoginToken authorizationToken, ProcessInList rawProcess)
		{
			this.PID = rawProcess.ProcessId;
			this.CreationTimeUtc = rawProcess.CreationTimeUTC;
			this.SecurityToken = authorizationToken;
		}

		internal CopyleaksProcess(LoginToken authorizationToken, CreateResourceResponse response)
		{
			this.PID = response.ProcessId;
			this.CreationTimeUtc = response.CreationTimeUTC;
			this.SecurityToken = authorizationToken;
		}
        #region IsCompleted
        /// <summary>
        /// Checks if the operation has been completed
        /// </summary>
        /// <returns>Returns true if the operation has been completed</returns>
        /// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
        public async Task<bool> IsCompletedAsync()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'.

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = await client.GetAsync(string.Format("{0}/detector/{1}/status", Resources.ServiceVersion, this.PID));
				if (!msg.IsSuccessStatusCode)
				{
					string errorResponse = await msg.Content.ReadAsStringAsync();
					BadResponse error = JsonConvert.DeserializeObject<BadResponse>(errorResponse);
					if (error == null)
						throw new JsonException("Unable to process server response.");
					else
						throw new CommandFailedException(error.Message, msg.StatusCode);
				}

				string json = await msg.Content.ReadAsStringAsync();

				CheckStatusResponse response = JsonConvert.DeserializeObject<CheckStatusResponse>(json);
				return response.Status == "Finished";
			}
		}

        /// <summary>
        /// Checks if the operation has been completed
        /// </summary>
        /// <returns>Returns true if the operation has been completed</returns>
        /// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
        public bool IsCompleted()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'.

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg;
				msg = Retry.Http<HttpResponseMessage>(
					() => client.GetAsync(string.Format("{0}/detector/{1}/status", Resources.ServiceVersion, this.PID)).Result,
					TimeSpan.FromSeconds(3), 
					3);

				if (!msg.IsSuccessStatusCode)
				{
					string errorResponse = msg.Content.ReadAsStringAsync().Result;
					BadResponse error = JsonConvert.DeserializeObject<BadResponse>(errorResponse);
					if (error == null)
						throw new JsonException("Unable to process server response.");
					else
						throw new CommandFailedException(error.Message, msg.StatusCode);
				}

				string json = msg.Content.ReadAsStringAsync().Result;

				CheckStatusResponse response = JsonConvert.DeserializeObject<CheckStatusResponse>(json);
				return response.Status == "Finished";
			}
		}
        #endregion

        #region GetResults
        /// <summary>
        /// Get the scan results from the server
        /// </summary>
        /// <returns>Scan results</returns>
        /// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
        public async Task<ResultRecord[]> GetResultsAsync()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = await client.GetAsync(string.Format("{0}/detector/{1}/result", Resources.ServiceVersion, this.PID));
				if (!msg.IsSuccessStatusCode)
				{
					string errorResponse = msg.Content.ReadAsStringAsync().Result;
					BadResponse error = JsonConvert.DeserializeObject<BadResponse>(errorResponse);
					if (error == null)
						throw new JsonException("Unable to process server response.");
					else
						throw new CommandFailedException(error.Message, msg.StatusCode);
				}

				string json = await msg.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<ResultRecord[]>(json);
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

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = client.GetAsync(string.Format("{0}/detector/{1}/result", Resources.ServiceVersion, this.PID)).Result;
				if (!msg.IsSuccessStatusCode)
				{
					string errorResponse = msg.Content.ReadAsStringAsync().Result;
					BadResponse error = JsonConvert.DeserializeObject<BadResponse>(errorResponse);
					if (error == null)
						throw new JsonException("Unable to process server response.");
					else
						throw new CommandFailedException(error.Message, msg.StatusCode);
				}

				string json = msg.Content.ReadAsStringAsync().Result;
				return JsonConvert.DeserializeObject<ResultRecord[]>(json);
			}
		}
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the process once it has finished running 
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">The login-token is undefined or expired</exception>
        public async void DeleteAsync()
		{
			this.SecurityToken.Validate(); // may throw 'UnauthorizedAccessException'.

			using (HttpClient client = new HttpClient())
			{
				client.SetCopyleaksClient(HttpContentTypes.Json, this.SecurityToken);

				HttpResponseMessage msg = await client.DeleteAsync(string.Format("detector/{0}/delete", this.PID));
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg.ReasonPhrase, msg.StatusCode);
			}
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

				HttpResponseMessage msg = client.DeleteAsync(string.Format("detector/{0}/delete", this.PID)).Result;
				if (!msg.IsSuccessStatusCode)
					throw new CommandFailedException(msg.ReasonPhrase, msg.StatusCode);
			}
		}
		#endregion
	}
}
