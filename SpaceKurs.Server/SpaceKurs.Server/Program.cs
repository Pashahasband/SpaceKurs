using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

using Microsoft.AspNet.SignalR;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using System.Reflection;


namespace SpaceKurs.Server
{

class Program
    {
        private static IDisposable SignalR { get; set; }
        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private static void StartServer()
        {

            const string ServerURI = "http://192.168.0.198:8080";

            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException e)
            {
                Console.WriteLine("Server failed to start. A server is already running on " + ServerURI);
                //Re-enable button to let user try to start server again
                //this.Invoke((Action)(() => ButtonStart.Enabled = true));
                return;
            }
            //this.Invoke((Action)(() => ButtonStop.Enabled = true));
            Console.WriteLine("Server started at " + ServerURI);
        }
        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting server...");
            Task.Run(() => StartServer());
            //StartServer();
            while (true) { }
        }

    }
    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
    /// <summary>
    /// Echoes messages sent using the Send message by calling the
    /// addMessage method on the client. Also reports to the console
    /// when clients connect and disconnect.
    /// </summary>
    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
        public override Task OnConnected()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }


}
