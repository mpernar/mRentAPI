using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIForRentersAPI.Exceptions
{
    public class Exception
    {
        public class EmailContentException : ApplicationException
        {
            public string ExceptionMessage { get; set; }

            public EmailContentException(string exceptionMessage)
            {
                ExceptionMessage = exceptionMessage;
            }
        }
    }
}
