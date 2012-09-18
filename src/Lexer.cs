using System;
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

        public void Analyse()
        {
            Source += " 3";

            try
            {
                while (true)
                    Tokens.Add(ReadNextToken());
            }
            catch
            {
            }
        }

        private char GetSourceChar()
        {
            return Source[SourceIndex];
        }

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

        private void SkipWhitespace()
        {
            while (GetSourceChar().Equals(' '))
            {
                if (GetSourceChar().Equals('\n'))
                {
                    CurrentLine++;
                    break;
                }
                SourceIndex++;
            }

            if (GetSourceChar().Equals('/') && GetNextSourceChar().Equals('/'))
            {
                while (!GetSourceChar().Equals('\n'))
                    SourceIndex++;
            }
        }

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

        private Token GetStringToken()
        {
            SourceIndex++;

            Token token = new Token();

            do
            {
                if (GetSourceChar().Equals('\n'))
                    Console.WriteLine("Error - expected ending quotation mark");

                token.Value += GetSourceChar();
                SourceIndex++;
            }
            while (!GetSourceChar().Equals('\"'));

            SourceIndex++;
            token.Type = TokenType.String;

            return token;
        }

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

        private Boolean IsPunctuation(Char c)
        {
            return !(Char.IsLetterOrDigit(c) || c.Equals(' '));
        }

        private Token ReadNextToken()
        {
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
