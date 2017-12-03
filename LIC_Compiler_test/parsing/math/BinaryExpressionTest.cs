﻿using LIC.Parsing;
using LIC.Parsing.Nodes;
using LIC.Tokenization;
using LIC.Language;
using NUnit.Framework;

namespace LIC_Compiler_test.parsing.math
{
    [TestFixture]
    public class BinaryExpressionTest
    {
        [Test]
        public void TestBinaryExpression_0_Simple()
        {
            foreach (Operator op in OperatorList.MathOperators)
            {
                TestExpression(2, op.Representation, 1);
            }
        }

        [Test]
        public void TestBinaryExpression_1_Comparison()
        {
            foreach (Operator op in OperatorList.ComparisonOperators)
            {
                TestExpression(2, op.Representation, 1);
            }
        }

        [Test]
        public void TestBinaryExpression_2_Assignment()
        {
            foreach (Operator op in OperatorList.AssignmentWithMathActionOperators)
            {
                TestExpression("a", op.Representation, "b");
            }
        }

        [Test]
        public void TestBinaryExpression_3_Special()
        {
            foreach (Operator op in OperatorList.MainOperators)
            {
                TestExpression("a", op.Representation, "b");
            }
        }

        [Test]
        public void TestBinaryExpression_4_Multiple()
        {
            TestExpression(
                "1 + 2 + 3",
                new BinaryOperatorNode(
                    OperatorList.Addition,
                    new BinaryOperatorNode(
                        OperatorList.Addition,
                        new NumberNode("1", false),
                        new NumberNode("2", false)
                    ),
                    new NumberNode("3", false)
                )
            );

            TestExpression(
                "1 + 2 * 3",
                new BinaryOperatorNode(
                    OperatorList.Addition,
                    new NumberNode("1", false),
                    new BinaryOperatorNode(
                        OperatorList.Multiplication,
                        new NumberNode("2", false),
                        new NumberNode("3", false)
                    )
                )
            );

            TestExpression(
                "(1 + 2) * 3",
                new BinaryOperatorNode(
                    OperatorList.Multiplication,
                    new BinaryOperatorNode(
                        OperatorList.Addition,
                        new NumberNode("1", false),
                        new NumberNode("2", false)
                    ),
                    new NumberNode("3", false)
                )
            );
        }

        private static void TestTypes<T>(BinaryOperatorNode ast)
        {
            Assert.IsNotNull(ast);
            Assert.IsInstanceOf<BinaryOperatorNode>(ast);

            Assert.IsInstanceOf<T>(ast.LeftOperand);
            Assert.IsInstanceOf<T>(ast.RightOperand);
        }

        private static void TestExpression(long a, string op, long b)
        {
            var ast = Parse($"{a} {op} {b}") as BinaryOperatorNode;
            TestTypes<NumberNode>(ast);

            var left = ast.LeftOperand as NumberNode;
            var right = ast.RightOperand as NumberNode;

            Assert.AreEqual(a.ToString(), left?.NumericValue);
            Assert.AreEqual(b.ToString(), right?.NumericValue);
            Assert.AreEqual(op, ast.Operation.Representation);
        }

        private static void TestExpression(string a, string op, string b)
        {
            var ast = Parse($"{a} {op} {b}") as BinaryOperatorNode;
            TestTypes<VariableNode>(ast);

            var left = ast.LeftOperand as VariableNode;
            var right = ast.RightOperand as VariableNode;

            Assert.AreEqual(a, left?.Name);
            Assert.AreEqual(b, right?.Name);
            Assert.AreEqual(op, ast.Operation.Representation);
        }

        private static ExpressionNode Parse(string data)
        {
            var tokenizer = new Tokenizer(data, new TokenizerOptions());
            var state = new Parser.State(tokenizer.Tokenize());
            return MathExpressionParser.Parse(state);
        }

        private static void TestExpression(string expression, ExpressionNode expected)
        {
            ValidateAst(expected, Parse(expression));
        }

        private static void ValidateAst(ExpressionNode expected, ExpressionNode ast)
        {
            Assert.IsNotNull(ast);
            Assert.IsInstanceOf(expected.GetType(), ast);

            if (!(ast is BinaryOperatorNode)) return;
            
            var bin = (BinaryOperatorNode) ast;
            var binExpected = expected as BinaryOperatorNode;
            Assert.AreEqual(binExpected?.Operation, bin.Operation);
            ValidateAst(binExpected?.LeftOperand, bin.LeftOperand);
            ValidateAst(binExpected?.RightOperand, bin.RightOperand);
        }
    }
}
