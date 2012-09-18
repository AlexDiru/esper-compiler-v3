using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    class Token
    {
        public String Value;
        public TokenType Type;
        public Int32 Line;

        public void Print()
        {
            String output = "";

            switch (Type)
            {
                case TokenType.Unknown:
                    output += "?(";
                    break;
                case TokenType.EOL:
                    output += "EOL(";
                    break;
                case TokenType.Number:
                    output += "NUM(";
                    break;
                case TokenType.String:
                    output += "STR(";
                    break;
                case TokenType.Symbol:
                    output += "SYM(";
                    break;
                default:
                    output += "ID(";
                    break;
            }

            output += Value + ", " + Line + ")";

            Console.Write(output + "\n");
        }
    }
}
