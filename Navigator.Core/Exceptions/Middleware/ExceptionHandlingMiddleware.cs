using System;
using System.Reflection;
using System.Threading.Tasks;
using Navigator.Core;
using Navigator.DataContracts;
using Navigator.Pipeline;

namespace Navigator.Exceptions.Middleware
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
                        ResultCode = 9,
                        ErrorMessage =
                            (e is TargetInvocationException ? e.InnerException : e)?.Message
                    };

                    context.Response.Body = response;
                }
            }
        }

        public ImmateriumHost ImmateriumHost { get; set; }
    }
}

