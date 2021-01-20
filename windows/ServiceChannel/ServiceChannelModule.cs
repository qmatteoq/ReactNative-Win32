using Microsoft.ReactNative.Managed;
using System;
using System.Net.Http;
using Windows.ApplicationModel;

namespace ServiceChannel
{
    [ReactModule]
    class ServiceChannelModule
    {
        [ReactMethod("launchFullTrustProcess")]
        public async void LaunchFullTrustProcessAsync(IReactPromise<bool> promise)
        {
            try
            {
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
    }
}

