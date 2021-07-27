using System;

namespace Navigator.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NavigatorControllerAttribute : Attribute
    {
        public virtual string ServiceName { get; }

        public virtual string Interface { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="serviceName"></param>
        public NavigatorControllerAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
