using System;

namespace Navigator.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NavigatorControllerAttribute : Attribute
    {

        public virtual string Interface { get; set; }

        public NavigatorControllerAttribute()
        {
        }
    }
}
