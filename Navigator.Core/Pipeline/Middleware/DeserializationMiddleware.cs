using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Navigator.Core.Exceptions;

namespace Navigator.Core.Pipeline.Middleware
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

            //object[] jArray = immateriumMessage.Body as object[];

            INavigatorSerializer navigatorSerializer = new JsonNavigatorSerializer();

            var requestModel = navigatorSerializer.ProcessBody(immateriumMessage.Body);

            if (requestModel == null)
            {
                throw new NavigatorException("Invalid request arguments format");
            }

            if (requestModel.Count() < requiredParamsLength)
            {
                // TODO: create exception class
                throw new NavigatorException("Not enough arguments");
            }

            if (!controllerAction.HasParamsArg && requestModel.Count() > totalParamsLength)
            {
                // TODO: create exception class
                throw new NavigatorException("Too few arguments");
            }

            object[] invokeParams = new object[methodParameters.Length];

            int i = 0;
            for (; i < requestModel.Count(); i++)
            {
                if (methodParameters[i].GetCustomAttributes<ParamArrayAttribute>().Any())
                    break;

                try
                {

                    invokeParams[i] = Validate(methodParameters[i].ParameterType, requestModel[i]);
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

                var remainingArgs = requestModel.Skip(i).ToList();
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
        private object Validate(Type targetType, IRequestBodyModel obj)
        {
            //var resObject = Convert.ChangeType(obj, targetType);
            var resObject = obj.GetObject(targetType);
            //var r =obj as (targetType. )
            if (resObject == null && targetType.IsClass)
                return null;

            Validator.ValidateObject(resObject, new ValidationContext(resObject));
            return resObject;
        }
    }
}
