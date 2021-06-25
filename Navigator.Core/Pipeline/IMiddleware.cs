using System;
using System.Threading.Tasks;

namespace Navigator.Pipeline
{
    public interface IMiddleware
    {
        Task Handle(NavigatorContext context, Func<Task> next);
    }
}
