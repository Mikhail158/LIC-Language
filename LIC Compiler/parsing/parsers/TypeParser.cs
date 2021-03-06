﻿using LIC.Parsing.Nodes;
using LIC.Tokenization;
using System.Text;

namespace LIC.Parsing
{
    public static class TypeParser
    {
        public static TypeNode Parse(Parser.State state)
        {
            state.Save();
            var type = new TypeNode();

            while (true)
            {
                var tok = state.GetToken();
                if (tok.Is(TokenType.Identifier, "const"))
                {
                    if (type.IsConstant)
                    {
                        state.ErrorCode = (uint)ErrorCodes.P_DuplicatedModifier;
                        state.ErrorMessage =
                            "<const> type modifier can not be used twice" +
                            " for the same type";
                        return null;
                    }

                    type.IsConstant = true;
                    state.GetNextNeToken();
                    continue;
                }
                else if (tok.Is(TokenType.Identifier, "ref"))
                {
                    state.GetNextNeToken();
                    type.IsReference = true;
                    type.ReferenceType = Parse(state);

                    if (type.IsConstant)
                    {
                        state.ErrorCode = (uint)ErrorCodes.P_ReferenceCanNotBeConstant;
                        state.ErrorMessage =
                            "Reference can not have a constant modifier:\n" +
                            type;
                    }

                    return type;
                }
                else if (tok.Is(TokenSubType.BraceSquareLeft))
                {
                    state.GetNextNeToken();
                    type.IsArrayType = true;
                    type.IsValueType = true;

                    if (!state.GetToken().Is(TokenSubType.BraceSquareRight))
                    {
                        state.ErrorCode = 999;
                        state.ErrorMessage =
                            "Array does not support sizes and dimmensions " +
                            "in the current version";
                        return type;
                    }
                    state.GetNextNeToken();

                    type.ReferenceType = Parse(state);

                    if (type.ReferenceType.IsReference)
                    {
                        state.ErrorCode = (uint)ErrorCodes.P_ArrayCanNotContainReferences;
                        state.ErrorMessage =
                            "Array can not consist of references";
                    }

                    return type;
                }

                type.TypePath = ParsePath(state);
                return type;
            }
        }

        public static string ParsePath(Parser.State state)
        {
            var pathBuilder = new StringBuilder();

            while (true)
            {
                if (state.GetToken().Type != TokenType.Identifier)
                {
                    state.ErrorCode = (uint)ErrorCodes.P_IdentifierExpected;
                    state.ErrorMessage =
                        $"Expected path name(<identifier>) " +
                        $"but <{state.GetToken().SubType.ToString()}> " +
                        $"were given";
                    return null;
                }

                pathBuilder.Append(state.GetToken().Value);
                if (state.GetNextNeToken().SubType == TokenSubType.Dot)
                {
                    pathBuilder.Append(".");
                    state.GetNextNeToken();
                    continue;
                }

                break;
            }

            return pathBuilder.ToString();
        }
    }
}
