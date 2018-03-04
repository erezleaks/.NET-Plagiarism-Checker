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
using System.Net.Http;

namespace Copyleaks.SDK.API.Models
{
	/// <summary>
	/// Additional options to your processes.
	/// </summary>
	public class ProcessOptions
	{
		/// <summary>
		/// Add Http callback to your requests.
		/// </summary>
		public Uri HttpCallback { get; set; }

		/// <summary>
		/// Notify you about new results exactly on the time the server found them.
		/// </summary>
		public Uri InProgressResultsCallback { get; set; }

		/// <summary>
		/// Add your own custom fields to your requests. 
		/// You can store any kind of information which will be later available under 'CopyleaksCloud.Processes'. 
		/// </summary>
		public Dictionary<string, string> CustomFields { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// You can register a callback email to get informed when the request is completed. 
		/// </summary>
		public string EmailCallback { get; set; }

		/// <summary>
		/// Enable Sandbox mode for testing purposes
		/// </summary>
		public bool SandboxMode { get; set; }

        /// <summary>
        /// Compare files and documents to one another and not against other online or external sources.
        /// </summary>
        public bool CompareDocumentsForSimilarity { get; set; }

        protected const string COPYLEAKS_HEADER_PREFIX = "copyleaks-";
		/// <summary>
		/// Add custom copyleaks-headers. 
		/// </summary>
		/// <param name="client">Http client for manipulation</param>
		public virtual void AddHeaders(HttpClient client)
		{
			if (this.HttpCallback != null)
				client.DefaultRequestHeaders.Add(
					COPYLEAKS_HEADER_PREFIX + "http-completion-callback",
					this.HttpCallback.AbsoluteUri); // Add HTTP callback to the request header.

			if (this.InProgressResultsCallback != null)
				client.DefaultRequestHeaders.Add(
					COPYLEAKS_HEADER_PREFIX + "in-progress-new-result",
					this.InProgressResultsCallback.AbsoluteUri); // Add HTTP callback to the request header.

			const string CLIENT_CUSTOM_PREFIX = COPYLEAKS_HEADER_PREFIX + "client-custom-";
			if (this.CustomFields != null)
				foreach (var header in this.CustomFields)
					if (client.DefaultRequestHeaders.Contains(CLIENT_CUSTOM_PREFIX + header.Key))
						throw new InvalidDataException("Cannot insert multiple headers with same name ('" + CLIENT_CUSTOM_PREFIX + header.Key + ")!");
					else
						client.DefaultRequestHeaders.Add(CLIENT_CUSTOM_PREFIX + header.Key, header.Value);

			const string EMAIL_CALLBACK = COPYLEAKS_HEADER_PREFIX + "email-callback";
			if (this.EmailCallback != null)
				client.DefaultRequestHeaders.Add(EMAIL_CALLBACK, this.EmailCallback);

			const string SANDBOX_MODE_HEADER = COPYLEAKS_HEADER_PREFIX + "sandbox-mode";
			if (this.SandboxMode)
				client.DefaultRequestHeaders.Add(SANDBOX_MODE_HEADER, "");

            const string COMPARE_DOCUMENTS_FOR_SIMILARITY_HEADER = COPYLEAKS_HEADER_PREFIX + "compare-documents-for-similarity";
            if (this.CompareDocumentsForSimilarity)
                client.DefaultRequestHeaders.Add(COMPARE_DOCUMENTS_FOR_SIMILARITY_HEADER, "");
        }

	}
}
