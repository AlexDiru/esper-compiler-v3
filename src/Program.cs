﻿using System;
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
		    lexer.Source = "PRINT 3\nIF 1 == 2 + 3 * 9 THEN\nPRINT 4\nELSE\nPRINT 2\nEND\nPRINT 1\nbool b\nb=1<=2\n";
		
		    Console.WriteLine("-Code-");
		    Console.WriteLine(lexer.Source);
		
		    lexer.Analyse();
		
		    Console.WriteLine("-Lexical Analysis-");
		
		    foreach (Token t in lexer.Tokens) {
			    t.Print();
			    Console.Write(" ");
		
		    }
		    Console.WriteLine("\n");
        }
    }
}