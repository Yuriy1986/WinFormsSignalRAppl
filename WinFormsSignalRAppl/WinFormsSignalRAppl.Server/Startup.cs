using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WinFormsSignalRAppl.Server.Startup))]
namespace WinFormsSignalRAppl.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
