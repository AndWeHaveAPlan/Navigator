using Microsoft.Extensions.DependencyInjection;
using Navigator.Pipeline;

namespace Navigator.Builder
{
    public interface IStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        void ConfigureServices(IServiceCollection services);

        // This method gets called by the runtime. Use this method to configure the request pipeline.
        void Configure(IPipelineBuilder pipeline);
    }
}
