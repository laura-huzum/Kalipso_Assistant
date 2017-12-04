using System;
using System.IO;
using System.Net;

namespace Microsoft.Translator.Samples
{
    class DetectSample
    {
        public static string Run(string authToken, string text)
        {
			string languageDetected;
			string textToDetect = text;
            string uri = "https://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + textToDetect;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            using (WebResponse response = httpWebRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                languageDetected = (string)dcs.ReadObject(stream);
                Console.WriteLine(string.Format("Language detected:{0}", languageDetected));
            }

			return languageDetected;
		}
    }
}
