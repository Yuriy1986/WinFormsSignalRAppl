using Microsoft.Owin.Hosting;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace WinFormsSignalRAppl.Server
{
    public partial class FormServer : Form
    {
        private IDisposable SignalR { get; set; }
        string ServerURI { get; } = "http://localhost:8080";

        public FormServer()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
                ServerLogBox.Text = "Server started at " + ServerURI + " \n";
            }
            catch (TargetInvocationException)
            {
                MessageBox.Show("Server failed to start. A server is already running on " + ServerURI);
                Close();
            }
        }

        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SignalR != null)
            {
                SignalR.Dispose();
            }
        }
    }

}
