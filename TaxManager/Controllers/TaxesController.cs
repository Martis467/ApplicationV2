using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxManager.DAL;
using TaxManager.Exceptions;
using TaxManager.Models;
using TaxManager.Services;

namespace TaxManager.Controllers
{
    [Route("api/taxes")]
    public class TaxesController : Controller
    {
        private readonly TaxContext _context;
        private readonly IMunicipalityTaxService _taxService;

        public TaxesController(TaxContext context, IServiceProvider services)
        {
            _context = context;
            _taxService = services.GetMunicipalityTaxService();
        }

        // For testing
        public TaxesController(TaxContext context)
        {
            _context = context;
            _taxService = new MunicipalityTaxService(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetTax(string municpality, DateTime date)
            => Ok(await _taxService.GetByMunicipalityAndDate(municpality, date));

        [HttpPost]
        public async Task<IActionResult> AddMunicipalityTax([FromBody] MunicipalityTax tax)
            => Ok(await _taxService.AddMunicipalityTax(tax));

        [HttpPost("import")]
        public async Task<IActionResult> ImportMunicipalities(IFormFile file)
        {
                if (file == null)
                    throw new TMException(TMExceptionCode.Import.FileNotGiven);

                if (!file.FileName.EndsWith(".xlsx"))
                    throw new TMException(TMExceptionCode.Import.BadFileExtension);

                await _taxService.ImportMunicipalities(file.OpenReadStream());
                return Ok();
        }
    }
}
