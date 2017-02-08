using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Dialog
{
    public partial class Form1 : Form
    {
        private WaveIn waveSource;
        Dialog dialog;
        public Form1()
        {
            InitializeComponent();
            String host = "";
            if (System.IO.File.Exists("default_host.txt"))
            {
                host = System.IO.File.ReadAllText("default_host.txt");
            }
            tbHostAddress.Text = host;

            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                lbDevices.Items.Add($"{waveInDevice + 1}. {deviceInfo.ProductName}");
            }
            lbDevices.SelectedIndex = 0;

            cbScenario.Items.Add(new MultiDevice());
            cbScenario.SelectedIndex = 0;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            dialog = (Dialog)cbScenario.SelectedItem;
            dialog.SetAddress(tbHostAddress.Text);

            waveSource = new WaveIn();
            waveSource.DeviceNumber = lbDevices.SelectedIndex;
            waveSource.WaveFormat = new WaveFormat(16000, 16, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(WaveDataAvailable);
            //waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            //
            //waveFile = new WaveFileWriter(@"C:\Temp\Test0001.wav", waveSource.WaveFormat);

            waveSource.StartRecording();
            dialog.Start(AppendTextBox);
        }

        private void WaveDataAvailable(object sender, WaveInEventArgs e)
        {
            short[] sdata = new short[e.Buffer.Length / 2];
            Buffer.BlockCopy(e.Buffer, 0, sdata, 0, e.Buffer.Length);
            dialog.AddSamples(sdata.ToList());
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            tbLog.Text += value + "\r\n";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }
    }
}
