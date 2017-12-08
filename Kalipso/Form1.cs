using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Kalipso
{
    public partial class Form1 : Form
    {
        private BufferedWaveProvider bwp;
		private const string API_KEY = "c5b4575db91c170538c449d22b7d1233";
		// change to celsius
		private const string CurrentUrl =
		"http://api.openweathermap.org/data/2.5/weather?" +
		"q=@LOC@&mode=xml&units=metric&APPID=" + API_KEY;

		WaveIn waveIn;
        WaveOut waveOut;
        WaveFileWriter writer;
        //WaveFileReader reader;
        string output = "audio.raw";
        string currentCmd;

		MailSender mailSender;
		public Translator translator;

		Dictionary<string, int> timeoffsetEU = new Dictionary<string, int>()
		{
			{ "Accra", -2 },
			{ "Dublin", -2 },
			{ "Lisbon", -2 },
			{ "London", -2 },

			{ "Berlin", -1 },
			{ "Madrid", -1 },
			{ "Paris", -1 },
			{ "Rome", -1 },
			{ "Vienna", -1 },
			{ "Warsaw", -1 },

			{ "Athens", 0 },
			{ "Bucharest", 0 },
			{ "Helsinki", 0 },
			{ "Jerusalem", 0 },
			{ "Kiev", 0 },

			{ "Istanbul", 1 },
			{ "Moscow", 1 },
			{ "Minsk", 1 }

		};


		Dictionary<string, string> currency_codes = new Dictionary<string, string>()
		{
			{ "russian ruble", "RUB" },
			{ "russian rubles", "RUB" },
			{ "romanian lei", "RON" },
			{ "romanian leu", "RON" },
			{ "dollar", "USD" },
			{ "dollars", "USD" },
			{ "pound", "GBP" },
			{ "pounds", "GBP" },
			{ "peso", "MXN"},
			{ "pesos", "MXN"},
			{ "euro", "EUR" },
			{ "euros", "EUR" },
			{ "yen", "JPY" }

		};


		public Form1()
        {
            InitializeComponent();

			waveOut = new WaveOut();
            waveIn = new WaveIn();

            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_DataAvailable);
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            bwp = new BufferedWaveProvider(waveIn.WaveFormat);
            bwp.DiscardOnBufferOverflow = true;

            btnRecordVoice.Enabled = true;
            btnSave.Enabled = false;
            btnOk.Enabled = false;

			mailSender = new MailSender();
		}

		void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);

        }

        private void waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveOut.Stop();
            //reader.Close();
           // reader = null;
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            Execute();
        }

        private void Execute()
        {
			currentCmd = currentCmd.ToLower().Trim();

            if (currentCmd.Equals("battery"))
            {
                PowerStatus status = SystemInformation.PowerStatus;
                textBoxAns.Text = "Battery:" + status.BatteryLifePercent.ToString("P0");
            }
            else if (currentCmd.Equals("Give my IP", StringComparison.CurrentCultureIgnoreCase))
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                string address = "";
                foreach (var ip in host.AddressList)
                    address = address + ip.ToString() + "\n";
                textBoxAns.Text = address;
            }
            else if (currentCmd.Equals("date"))
            {
                textBoxAns.Text = "Today is " + DateTime.Today.ToString("d");
            }
            else if (currentCmd.Equals("my time"))
            {
                textBoxAns.Text = "It's " + DateTime.Now.ToString("t");
            }
            else if (currentCmd.Equals("calculator"))
            {
                Process.Start(@"C:\Windows\System32\calc.exe");
            }
            else if (currentCmd.Equals("paint"))
            {
                Process.Start(@"C:\Windows\System32\mspaint.exe");
            }
            else if (currentCmd.Equals("perf monitor"))
            {
                Process.Start(@"C:\WINDOWS\system32\perfmon.msc");
            }
            else if (currentCmd.Equals("command line")) {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = @"C:\";
                p.StartInfo.UseShellExecute = false;
                p.Start();
            }
            else if (currentCmd.Equals("perf monitor"))
            {
                Process.Start(@"C:\WINDOWS\system32\perfmon.msc");
            }
            else if (currentCmd.Equals("bye bye"))
            {
                Application.Exit();
            }
            else if (currentCmd.StartsWith("time") && currentCmd.Trim().Count(f => f == ' ') == 1)
            {
                string city = currentCmd.Trim().Split(' ').ToArray()[1];
                //DateTime.Now.AddHours(timeoffsetEU);

                //textBoxAns.Text = "Time at " + city + " " +  time.ToString("hh:mm tt");
                
            }
			else if (currentCmd.StartsWith("send mail"))
			{
				string mailText = currentCmd.Trim().Substring(10);
				mailSender.SendMail(mailText);
				textBoxAns.Text = "Mail sent!";
			}
			else if(currentCmd.StartsWith("translate") && currentCmd.Length > 9)
			{
				string toTranslate = currentCmd.Substring(currentCmd.IndexOf(' ') + 1);
				textBoxAns.Text = toTranslate + " = " + translator.Translate(toTranslate);
			}
			else if (currentCmd.Trim().StartsWith("weather") && currentCmd.Trim().Count(f => f == ' ') == 1)
			{
				string city = currentCmd.Trim().Split(' ').ToArray()[1];
				textBoxAns.Text = getWeatherSummary(city);


			}

			else if (currentCmd.Trim().StartsWith("currency"))
			{
				if (currentCmd.Split(' ').ToArray().Count() < 4)
					textBoxAns.Text = "This command doesn't look complete, are you sure this is what you wanted to say? \r\n" + currentCmd;
				textBoxAns.Text = "Here's the sum I got, from today's rates: " + conversionCMD(currentCmd);
			}
			else


            {
                textBoxAns.Text = ("de bota sa ma iei");
            }
        }

        private void Decode()
        {

            btnRecordVoice.Enabled = true;
            btnSave.Enabled = false;
            btnOk.Enabled = true;

            textBoxAns.Text = "Decoding...";

            if (File.Exists("audio.raw"))
            {

                var speech = SpeechClient.Create();
                var response = speech.Recognize(new RecognitionConfig()
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = "en",
                }, RecognitionAudio.FromFile("audio.raw"));


                currentCmd = "";

                foreach (var result in response.Results)
                {
                    foreach (var alternative in result.Alternatives)
                    {
                        currentCmd = currentCmd + " " + alternative.Transcript;
                    }
                }

                textBoxAns.Text = currentCmd;

                if (textBoxAns.Text.Length == 0)
                    textBoxAns.Text = "No Data ";


            }
            else
            {

                textBoxAns.Text = "Audio File Missing ";
            }
        }

        private void btnRecordVoice_Click(object sender, EventArgs e)
        {
            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                return;
            }

            waveIn.StartRecording();

            btnRecordVoice.Enabled = false;
            btnSave.Enabled = true;
            btnOk.Enabled = false;

        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                waveIn.StopRecording();

                writer = new WaveFileWriter(output, waveIn.WaveFormat);

                btnRecordVoice.Enabled = false;
                btnSave.Enabled = false;
                btnOk.Enabled = true;

                byte[] buffer = new byte[bwp.BufferLength];
                int offset = 0;
                int count = bwp.BufferLength;

                var read = bwp.Read(buffer, offset, count);
                if (count > 0)
                {
                    writer.Write(buffer, offset, read);
                }

                waveIn.Dispose();
                waveIn = new WaveIn();
                waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_DataAvailable);
                waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
                writer.Close();
                writer = null;

               // reader = new WaveFileReader("audio.raw"); 
      
                Decode();

                // reader.Dispose();
                if (File.Exists("audio.raw"))
                    File.Delete("audio.raw");
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
			//textBoxAns.Text = "Am tradus: " + translator.Translate("I want gifts");
        }

		// Get meteo conditions
		private string getConditions(string city)
		{
			// Compose the query URL
			string url = CurrentUrl.Replace("@LOC@", city);
			string textXML = GetFormattedXml(url);
			int startIndex = textXML.IndexOf("temp");
			int length = (textXML.IndexOf("visibility") - 4) - startIndex;
			return textXML.Substring(startIndex, length);

		}

		private string GetFormattedXml(string url)
		{
			// Create a web client.
			using (WebClient client = new WebClient())
			{
				// Get the response string from the URL.
				string xml = client.DownloadString(url);
				return xml;
			}
		}

		private string getWeatherSummary(string city)
		{
			string current_weather = getConditions(city);
			string pattern = @"[0-9]+\.?[0-9]*";
			// parse data

			Match m = Regex.Match(current_weather, pattern);

			string text = "Here is the current weather information for " + city + ":\r\n";
			text = text + "Temperature: " + m.Value + "°C\r\n";

			m = m.NextMatch();
			m = m.NextMatch();
			m = m.NextMatch();

			text = text + "Humidity: " + m.Value + "%\r\n";

			m = m.NextMatch();

			text = text + "Pressure: " + m.Value + "hPa\r\n";

			// different regex
			pattern = @"name=(.*)></speed>";
			m = Regex.Match(current_weather, pattern);
			text = text + "Wind conditions: " + m.Groups[1].Value + "\r\n";

			pattern = @"clouds value.*name=(.*)></clo";
			m = Regex.Match(current_weather, pattern);
			text = text + "Sky: " + m.Groups[1].Value;

			return text;
		}


		public float conversionCMD(string cmd)
		{

			string from, to;
			from = "";
			int value;
			string[] parts = cmd.Trim().Split(' ').ToArray();
			if (parts[1].Contains('$'))
			{
				value = int.Parse(parts[1].Substring(1));
				from = "USD";
			}
			else
			{
				value = int.Parse(parts[1]);
			}

			string pattern = @"currency [$0-9]+ ?(?<from>\w+)? to (?<to>\w+)";
			Match m = Regex.Match(cmd, pattern);
			if (!m.Success)
				Debug.WriteLine("no match");

			Debug.WriteLine(m.Groups["from"].Value + " to " + m.Groups["to"].Value + "\n");

			if (m.Groups["from"].Value.Length != 0)
			{
				//from = m.Groups["from"].Value;
				if (!currency_codes.TryGetValue(m.Groups["from"].Value, out from))
				{
					textBoxAns.Text = "I didn't understand the first currency :(";
					return -1;
				}
			}


			if (!currency_codes.TryGetValue(m.Groups["to"].Value, out to))
			{
				textBoxAns.Text = "I didn't understand the second currency :'(";
				return -1;
			}

			return CurrencyConvert(value, from, to);
		}

		public float CurrencyConvert(decimal amount, string fromCurrency, string toCurrency)
		{

			//Grab your values and build your Web Request to the API
			//https://coinmill.com/{0}_{1}.html?{0}={2}
			string apiURL = String.Format("https://coinmill.com/{0}_{1}.html?{0}={2}", fromCurrency, toCurrency, amount);

			//Make your Web Request and grab the results
			var request = WebRequest.Create(apiURL);
			//request.UseDefaultCredentials = true;
			//request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			//Get the Response
			var streamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.ASCII);

			//Grab your converted value (ie 2.45 USD)
			string pattern = "name=\"" + toCurrency + "\" value=\"([0-9]+\\.?[0-9]*)\"";
			//Debug.WriteLine(streamReader.ReadToEnd());
			Debug.WriteLine(pattern);

			Match result = Regex.Match(streamReader.ReadToEnd(), pattern);

			return float.Parse(result.Groups[1].Value);

		}
	}
}
