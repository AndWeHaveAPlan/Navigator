using System;
using System.Threading.Tasks;
using Immaterium;
using Microsoft.Extensions.Logging;
using Navigator.Core.Exceptions;

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
        public NavigatorClient(ImmateriumHost host, ILogger<NavigatorClient> logger)
        {
            _host = host;
            _logger = logger;
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
            ImmateriumMessage message = BuildMessage(address, args);
            message.Type = ImmateriumMessageType.Common;
            await _host.Send(message);
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

            ImmateriumMessage message = BuildMessage(address, args);
            message.Type = ImmateriumMessageType.Request;

            ImmateriumMessage responseMessage = await _host.Send(message);

            ActionResult<T> returnedObject = responseMessage.Body as ActionResult<T>;

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
            ImmateriumMessage message = BuildMessage(address, args);
            message.Type = ImmateriumMessageType.Request;

            ActionResult<T> returnedObject;

            try
            {
                ImmateriumMessage responseMessage = await _host.Send(message);
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
        public async Task Publish(string method, object body)
        {
            await _host.Publish(method, body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ImmateriumMessage BuildMessage(NavigatorAddress address, object[] args)
        {
            ImmateriumMessage message = new ImmateriumMessage
            {
                Receiver = address.Service
            };

            args ??= new object[] { null };

            //Method = address.Method
            message.Headers.Add("Interface", address.Interface);
            message.Headers.Add("Method", address.Method);
            message.Headers.Add("Timeout", ((int)Timeout.TotalMilliseconds).ToString());

            message.Body = args;

            return message;
        }
    }
}
