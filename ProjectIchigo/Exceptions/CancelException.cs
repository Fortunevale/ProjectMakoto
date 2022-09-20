using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectIchigo.Exceptions;
internal class CancelException : Exception
{
    public CancelException(string message = null) : base(message)
    {
    }
}
