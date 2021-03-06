﻿using System.Linq;
using LIC.Language;
using LIC.Parsing.Nodes;
using LIC.Tokenization;

namespace LIC.Parsing
{
    public static class MathExpressionParser
    {
        public static ExpressionNode Parse(Parser.State state)
        {
            var leftOperand = ParseUnit(state);
            if (state.IsErrorOccured()) { return null; }
            return Parse(state, leftOperand);
        }

        private static ExpressionNode Parse
            (Parser.State state, ExpressionNode leftOperand, uint basePriority = 0)
        {
            var operation = ParseOperator(state);
            if (operation.Equals(OperatorList.Unknown)) { return leftOperand; }

            var rightOperand = ParseUnit(state);
            if (state.IsErrorOccured()) { return null; }

            // If operator is rigth-sided(like equal sign(=)) then parse block after it first.
            if (operation.IsRightSided)
            {
                return new BinaryOperatorNode(
                    operation,
                    leftOperand,
                    Parse(state, rightOperand)
                );
            }

            // Get next operator
            var nextOperation = ParseOperator(state);
            if (!nextOperation.Equals(OperatorList.Unknown)
                && nextOperation.Priority <= basePriority) state.Index -= 1;

            while (
                !nextOperation.Equals(OperatorList.Unknown)
                && nextOperation.Priority > basePriority
            )
            {
                if (nextOperation.Priority > operation.Priority)
                {
                    state.Index -= 1;
                    rightOperand = Parse(state, rightOperand, operation.Priority);
                    leftOperand = new BinaryOperatorNode(operation, leftOperand, rightOperand);
                    if (state.GetToken().Type == TokenType.Identifier)
                    {
                        return leftOperand;
                    }

                    operation = ParseOperator(state);
                    if (operation.Equals(OperatorList.Unknown))
                    {
                        return leftOperand;
                    }
                }
                else
                {
                    leftOperand = new BinaryOperatorNode(operation, leftOperand, rightOperand);
                    operation = nextOperation;
                }

                rightOperand = ParseUnit(state);
                if (state.IsErrorOccured()) { return null; }
                nextOperation = ParseOperator(state);
            }

            return new BinaryOperatorNode(operation, leftOperand, rightOperand);
        }

        private static Operator ParseOperator(Parser.State state)
        {
            var op = OperatorList.Unknown;
            var representation = "";
            state.Save();
            while ((state.GetToken().Is(TokenType.MathOperator)
                || state.GetToken().Is(TokenType.SpecialOperator))
                && representation.Length < 5)
            {
                representation += state.GetTokenAndMove().Value;
                op = OperatorList
                        .Operators
                        .Where(o => o.Representation == representation)
                        .Cast<Operator?>()
                        .FirstOrDefault()
                    ?? op;
            }

            state.Restore();
            for (int i = 0; i < op.Representation?.Length; ++i)
                state.GetNextToken();

            return op;
        }

        private static ExpressionNode ParseUnit(Parser.State state)
        {
            return ParseComplexUnit(state, ParseValue(state));
        }

        private static ExpressionNode ParseComplexUnit(Parser.State state, ExpressionNode value)
        {
            if (state.GetToken().Is(TokenSubType.BraceCurlyLeft))
            {
                var call = ExpressionParser.ParseFunctionCall(state);
                call.CalleeExpression = value;
                return ParseComplexUnit(state, call);
            }
            else if (state.GetToken().Is(TokenSubType.BraceSquareLeft))
            {
                var indexer = ExpressionParser.ParseIndexerCall(state);
                indexer.CalleeExpression = value;
                return ParseComplexUnit(state, indexer);
            }

            return value;
        }

        private static ExpressionNode ParseValue(Parser.State state)
        {
            var token = state.GetTokenAndMoveNe();

            if (token.Is(TokenType.Number))
            {
                return new NumberNode(
                    value: token.Value,
                    isDecimal: token.Is(TokenSubType.Decimal)
                );
            }
            if (token.Is(TokenType.Character))
            {
                return new CharacterNode(token.Value);
            }
            else if (token.Is(TokenType.Identifier) || token.Is(TokenSubType.Colon))
            {
                var identifier = token.Value;
                var nextTarget =
                    token.Is(TokenType.Identifier)
                        ? TokenSubType.Colon
                        : TokenSubType.Identifier;

                while (state.GetToken().Is(nextTarget))
                {
                    identifier += state.GetTokenAndMoveNe().Value;
                    nextTarget =
                        nextTarget == TokenSubType.Identifier
                            ? TokenSubType.Colon
                            : TokenSubType.Identifier;
                }

                return new VariableNode(identifier);
            }
            else if (token.Is(TokenType.String))
            {
                return new StringNode(token.Value);
            }
            else if (token.Is(TokenSubType.BraceRoundLeft))
            {
                var node = Parse(state);

                if (!state.GetToken().Is(TokenSubType.BraceRoundRight))
                {
                    state.ErrorCode = (uint)ErrorCodes.P_ClosingBraceRequired;
                    state.ErrorMessage =
                        "Expected <BraceRoundRight>, " +
                        $"but <{state.GetToken().SubType}> was given";
                }

                state.GetNextNeToken();
                return node;
            }

            state.ErrorCode = (uint)ErrorCodes.P_UnknownUnit;
            state.ErrorMessage = "Can not parse expression. Structure might be corrupted.";
            return null;
        }
    }
}
