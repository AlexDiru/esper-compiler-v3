using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    /// <summary>
    /// A type that a token can be
    /// </summary>
    enum TokenType
    {
        Number, // <NUMBER> := <DIGIT> | <DIGIT> <NUMBER>
        String, // <STRING> := '"', <CHARACTERLIST>, '"'
                // <CHARACTERLIST> := <CHARACTER> | <CHARACTER> <CHARACTERLIST>
        Symbol, // <SYMBOL> := '(' | ')' | '{' | '}' | '.' | ',' | ';' | '*' | '&' | '[' | 
                //             ']' | '<' | '>' | '=' | '!' | '+' | '-' | '/' | '?' | '<=' |
                //             '>=' | '==' | '!='
        Identifier, // <IDENTIFIER> := <ALPHABETIC_CHARACTER> <ALPHANUMERIC_CHARACTERLIST>
                    // <ALPHANUMERIC_CHARACTERLIST> := E | <ALPHANUMBERIC_CHARACTER> <ALPHANUMERIC_CHARACTERLIST>
        EOL, // <EOL> := '\n'
        Unknown
    }
}
