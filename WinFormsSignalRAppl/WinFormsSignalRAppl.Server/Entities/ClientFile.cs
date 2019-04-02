using System;

namespace WinFormsSignalRAppl.Server.Entities
{
    public class ClientFile
    {
        public int Id { get; set; }

        public DateTime MessageTime { get; set; }

        public string Name { get; set; }

        public string FileContent { get; set; }
    }
}
