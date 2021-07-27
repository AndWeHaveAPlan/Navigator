using System;

namespace Navigator.Core.Exceptions
{
    [Serializable]
    public class ApiMethodNotFoundException : NavigatorException
    {
        public string MethodName { get; }

        public ApiMethodNotFoundException(string methodName) : base("No handlers for " + methodName)
        {
            MethodName = methodName;
        }
    }
}
