using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLStructurizer
{
    class MyExceptions
    {
    }

    [Serializable]
    public class ErrorConnectToOracle : Exception
    {
        public ErrorConnectToOracle()
        { }

        public ErrorConnectToOracle(string message)
            : base(message)
        { }

        public ErrorConnectToOracle(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
