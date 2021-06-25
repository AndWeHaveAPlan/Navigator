using System;

namespace Navigator.Exceptions
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
