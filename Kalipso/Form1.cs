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

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System.Net;
using System.Net.Sockets;

namespace Kalipso
{
    public partial class Form1 : Form
    {
        private BufferedWaveProvider bwp;

        WaveIn waveIn;
        WaveOut waveOut;
        WaveFileWriter writer;
        //WaveFileReader reader;
        string output = "audio.raw";
        string currentCmd;

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
            if (currentCmd.Trim().Equals("battery"))
            {
                PowerStatus status = SystemInformation.PowerStatus;
                textBoxAns.Text = "Battery:" + status.BatteryLifePercent.ToString("P0");
            }
            else if(currentCmd.Trim().Equals("Give my IP", StringComparison.CurrentCultureIgnoreCase))
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                string address = "";
                foreach (var ip in host.AddressList)
                    address = address + ip.ToString() + "\n";
                textBoxAns.Text = address;
            }
            else if(currentCmd.Trim().Equals("date"))
            {
                textBoxAns.Text = "Today is " + DateTime.Today.ToString("d");
            }
            else if (currentCmd.Trim().Equals("time"))
            {
                textBoxAns.Text = "It's " + DateTime.Now.ToString("t");
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
    }
}
