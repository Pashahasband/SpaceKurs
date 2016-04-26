namespace SpaceKurs.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Hosting;

    using Owin;

    class Program
    {
        private static IDisposable SignalR { get; set; }

        public static IList<string> Images { get; private set; }

        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private static void StartServer()
        {

            //const string ServerUri = "http://localhost:8080";
            const string ServerUri = "http://192.168.0.203:8080";

            try
            {
                //TODO Наверное, тут надо распарсить наличие фоточек, до старта ВебАппа. YНапример в Images (см. выше)
                SignalR = WebApp.Start(ServerUri);
            }
            catch (TargetInvocationException e)
            {
                Console.WriteLine("Server failed to start. A server is already running on " + ServerUri);
                //Re-enable button to let user try to start server again
                //this.Invoke((Action)(() => ButtonStart.Enabled = true));
                return;
            }
            //this.Invoke((Action)(() => ButtonStop.Enabled = true));
            Console.WriteLine("Server started at " + ServerUri);
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            Console.WriteLine("Processed file '{0}'.", path);
        }

        public static void RecursiveFileProcessor(string[] args)
        {
            foreach (string path in args)
            {
                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }
        }

        public static IList<string> GetFolderList(string dirPath)
        {
            var paths = Directory.GetDirectories(dirPath);
            return new List<string>(paths);
        }
        public static IList<string> GetFiles(string dirPath)//, string[]filesto, int tempfiles)
        {
            var paths = Directory.GetFiles(dirPath);
            return new List<string>(paths);
        }

        private static void TextChanged()
        {
            
            string dirPath = "C:\\дипломный проект\\SpaceKurs\\SpaceKurs.Server\\SpaceKurs.Server\\bin\\Debug\\photos\\";

            //хз, что с этим дальше делать =)
            var imagePaths = GetFiles(dirPath);
        }

        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting server...");
            Task.Run(() => StartServer());
            //StartServer();

            //RecursiveFileProcessor(args);
            


            TextChanged();
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

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseWebApi(config);
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
