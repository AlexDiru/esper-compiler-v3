using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    class Parser
    {
        /// <summary>
        /// The list of tokens from the lexer to parse
        /// </summary>
        private List<Token> Tokens;

        /// <summary>
        /// The index of the current token the parser is on
        /// </summary>
        private Int32 TokenIndex;

        /// <summary>
        /// A list of all the variables found in the code
        /// </summary>
        private List<VariableInfo> Variables;

        /// <summary>
        /// A list containing all the keywords
        /// e.g. PRINT, INPUT etc
        /// </summary>
        private List<String> Keywords;

        /// <summary>
        /// A list of all the variable types
        /// e.g. INT, BOOL etc
        /// </summary>
        private List<String> VariableTypes;

        /// <summary>
        /// The root node of the parse tree
        /// </summary>
        public Root RootNode;

        /// <summary>
        /// The number of lines of code
        /// </summary>
        private Int32 LineCount;

        public Parser()
        {
            RootNode = new Root();
            Variables = new List<VariableInfo>();
            Tokens = new List<Token>();

            SetKeywords();
            SetVariables();
        }

        /// <summary>
        /// Sets the list of token
        /// </summary>
        public void SetTokens(List<Token> LexerOutput)
        {
            Tokens = LexerOutput;
            LineCount = Tokens.Max(t => t.Line);
        }

        /// <summary>
        /// Gets the current token the parser is on
        /// </summary>
        private Token GetCurrentToken()
        {
            try
            {
                return Tokens[TokenIndex];
            }
            catch
            {
                Console.WriteLine("Error - out of tokens");
                return null;
            }
        }

        /// <summary>
        /// Adds all the variable types to the list
        /// </summary>
        private void SetVariables()
        {
            VariableTypes = new List<String>();

            VariableTypes.Add("INT");
            VariableTypes.Add("STRING");
            VariableTypes.Add("BOOL");
        }

        /// <summary>
        /// Adds all the keywords to the list
        /// </summary>
        private void SetKeywords()
        {
            Keywords = new List<String>();

            Keywords.Add("PRINT");
            Keywords.Add("INPUT");
        }

        /// <summary>
        /// Checks the variable list to see if a name already exists
        /// </summary>
        public Boolean VariableExists(String name)
        {
            return Variables.Any(v => v.Name.Equals(name));
        }

        /// <summary>
        /// Gets the type of the given variable name
        /// </summary>
        private VariableType GetVariableType(String name)
        {
            return Variables.Where(v => v.Name.Equals(name)).First().Type;
        }

        /// <summary>
        /// Checks if a name is valid for a variable
        /// It can't be a keyword or the name of an already declared variable
        /// </summary>
        private Boolean IsNewVariableNameInvalid(String name)
        {
            if (Keywords.Contains(name))
                return true;

            return VariableExists(name);
        }

        /// <summary>
        /// Shows an error message if there are no tokens left to parse
        /// </summary>
        private void CheckIfOutOfTokens()
        {
            if (TokenIndex >= Tokens.Count)
                Console.WriteLine("Error - program ended unexpectedly");
        }

        /// <summary>
        /// Parse a facor in an expression
        /// EBNF: <factor> ::= <IDENTIFIER> | <CONSTANT> | ( ‘(’, <expression>, ‘)’ ) | ( '-', <factor> );
        /// We need to have the node where the factor is to be held
        /// A the type so we know whether we are expecting an int or a string
        /// </summary>
        private Node ParseFactor(Node node, VariableType type)
        {
            //Check if we have any tokens left
            CheckIfOutOfTokens();


            //First check for a parenthesised expression
            //( ‘(’, <expression>, ‘)’ )
            if (GetCurrentToken().Value.Equals("("))
            {
                //Skip the parenthesis
                TokenIndex++;

                //Get the expression
                node = ParseExpression(node, type);

                if (!GetCurrentToken().Value.Equals(")"))
                    Console.WriteLine("Expected closing bracket");

                //Skip the closing paren
                TokenIndex++;

                return node;
            }

            //If it's not a parenthesised expression
            //Check for a negative number
            //( '-', <factor> )
            if (type.Equals(VariableType.Integer))
            {
                if (GetCurrentToken().Value.Equals("-"))
                {
                    TokenIndex++;
                    node = ParseFactor(node, type);

                    //Apply the negative sign to the parsed factor
                    node.Value = "-" + node.Value;
                    return node;
                }
            }

            //We have narrowed it down to either a <IDENTIFIER> or <CONSTANT>
            node = new Node();

            //Store the value in the node
            node.Value = GetCurrentToken().Value;

            //Semantics - check if variable exists
            if (GetCurrentToken().Type.Equals(TokenType.Identifier) && !VariableExists(GetCurrentToken().Value))
                Console.WriteLine("Undeclared variable");

            //Give the node extra info to aid in code gen
            if (GetCurrentToken().Type.Equals(TokenType.Identifier))
                node.Attributes[0] = "VARIABLE";
            else
                node.Attributes[1] = "VALUE";

            TokenIndex++;

            return node;
        }

        /// <summary>
        /// To parse a term in the epression
        /// In EBNF: <term> ::= <factor>, { ‘*’|’/’, <factor> };
        /// </summary>
        private Node ParseTerm(Node node)
        {
            CheckIfOutOfTokens();

            //Parse the first factor
            node = ParseFactor(node, VariableType.Integer);

            //Then the {...}
            //For each * or / parse another factor
            while (GetCurrentToken().Value.Equals("*") || GetCurrentToken().Value.Equals("/"))
            {
                Node clonedNode = node.Clone();
                node.Left = clonedNode;
                node.Value = GetCurrentToken().Value;
                TokenIndex++;
                node.Right = ParseFactor(node.Right, VariableType.Integer);
            }

            return node;
        }

        /// <summary>
        /// Parses a condition
        /// EBNF: <condition> ::= <expression> | <expression> '==' <expression>
        /// </summary>
        private Node ParseCondition(Node node)
        {
            node = new Node();
            node.Value = "CONDITION";

            //LHS of condition
            node.Left = ParseExpression(node, VariableType.Integer);

            //Check for anything that isn't a variable or value
            //If we don't find either, the condition has been parsed
            if (!GetCurrentToken().Type.Equals(TokenType.Identifier) && !GetCurrentToken().Type.Equals(TokenType.Number)
                && !IsConditionalOperator(GetCurrentToken().Value))
                return node;

            //Get the equality comparators
            node.Attributes[0] = GetCurrentToken().Value;

            TokenIndex++;
        
            //Get the RHS of the condition
            node.Right = ParseExpression(node, VariableType.Integer);

            return node;
        }

        /// <summary>
        /// Checks if a given string is a conditional operator
        /// </summary>
        private bool IsConditionalOperator(string str)
        {
            return str.Equals("==") || str.Equals(">=") || str.Equals("<=") || str.Equals("!=");
        }

        /// <summary>
        /// To parse the expression
        /// In EBNF: <expression> ::= <term>, { ‘+’|’-’, <term> };
        /// </summary>
        private Node ParseExpression(Node node, VariableType type)
        {
            CheckIfOutOfTokens();

            //A numeric expr can be made up of terms, but a string expr is made up of string factors only
            //(we can't multilply, divide or subtract strings)
            if (type.Equals(VariableType.Integer))
                node = ParseTerm(node);
            else if (type.Equals(VariableType.Boolean))
                node = ParseCondition(node);
            else
                node = ParseFactor(node, VariableType.String);

            //While a valid operator is found
            while (GetCurrentToken().Value.Equals("+") || (type.Equals(VariableType.Integer) &&
                                                           GetCurrentToken().Value.Equals("-")))
            {
                if (node == null)
                    node = new Node();

                Node clonedNode = node.Clone();
                node.Left = clonedNode;
                node.Value = GetCurrentToken().Value;
                TokenIndex++;

                if (type.Equals(VariableType.Integer))
                    node.Right = ParseTerm(node.Right);
                else
                    node.Right = ParseFactor(node.Right, VariableType.String);
            }

            return node;
        }

        /// <summary>
        /// Gets the type of the variable from a token's value
        /// </summary>
        private VariableType GetVariableFromValue(String tokenValue)
        {
            switch (tokenValue.ToUpper())
            {
                case "STRING":
                    return VariableType.String;
                case "INT":
                    return VariableType.Integer;
                case "BOOL":
                    return VariableType.Boolean;
            }

            return VariableType.Unknown;
        }

        /// <summary>
        /// To declare a variable
        /// In EBNF:
        /// 	         <DECLARE> ::= ( 'INT' | 'STRING' | 'BOOL'), <IDENTIFIER>;
        /// </summary>
        private Node DeclareVariable(Node node)
        {
            node = new Node();
            node.Value = "DECLARE";
            node.Attributes[1] = GetCurrentToken().Value;
            TokenIndex++;
            CheckIfOutOfTokens();
            node.Attributes[0] = GetCurrentToken().Value;

            //Semantics - check variable name is valid
            if (!GetCurrentToken().Type.Equals(TokenType.Identifier) || IsNewVariableNameInvalid(GetCurrentToken().Value))
                Console.WriteLine("Invalid variable name used in declaration");

            //Add the variable to the list for future reference
            var varInfo = new VariableInfo();
            varInfo.Name = GetCurrentToken().Value;

            varInfo.Type = GetVariableFromValue(node.Attributes[1]);

            Variables.Add(varInfo);

            TokenIndex++;

            return node;
        }

        /// <summary>
        /// To assign expression to a varibale
        /// In EBNF:
        ///	       <ASSIGN> ::= <IDENTIFIER>, '=', <EXPRESSION>;
        /// </summary>
        private Node AssignExpression(Node node)
        {
            node.Value = "ASSIGN";

            node.Left = new Node();
            node.Left.Value = GetCurrentToken().Value;

            //Skip through '=' symbol
            TokenIndex += 2;

            node.Right = ParseExpression(node.Right, GetVariableType(node.Left.Value));

            //Semantics - checks variable exists
            if (!VariableExists(node.Left.Value))
                Console.WriteLine("Undeclared var name used in assignment");

            //Additional node information to aid code gen
            if (GetVariableType(node.Left.Value).Equals(VariableType.Integer))
                node.Attributes[0] = "INT";
            else if (GetVariableType(node.Left.Value).Equals(VariableType.Boolean))
                node.Attributes[0] = "BOOL";
            else
                node.Attributes[0] = "STRING";

            return node;
        }

        /// <summary>
        /// <PRINT> ::= 'PRINT', ( <IDENTIFIER> | <CONSTANT> );
        /// </summary>
        private Node ParsePrintFunction(Node node)
        {
            node = new Node();
            node.Value = "PRINT";
            TokenIndex++;
            CheckIfOutOfTokens();
            node.Attributes[0] = GetCurrentToken().Value;

            //Semantic part
		    //Check for validity - argument should be variable or constant
		    if (!GetCurrentToken().Type.Equals(TokenType.Identifier) && !GetCurrentToken().Type.Equals(TokenType.String)
			    && !GetCurrentToken().Type.Equals(TokenType.Number))
			    Console.Write("Invalid argument for PRINT statement");
		    //And in case of variable, it must exist
		    if (GetCurrentToken().Type.Equals(TokenType.Identifier) && !VariableExists(GetCurrentToken().Value))
                Console.Write("Undeclared variable name used as argument in PRINT");
		
		    //Store some attributes to aid in code gen
		    if (GetCurrentToken().Type.Equals(TokenType.Number) || 
			    GetVariableType(GetCurrentToken().Value).Equals(VariableType.Integer))
			    node.Attributes[1] = "INT";
		    else
			    node.Attributes[1] = "STRING";
		
		    if (GetCurrentToken().Type.Equals(TokenType.Identifier))
			    node.Attributes[2] = "VARIABLE";
		    else
			    node.Attributes[2] = "VALUE";
		
		    TokenIndex++;
		
		    return node;
        }

        /// <summary>
        /// <INPUT> ::= 'INPUT', <IDENTIFIER>
        /// </summary>
        private Node ParseInputFunction(Node node)
        {

            node = new Node();
            node.Value = "INPUT";
            TokenIndex++;
            CheckIfOutOfTokens();
            node.Attributes[0] = GetCurrentToken().Value;

            //Semantic part
            if (!GetCurrentToken().Type.Equals(TokenType.Identifier))
                Console.WriteLine("Invalid argument for input statement");

            if (!VariableExists(GetCurrentToken().Value))
                Console.WriteLine("Undeclared variable name used as argument in INPUT");

            if (GetVariableType(GetCurrentToken().Value).Equals(VariableType.Integer))
                node.Attributes[1] = "INT";
            else
                node.Attributes[1] = "STRING";

            TokenIndex++;

            return node;
        }

        /// <summary>
        /// Parse a new statement
        /// </summary>
        private Node parseStatement(Node node) 
        {
		
		    //Call appropriate parse function according to first Token in the statement
	
		    if (VariableTypes.Contains(GetCurrentToken().Value))
			    node = DeclareVariable(node);
		    else if (GetCurrentToken().Value.Equals("INPUT"))
			    node = ParseInputFunction(node);
		    else if (GetCurrentToken().Value.Equals("PRINT"))
			    node = ParsePrintFunction(node);
		    else if (GetCurrentToken().Type.Equals(TokenType.Identifier) && Tokens[TokenIndex+1].Value.Equals("="))
			    node = AssignExpression(node);
		    //else if (GetCurrentToken().Value.Equals("IF"))
			//    node = ParseIfStatement(node);
		    else
			    Console.WriteLine("INVALID STATEMENT on line " + GetCurrentToken().Line + ": " + GetCurrentToken().Value);
		
		    return node;
	    }

        /// <summary>
        /// Parses all the tokens
        /// </summary>
        public void ParseProgram()
        {
            //Last Token must be EOL so add one in case
            Token eol = new Token();
            eol.Line = Tokens[Tokens.Count - 1].Line + 1;
            eol.Type = TokenType.EOL;
            eol.Value = "\n";
            Tokens.Add(eol);

            //Start at the first Token
            TokenIndex = 0;

            for (int line = 0; line < LineCount; line++)
            {

                Node currentNode = new Node();

                if (!GetCurrentToken().Type.Equals(TokenType.EOL))
                {

                    if (line == LineCount - 1)
                    {
                        currentNode = parseStatement(currentNode);
                        RootNode.InsertFinal(currentNode);
                    }
                    else
                    {
                        RootNode.Insert("STATEMENTS", parseStatement(currentNode));
                    }
                }

                TokenIndex++;
            }

            RootNode.NullifyEmptyChildren();
        }

        /// <summary>
        /// Displays the output of the parser
        /// </summary>
        public void DisplayOutput()
        {
            Node.DisplayTree(RootNode, "");
        }
    }
}
