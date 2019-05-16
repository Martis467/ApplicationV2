using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxManager.Services
{
    public static class ServiceExtensions
    {
        public static IMunicipalityTaxService GetMunicipalityTaxService(this IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IMunicipalityTaxService>();

        // Called on API startup. DI configuration.
        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
        {
            services.AddScoped<IMunicipalityTaxService, MunicipalityTaxService>();
        }
    }
}
