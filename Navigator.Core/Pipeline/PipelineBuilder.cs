using System;
using System.Threading.Tasks;

namespace Navigator.Core.Pipeline
{
    public class PipelineBuilder : IPipelineBuilder
    {
        public AppPipeline Pipeline { get; private set; }

        public PipelineBuilder()
        {
            Pipeline = new AppPipeline();
        }

        public void Use(Func<NavigatorContext, Func<Task>, Task> middleware)
        {
            Pipeline.Funcs.Add(middleware);
        }

        public void Use(IMiddleware middleware)
        {
            Pipeline.Funcs.Add(middleware.Handle);
        }
    }
}