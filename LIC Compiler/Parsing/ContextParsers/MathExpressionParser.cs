﻿namespace LIC_Compiler.Parsing.ContextParsers
{
    public static class MathExpressionParser
    {
        public static Node Parse(Parser.State state)
        {
            state.GetNextNEToken();
            return null;
        }
    }
}
