﻿using LIC.Parsing.Nodes;
using LIC.Tokenization;

namespace LIC.Parsing
{
    public static class ModuleParser
    {
        public static CoreNode Parse(Parser.State state)
        {
            var coreNode = new CoreNode();

            while (!state.IsErrorOccured() && !state.GetToken().Is(TokenType.EOF))
            {
                if (!ParseUse(state, coreNode)
                    && !ParseFunctionDeclaration(state, coreNode)
                    && !ParseAttribute(state, coreNode))
                {
                    state.ErrorCode = (uint)ErrorCodes.P_UnknownUnit;
                    state.ErrorMessage = "Got an invalid expression";
                }
            }

            return coreNode;
        }

        private static bool ParseUse(Parser.State state, CoreNode coreNode)
        {
            if (state.IsErrorOccured()) { return false; }
            if (!state.GetToken().Is(TokenType.CompilerDirective, "#use")) {
                return false;
            }

            state.GetNextNeToken();

            var usePath = TypeParser.ParsePath(state);
            string useAlias = null;

            if (state.IsErrorOccured()) { return false; }

            if (state.GetToken().Is(TokenType.Identifier, "as"))
            {
                state.GetNextNeToken();
                useAlias = TypeParser.ParsePath(state);

                if (state.IsErrorOccured()) { return false; }
            }

            coreNode.UsesNodes.Add(new UseNode(usePath, useAlias));
            return true;
        }

        private static bool ParseFunctionDeclaration(Parser.State state, CoreNode coreNode)
        {
            if (state.GetToken().Type != TokenType.Identifier)
            {
                return false;
            }

            state.Save();
            var name = state.GetTokenAndMoveNe().Value;

            if (!state.GetToken().Is(TokenSubType.Colon))
            {
                state.ErrorCode = (uint)ErrorCodes.P_ColonBeforeTypeSpeceficationNotFound;
                state.ErrorMessage =
                    $"Expected <Colon>(:) before function type specification " +
                    $"but <{state.GetToken().SubType}>({state.GetToken().Value}) were given";
                return false;
            }
            state.GetNextNeToken();

            TypeNode type;
            if (state.GetToken().SubType == TokenSubType.Colon)
            {
                type = TypeNode.AutoType;
                state.GetNextNeToken();
            }
            else
            {
                type = TypeParser.Parse(state);
            }
            if (state.IsErrorOccured()) { return false; }

            /*
            // Only for class methods
            bool isInstance = state.GetToken().Is(TokenSubType.Dot, ".");
            bool isStatic = state.GetToken().Is(TokenSubType.Colon, ":");

            if (!(isStatic || isInstance))
            {
                state.ErrorCode = (uint)ErrorCodes.P_MethodTypeExpected;
                state.ErrorMessage =
                    $"Expected '.' or ':' as type modifier, but " +
                    $"<{state.GetToken().SubType.ToString()}> were found";
                return false;
            }
            */

            var function = new FunctionNode
            {
                Parent = coreNode,
                Name = name,
                Type = FunctionNode.EType.Static,

                ReturnType = type
            };

            coreNode.FunctionNodes.Add(function);

            if (!state.GetToken().Is(TokenSubType.BraceCurlyLeft))
            {
                state.ErrorCode = (uint)ErrorCodes.P_OpeningBracketExpected;
                state.ErrorMessage =
                    $"Opening curly brace is required before parameters list. " +
                    $"Instead <{state.GetToken().SubType}> were given";

                return false;
            }

            state.GetNextNeToken();
            FunctionParser.ParseParametersList(state, function);
            if (state.IsErrorOccured()) { return true; }

            function.Attributes.AddRange(state.GetAttributes());
            state.ClearAttributes();

            // FunctionParser.ParseTemplateValues(state, function);
            function.Code = CodeParser.Parse(state);
            return !state.IsErrorOccured();
        }

        private static bool ParseAttribute(Parser.State state, CoreNode coreNode)
        {
            if (!state.GetToken().Is(TokenSubType.AtSign))
            {
                return false;
            }

            state.Save();
            state.GetNextNeToken();

            var attributeName = TypeParser.ParsePath(state);
            if (state.IsErrorOccured()) return true;

            var call = ExpressionParser.ParseFunctionCall(state);
            if (state.IsErrorOccured()) return true;

            call.CalleeExpression = new VariableNode(attributeName);
            state.PushdAttribute(call);
            return true;
        }
    }
}
