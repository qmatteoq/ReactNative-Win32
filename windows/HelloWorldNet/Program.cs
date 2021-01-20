using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using Windows.System;

namespace HelloWorldNet
{
    class Program
    {
        static AutoResetEvent appServiceExit;

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
                string baseAddress = "http://localhost:5000/";
                WebApp.Start<Startup>(url: baseAddress);
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
