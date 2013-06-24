using System;
using System.Collections.Generic;
using System.Linq;

namespace Talifun.Web.Crusher
{
    public class AggregateException : Exception
    {
        private readonly IEnumerable<Exception> _jsExceptions;

        public AggregateException(IEnumerable<Exception> jsExceptions)
        {
            _jsExceptions = jsExceptions;
        }

        public override string ToString()
        {
            var message = _jsExceptions.Select(x => x.ToString()).Aggregate((a, b) => a + Environment.NewLine + "--------------------------------------------------------" + Environment.NewLine + b);
            return message;
        }
    }
}
