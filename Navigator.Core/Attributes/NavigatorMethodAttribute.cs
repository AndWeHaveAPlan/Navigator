using System;

namespace Navigator.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class NavigatorMethodAttribute : Attribute
    {
        public string MethodName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName"></param>
        public NavigatorMethodAttribute(string methodName = null)
        {
            MethodName = methodName;
        }
    }
}