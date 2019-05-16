using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxManager.Services;

namespace TaxManager.Tests
{
    public class Tests
    {
        private readonly IMunicipalityTaxService _municipalityTaxService;

        public Tests(IServiceProvider services)
        {
            _municipalityTaxService = services.GetMunicipalityTaxService();
        }


    }
}
