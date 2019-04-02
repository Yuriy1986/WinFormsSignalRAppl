using System;
using System.Windows.Forms;
using Microsoft.AspNet.SignalR.Client;
using System.Net.Http;
using System.IO;

namespace WinFormsSignalRAppl.Client
{
    public partial class ClientForm : Form
    {
        IHubProxy HubProxy { get; set; }
        string ServerURI { get; } = "http://localhost:8080/signalR";
        HubConnection Connection { get; set; }
        bool CheckConnection_Closed { get; set; } = false;
        Timer Timer { get; set; }
        Random random = new Random();

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
                ConnectAsync();
        }

        private void SaveFile(string fileContent)
        {
            string pathDirectory = SaveBox.Text;
            if (Directory.Exists(pathDirectory))
            {
                string path = pathDirectory +@"\"+ DateTime.Now.ToShortDateString() +"-"+ random.Next(2147483647) + ".txt";
                File.WriteAllText(path, fileContent);
                return;
            }
            MessageBox.Show("Folder for save does not exist");
        }

        private async void ConnectAsync()
        {
            bool checkIfSameName = true;

            NameBox.Enabled = false;
            MonitoringButton.Enabled = false;
            SaveButton.Enabled = false;
            ConnectButton.Enabled = false;

            Connection = new HubConnection(ServerURI);
            Connection.Headers["userName"] = NameBox.Text;
            HubProxy = Connection.CreateHubProxy("SignalRHub");

            HubProxy.On<string, string>("GetFile", (name, fileContent) =>
                this.Invoke((Action)(() =>
                {
                    LogBox.AppendText(name + " send file\n");
                    SaveFile(fileContent);
                })));

            HubProxy.On("IfSameName", () =>
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("User with the same name is exist.");
                    Connection_Closed();
                    checkIfSameName = false;
                })));

            try
            {
                await Connection.Start();
                if (checkIfSameName)
                {
                    DisconnectButton.Enabled = true;
                    Connection.Closed += Connection_Closed;
                    LogBox.AppendText("Connected to server at " + ServerURI + "\n");
                    CheckConnection_Closed = true;
                    Timer = new Timer();
                    Timer.Interval = 20000;
                    Timer.Tick += Timer_Tick;
                    Timer.Start();
                }
            }
            catch (HttpRequestException)
            {
                LogBox.AppendText("Unable to connect to server: Start server before connecting clients.\n");
                CheckConnection_Closed = false;
                Connection_Closed();
                return;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string pathDirectory = MonitoringBox.Text;
            if (Directory.Exists(pathDirectory))
            {
                string[] files = Directory.GetFiles(pathDirectory, "*.txt");
                if (files.Length != 0)
                {
                    foreach (var file in files)
                    {
                        string fileContent = File.ReadAllText(file);
                        HubProxy.Invoke("SendFile", fileContent);
                        File.Delete(file);
                    }
                }
                return;
            }
            MessageBox.Show("Folder for monitoring  does not exist");
        }

        private void Connection_Closed()
        {
            this.Invoke((Action)(() =>
            {
                NameBox.Enabled = true;
                MonitoringButton.Enabled = true;
                SaveButton.Enabled = true;

                ConnectButton.Enabled = true;
                DisconnectButton.Enabled = false;
                if (CheckConnection_Closed)
                {
                    LogBox.AppendText("Server " + ServerURI + " disconnected.\n");
                    Timer.Stop();
                }
                Connection.Closed -= Connection_Closed;
            }));
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            Connection_Closed();
            CloseConnection();
        }

        private bool ValidateForm()
        {
            if (NameBox.Text != "" && MonitoringBox.Text != "" && SaveBox.Text != "" && MonitoringBox.Text != SaveBox.Text)
                return true;

            else if (NameBox.Text == "" || MonitoringBox.Text == "" || SaveBox.Text == "")
                MessageBox.Show("All fields are required.");
            else
                MessageBox.Show("Paths are same.");
            return false;
        }

        private void MonitoringButton_Click(object sender, EventArgs e)
        {
            if (MonitoringDialog.ShowDialog() == DialogResult.OK)
                MonitoringBox.Text = MonitoringDialog.SelectedPath;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (SaveDialog.ShowDialog() == DialogResult.OK)
                SaveBox.Text = SaveDialog.SelectedPath;
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnection();
        }

        private void CloseConnection()
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

    }
}
