using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Attributes;
using Navigator.Core;
using Navigator.Exceptions;

namespace Navigator.Pipeline.Middleware
{
    internal class MethodsCollectionMiddleware : IMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        // TODO fix
        private readonly ILogger _logger = null;//TbxLogger.TbxLogger.GetLogger<MethodsCollectionMiddleware>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, NavigatorInterface> _interfaces = new Dictionary<string, NavigatorInterface>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, ControllerAction>> _eventControllersMethods = new Dictionary<string, Dictionary<string, ControllerAction>>();

        /// <summary>
        /// 
        /// </summary>
        public ImmateriumHost ImmateriumHost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="assemblyToCollect"></param>
        /// <param name="serviceCollection"></param>
        public MethodsCollectionMiddleware(string serviceName, Assembly assemblyToCollect, IServiceCollection serviceCollection)
        {
            CollectControllersMethods(serviceName, assemblyToCollect);
            CollectEventControllersMethods(assemblyToCollect);

            foreach (var keyValuePair in _interfaces)
            {
                serviceCollection.AddScoped(keyValuePair.Value.ControllerType, keyValuePair.Value.ControllerType);
            }

            foreach (Dictionary<string, ControllerAction> controllerActions in _eventControllersMethods.Values)
            {
                foreach (ControllerAction controllerActionsValue in controllerActions.Values)
                {
                    serviceCollection.AddScoped(controllerActionsValue.ControllerType,
                        controllerActionsValue.ControllerType);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Handle(NavigatorContext context, Func<Task> next)
        {
            var requestMessage = context.Request.RawMessage;

            if (context.IsEvent) // handle events messages
            {
                _logger.LogTrace("Navigator begin handle event");
                string emitterService = requestMessage.Sender;
                string messageMethod = requestMessage.Headers["Method"].ToUpper();

                if (!_eventControllersMethods.ContainsKey(emitterService))
                {
                    return;
                }

                Dictionary<string, ControllerAction> methodsForService = _eventControllersMethods[emitterService];

                if (!methodsForService.ContainsKey(messageMethod))
                {
                    return;
                }

                context.ControllerAction = methodsForService[messageMethod];
                await next();
            }
            else // handle common messages
            {
                _logger.LogTrace("Navigator begin handle message");

                string interfaceHeader = requestMessage.Headers["Interface"] ?? "";

                NavigatorInterface navigatorInterface = _interfaces[interfaceHeader.ToUpper()];

                if (navigatorInterface == null)
                {
                    throw new ApiMethodNotFoundException(requestMessage.Headers["Interface"]);
                }

                string messageMethod = requestMessage.Headers["Method"].ToUpper();

                if (!navigatorInterface.ContainsKey(messageMethod))
                {
                    throw new ApiMethodNotFoundException(messageMethod);
                }

                context.ControllerAction = navigatorInterface[messageMethod];
                await next();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        private string RemoveTail(string str, string tail)
        {
            if (str.EndsWith(tail, StringComparison.InvariantCultureIgnoreCase))
            {
                return str.Remove(str.Length - tail.Length);
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assembly"></param>
        private void CollectControllersMethods(string name, Assembly assembly)
        {
            IEnumerable<Type> controllers =
                assembly.GetExportedTypes()
                    //.Where(t => typeof(BaseNavigatorController).IsAssignableFrom(t))
                    .Where(t =>
                        t.GetCustomAttributes<NavigatorControllerAttribute>()
                        .Any(a => string.Equals(a.ServiceName, name, StringComparison.CurrentCultureIgnoreCase)));


            foreach (Type controllerType in controllers)
            {
                ConstructorInfo[] constructors = controllerType.GetConstructors(BindingFlags.Default);
                if (constructors.Length > 1)
                {
                    throw new IllegalControllerDeclarationException("Too few constructors for controller " + controllerType.Name, controllerType);
                }

                string controllerInterface = controllerType.GetCustomAttribute<NavigatorControllerAttribute>().Interface ?? controllerType.Name;
                string controllerInterfaceName = RemoveTail(controllerInterface, "Controller");

                if (_interfaces.ContainsKey(controllerInterfaceName))
                {
                    throw new IllegalControllerDeclarationException($"Interface '{controllerInterfaceName}' already declared", controllerType);
                }

                NavigatorInterface navigatorInterface = new NavigatorInterface(controllerInterfaceName)
                {
                    ControllerType = controllerType
                };
                _interfaces.Add(navigatorInterface.Name.ToUpper(), navigatorInterface);

                _logger.LogTrace($"Add interface '{navigatorInterface.Name}'");
                foreach (MethodInfo methodInfo in controllerType.GetMethods())
                {
                    var methodAttribute = methodInfo.GetCustomAttribute<NavigatorMethodAttribute>(false);

                    if (methodAttribute == null) continue;

                    var methodParameters = methodInfo.GetParameters();
                    ControllerAction controllerAction = new ControllerAction
                    {
                        ControllerMethod = methodInfo,
                        ControllerType = controllerType,
                        MessageMethodName = (methodAttribute.MethodName ?? methodInfo.Name).ToUpper(),
                        TotalParamsLength = methodParameters.Length,
                        RequiredParamsLength = methodParameters.Count(p => !p.IsOptional),
                        HasParamsArg = methodParameters.LastOrDefault()?.GetCustomAttributes<ParamArrayAttribute>().Any() ?? false
                    };

                    _logger.LogTrace($"-- Add {controllerAction.MessageMethodName}");

                    navigatorInterface[controllerAction.MessageMethodName] = controllerAction;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        private void CollectEventControllersMethods(Assembly assembly)
        {
            // filter controllers
            IEnumerable<Type> controllers =
                assembly.GetExportedTypes()
                    //.Where(t => typeof(BaseNavigatorController).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttributes<NavigatorEventControllerAttribute>().Any());


            foreach (Type controller in controllers)
            {
                var constructors = controller.GetConstructors(BindingFlags.Default);
                if (constructors.Length > 1)
                {
                    throw new IllegalControllerDeclarationException("Too few constructors for controller " + controller.Name, controller);
                }

                var eventControllerAttribute = controller.GetCustomAttribute<NavigatorEventControllerAttribute>();

                Dictionary<string, ControllerAction> methods;

                if (!_eventControllersMethods.ContainsKey(eventControllerAttribute.ServiceName))
                {
                    methods = new Dictionary<string, ControllerAction>();
                    _eventControllersMethods.Add(eventControllerAttribute.ServiceName, methods);
                }
                else
                {
                    methods = _eventControllersMethods[eventControllerAttribute.ServiceName];
                }

                // collect methods
                foreach (MethodInfo methodInfo in controller.GetMethods())
                {
                    var methodAttribute = methodInfo.GetCustomAttribute<NavigatorMethodAttribute>(false);

                    if (methodAttribute == null) continue;

                    var methodParameters = methodInfo.GetParameters();

                    var method = new ControllerAction
                    {
                        ControllerMethod = methodInfo,
                        ControllerType = controller,
                        MessageMethodName = (methodAttribute.MethodName ?? methodInfo.Name).ToUpper(),
                        TotalParamsLength = methodParameters.Length,
                        RequiredParamsLength = methodParameters.Count(p => !p.IsOptional),
                        HasParamsArg = methodParameters.LastOrDefault()?.GetCustomAttributes<ParamArrayAttribute>().Any() ?? false
                    };

                    if (methods.ContainsKey(method.MessageMethodName))
                    {
                        throw new IllegalControllerDeclarationException("Duplicate action definition for event method " + method.MessageMethodName, controller);
                    }

                    methods.Add(method.MessageMethodName, method);
                }
            }
        }
    }
}
