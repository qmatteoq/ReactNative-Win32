using Microsoft.Owin.Hosting;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Windows.Storage;
using Windows.System;


namespace HelloWorldNet
{
    class Program
    {
        static AutoResetEvent appServiceExit;
        private static StreamReader reader;
        private static StreamWriter writer;
        private DateTime lastSentMessage;
        private bool waitingForReply;

        static void Main(string[] args)
        {
            appServiceExit = new AutoResetEvent(false);
            Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
            appServiceThread.Start();
            appServiceExit.WaitOne();
        }

        static async void ThreadProc()
        {
            var result = await AppDiagnosticInfo.RequestAccessAsync();
            if (result == DiagnosticAccessStatus.Allowed)
            {
                //string baseAddress = "http://localhost:5000/";
                //WebApp.Start<Startup>(url: baseAddress);

                var sid = ApplicationData.Current.LocalSettings.Values["PackageSid"].ToString();

                PipeSecurity pipeSecurity = new PipeSecurity();
                //SecurityIdentifier id = new SecurityIdentifier(sid);
                //PipeAccessRule rule = new PipeAccessRule(id, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                //pipeSecurity.SetAccessRule(rule);

                Vanara.PInvoke.AdvApi32.SafeAllocatedSID appSid;
                Vanara.PInvoke.UserEnv.DeriveAppContainerSidFromAppContainerName("AppServiceSample_e627vcndsd2rc", out appSid);
                var ptr = appSid.DangerousGetHandle();

                var id = new SecurityIdentifier(ptr);


                var pipe = new NamedPipeServerStream(@"LOCAL\pipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0x4000, 0x400);
                var acInfo = pipe.GetAccessControl();
                

                Console.WriteLine("Waiting for connection");

                pipe.WaitForConnection();
                Console.WriteLine("Connection established");



                reader = new StreamReader(pipe);
                writer = new StreamWriter(pipe);

                while (true)
                {
                    var responseMessage = reader.ReadLine();
                    var message = $"Hello {responseMessage}";
                    writer.WriteLine(message);
                    writer.Flush();
                }

            }

            var info = await AppDiagnosticInfo.RequestInfoForAppAsync();
            var watcher = info.FirstOrDefault().CreateResourceGroupWatcher();
            watcher.ExecutionStateChanged += Watcher_ExecutionStateChanged;
            watcher.Start();
        }

        private static void Watcher_ExecutionStateChanged(AppResourceGroupInfoWatcher sender, AppResourceGroupInfoWatcherExecutionStateChangedEventArgs args)
        {
            var state = args.AppResourceGroupInfo.GetStateReport();
            if (state.ExecutionState == AppResourceGroupExecutionState.NotRunning)
            {
                appServiceExit.Set();
            }
        }
    }
}
