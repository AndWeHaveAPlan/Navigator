using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navigator.Pipeline
{
    public class AppPipeline
    {
        public readonly List<Func<NavigatorContext, Func<Task>, Task>> Funcs = new List<Func<NavigatorContext, Func<Task>, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Run(NavigatorContext context)
        {
            return Process(context, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Process(NavigatorContext context, int i)
        {
            if (Funcs.Count >= i + 1)
                await Funcs[i](context, async () =>
                {
                    await Process(context, i + 1);
                });
        }
    }
}
