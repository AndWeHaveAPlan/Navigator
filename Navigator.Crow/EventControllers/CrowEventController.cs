using System;
using Microsoft.Extensions.Logging;
using Navigator.Core.Attributes;

namespace Navigator.Crow.EventControllers
{
    [NavigatorEventController("crow")]
    public class CrowEventController
    {
        private readonly ILogger<CrowEventController> _logger;
        public static event EventHandler<string> OnAck;

        public CrowEventController(ILogger<CrowEventController> logger)
        {
            _logger = logger;
        }

        private static int i = 0;

        [NavigatorMethod("ACK")]
        public void SampleMethod(object srt)
        {
            Console.WriteLine(i);
            i++;
            _logger.LogInformation(srt.ToString());
            OnAck?.Invoke(this, srt.ToString());
        }
    }
}
