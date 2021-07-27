using System;
using System.Threading.Tasks;

namespace Navigator.Core.Pipeline
{
    public interface IMiddleware
    {
        Task Handle(NavigatorContext context, Func<Task> next);
    }
}
