using System;
using System.Reflection;

namespace Navigator.Core
{
    internal class ControllerAction
    {
        public string MessageMethodName;

        public MethodInfo ControllerMethod;

        public Type ControllerType;

        public int TotalParamsLength;
        public int RequiredParamsLength;

        public bool HasParamsArg;
    }
}
