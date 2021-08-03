using System;
using Navigator.Core.Attributes;

namespace Navigator.Crow.EventControllers
{
    [NavigatorEventController("crow")]
    public class CrowEventController
    {
        public static event EventHandler<string> OnAck;

        [NavigatorMethod("ACK")]
        public void SampleMethod(object srt)
        {
            //Logger.LogInformation(srt);
            OnAck?.Invoke(this, srt.ToString());
        }
    }
}
