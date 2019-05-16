using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TaxManager.Extensions;

namespace TaxManager.Exceptions
{
    public class TMException : Exception, ITMException
    {
        public Enum Code { get; set; }
        public LogLevel Level { get; set; } = LogLevel.None;

        public TMException(Enum code)
            : base(code.GetDescription()) => Code = code;

        public TMException(Enum code, string message)
            : base($"{code.GetDescription()} {Environment.NewLine}{message}") => Code = code;

        public TMException(Enum code, string message, LogLevel logLevel)
            : base($"{code.GetDescription()} {Environment.NewLine}{message}")
        {
            Code = code;
            Level = logLevel;
        }
    }


    public interface ITMException
    {
        Enum Code { get; set; }
    }
}
