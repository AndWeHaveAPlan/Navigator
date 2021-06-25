using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Navigator.Core;
using Navigator.DataContracts;
using Navigator.Pipeline;
using Navigator.Serialization;

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
                            JsonSerializerWrapper.Serialize(e is TargetInvocationException ? e.InnerException : e)
                    };


                    context.Response.Body = JsonSerializerWrapper.Serialize(response);

                }
            }
        }

        public ImmateriumHost ImmateriumHost { get; set; }
    }
}

