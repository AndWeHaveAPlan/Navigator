using System;

namespace Navigator.Core.Exceptions
{
    [Serializable]
    public class IllegalControllerDeclarationException : NavigatorException
    {
        public Type ControllerType { get; }

        public IllegalControllerDeclarationException(string message, Type controllerType) : base(message)
        {
            ControllerType = controllerType;
        }
    }
}
