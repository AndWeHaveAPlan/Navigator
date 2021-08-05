using System;

namespace Navigator.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class NavigatorAddress
    {
        public const string Separator = "::";

        public string Service
        {
            get => _strings[0];
            set => _strings[0] = value;
        }

        public string Interface
        {
            get => _strings[1];
            set => _strings[1] = value;
        }

        public string Method
        {
            get => _strings[2];
            set => _strings[2] = value;
        }

        private readonly string[] _strings = new string[3];

        /// <summary>
        /// 
        /// </summary>
        public NavigatorAddress()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="methodName"></param>
        public NavigatorAddress(string serviceName, string interfaceName, string methodName)
        {
            Service = serviceName;
            Interface = interfaceName;
            Method = methodName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullString"></param>
        public NavigatorAddress(string fullString)
        {
            if (fullString == null)
                throw new ArgumentNullException(nameof(fullString));

            string[] parts = fullString.Split(Separator);

            if (parts.Length > 3 || parts.Length == 0)
                throw new ArgumentException("invalid NavigatorAddress format", nameof(fullString));

            for (int i = 0; i < parts.Length; i++)
            {
                _strings[i] = parts[i];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public NavigatorAddress Combine(string str)
        {
            var result = new NavigatorAddress(Service, Interface, Method);

            var parts = str.Split(Separator);

            for (int i = 3 - parts.Length, j = 0; j < parts.Length; i++, j++)
            {
                result._strings[i] = parts[j];
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Service}{Separator}{Interface}{Separator}{Method}";
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator NavigatorAddress(string fullString)
        {
            var address = new NavigatorAddress();

            if (fullString == null)
                return address;

            string[] parts = fullString.Split(Separator);

            if (parts.Length > 3 || parts.Length == 0)
                throw new ArgumentException("invalid NavigatorAddress format", nameof(fullString));

            for (int i = 0; i < parts.Length; i++)
            {
                address._strings[2 - i] = parts[i];
            }

            return address;
        }
    }
}
