using Complier.Model;
using Complier.Model.Ast;
using Complier.Model.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.SyntaxAnalysis
{
    public class Parser
    {
        public Token[] tokens { get; private set; }
        private static int readingPosition;
        private Stack<StatementSequenceNode> scopes;
        private static readonly KeywordType[] typeKeywords = { KeywordType.Int, KeywordType.Void };
        /// <summary>
        /// 符号表
        /// </summary>
        public struct Vartable
        {

            public string name; //符号字符串
            public VariableType type; //变量类型
            public AstNode nodeType;//所在节点
            /// <summary>
            /// 所在行数
            /// </summary>
            public int LineNum;
        }

        public List<Vartable> vartable;//存储符号

        public Parser(Token[] tokens)
        {
            this.tokens = tokens;
            readingPosition = 0;
            scopes = new Stack<StatementSequenceNode>();
            vartable = new List<Vartable>(); 
        }

        /// <summary>
        /// 解析成抽象语法树
        /// </summary>
        /// <returns></returns>
        public ProgramNode ParseToAst()
        {
            if(scopes.Count==0)
                scopes.Push(new ProgramNode());
            while (!Eof())
            {
                //是否为关键字类型"if"、"int"、"return"、"void"、"while"
                if (Peek() is KeywordToken)
                {
                    //取得当前token,指向下一token
                    var keyword = (KeywordToken)Next();

                    if (scopes.Count == 1)
                    {
                        //是否为关键类型“int”，“void”
                        if (keyword.IsTypeKeyword)
                        {
                            //转换为变量类型
                            var varType = keyword.ToVariableType();
                            //读取当前token,判断是否为IdentifierToken
                            var name = ReadToken<IdentifierToken>();

                            //将标识符添加到符号表
                            vartable.Add(new Vartable()
                            {
                                name=name.Content,
                                type = varType,
                                nodeType = scopes.Peek(),
                                LineNum = name.LineNum
                            });

                            //超前搜索
                            Token lookahead = Peek();

                            //如果是等号=或者分号;则为变量定义表达式
                            if (lookahead is OperatorToken && (((OperatorToken)lookahead).OperatorType == OperatorType.Assignment) || lookahead is StatementSperatorToken) 
                            {
                                //如果是=号，设置下一token,将识别出  int x=0;
                                //如果是;号，将变量默认值设置为0，识别出 int x;

                                if (lookahead is OperatorToken)
                                    Next();
                                //给当前栈的节点添加一个变量定义节点VariableDeclarationNode
                                scopes.Peek().AddStatement(new VariableDeclarationNode(varType, name.Content, ExpressionNode.CreateFromTokens(ReadUntilStatementSeperator())));

                                /*--------到这里识别出一个定义变量的表达式------*/
                            }
                            //如果是左括号(  ，则为方法
                            else if (lookahead is OpenBraceToken && (((OpenBraceToken)lookahead).BraceType == BraceType.Round)) 
                            {
                                var func = new FunctionDeclarationNode(name.Content);
                                scopes.Peek().AddStatement(func); 
                                scopes.Push(func); 
                                Next(); 
                                //右括号之前的所有内容，识别为参数定义
                                //TODO 增加无参数函数识别在这里
                                while (!(Peek() is CloseBraceToken && ((CloseBraceToken)Peek()).BraceType == BraceType.Round)) 
                                {
                                    //变量类型
                                    var argType = ReadToken<KeywordToken>();
                                    if (!argType.IsTypeKeyword)
                                        throw new ParsingException("Expected type keyword!");
                                    var argName = ReadToken<IdentifierToken>();
                                    //将标识符添加到符号表
                                    vartable.Add(new Vartable()
                                    {
                                        name = argName.Content,
                                        type = argType.ToVariableType(),
                                        nodeType = scopes.Peek(),
                                        LineNum=name.LineNum
                                    });
                                    //函数节点增加一个参数节点
                                    func.AddParameter(new ParameterDeclarationNode(argType.ToVariableType(), argName.Content));
                                    if (Peek() is ArgSeperatorToken) 
                                        Next();
                                }

                                Next();
                                var curlyBrace = ReadToken<OpenBraceToken>();
                                //如果不是{}，出错
                                if (curlyBrace.BraceType != BraceType.Curly)
                                    //
                                    throw new ParsingException("Wrong brace type found!");

                                /*---------到这里为止，识别出的是函数名，参数，半个大括号{  ------------*/
                            }
                            else
                                //TODO 类型+变量之后，如果不是= ; ( 这三种，则出错
                                throw new Exception("The parser encountered an unexpected token.");
                        }
                        else
                            //程序的开始不是int或者void，发生语法错误
                            //TODO 记录语法错误 或者新增变量识别，如char
                            throw new ParsingException("Found non-type keyword on top level.");
                    }
                    //栈的元素多于1
                    else
                    {
                        if (keyword.IsTypeKeyword)
                        {
                            var varType = keyword.ToVariableType();
                            
                            var name = ReadToken<IdentifierToken>();
                            //将标识符添加到符号表
                            vartable.Add(new Vartable()
                            {
                                name = name.Content,
                                type = varType,
                                nodeType = scopes.Peek(),
                                LineNum = name.LineNum
                            });
                            Token lookahead = Peek();
                            //如果是赋值 = 或者 分号 ;
                            if (lookahead is OperatorToken && (((OperatorToken)lookahead).OperatorType == OperatorType.Assignment) || lookahead is StatementSperatorToken) //变量定义
                            {
                                if (lookahead is OperatorToken)
                                    Next();
                                //添加定义变量节点
                                scopes.Peek().AddStatement(new VariableDeclarationNode(varType, name.Content, ExpressionNode.CreateFromTokens(ReadUntilStatementSeperator())));
                            }
                            //else 这里可以添加运算符类型
                        }
                        else
                        {
                            //TODO 在这里添加方法里面的其他内容，比如for foreach switch
                            switch (keyword.KeywordType)
                            {
                                case KeywordType.Return:
                                    scopes.Peek().AddStatement(new ReturnStatementNode(ExpressionNode.CreateFromTokens(ReadUntilStatementSeperator())));
                                    break;
                                case KeywordType.If:
                                    var @if = new IfStatementNode(ExpressionNode.CreateFromTokens(ReadUntilClosingBrace()));
                                    scopes.Peek().AddStatement(@if);
                                    scopes.Push(@if);
                                    if (Peek() is OpenBraceToken && ((OpenBraceToken)Peek()).BraceType == BraceType.Curly)
                                    {
                                        Next();
                                    }
                                    break;
                                case KeywordType.While:
                                    var @while = new WhileLoopNode(ExpressionNode.CreateFromTokens(ReadUntilClosingBrace()));
                                    scopes.Peek().AddStatement(@while);
                                    scopes.Push(@while);
                                    if(Peek() is OpenBraceToken && ((OpenBraceToken)Peek()).BraceType == BraceType.Curly)
                                    {
                                        Next();
                                    }
                                    break;
                                default:
                                    throw new ParsingException("Unexpected keyword type.");
                            }
                        }
                    }
                }
                //自定义变量
                else if (Peek() is IdentifierToken && scopes.Count > 1)
                {
                    var name = ReadToken<IdentifierToken>();

                    //=号，赋值语句
                    if (Peek() is OperatorToken && ((OperatorToken)Peek()).OperatorType == OperatorType.Assignment)
                    {
                        Next(); 
                        scopes.Peek().AddStatement(new VariableAssingmentNode(name.Content, ExpressionNode.CreateFromTokens(ReadUntilStatementSeperator())));
                    }
                    //变量后面是 ( 号,函数调用
                    else
                        scopes.Peek().AddStatement(ExpressionNode.CreateFromTokens(new[] { name }.Concat(ReadUntilStatementSeperator())));
                }
                //  }
                else if (Peek() is CloseBraceToken)
                {
                    var brace = ReadToken<CloseBraceToken>();
                    if (brace.BraceType != BraceType.Curly)
                        throw new ParsingException("Wrong brace type found!");
                    scopes.Pop();
                }
                else
                    throw new ParsingException("The parser ran into an unexpeted token.");
            }

            if (scopes.Count != 1)
                throw new ParsingException("The scopes are not correctly nested.");

            return (ProgramNode)scopes.Pop();
        }

        /// <summary>
        /// 是否到达结尾
        /// </summary>
        /// <returns></returns>
        private bool Eof()
        {
            return readingPosition >= tokens.Length;
        }

        //private IEnumerable<Token> ReadTokenSeqence(params Type[] expectedTypes)
        //{
        //    foreach (var t in expectedTypes)
        //    {
        //        if (!t.IsAssignableFrom(Peek().GetType()))
        //            throw new ParsingException("Unexpected token");
        //        yield return Next();
        //    }
        //}
        /// <summary>
        /// 从当前token开始读取右括号之前的所有token,
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Token> ReadUntilClosingBrace()
        {
            //while (!Eof() && !(Peek() is CloseBraceToken))
            //    yield return Next();
            //Next();
            bool getNext = true;
            while (!Eof() && !(Peek() is CloseBraceToken))
                yield return Next();
            while (getNext)
            {
                getNext = false;
                yield return Peek();
            }
            Next();
        }
        /// <summary>
        /// 从当前token开始读取分号之前的所有token,
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Token> ReadUntilStatementSeperator()
        {
            //循环条件：没到tokens末尾和不是分号
            while (!Eof() && !(Peek() is StatementSperatorToken))
                //将符合条件的token添加到IEnumerable<Token>，运行到方法尾的时候，返回IEnumerable<Token>
                yield return Next();
            Next();
        }
        /// <summary>
        /// 读取当前token,并与预期token进行对比，对比失败抛错,对比成功设置下一token
        /// </summary>
        /// <typeparam name="TExpected"></typeparam>
        /// <returns></returns>
        private TExpected ReadToken<TExpected>() where TExpected : Token
        {
            if (Peek() is TExpected)
                return (TExpected)Next();
            else
                throw new ParsingException("Unexpected token " + Peek());
        }
        /// <summary>
        /// 取得当前token
        /// </summary>
        /// <returns></returns>
        private Token Peek()
        {
            if (!Eof())
                return tokens[readingPosition];
            else
                return null;
        }
        /// <summary>
        /// 获取当前token,并指向下一token
        /// </summary>
        /// <returns></returns>
        private Token Next()
        {
            var ret = Peek();
            readingPosition++;
            return ret;
        }

        
    }
}
