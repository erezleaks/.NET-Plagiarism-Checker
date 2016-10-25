<h2>Copyleaks SDK</h2>
<p>
Copyleaks SDK is a simple framework that allows you to perform plagiarism scans and track content distribution around the web, using Copyleaks cloud.
</p>
<p>
With Copyleaks SDK you can submit for scan:  
<ul>
<li>Webpages</li>
<li>Local files - pdf, doc, docx, rtf and more <a href="https://api.copyleaks.com/GeneralDocumentation/TechnicalSpecifications#supportedfiletypes">(see full list)</a></li>
<li>Free text</li>
<li>OCR (Optical Character Recognition) - scanning pictures containing textual content <a href="https://api.copyleaks.com/GeneralDocumentation/TechnicalSpecifications#supportedfiletypes">(see full list)</a></li>
</ul>
Instructions for using the SDK are below. For a quick example demonstrating the SDK capabilities just look at the code examples under “examples”.
</p>
<h3>Integration</h3>
<p>You can integrate with the Copyleaks SDK in one of two ways:</p>
<ol>
<li>Download the code from here, compile it and add reference to the assembly.</li>
<li>Add <i>CopyleaksAPI</i> NuGet by running the following command in the <a href="http://docs.nuget.org/consume/package-manager-console">Package Manager Console</a></li>
<pre>
Install-Package CopyleaksAPI
</pre>
</ol>
<h3>Signing Up and Getting Your API Key</h3>
 <p>To use the Copyleaks API you need to be a registered user. Signing up is quick and free of charge.</p>
 <p><a href="https://copyleaks.com/Account/Register">Signup</a> to Copyleaks and confirm your account by clicking the link on the confirmation email. Generate your personal API key on your dashboard (<a href="https://api.copyleaks.com/businessesapi">Businesses dashboard/</a><a href="https://api.copyleaks.com/academicapi">Academic dashboard/</a><a href="https://api.copyleaks.com/websitesapi">Websites dashboard</a>) under 'Access Keys'. </p>
 <p>For more information check out our <a href="https://api.copyleaks.com/Guides/HowToUse">API guide</a>.</p>
<h3>Example</h3>
<p>This code will show you where the textual content in the parameter ‘url’ has been used online:</p>
<pre>
using System;
using System.Threading;
using Copyleaks.SDK.API;
using Copyleaks.SDK.API.Exceptions;
using Copyleaks.SDK.API.Models;
//...
private static void Scan(string email, string apiKey, string url)
{
	CopyleaksCloud copyleaks = new CopyleaksCloud();
	CopyleaksProcess createdProcess;
	ProcessOptions scanOptions = new ProcessOptions()
	{
		// SandboxMode = true // -------------------> Read more https://api.copyleaks.com/Documentation/RequestHeaders#sandbox-mode
	};
	try
	{
		#region Login to Copyleaks cloud
		Console.Write("Login to Copyleaks cloud...");
		copyleaks.Login(email, apiKey);
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
		createdProcess = copyleaks.CreateByUrl(new Uri(url), scanOptions);
		Console.WriteLine("Done (PID={0})!", createdProcess.PID);
		#endregion
		#region Waiting for server's process completion
		Console.Write("Scanning... ");
		ushort currentProgress;
		while (!createdProcess.IsCompleted(out currentProgress))
			Thread.Sleep(5000);
		Console.WriteLine("Done.");
		#endregion
		#region Processing finished. Getting results
		ResultRecord[] results = createdProcess.GetResults();
		if (results.Length == 0)
		{
			Console.WriteLine("No results.");
		}
		else
		{
			for (int i = 0; i < results.Length; ++i)
			{
				Console.WriteLine();
				Console.WriteLine("Result {0}:", i + 1);
				Console.WriteLine("Url: {0}", results[i].URL);
				Console.WriteLine("Percents: {0}", results[i].Percents);
				Console.WriteLine("CopiedWords: {0}", results[i].NumberOfCopiedWords);
			}
		}
		#endregion
	}
	catch (UnauthorizedAccessException)
	{
		Console.WriteLine("Failed!");
		Console.WriteLine("Authentication with the server failed!");
		Console.WriteLine("Possible reasons:");
		Console.WriteLine("* You did not log in to Copyleaks cloud");
		Console.WriteLine("* Your login token has expired");
	}
	catch (CommandFailedException theError)
	{
		Console.WriteLine("Failed!");
		Console.WriteLine("*** Error {0}:", theError.CopyleaksErrorCode);
		Console.WriteLine("{0}", theError.Message);
	}
}                
</pre>
<h3>Dependencies:</h3>
<ul>
<li><a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">.Net framework 4.5</a></li>
</ul>
<h5>Referenced Assemblies:</h5>
<ul>
<li><a href="https://www.nuget.org/packages/Microsoft.Net.Http">Microsoft.Net.Http</a></li>
<li><a href="https://www.nuget.org/packages/Newtonsoft.Json">Newtonsoft.Json</a></li>
<li><a href="https://www.nuget.org/packages/Microsoft.Bcl">Microsoft.Bcl</a></li>
<li><a href="https://www.nuget.org/packages/Microsoft.Bcl.Build/1.0.21">Microsoft.Bcl.Build</a></li>
</ul>

<h3>Read More</h3>
<ul>
<li><a href="https://api.copyleaks.com/Guides/HowToUse">Copyleaks API guide</a></li>
<li><a href="https://www.nuget.org/packages/CopyleaksAPI/">Copyleaks NuGet package</a></li>
</ul>
