using System;

namespace Navigator.Core.Exceptions
{
    [Serializable]
    public class NavigatorRemoteException : Exception
    {
        public string SerializedInnerException { get; }
        public int ResultCode { get; }

        public NavigatorRemoteException(int resultCode, string message) : base(message)
        {
            ResultCode = resultCode;
        }

        public NavigatorRemoteException(Exception innException) : base("Exception from remote service, see inner exception for details", innException)
        {
        }

        public NavigatorRemoteException(string serializedInnerException) : base("Exception from remote service, unable to deserialize, see SerializedInnerException")
        {
            SerializedInnerException = serializedInnerException;
        }
    }
}
