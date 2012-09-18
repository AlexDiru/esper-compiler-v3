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
        private Node Root;

        /// <summary>
        /// The number of lines of code
        /// </summary>
        private Int32 LineCount;

        public Parser()
        {
            Root = new Node();
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
            LineCount = Tokens.Last().Line;
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

            if (!GetCurrentToken().Type.Equals(TokenType.Identifier) && !GetCurrentToken().Type.Equals(Tokens.Number)
                && !IsConditionalOperator(GetCurrentToken().Value))
                return node;

            node.Attributes[0] = GetCurrentToken().Value;

            TokenIndex++;

            node.Right = ParseExpression(node, VariableType.Integer);

            return node;
        }

        private bool IsConditionalOperator(string p)
        {
            throw new NotImplementedException();
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
    }
}
