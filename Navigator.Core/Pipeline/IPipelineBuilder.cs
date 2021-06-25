using System;
using System.Threading.Tasks;

namespace Navigator.Pipeline
{
    public interface IPipelineBuilder
    {
        void Use(Func<NavigatorContext, Func<Task>, Task> middleware);

        void Use(IMiddleware middleware);
    }
}
