using System;

namespace Navigator.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NavigatorControllerAttribute : Attribute
    {

        public string Interface { get; set; }
    }
}
