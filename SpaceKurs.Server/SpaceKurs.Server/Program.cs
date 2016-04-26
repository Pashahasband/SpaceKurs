namespace SpaceKurs.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
                //TODO Наверное, тут надо распарсить наличие фоточек, до старта ВебАппа. Например в Images (см. выше)

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


        public static IList<string> GetFolderList(string dirPath)
        {
            var paths = Directory.GetDirectories(dirPath);
            return new List<string>(paths);
        }
        public static IList<string> GetFiles(string dirPath)
        {
            var paths = Directory.GetFiles(dirPath);
            return new List<string>(paths);
        }

        private static void TextChanged()
        {
            
            string dirPath = "C:\\дипломный проект\\SpaceKurs\\SpaceKurs.Server\\SpaceKurs.Server\\bin\\Debug\\photos\\";

            //хз, что с этим дальше делать =)
            var imagePaths = GetFiles(dirPath);

            if ((Images == null || Images.Count == 0) && (imagePaths.Count > 0))
            {
                Images = new List<string>(imagePaths);
                Console.WriteLine("First start....");
            }
            else
            {
                var addedImages = imagePaths.Where(ip => !Images.Contains(ip));
                var deletedImages = Images.Where(i => !imagePaths.Contains(i));

                foreach (var addedImage in addedImages)
                {
                    Console.WriteLine("New file was found: {0}", addedImage);
                    Images.Add(addedImage); 
                }

                //TODO Список неудобен тем, что нельзя просто так взять и удалить, так как тогда все индексы сдвинуться. 
                //А тут вдруг какой-нибудь тормоз может проснуться и затребовать картиночку.
                foreach (var deletedImage in deletedImages)
                {
                    Console.WriteLine("This file was removed: {0}", deletedImage);
                    Images[Images.IndexOf(deletedImage)] = null;
                }
            }
            
            //if (Images.Count != imagePaths.Count )
            //{
            //    for (int j = 0; j < imagePaths.Count; j++)
            //    {
            //        if (!Images.Contains(imagePaths[j]))
            //        {
            //            Console.WriteLine("Find new file... ...." + imagePaths[j]);
            //            Images.Add(imagePaths[j]);
            //        }
            //    }
            //    if (Images.Count > imagePaths.Count)
            //    {
            //        Console.WriteLine("File deleted... ....");
            //        Images.Clear();
            //    }
            //}

        }

        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting server...");
            Task.Run(() => StartServer());
            //StartServer();

            
            while (true) { TextChanged(); }

            
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
