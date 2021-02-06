using Microsoft.ReactNative.Managed;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace ServiceChannel
{
    [ReactModule]
    class ServiceChannelModule
    {

        static StreamReader reader;
        static StreamWriter writer;

        [ReactMethod("launchFullTrustProcess")]
        public async void LaunchFullTrustProcessAsync(IReactPromise<bool> promise)
        {
            try
            {
                var sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();
                ApplicationData.Current.LocalSettings.Values["PackageSid"] = sid;

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                promise.Resolve(true);
            }
            catch (Exception exc)
            {
                promise.Reject(new ReactError { Exception = exc });
            }
        }

        [ReactMethod("sendMessage")]
        public async void SendMessageAsync(string name, string surname, IReactPromise<string> promise)
        {
            HttpClient client = new HttpClient();
            var result = await client.GetStringAsync($"http://localhost:5000/api/sdk?name={name}&surname={surname}");
            promise.Resolve(result);
        }

        [ReactMethod("sendPipeMessage")]
        public void SendPipeMessage(string message, IReactPromise<string> promise)
        {
            uint processId = Vanara.PInvoke.Kernel32.GetCurrentProcessId();
            uint sessionId;
            Vanara.PInvoke.Kernel32.ProcessIdToSessionId(processId, out sessionId);

            Debug.WriteLine($"Session id: {sessionId}");
            var foo = $"Sessions\\{sessionId}\\AppContainerNamedObjects\\{ApplicationData.Current.LocalSettings.Values["PackageSid"]}\\pipe";

            var client = new NamedPipeClientStream(".", @"LOCAL\pipe", PipeDirection.InOut, PipeOptions.Asynchronous);

            client.Connect(10000);

            reader = new StreamReader(client);
            writer = new StreamWriter(client);
            writer.WriteLine(message);
            var result = reader.ReadLine();

            promise.Resolve(result);
        }

    }
}

