using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    /// <summary>
    /// A class storing information about a variable
    /// </summary>
    class VariableInfo
    {
        /// <summary>
        /// The unique name of the variable
        /// </summary>
        public String Name;

        /// <summary>
        /// The type of the variable i.e. integer, boolean, string etc
        /// </summary>
        public VariableType Type;
    }
}
