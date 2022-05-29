using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace LSystem
{
    class LSystemCompileException: Exception
    {
        public LSystemCompileException(string message) : base($"Exception while compiling L-System: {message}") { }
    }

    class LSystemXmlParseException : LSystemCompileException
    {
        public LSystemXmlParseException(XElement element, string message) : base($"\n{element}\n{message}") { }
    }

    class LSystemExecutionException: Exception
    {
        public LSystemExecutionException(string message) : base($"Exception while L-System execution: {message}") { }
    }
}
