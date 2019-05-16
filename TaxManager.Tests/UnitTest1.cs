using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Reflection;
using TaxManager.Controllers;
using TaxManager.DAL;
using TaxManager.Exceptions;
using TaxManager.Models;
using TaxManager.Services;
using Xunit;

namespace TaxManager.Tests
{
    public class UnitTest1
    {
        private readonly TaxContext _context;
        private readonly IMunicipalityTaxService _municipalityTaxService;
        public UnitTest1()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaxContext>();
            optionsBuilder.UseInMemoryDatabase();
            _context = new TaxContext(optionsBuilder.Options);
            _municipalityTaxService = new MunicipalityTaxService(_context);

        }

        [Fact]
        public async void LoadExcell()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Test.xlsx");
            await _municipalityTaxService.ImportMunicipalities(File.OpenRead(path));
        }

        [Fact]
        public async void AddNewMunicipality()
        {
            var tax = new MunicipalityTax()
            {
                Value = 0.2m,
                Type = Models.Database.Tax.TaxType.Yearly,
                StartDate = Convert.ToDateTime("2019-06-05"),
                MunicipalityName = "Klaipeda"
            };

            await _municipalityTaxService.AddMunicipalityTax(tax);

            // trying again will result in an error

            await Assert.ThrowsAsync<TMException>(() => _municipalityTaxService.AddMunicipalityTax(tax));
        }

        [Fact]
        public async void GetTaxByMunicipalityAndTax()
        {
            var municipality = "Vilnius";
            var time = Convert.ToDateTime("2016-07-19");

            var tax = await _municipalityTaxService.GetByMunicipalityAndDate(municipality, time);

            // There are two taxes that start on the given day:
            // weekly with value 0.2 and daily with value 0.1
            Assert.Equal(0.1m, tax.Value);
        }
    }
}
