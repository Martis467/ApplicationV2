using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TaxManager.DAL;
using TaxManager.Exceptions;
using TaxManager.Extensions;
using TaxManager.Models;
using TaxManager.Models.Database;

namespace TaxManager.Services
{
    public interface IMunicipalityTaxService
    {
        Task<MunicipalityTax> AddMunicipalityTax(MunicipalityTax municipalityTax);
        Task<MunicipalityTax> GetByMunicipalityAndDate(string municipality, DateTime date);
        Task ImportMunicipalities(Stream stream);
    }
    public class MunicipalityTaxService : IMunicipalityTaxService
    {
        private readonly TaxContext _context;

        public MunicipalityTaxService(TaxContext context)
        {
            _context = context;
        }

        public async Task<MunicipalityTax> GetByMunicipalityAndDate(string municipality, DateTime date)
        {
            // Recreating to keep only date
            date = new DateTime(date.Year, date.Month, date.Day);

            var taxes = await _context.Taxes.Include(m => m.Municipality)
                .Where(mt => mt.Municipality.Name == municipality && mt.StartDate == date).ToListAsync();

            if (!taxes.Any())
                throw new TMException(TMExceptionCode.Tax.TaxNotFound);

            Tax appliedTax = taxes.FirstOrDefault();

            taxes.ForEach(t =>
            {
                if (t.Type < appliedTax.Type)
                    appliedTax = t;
            });

            return new MunicipalityTax(appliedTax, appliedTax.Municipality);
        }

        public async Task<MunicipalityTax> AddMunicipalityTax(MunicipalityTax municipalityTax)
        {
            municipalityTax.Verify();

            var municipality = _context.Municapilities.Where(m => m.Name == municipalityTax.MunicipalityName).FirstOrDefault();
            var tax = Tax.FromMunicipalityTax(municipalityTax);

            if (municipality == null)
            {
                _context.Municapilities.Add(new Municipality { Name = municipalityTax.MunicipalityName });
                _context.SaveChanges();
                municipality = _context.Municapilities.Where(m => m.Name == municipalityTax.MunicipalityName).FirstOrDefault();
            }

            tax.MunicipalityId = municipality.Id;
            ValidateTax(tax);

            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            municipalityTax.EndDate = tax.EndDate;

            return municipalityTax;
        }

        public async Task ImportMunicipalities(Stream stream)
        {
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets[0];

                ParseHeader(ref workSheet);
                var taxes = ParseWorksheet(ref workSheet);

                // Add new taxes
                foreach (var tax in taxes)
                    await AddMunicipalityTax(tax);
            }
        }

        private IEnumerable<MunicipalityTax> ParseWorksheet(ref ExcelWorksheet workSheet)
        {
            var row = workSheet.Dimension.Start.Row + 1;
            var list = new List<MunicipalityTax>();

            while (RowHasMandatoryValues(ref workSheet, row))
            {
                list.Add(ParseTax(ref workSheet, row));
                row++;
            }

            return list;
        }

        private MunicipalityTax ParseTax(ref ExcelWorksheet workSheet, int row)
        {
            var tax = new MunicipalityTax();

            tax.MunicipalityName = workSheet.Cells[row, 1].Value.ToString().Trim();
            tax.Value = ParseValue(workSheet.Cells[row, 2].Value.ToString());
            tax.Type = workSheet.Cells[row, 3].Value.ToString().StringToEnum<Tax.TaxType>();
            tax.StartDate = Convert.ToDateTime(workSheet.Cells[row, 4].Value.ToString());

            return tax;
        }

        private bool RowHasMandatoryValues(ref ExcelWorksheet workSheet, int row)
        {
            // If we find an empyt row we can asume we reached the end
            if (workSheet.Cells[row, 1, row, 4].All(cell => cell.Value == null))
                return false;

            return true;
        }

        private void ParseHeader(ref ExcelWorksheet workSheet)
        {
            // Validate if predefined headers are correct

            if (!workSheet.Cells[1, 1].Value.ToString().Equals("Municipality") ||
                !workSheet.Cells[1, 2].Value.ToString().Equals("Tax value") ||
                !workSheet.Cells[1, 3].Value.ToString().Equals("Schedule") ||
                !workSheet.Cells[1, 4].Value.ToString().Equals("Starting date"))
                throw new TMException(TMExceptionCode.Import.IncorrectHeaderFormat);
        }

        // Utility function to parse decimal string for cultural invariance
        private decimal ParseValue(string value)
        {
            value = value.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            var valueFormat = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue);

            if (!valueFormat)
                throw new Exception();

            return parsedValue;
        }

        private void ValidateTax(Tax tax)
        {
            // Get all taxes in the given year
            var taxes = _context.Taxes.Where(t => t.MunicipalityId == tax.MunicipalityId && t.StartDate.Year == tax.StartDate.Year).ToList();

            var yearlyTax = taxes.FirstOrDefault(t => t.Type == Tax.TaxType.Yearly);
            var monthlyTax = taxes.FirstOrDefault(t => t.Type == Tax.TaxType.Monthly && t.StartDate.Month == tax.StartDate.Month);

            var taxInterval = tax.StartDate.AddDays(6).DayOfYear;

            var weeklyTaxes = taxes.Where(t => t.Type == Tax.TaxType.Weekly &&
            ((t.StartDate.DayOfYear < tax.StartDate.DayOfYear && t.EndDate.DayOfYear > tax.StartDate.DayOfYear) || 
            (t.StartDate.DayOfYear < taxInterval && t.EndDate.DayOfYear > taxInterval)));

            var dailyTax = taxes.FirstOrDefault(t => t.StartDate == tax.StartDate && t.EndDate == tax.EndDate);

            if (yearlyTax != null)
                throw new TMException(TMExceptionCode.Tax.YearlyTaxAlreadyExists);

            // Assuming that month taxes always have the same starting day for each month
            if (monthlyTax != null)
                throw new TMException(TMExceptionCode.Tax.MonthlyTaxAlreadyExists);

            if (weeklyTaxes.Any())
                throw new TMException(TMExceptionCode.Tax.WeeklyTaxAlreadyExists);

            if (dailyTax != null)
                throw new TMException(TMExceptionCode.Tax.DailyTaxAlreadyExists);
        }
    }
}