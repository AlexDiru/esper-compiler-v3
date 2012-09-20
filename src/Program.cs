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
		    lexer.Source = "bool a\na = 6 + 7-90*3/3+4 <=1-(4*7--9+21*3)+33-093+22/9\nbool b\nb=a\n";
		
		    Console.WriteLine("-Code-");
		    Console.WriteLine(lexer.Source);
		
		    lexer.Analyse();
		
		    Console.WriteLine("\n-Lexical Analysis-");
		
		    foreach (Token t in lexer.Tokens) {
			    t.Print();
			    Console.Write(" ");
		
		    }

            Console.WriteLine("\n-Parsing-");

            Parser parser = new Parser();
            parser.SetTokens(lexer.Tokens);
            parser.ParseProgram();
            parser.DisplayOutput();

            Console.WriteLine("\n-Optimising-");

            Optimiser optimiser = new Optimiser();
            optimiser.Root = parser.RootNode;
            optimiser.Optimise();
            Node.DisplayTree(optimiser.Root, "");
        }
    }
}
