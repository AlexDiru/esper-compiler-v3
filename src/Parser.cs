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
        private Root RootNode;

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

        public Boolean VariableExists(String name)
        {
            return Variables.Any(v => v.Name.Equals(name));
        }

        private VariableType GetVariableType(String name)
        {
            return Variables.Where(v => v.Name.Equals(name)).First().Type;
        }

        private Boolean IsNewVariableNameInvalid(String name)
        {
            if (Keywords.Contains(name))
                return true;

            return VariableExists(name);
        }

        private void CheckIfOutOfTokens()
        {
            if (TokenIndex >= Tokens.Count)
                Console.WriteLine("Error - program ended unexpectedly");
        }

        private Node ParseFactor(Node node, VariableType type)
        {
            CheckIfOutOfTokens();

            if (GetCurrentToken().Value.Equals("("))
            {
                TokenIndex++;

                node = ParseExpression(node, type);

                if (!GetCurrentToken().Value.Equals(")"))
                    Console.WriteLine("Expected closing bracket");

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

            node = new Node();

            node.Value = GetCurrentToken().Value;

            if (GetCurrentToken().Type.Equals(TokenType.Identifier) && !VariableExists(GetCurrentToken().Value))
                Console.WriteLine("Undeclared variable");

            if (GetCurrentToken().Type.Equals(TokenType.Identifier))
                node.Attributes[0] = "VARIABLE";
            else
                node.Attributes[1] = "VALUE";

            TokenIndex++;

            return node;
        }

        private Node ParseTerm(Node node)
        {
            CheckIfOutOfTokens();

            node = ParseFactor(node, VariableType.Integer);

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

        private Node ParseCondition(Node node)
        {
            node = new Node();
            node.Value = "CONDITION";

            node.Left = ParseExpression(node, VariableType.Integer);

            if (!GetCurrentToken().Type.Equals(TokenType.Identifier) && !GetCurrentToken().Type.Equals(TokenType.Number)
                && !IsConditionalOperator(GetCurrentToken().Value))
                return node;

            node.Attributes[0] = GetCurrentToken().Value;

            TokenIndex++;

            node.Right = ParseExpression(node, VariableType.Integer);

            return node;
        }

        private bool IsConditionalOperator(string str)
        {
            return str.Equals("==") || str.Equals(">=") || str.Equals("<=") || str.Equals("!=");
        }

        private Node ParseExpression(Node node, VariableType type)
        {
            CheckIfOutOfTokens();

            if (type.Equals(VariableType.Integer))
                node = ParseTerm(node);
            else if (type.Equals(VariableType.Boolean))
                node = ParseCondition(node);
            else
                node = ParseFactor(node, VariableType.String);

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

        private Node DeclareVariable(Node node)
        {
            node = new Node();
            node.Value = "DECLARE";
            node.Attributes[1] = GetCurrentToken().Value;
            TokenIndex++;
            CheckIfOutOfTokens();
            node.Attributes[0] = GetCurrentToken().Value;

            if (!GetCurrentToken().Type.Equals(TokenType.Identifier) || IsNewVariableNameInvalid(GetCurrentToken().Value))
                Console.WriteLine("Invalid variable name used in declaration");

            var varInfo = new VariableInfo();
            varInfo.Name = GetCurrentToken().Value;

            varInfo.Type = GetVariableFromValue(node.Attributes[1]);

            Variables.Add(varInfo);

            TokenIndex++;

            return node;
        }

        private Node AssignExpression(Node node)
        {
            node.Value = "ASSIGN";

            node.Left = new Node();
            node.Left.Value = GetCurrentToken().Value;

            TokenIndex += 2;

            node.Right = ParseExpression(node.Right, GetVariableType(node.Left.Value));

            if (!VariableExists(node.Left.Value))
                Console.WriteLine("Undeclared var name used in assignment");

            if (GetVariableType(node.Left.Value).Equals(VariableType.Integer))
                node.Attributes[0] = "INT";
            else if (GetVariableType(node.Left.Value).Equals(VariableType.Boolean))
                node.Attributes[0] = "BOOL";
            else
                node.Attributes[0] = "STRING";

            return node;
        }

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

        // <INPUT> ::= 'INPUT', <IDENTIFIER>
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

        //Parse a new statement
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

        private void displayTree(Node node, String inner) {
		
		if (node == null)
			return;
		
		Console.Write(inner + ">" + node.Value);
		
		String Attrib = "";
		
		foreach (String Attribute in node.Attributes)
			if (Attribute != "")
				Attrib += " " + Attribute;
				
		if (Attrib != "")
			Console.Write("  (Attributes: " + Attrib + ")");
		
		Console.Write("\n");
		
		displayTree(node.Left, inner + "-");
		displayTree(node.Right, inner + "-");
	}

        public void DisplayOutput()
        {

            displayTree(RootNode, "");
        }
    }
}
