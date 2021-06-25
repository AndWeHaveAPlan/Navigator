using System;

namespace Navigator.Exceptions
{
    [Serializable]
    public class NavigatorException : Exception
    {
        public NavigatorException()
        {
        }

        public NavigatorException(string message) : base(message)
        {
        }

        public NavigatorException(string message, Exception innException) : base(message, innException)
        {
        }
    }
}
