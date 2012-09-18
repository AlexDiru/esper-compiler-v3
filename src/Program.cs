using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer();
		    //lexer.source = "bool b\nb=2+6==3/2\nbool c\nc=b\nbool d\nd=c==1\n";
		    lexer.Source = "PRINT 3\nPRINT 1\nbool b\nb=1<=2\nint a\na=3+2*7/(3--4)";
		
		    Console.WriteLine("-Code-");
		    Console.WriteLine(lexer.Source);
		
		    lexer.Analyse();
		
		    Console.WriteLine("-Lexical Analysis-");
		
		    foreach (Token t in lexer.Tokens) {
			    t.Print();
			    Console.Write(" ");
		
		    }
		    Console.WriteLine("\n");

            Console.WriteLine("-Parsing-");

            Parser parser = new Parser();
            parser.SetTokens(lexer.Tokens);
            parser.ParseProgram();
            parser.DisplayOutput();
        }
    }
}
