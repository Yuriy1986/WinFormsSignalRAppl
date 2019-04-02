using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsSignalRAppl.Server.EF;
using WinFormsSignalRAppl.Server.Models;
using System.Configuration;
using WinFormsSignalRAppl.Server.Entities;
using System.Data.Entity;

namespace WinFormsSignalRAppl.Server
{
    public class SignalRHub : Hub
    {
        FormServer formServer = (FormServer)Application.OpenForms[0];
        static List<UserConnected> UsersConnected = new List<UserConnected>();
        ApplicationDbContext Db { get; }

        public SignalRHub()
        {
            string root = Environment.CurrentDirectory;
            root = root.Replace(@"\bin\Debug", @"\App_Data");
            string connection = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            connection = connection.Replace("DataDirectory", root);
            Db = new ApplicationDbContext(connection);
        }

        public void SendFile(string fileContent)
        {
            var userName = base.Context.Headers["userName"];
            Clients.AllExcept(Context.ConnectionId).getFile(userName, fileContent);
            ClientFile clientFileCurrent = new ClientFile { Name = userName, FileContent = fileContent, MessageTime = DateTime.Now };

            formServer.Invoke((Action)(() =>
            ((RichTextBox)formServer.Controls["ServerLogBox"]).AppendText("Client \"" + userName + " send file\n")));
            Db.ClientFiles.Add(clientFileCurrent);
            Db.Entry(clientFileCurrent).State = EntityState.Added;
            Db.SaveChangesAsync();
        }

        public override Task OnConnected()
        {
            var userName = base.Context.Headers["userName"];
            if(UsersConnected.FirstOrDefault(x=>x.Name==userName)!=null)
            {
                base.Clients.Caller.ifSameName();
                return null;
            }

            UsersConnected.Add(new UserConnected { ConnectionId = Context.ConnectionId, Name = userName });
            formServer.Invoke((Action)(() =>
            ((RichTextBox)formServer.Controls["ServerLogBox"]).AppendText("Client \"" + userName + "\" connected: " + Context.ConnectionId + "\n")));
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = base.Context.Headers["userName"];
            UsersConnected.Remove(UsersConnected.First(x => x.ConnectionId == Context.ConnectionId));
            formServer.Invoke((Action)(() =>
            ((RichTextBox)formServer.Controls["ServerLogBox"]).AppendText("Client \"" + userName + "\" disconnected: " + Context.ConnectionId + "\n")));
            return base.OnDisconnected(stopCalled);
        }
    }
}

