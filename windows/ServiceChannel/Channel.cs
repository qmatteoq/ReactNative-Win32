using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace ServiceChannel
{
    public static class Channel
    {
        public static AppServiceConnection Connection { get; set; }
    }
}
