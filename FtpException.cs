using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crosse.Net.NewAspFtp
{
    public class FtpException : Exception
    {
        const int GENERIC_ERROR = -2;

        public FtpException()
            : base()
        {
            HResult = GENERIC_ERROR;
        }

        public FtpException(string message)
            : base(message)
        {
            this.HResult = GENERIC_ERROR;
        }

        public FtpException(string message, Exception inner)
            : base(message, inner)
        {
            this.HResult = GENERIC_ERROR;
        }

        public FtpException(string message, int hResult, Exception inner)
            : base(message, inner)
        {
            this.HResult = HResult;
        }

        public FtpException(string message, int hResult)
            : base(message)
        {
            this.HResult = hResult;
        }

        public FtpException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
