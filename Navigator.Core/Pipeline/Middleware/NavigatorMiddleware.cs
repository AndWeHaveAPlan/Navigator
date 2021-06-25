using System;
using System.Reflection;
using System.Threading.Tasks;
using Immaterium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Core;
using Navigator.Exceptions;

namespace Navigator.Pipeline.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NavigatorMiddleware : IMiddleware
    {
        // TODO fix
        private readonly ILogger _logger = null;// TbxLogger.TbxLogger.GetLogger<NavigatorMiddleware>();

        public ImmateriumHost ImmateriumHost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Handle(NavigatorContext context, Func<Task> next)
        {
            ImmateriumMessage requestMessage = context.Request;

            _logger.LogTrace("Navigator begin handle message");

            ControllerAction controllerAction = context.ControllerAction;

            object returnedObject = await InvokeControllerMethod(controllerAction, requestMessage, context);

            if (requestMessage.Type == ImmateriumMessageType.StrictRequest)
            {
                context.ResponseObject = returnedObject;
            }

            await next();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerAction"></param>
        /// <param name="immateriumMessage"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<object> InvokeControllerMethod(ControllerAction controllerAction, ImmateriumMessage immateriumMessage, NavigatorContext context)
        {
            object controllerInstance = CreateController(controllerAction.ControllerType, immateriumMessage, context);

            object[] invokeParams = context.RequestParameters;

            object returnedObject;
            try
            {
                returnedObject = controllerAction.ControllerMethod.Invoke(controllerInstance, invokeParams);
            }
            catch (TargetInvocationException e)
            {
                var wrappedException =
                    new NavigatorException("Exception in " + controllerAction.ControllerMethod.Name, e.InnerException);
                throw wrappedException;
            }
            catch (Exception e)
            {
                var wrappedException =
                    new NavigatorException("Exception in " + controllerAction.ControllerMethod.Name, e);
                throw wrappedException;
            }

            // If async controllerAction
            if (returnedObject is Task task)
            {
                await task.ConfigureAwait(false);
                PropertyInfo resultProperty = task.GetType().GetProperty("Result");

                returnedObject = resultProperty.GetValue(task);
            }

            return returnedObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerType"></param>
        /// <param name="immateriumMessage"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private object CreateController(Type controllerType, ImmateriumMessage immateriumMessage, NavigatorContext context)
        {
            object controllerInstance = context.ServiceProvider.GetRequiredService(controllerType);

            typeof(BaseNavigatorController).GetProperty("Logger")
                .SetValue(controllerInstance, context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(controllerType.Name));

            typeof(BaseNavigatorController).GetProperty("Context")
                .SetValue(controllerInstance, context);

            return controllerInstance;
        }
    }
}