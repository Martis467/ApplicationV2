using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TaxManager.Exceptions
{
    public class TMExceptionCode
    {
        public enum General
        {
            [Description("Function not implemented yet")]
            NotImplemented = 1,

            [Description("Unknown error occured")]
            UnknownError = 2,

            [Description("Parameters not provided")]
            ParametersNotProvided = 3,
        }

        public enum Tax
        {

            [Description("Given municipality was not found")]
            MunicipalityNotFound = 1000,

            [Description("Tax fields were in incorrect")]
            IncorectTax = 1001,

            [Description("Yearly tax already exists")]
            YearlyTaxAlreadyExists = 1002,

            [Description("Monthly tax already exists")]
            MonthlyTaxAlreadyExists = 1003,

            [Description("Weekly tax already exists")]
            WeeklyTaxAlreadyExists = 1004,

            [Description("Daily tax already exists")]
            DailyTaxAlreadyExists = 1005,

            [Description("Tax not found")]
            TaxNotFound = 1006,
        }

        public enum Import
        {
            [Description("File was not provided")]
            FileNotGiven = 2000,

            [Description("Bad file format")]
            BadFileExtension = 2001,

            [Description("Incorrect header format")]
            IncorrectHeaderFormat = 2002,
        }

    }
}
