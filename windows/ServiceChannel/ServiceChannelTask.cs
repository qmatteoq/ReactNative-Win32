using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace ServiceChannel
{
    public sealed class ServiceChannelTask: IBackgroundTask
    {
        private BackgroundTaskDeferral appServiceDeferral = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance.TriggerDetails is AppServiceTriggerDetails)
            {
                appServiceDeferral = taskInstance.GetDeferral();
                AppServiceTriggerDetails details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                Channel.Connection = details.AppServiceConnection;
                Channel.Connection.ServiceClosed += Connection_ServiceClosed;
            }
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            appServiceDeferral.Complete();
        }
    }
}
