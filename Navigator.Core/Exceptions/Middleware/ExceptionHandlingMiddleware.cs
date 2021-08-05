using System;
using System.Reflection;
using System.Threading.Tasks;
using Navigator.Core.Pipeline;
using Navigator.Core.Pipeline.Middleware;

namespace Navigator.Core.Exceptions.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        public async Task Handle(NavigatorContext context, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (Exception e)
            {
                // TODO fix
                //TbxLogger.TbxLogger.GetLogger<ExceptionHandlingMiddleware>().LogError(e, "Exception in pipeline processing. Passed to sender");
                if (context.ResponseRequired)
                {
                    context.Response.Headers["Exception"] = "1";

                    var response = new ActionResult
                    {
                        ResultCode = 500,
                        ErrorMessage =
                            (e is TargetInvocationException ? e.InnerException : e)?.Message
                    };


                    INavigatorSerializer navigatorSerializer = new JsonNavigatorSerializer();
                    context.Response.Body = navigatorSerializer.CreateBody(new object[] { response });
                }
            }
        }

        public ImmateriumHost ImmateriumHost { get; set; }
    }
}

