using Complier.Model;
using Complier.Model.Ast;
using Complier.Model.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Complier.SyntaxAnalysis
{
    class ExpressionParser
    {
        //表达式节点
        private Stack<ExpressionNode> working = new Stack<ExpressionNode>();
        private Stack<ExpressionOperationType> operators = new Stack<ExpressionOperationType>();
        private Stack<int> arity = new Stack<int>();

        //运算符优先级集合
        private static readonly Dictionary<ExpressionOperationType, int> operationPrecedence = new Dictionary<ExpressionOperationType, int>()
        {    
            {ExpressionOperationType.OpenBrace, 0},
            { ExpressionOperationType.FunctionCall, 2 },
            { ExpressionOperationType.Negate, 3 },
            { ExpressionOperationType.Not, 3 },
            { ExpressionOperationType.Multiply, 5 },
            { ExpressionOperationType.Divide, 5 },
            { ExpressionOperationType.Modulo, 5 },
            { ExpressionOperationType.Add, 6 },
            { ExpressionOperationType.Substract, 6 },
            { ExpressionOperationType.LessThan, 8 },
            { ExpressionOperationType.LessEquals, 8 },
            { ExpressionOperationType.GreaterThan, 8 },
            { ExpressionOperationType.GreaterEquals, 8 },
            { ExpressionOperationType.Equals, 9 },
            { ExpressionOperationType.NotEquals, 9 },
            { ExpressionOperationType.And, 13 },
            { ExpressionOperationType.Or, 14 },
            { ExpressionOperationType.Assignment, 16 },
        };

        private static readonly ExpressionOperationType[] unaryOperators = { ExpressionOperationType.Negate, ExpressionOperationType.Not };

        private static readonly Dictionary<OperatorType, ExpressionOperationType> operatorToOperation = new Dictionary<OperatorType, ExpressionOperationType>()
        {
            { OperatorType.Add, ExpressionOperationType.Add},
            { OperatorType.Multiply, ExpressionOperationType.Multiply},
            { OperatorType.Divide, ExpressionOperationType.Divide},
            { OperatorType.Modulo, ExpressionOperationType.Modulo},
            { OperatorType.Assignment,ExpressionOperationType.Assignment},
            { OperatorType.Equals, ExpressionOperationType.Equals},
            { OperatorType.GreaterThan, ExpressionOperationType.GreaterThan},
            { OperatorType.LessThan, ExpressionOperationType.LessThan},
            { OperatorType.GreaterEquals, ExpressionOperationType.GreaterEquals},
            { OperatorType.LessEquals, ExpressionOperationType.LessEquals},
            { OperatorType.NotEquals, ExpressionOperationType.NotEquals},
            { OperatorType.Not, ExpressionOperationType.Not},
            { OperatorType.And, ExpressionOperationType.And},
            { OperatorType.Or, ExpressionOperationType.Or},
        };

        //上一个token是运算符或者左括号
        private bool lastTokenWasOperatorOrLeftBrace = true;

        /// <summary>
        /// 解析token到抽象语法树-> 表达式
        /// </summary>
        public ExpressionNode Parse(IEnumerable<Token> tokens)
        {
            //tokens是否为空
            bool sequenceWasEmpty = true;
            //TODO 在这里判断赋值号=号后面语法对错，比如=号之后是 +6； 则出错
            foreach (var token in tokens)
            {
                sequenceWasEmpty = false;
                //如果是数字
                if (token is NumberLiteralToken)
                {
                    working.Push(new NumberLiteralNode(((NumberLiteralToken)token).Number));
                    lastTokenWasOperatorOrLeftBrace = false;
                }
                //运算符
                else if (token is OperatorToken)
                {
                    //运算类型
                    ExpressionOperationType op;
                    //减
                    if (((OperatorToken)token).OperatorType == OperatorType.SubstractNegate)
                        //TODO 判断运算符类型
                        op = lastTokenWasOperatorOrLeftBrace ? ExpressionOperationType.Negate : ExpressionOperationType.Substract;
                    else
                        //其他类型
                        op = operatorToOperation[((OperatorToken)token).OperatorType];
                    //判断运算符优先级，并调整
                    while (operators.Count != 0 && operationPrecedence[operators.Peek()] > operationPrecedence[op]) 
                    {
                        PopOperator();
                    }
                    operators.Push(op);
                    lastTokenWasOperatorOrLeftBrace = true;
                }
                //如果是左括号
                else if (token is OpenBraceToken && ((OpenBraceToken)token).BraceType == BraceType.Round)
                {
                    if(working.Count > 0 && working.Peek() is VariableReferenceExpressionNode) 
                    {
                        var variable = (VariableReferenceExpressionNode)working.Pop();
                        working.Push(new FunctionCallExpressionNode(variable.VariableName));
                        operators.Push(ExpressionOperationType.FunctionCall);
                        arity.Push(1);
                    }
                    
                    operators.Push(ExpressionOperationType.OpenBrace);
                    lastTokenWasOperatorOrLeftBrace = true;
                }
                //右括号
                else if (token is CloseBraceToken && ((CloseBraceToken)token).BraceType == BraceType.Round)
                {
                    while (operators.Peek() != ExpressionOperationType.OpenBrace)
                        PopOperator();
                    operators.Pop();

                    if(operators.Count > 0 && operators.Peek() == ExpressionOperationType.FunctionCall)
                        PopOperator();

                    lastTokenWasOperatorOrLeftBrace = false;
                }
                //变量
                else if(token is IdentifierToken)
                {
                    
                    working.Push(new VariableReferenceExpressionNode(((IdentifierToken)token).Content));

                    lastTokenWasOperatorOrLeftBrace = false;
                }
                //逗号
                else if(token is ArgSeperatorToken)
                {
                    //参数个数
                    arity.Push(arity.Pop() + 1); 
                    
                    while (operators.Peek() != ExpressionOperationType.OpenBrace)
                        PopOperator();
                }
                else
                    throw new ParsingException("Found unknown token while parsing expression!");
            }

            if (sequenceWasEmpty)
                return null;

            
            while (operators.Count != 0)
                PopOperator();
            //TODO 表达式不合法
            if (working.Count != 1)
                throw new ParsingException("Expression seems to be incomplete/invalid.");

            return working.Pop();
        }
        /// <summary>
        /// 根据运算符优先级调整表达式语法树
        /// </summary>
        private void PopOperator()
        {
            //上一运算符 符号
            var op = operators.Pop();
            //参数调用
            if(op == ExpressionOperationType.FunctionCall)
            {
                List<ExpressionNode> args = new List<ExpressionNode>();
                int functionArity = arity.Pop();
                for (int i = 0; i < functionArity; i++)
                    args.Add(working.Pop());
                ((FunctionCallExpressionNode)working.Peek()).AddArguments(args);
            }
            else if (unaryOperators.Contains(op))
                working.Push(new UnaryOperationNode(op, working.Pop()));
            else
            {
                var opB = working.Pop();
                var opA = working.Pop();

                working.Push(new BinaryOperationNode(op, opA, opB));
            }
        }
    }
}
