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
using System.IO;
using System.Threading;
using Copyleaks.SDK.API;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Models;
using Copyleaks.SDK.SampleCode.Helpers;

namespace Copyleaks.SDK.SampleCode
{
	public class Program
	{
		static void Main(string[] args)
		{
			// Usage:
			// SampleCode.exe -e "<YOUR_EMAIL>" -k "Your Account Key" --url "http://site.com/your-webpage"
			// OR 
			// SampleCode.exe -e "<YOUR_EMAIL>" -k "Your Account Key" --local-document "C:\your-directory\document.doc"

			CommandLineOptions options = new CommandLineOptions();
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
				Environment.Exit(1);
			else if ((options.URL == null && options.LocalFile == null) || (options.URL != null && options.LocalFile != null))
			{
				Console.WriteLine("Error: You can speicfy either a URL or a local document to scan. For more information please enter '--help'.");
				Environment.Exit(1);
			}

			Uri httpCallback = null;
			if (options.HttpCallback != null)
			{
				// Http callback example value:
				// https://your-website.com/copyleaks-gateway?id={PID}
				// Copyleaks server will replace the "{PID}" token with the actual process id.
				if (!Uri.TryCreate(options.HttpCallback, UriKind.Absolute, out httpCallback))
				{
					Console.WriteLine("ERROR: Bad Http-Callback.");
					Environment.Exit(3);
				}
			}

			// For more information, visit Copyleaks "How-To page": https://api.copyleaks.com/Guides/HowToUse
			// Creating Copyleaks account: https://copyleaks.com/Account/Register
			// Use your Copyleaks account information.
			// Generate your Account API Key: https://api.copyleaks.com/Home/Dashboard

			// Copyleaks api supports two products: Businesses and Academic. 
			// Select the product the suitible for you.
			CopyleaksCloud copyleaks = new CopyleaksCloud(eProduct.Businesses);
			CopyleaksProcess createdProcess;
			ResultRecord[] results;
			ProcessOptions scanOptions = new ProcessOptions();
			scanOptions.HttpCallback = httpCallback;

			// In Sandbox scan you don't need credits. 
			// Read more @ https://api.copyleaks.com/Documentation/RequestHeaders#sandbox-mode
			// After you finish the integration with Copyleaks, remove this line.
			scanOptions.SandboxMode = true;

			try
			{
				#region Login to Copyleaks cloud

				Console.Write("Login to Copyleaks cloud...");
				copyleaks.Login(options.Email, options.ApiKey);
				Console.WriteLine("Done!");

				#endregion

				#region Checking account balance

				Console.Write("Checking account balance...");
				uint creditsBalance = copyleaks.Credits;
				Console.WriteLine("Done ({0} credits)!", creditsBalance);
				if (creditsBalance == 0)
				{
					Console.WriteLine("ERROR: You do not have enough credits to complete this scan. Your current credit balance is {0}).", creditsBalance);
					Environment.Exit(2);
				}

				#endregion

				#region Submitting a new scan process to the server

				Console.Write("Creating process...");
				if (options.URL != null)
				{
					Uri uri;
					if (!Uri.TryCreate(options.URL, UriKind.Absolute, out uri))
					{
						Console.WriteLine("ERROR: The URL ('{0}') is invalid.", options.URL); // Bad URL format.
						Environment.Exit(1);
					}
					createdProcess = copyleaks.CreateByUrl(uri, scanOptions);
				}
				else
				{
					if (!File.Exists(options.LocalFile))
					{
						Console.WriteLine("ERROR: The file '{0}' does not exist.", options.LocalFile); // Bad URL format.
						Environment.Exit(1);
					}

					createdProcess = copyleaks.CreateByFile(new FileInfo(options.LocalFile), scanOptions);
				}
				Console.WriteLine("Done (PID={0})!", createdProcess.PID);

				#endregion

				#region Waiting for server's process completion

				// Note: We are strongly recommending to use "callbacks" instead of "busy-polling". Use HTTP-callbacks whenever it's possible.
				// Read more @ https://api.copyleaks.com/GeneralDocumentation/RequestHeaders#http-callbacks
				Console.Write("Scanning... ");
				using (var progress = new ProgressBar())
				{
					ushort currentProgress;
					while (!createdProcess.IsCompleted(out currentProgress))
					{
						progress.Report(currentProgress / 100d);
						Thread.Sleep(5000);
					}
				}
				Console.WriteLine("Done.");

				#endregion

				#region Processing finished. Getting results

				results = createdProcess.GetResults();

				if (results.Length == 0)
				{
					Console.WriteLine("No results.");
				}
				else
				{
					for (int i = 0; i < results.Length; ++i)
					{
						Console.WriteLine();
						Console.WriteLine("------------------------------------------------");
						Console.WriteLine("Title: {0}", results[i].Title);
						Console.WriteLine("Information: {0} copied words ({1}%)", results[i].NumberOfCopiedWords, results[i].Percents);
						Console.WriteLine("Introduction: {0}", results[i].Introduction);
						if (results[i].URL != null)
							Console.WriteLine("Url: {0}", results[i].URL);
						Console.WriteLine("Comparison link: {0}", results[i].EmbededComparison);

						#region Optional: Download result full text. Uncomment to activate

						//using (var stream = createdProcess.DownloadResultText(results[i]))
						//using (var sr = new StreamReader(stream, Encoding.UTF8))
						//{
						//	string resultFullText = sr.ReadToEnd();
						//	resultFullText = WebUtility.HtmlDecode(resultFullText); // Decode the text. Treat it like HTML.
						//	Console.WriteLine("Result full-text:");
						//	Console.WriteLine("*****************");
						//	Console.WriteLine(resultFullText);
						//}

						#endregion
					}
				}

				#endregion

				#region Optional: Download source full text. Uncomment to activate.

				//using (var stream = createdProcess.DownloadSourceText())
				//using (var sr = new StreamReader(stream, Encoding.UTF8))
				//{
				//	string sourceFullText = sr.ReadToEnd();
				//	sourceFullText = WebUtility.HtmlDecode(sourceFullText); // Decode the text. Treat it like HTML.
				//	Console.WriteLine("Source full-text:");
				//	Console.WriteLine("*****************");
				//	Console.WriteLine(sourceFullText);
				//}

				#endregion
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine("Failed!");
				Console.WriteLine("Authentication with the server failed!");
				Console.WriteLine("Possible reasons:");
				Console.WriteLine("* You did not log in to Copyleaks cloud");
				Console.WriteLine("* Your login token has expired");
				Environment.Exit(1);
			}
			catch (CommandFailedException theError)
			{
				Console.WriteLine("Failed!");
				Console.WriteLine("*** Error {0}:", theError.CopyleaksErrorCode);
				Console.WriteLine("{0}", theError.Message);
				Environment.Exit(1);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed!");
				Console.WriteLine("Unhandled Exception");
				Console.WriteLine(ex);
				Environment.Exit(1);
			}

			Environment.Exit(0); // SUCCESS
		}
	}
}
