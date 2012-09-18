﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    class Lexer
    {
        public List<Token> Tokens = new List<Token>();
        public String Source = String.Empty;

        private Int32 SourceIndex = 0;
        private Int32 CurrentLine = 1;

        public void SetCode(String code)
        {
            Source = code;
        }

        /// <summary>
        /// Converts the input code into tokens
        /// </summary>
        public void Analyse()
        {
            //Have to append an extra token otherwise it doesn't analsyse
            //the last token
            Source += " 3";

            //Loop until end of tokens
            try
            {
                while (true)
                    Tokens.Add(ReadNextToken());
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the current character of the source
        /// </summary>
        private char GetSourceChar()
        {
            return Source[SourceIndex];
        }

        /// <summary>
        /// Gets the next characters of the source
        /// </summary>
        private char GetNextSourceChar()
        {
            if (SourceIndex + 1 < Source.Length)
            {
                return Source[SourceIndex + 1];
            }
            else
            {
                Console.WriteLine("Error - no more characters detected");
                return '\0';
            }
        }

        /// <summary>
        /// Keeps spaces, tabs and comments
        /// </summary>
        private void SkipWhitespace()
        {
            //Skip spaces until a new character
            while (GetSourceChar().Equals(' '))
            {
                if (GetSourceChar().Equals('\n'))
                {
                    CurrentLine++;
                    break;
                }
                SourceIndex++;
            }

            //Skip the rest of the line if a comment is detected
            if (GetSourceChar().Equals('/') && GetNextSourceChar().Equals('/'))
            {
                while (!GetSourceChar().Equals('\n'))
                    SourceIndex++;
            }
        }

        /// <summary>
        /// Gets the number type of token
        /// </summary>
        private Token GetNumberToken()
        {
            Token token = new Token();

            do
            {
                token.Value += GetSourceChar();
                SourceIndex++;
            }
            while (Char.IsDigit(GetSourceChar()));

            token.Type = TokenType.Number;
            return token;
        }

        /// <summary>
        /// Gets the string type of token
        /// </summary>
        private Token GetStringToken()
        {
            //Skip the first quote mark
            SourceIndex++;

            Token token = new Token();

            do
            {
                //Can't EOL in middle of string
                if (GetSourceChar().Equals('\n'))
                    Console.WriteLine("Error - expected ending quotation mark");

                token.Value += GetSourceChar();
                SourceIndex++;
            }
            //Scan until end quote mark
            while (!GetSourceChar().Equals('\"'));

            SourceIndex++;
            token.Type = TokenType.String;

            return token;
        }

        /// <summary>
        /// Gets the symbol type of token
        /// </summary>
        private Token GetSymbolToken()
        {
            Token token = new Token();

            token.Value += GetSourceChar();

            if ((GetSourceChar().Equals('<') || GetSourceChar().Equals('>') ||
                 GetSourceChar().Equals('!') || GetSourceChar().Equals('='))
                && GetNextSourceChar().Equals('='))
            {
                SourceIndex++;
                token.Value += GetSourceChar();
            }

            SourceIndex++;
            token.Type = TokenType.Symbol;

            return token;
        }

        /// <summary>
        /// Get the identifier type of token
        /// </summary>
        private Token GetIdentifierToken()
        {
            Token token = new Token();

            do
            {
                token.Value += Char.ToUpper(GetSourceChar());
                SourceIndex++;
            }
            while (Char.IsLetterOrDigit(GetSourceChar()) || GetSourceChar().Equals('_'));

            token.Type = TokenType.Identifier;
            return token;
        }

        /// <summary>
        /// Reads the next token from the code
        /// </summary>
        private Token ReadNextToken()
        {
            //Skip redundant data
            SkipWhitespace();

            Token nextToken = null;

            if (GetSourceChar().Equals('\n'))
            {
                nextToken = new Token();
                nextToken.Type = TokenType.EOL;
                nextToken.Line = CurrentLine - 1;
                SourceIndex++;
                return nextToken;
            }
            else if (Char.IsLetter(GetSourceChar()))
            {
                nextToken = GetIdentifierToken();
            }
            else if (Char.IsDigit(GetSourceChar()))
            {
                nextToken = GetNumberToken();
            }
            else if (GetSourceChar().Equals('\"'))
            {
                nextToken = GetStringToken();
            }
            else if ("{}.,;*&[]<>=!+-/?".Contains(GetSourceChar()))
            {
                nextToken = GetSymbolToken();
            }
            else
            {
                Console.WriteLine("Error - undefined token");
            }

            nextToken.Line = CurrentLine;
            return nextToken;
        }
    }
}
