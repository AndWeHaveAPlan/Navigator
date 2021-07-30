using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Immaterium;
using Microsoft.Extensions.Logging;
using Navigator.Core.Exceptions;
using Navigator.Core.Pipeline.Middleware;

namespace Navigator.Core.Client
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum NavigatorClientType
    {
        Authorized,
        Anonymous,
        Forward
    }

    /// <summary>
    /// 
    /// </summary>
    public class NavigatorClient : INavigatorClient
    {
        private readonly ImmateriumHost _host;
        private readonly ILogger _logger;
        private readonly IEnumerable<INavigatorSerializer> _navigatorSerializers;

        /// <summary>
        /// 
        /// </summary>
        public NavigatorClientType Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public NavigatorAddress BaseAddress = new NavigatorAddress();

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        public NavigatorClient(ImmateriumHost host, ILogger<NavigatorClient> logger, IEnumerable<INavigatorSerializer> navigatorSerializers)
        {
            _host = host;
            _logger = logger;
            _navigatorSerializers = navigatorSerializers;
            Type = NavigatorClientType.Anonymous;
            Timeout = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Send(string to, params object[] args)
        {
            NavigatorAddress address = BaseAddress.Combine(to);
            await Send(address, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Send(NavigatorAddress address, params object[] args)
        {
            var headers = BuildMessageHeaders(address);
            headers.Type = ImmateriumMessageType.Common;
            await _host.Send(headers, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(string to, params object[] args) where T : class
        {
            var address = BaseAddress.Combine(to);
            return await Post<T>(address, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(NavigatorAddress address, params object[] args) where T : class
        {

            var headers = BuildMessageHeaders(address);
            headers.Type = ImmateriumMessageType.Request;

            ImmateriumMessage responseMessage = await _host.Send(headers, args);

            var models = _navigatorSerializers.First().ProcessBody(responseMessage.Body);

            var ret = models.First().GetObject(typeof(ActionResult<T>));

            ActionResult<T> returnedObject = ret as ActionResult<T>;

            if (returnedObject.ResultCode == 0)
                return returnedObject.Value;

            NavigatorRemoteException exception = new NavigatorRemoteException(returnedObject.ResultCode, returnedObject.ErrorMessage);
            throw exception;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<ActionResult<T>> TryPost<T>(string to, params object[] args) where T : class
        {
            var address = BaseAddress.Combine(to);
            return await TryPost<T>(address, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<ActionResult<T>> TryPost<T>(NavigatorAddress address, params object[] args) where T : class
        {
            var headers = BuildMessageHeaders(address);
            headers.Type = ImmateriumMessageType.Request;

            ActionResult<T> returnedObject;

            try
            {
                ImmateriumMessage responseMessage = await _host.Send(headers, args);
                var deserialized = responseMessage.Body as ActionResult<T>;
                returnedObject = deserialized;
            }
            catch (TimeoutException e)
            {
                returnedObject = new ActionResult<Empty>() { ResultCode = 10, ErrorMessage = e.ToString() };
            }
            catch (Exception e)
            {
                returnedObject = new ActionResult<Empty>() { ResultCode = 7, ErrorMessage = e.ToString() };
            }

            return returnedObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task Publish(string method, params object[] body)
        {
            await _host.Publish(method, body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ImmateriumHeaderCollection BuildMessageHeaders(NavigatorAddress address)
        {
            var headers = new ImmateriumHeaderCollection()
            {
                Receiver = address.Service
            };

            headers.Add("Interface", address.Interface);
            headers.Add("Method", address.Method);

            return headers;
        }
    }
}
