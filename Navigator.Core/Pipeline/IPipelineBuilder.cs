using System;
using System.Threading.Tasks;

namespace Navigator.Core.Pipeline
{
    public interface IPipelineBuilder
    {
        void Use(Func<NavigatorContext, Func<Task>, Task> middleware);

        void Use(IMiddleware middleware);
    }
}
