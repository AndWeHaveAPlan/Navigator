using System;
using System.Threading.Tasks;

namespace Navigator.Core.Pipeline.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class SerializationMiddleware : IMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Handle(NavigatorContext context, Func<Task> next)
        {
            await next();

            if (!context.ResponseRequired)
                return;

            var returnedObject = context.ResponseObject;

            // var returnedObjectType = context.ControllerAction.ControllerMethod.ReturnType;

            if (returnedObject != null)
            {
                Type returnedObjectType = returnedObject.GetType();
                if (!(returnedObjectType.IsGenericType &&
                      returnedObjectType.GetGenericTypeDefinition() == typeof(ActionResult<>) ||
                      returnedObjectType == typeof(ActionResult)))
                {
                    Type actionGeneric = typeof(ActionResult<>).MakeGenericType(returnedObjectType);
                    returnedObject = Activator.CreateInstance(actionGeneric, returnedObject);
                }
            }
            else
            {
                returnedObject = new ActionResult<Empty>();
            }

            //string bodyString = JsonSerializerWrapper.Serialize(returnedObject);
            INavigatorSerializer navigatorSerializer = new JsonNavigatorSerializer();

            context.Response.Body = navigatorSerializer.CreateBody(new[] { returnedObject });
        }

        /// <summary>
        /// 
        /// </summary>
        public ImmateriumHost ImmateriumHost { get; set; }
    }
}
