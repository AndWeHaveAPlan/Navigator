using System;
using System.Threading.Tasks;

namespace Navigator.Core.Client
{
    public interface INavigatorClient
    {
        /// <summary>
        /// 
        /// </summary>
        NavigatorClientType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Send(string to, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Send(NavigatorAddress address, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<T> Post<T>(string to, params object[] args) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<T> Post<T>(NavigatorAddress address, params object[] args) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<ActionResult<T>> TryPost<T>(string to, params object[] args) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<ActionResult<T>> TryPost<T>(NavigatorAddress address, params object[] args) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task Publish(string method, params object[] body);
    }
}