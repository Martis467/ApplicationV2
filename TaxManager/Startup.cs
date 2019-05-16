using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedbullRuby.API.Middleware;
using TaxManager.DAL;
using TaxManager.Services;

namespace TaxManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adding database
            services.AddDbContext<TaxContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            var config = services.BuildServiceProvider().CreateScope().ServiceProvider.GetService<IConfigurationRoot>();

            ServiceExtensions.ConfigureServices(services, config);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ApiExceptionMiddleware>();
            app.UseMvc();
        }
    }
}
