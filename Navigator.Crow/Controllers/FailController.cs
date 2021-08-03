using System;
using System.Threading.Tasks;
using Navigator.Core.Attributes;
using Navigator.Core.Exceptions;

namespace Navigator.Crow.Controllers
{
    [NavigatorController()]
    public class FailController
    {
        /// <summary>
        /// Always throw exception
        /// </summary>
        /// <param name="anyString"></param>
        /// <returns></returns>
        [NavigatorMethod]
        public string Puke(string anyString)
        {
            Exception exception = new Exception("Top level exception", new NavigatorException("inner navigator exception"));
            throw exception;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NavigatorMethod]
        public string NoAnswer()
        {
            Task.Delay(1000 * 60 * 2).Wait();
            return "";
        }
    }
}
