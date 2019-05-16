using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxManager.Exceptions
{
    public class ApiErrorResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ApiErrorResponse() { }

        public ApiErrorResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
