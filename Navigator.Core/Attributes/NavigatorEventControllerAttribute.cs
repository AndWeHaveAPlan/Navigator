using System;

namespace Navigator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NavigatorEventControllerAttribute : Attribute
    {
        public string ServiceName { get; }

        /// <summary>
        /// NavigatorHost name to listen for events
        /// </summary>
        /// <param name="serviceName"></param>
        public NavigatorEventControllerAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}