using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Navigator.Core;
using Navigator.Exceptions;
using Newtonsoft.Json.Linq;

namespace Navigator.Pipeline.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class DeserializationMiddleware : IMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Handle(NavigatorContext context, Func<Task> next)
        {
            context.RequestParameters = PrepareArgs(context);
            await next();
        }

        /// <summary>
        /// 
        /// </summary>
        public ImmateriumHost ImmateriumHost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private object[] PrepareArgs(NavigatorContext context)
        {
            var controllerAction = context.ControllerAction;
            var immateriumMessage = context.Request;

            ParameterInfo[] methodParameters = controllerAction.ControllerMethod.GetParameters();

            int totalParamsLength = controllerAction.TotalParamsLength;
            int requiredParamsLength = controllerAction.RequiredParamsLength;

            if (controllerAction.HasParamsArg)
                requiredParamsLength -= 1;

            JArray jArray = JArray.Parse(immateriumMessage.Body);

            if (jArray.Count < requiredParamsLength)
            {
                // TODO: create exception class
                throw new NavigatorException("Not enough arguments");
            }

            if (!controllerAction.HasParamsArg && jArray.Count > totalParamsLength)
            {
                // TODO: create exception class
                throw new NavigatorException("Too few arguments");
            }

            object[] invokeParams = new object[methodParameters.Length];

            int i = 0;
            for (; i < jArray.Count; i++)
            {
                if (methodParameters[i].GetCustomAttributes<ParamArrayAttribute>().Any())
                    break;

                try
                {
                    invokeParams[i] = Validate(methodParameters[i].ParameterType, jArray[i]);
                }
                catch (Exception e)
                {
                    throw new NavigatorException($"Invalid parameter #{i + 1}, {methodParameters[i].Name}", e);
                }
            }

            if (controllerAction.HasParamsArg)
            {
                var paramsType = methodParameters[i].ParameterType.GetElementType();
                var paramsTypeArray = paramsType.MakeArrayType();

                var remainingArgs = jArray.Skip(i).ToList();
                Array paramsArgument = (Array)Activator.CreateInstance(paramsTypeArray, remainingArgs.Count);

                for (int index = 0; index < remainingArgs.Count; index++)
                {
                    paramsArgument.SetValue(Validate(paramsType, remainingArgs[index]), index);
                }

                invokeParams[i] = paramsArgument;
            }
            else
            {
                for (; i < methodParameters.Length; i++)
                {
                    invokeParams[i] = Type.Missing;
                }
            }

            return invokeParams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private object Validate(Type targetType, JToken obj)
        {
            var resObject = obj.ToObject(targetType);

            if (resObject == null && targetType.IsClass)
                return null;

            Validator.ValidateObject(resObject, new ValidationContext(resObject));
            return resObject;
        }
    }
}
