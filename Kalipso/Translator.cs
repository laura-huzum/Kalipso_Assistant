using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using Microsoft.Translator.Samples;

namespace Kalipso
{
	class Translator
	{
		public static string authTokenTranslate;

		public Translator()
		{
			MainAsync().Wait();
		}

		private static async Task MainAsync()
		{
			string key = "ad4ad4c1cf0640b68a518a34b31c29a5";

			try
			{
				if (string.IsNullOrWhiteSpace(key))
				{
					throw new ArgumentException("The subscription key has not been provided.");
				}

				var authTokenSource = new AzureAuthToken(key.Trim());
				try
				{
					authTokenTranslate = await authTokenSource.GetAccessTokenAsync();
				}
				catch (HttpRequestException)
				{
					if (authTokenSource.RequestStatusCode == HttpStatusCode.Unauthorized)
					{
						Console.WriteLine("Request to token service is not authorized (401). Check that the Azure subscription key is valid.");
						return;
					}
					if (authTokenSource.RequestStatusCode == HttpStatusCode.Forbidden)
					{
						Console.WriteLine("Request to token service is not authorized (403). For accounts in the free-tier, check that the account quota is not exceeded.");
						return;
					}
					throw;
				}
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine("Error:\n  {0}\n", ex.Message);
			}
		}

		public string Translate(string text)
		{
			try
			{
				return TranslateSample.Run(authTokenTranslate, text);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Eroare la traducerea -- " + text);
				return text;
			}
		}
	}
}
