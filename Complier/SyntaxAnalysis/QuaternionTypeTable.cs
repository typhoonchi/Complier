using Complier.Model.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complier.Model;

namespace Complier.SyntaxAnalysis
{
    /// <summary>
    /// 四元式表
    /// </summary>
    public class QuaternionTypeTable
    {
        private StatementSequenceNode ast;

        /// <summary>
        /// 四元式结构
        /// </summary>
        public struct _Siyuanshi
        {

            public string op;
            public string oper1;
            public string oper2;
            public string result;
        }
        /// <summary>
        /// 四元式列表
        /// </summary>
        List<_Siyuanshi> siyuanshi ;
        /// <summary>
        /// 中间变量列表
        /// </summary>
        List<string> TempTable = new List<string>();
        /// <summary>
        /// 中间变量标号
        /// </summary>
        int tempIndex = 1;
        /// <summary>
        /// 符号表
        /// </summary>

        private List<Parser.Vartable> vartable;
        public QuaternionTypeTable(StatementSequenceNode ast, List<Parser.Vartable> vartable, ref List<_Siyuanshi> siyuanshiList)
        {
            this.ast = ast;
            this.vartable = vartable;
            siyuanshi=siyuanshiList;
            Errors = new Error
            {
                HasErros = false,
                Tokens = new List<Token>(),
                ErrorTexts=new List<string>()
            };
        }
        /// <summary>
        /// 中间变量栈
        /// </summary>
        Stack<string> StackTempTable = new Stack<string>();

        public struct Error
        {
            /// <summary>
            /// 指示是否含有错误
            /// </summary>
            public bool HasErros;
            /// <summary>
            /// 出错的token
            /// </summary>
            public List<Token> Tokens;

            public List<string> ErrorTexts;

        }

        public Error Errors ;

        /// <summary>
        /// 将抽象语法树转换为四元式表
        /// </summary>
        /// <param name="ast"></param>
        public void PrintAst(StatementSequenceNode ast)

        {
            foreach (var item in ast.SubNodes)
            {
                //变量声明节点
                if (item is VariableDeclarationNode)
                {
                    var node = item as VariableDeclarationNode;
                    Readnode(node);
                }
                //函数节点 只有遇到主函数节点才行，其他函数节点通过调用的方式进行访问  a=fun(10,5);
                else if (item is FunctionDeclarationNode)
                {
                    var node = item as FunctionDeclarationNode;
                    if (node.FunctionName == "main")
                    {
                        //函数入口，声明一个中间变量，用于返回值
                        newTemp();
                        Readnode(node);
                    }
                }
                //变量赋值节点
                else if (item is VariableAssingmentNode)
                {
                    var node = item as VariableAssingmentNode;
                    Readnode(node);
                }
                else if (item is ReturnStatementNode)
                {
                    var node = item as ReturnStatementNode;
                    Readnode(node);
                }
                else if(item is WhileLoopNode)
                {
                    var node = item as WhileLoopNode;
                    ReadNode(node);
                }
            }
        }
        /// <summary>
        /// 生成一个四元式
        /// </summary>
        /// <param name="op"></param>
        /// <param name="oper1"></param>
        /// <param name="oper2"></param>
        /// <param name="result"></param>
        _Siyuanshi emit(string op, string oper1, string oper2, string result) //生成一个四元式
        {
            return new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            };
        }
        /// <summary>
        /// 生成四元式中间变量T1，T2...，并将该变量压入栈顶
        /// </summary>
        /// <returns></returns>
        string newTemp()
        {
            string temp = string.Format("T{0}", tempIndex);

            TempTable.Add(temp); //存入中间变量表
            //压入栈顶
            StackTempTable.Push(temp);
            tempIndex++;
            return temp;
        }

        string ReadNode(NumberLiteralNode node)
        {
            return node.Value.ToString();
        }

        /// <summary>
        /// 将变量定义节点转成四元式
        /// </summary>
        /// <param name="vardecNode"></param>
        /// <returns></returns>
        void Readnode(VariableDeclarationNode vardecNode)
        {
            var op = "=";
            var oper1 = "";
            var oper2 = "_";
            var result = vardecNode.Name;
            //TODO 需要根据类型来判断与值是否匹配
            if (vardecNode.InitialValueExpression is NumberLiteralNode)
                oper1 = ((NumberLiteralNode)vardecNode.InitialValueExpression).Value.ToString();
            else if (vardecNode.InitialValueExpression is BinaryOperationNode)
            {
                //该变量定义的初始值为两个操作数的表达式，在这个用中间变量代替
                //生成中间变量并推到栈顶
                oper1 = newTemp();

                //
                var temp = vardecNode.InitialValueExpression as BinaryOperationNode;
                ReadNode(temp);

                //出栈
                StackTempTable.Pop();
            }
            siyuanshi.Add(new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            });
        }

        /// <summary>
        /// 将函数节点转成四元式
        /// </summary>
        /// <param name="vardecNode"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        void Readnode(FunctionDeclarationNode node, IEnumerable<ExpressionNode> parameters = null)
        {
            //为参数列表生成四元式
            if (node.Parameters.Any() && parameters != null)
            {
                ReadNode(node.Parameters, parameters);

            }
            //ProgramNode
            PrintAst(node);
        }
        /// <summary>
        /// 将变量定义节点转成四元式
        /// </summary>
        /// <param name="vardecNode"></param>
        /// <returns></returns>
        void Readnode(ParameterDeclarationNode vardecNode)
        {
            var op = "=";
            var oper1 = "";
            var oper2 = "_";
            var result = vardecNode.Name;
            //if (vardecNode. is NumberLiteralNode)
            //{

            //}
            //oper1 = ((NumberLiteralNode)vardecNode.Type).Value.ToString();
            //else if ()
            //{

            //}
            siyuanshi.Add(new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            });
        }

        /// <summary>
        /// 参数节点
        /// </summary>
        /// <param name="vardecNode"></param>
        /// <param name="values"></param>
        List<_Siyuanshi> ReadNode(IEnumerable<ParameterDeclarationNode> vardecNode, IEnumerable<ExpressionNode> values)
        {
            var op = "=";
            var oper1 = "";
            var oper2 = "_";
            var result = "";
            int i = vardecNode.ToArray().Length-1;
            foreach (var item in values)
            {
                result = vardecNode.ToArray()[i].Name;

                if (item is NumberLiteralNode)
                {
                    var temp = item as NumberLiteralNode;
                    oper1 = ReadNode(temp);
                }
                else
                {

                }
                siyuanshi.Add(new _Siyuanshi()
                {
                    op = op,
                    oper1 = oper1,
                    oper2 = oper2,
                    result = result
                });
                i--;
            }
            return new List<_Siyuanshi>();
        }
        /// <summary>
        /// 赋值节点转成四元式 a=0;
        /// </summary>
        /// <param name="vardecNode"></param>
        /// <returns></returns>
        List<_Siyuanshi> Readnode(VariableAssingmentNode node)
        {
            var op = "=";
            var oper1 = "";
            var oper2 = "_";
            var result = node.VariableName;

            if (node.ValueExpression is NumberLiteralNode)
            {
                var temp = node.ValueExpression as NumberLiteralNode;
                oper1 = ReadNode(temp);
            }
            else if (node.ValueExpression is FunctionCallExpressionNode)
            {
                //如果表达式是函数调用，那么生成一个中间变量，并将其推到栈顶，生成四元式之后再出栈
                //只有在调用newTemp()生成中间变量的方法里面才能调用出栈功能
                oper1 = newTemp();

                //查找调用的函数节点
                var temp = node.ValueExpression as FunctionCallExpressionNode;
                var funcDecNode = FindFunctionByName(temp.FunctionName);

                //输入参数，生成该调用函数节点对应的四元式
                Readnode(funcDecNode, temp.Arguments);

                //中间变量出栈
                StackTempTable.Pop();
            }
            siyuanshi.Add(new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            });
            return new List<_Siyuanshi>()
            {
                new _Siyuanshi()
                {
                    op = op,
                    oper1 = oper1,
                    oper2 = oper2,
                    result =result
                }
            };
        }
        /// <summary>
        /// 查找函数节点
        /// </summary>
        /// <param name="funName"></param>
        /// <returns></returns>
        FunctionDeclarationNode FindFunctionByName(string funName)
        {
            var funMain =
                ast.SubNodes.ToArray()
                    .Where(o => o.GetType().Name == "FunctionDeclarationNode")
                    .Cast<FunctionDeclarationNode>()
                    .Where(o => o.FunctionName == funName)
                    .ToArray();
            return funMain.Length > 0 ? funMain[0] : new FunctionDeclarationNode("");
        }
        /// <summary>
        /// 函数返回值节点
        /// </summary>
        /// <param name="node"></param>
        void Readnode(ReturnStatementNode node)
        {
            var op = "=";
            var oper1 = "";
            var oper2 = "_";
            var result = StackTempTable.Peek();
            if (node.ValueExpression is VariableReferenceExpressionNode)
            {
                var temp = node.ValueExpression as VariableReferenceExpressionNode;
                //检查变量是否存在语义错误
                ReadNode(temp);
                oper1 = temp.VariableName;
            }
            else if(node.ValueExpression is BinaryOperationNode)
            {
                //二元运算
                oper1 = newTemp();

                var temp = node.ValueExpression as BinaryOperationNode;
                ReadNode(temp);

                //中间变量出栈
                StackTempTable.Pop();

            }
            siyuanshi.Add(new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            });
        }
        /// <summary>
        /// 二元运算节点
        /// </summary>
        /// <param name="node"></param>
        object ReadNode(BinaryOperationNode node)
        {
            var op = "";
            var oper1 = "";
            var oper2 = "";
            var result = StackTempTable.Peek();

            #region 操作数A
            if (node.OperandA is VariableReferenceExpressionNode)
            {
                //变量引用
                var temp = node.OperandA as VariableReferenceExpressionNode;
                //检查变量是否存在语义错误
                ReadNode(temp);
                oper1 = temp.VariableName;
            }
            else if (node.OperandA is NumberLiteralNode)
            {
                //数字
                var temp = node.OperandA as NumberLiteralNode;
                oper1 = temp.Value.ToString();
            }
            else if (node.OperandA is BinaryOperationNode)
            {
                //二元运算
                oper1 = newTemp();

                var temp = node.OperandA as BinaryOperationNode;
                ReadNode(temp);

                //中间变量出栈
                StackTempTable.Pop();
            }else if (node.OperandA is FunctionCallExpressionNode)
            {
                //如果表达式是函数调用，那么生成一个中间变量，并将其推到栈顶，生成四元式之后再出栈
                //只有在调用newTemp()生成中间变量的方法里面才能调用出栈功能
                oper1 = newTemp();

                //查找调用的函数节点
                var temp = node.OperandA as FunctionCallExpressionNode;
                var funcDecNode = FindFunctionByName(temp.FunctionName);

                //输入参数，生成该调用函数节点对应的四元式
                Readnode(funcDecNode, temp.Arguments);

                //中间变量出栈
                StackTempTable.Pop();
            }
            else
            {
                    
            }
            #endregion
            #region 操作数B
            if (node.OperandB is VariableReferenceExpressionNode)
            {
                var temp = node.OperandB as VariableReferenceExpressionNode;
                //检查变量是否存在语义错误
                ReadNode(temp);
                oper2 = temp.VariableName;
            }
            else if (node.OperandB is NumberLiteralNode)
            {
                //数字
                var temp = node.OperandB as NumberLiteralNode;
                oper2 = temp.Value.ToString();
            }
            else if (node.OperandB is BinaryOperationNode)
            {
                oper2 = newTemp();

                var temp = node.OperandB as BinaryOperationNode;
                ReadNode(temp);

                //中间变量出栈
                StackTempTable.Pop();
            }
            else if (node.OperandB is FunctionCallExpressionNode)
            {
                //如果表达式是函数调用，那么生成一个中间变量，并将其推到栈顶，生成四元式之后再出栈
                //只有在调用newTemp()生成中间变量的方法里面才能调用出栈功能
                oper1 = newTemp();

                //查找调用的函数节点
                var temp = node.OperandB as FunctionCallExpressionNode;
                var funcDecNode = FindFunctionByName(temp.FunctionName);

                //输入参数，生成该调用函数节点对应的四元式
                Readnode(funcDecNode, temp.Arguments);

                //中间变量出栈
                StackTempTable.Pop();
            }
            #endregion
            #region  判断运算符类型
            switch (node.OperationType)
            {
                case ExpressionOperationType.Add:
                    op = "+";
                    break;
                case ExpressionOperationType.Substract:
                    op = "-";
                    break;
                case ExpressionOperationType.Multiply:
                    op = "*";
                    break;
                case ExpressionOperationType.Divide:
                    op = "/";
                    break;
                case ExpressionOperationType.Modulo:
                    op = "%";
                    break;
                case ExpressionOperationType.Assignment:
                    op = "=";
                    break;
                case ExpressionOperationType.Equals:
                    op = "==";
                    break;
                case ExpressionOperationType.GreaterThan:
                    op = ">";
                    break;
                case ExpressionOperationType.LessThan:
                    op = "<";
                    break;
                case ExpressionOperationType.GreaterEquals:
                    op = ">=";
                    break;
                case ExpressionOperationType.LessEquals:
                    op = "<=";
                    break;
                case ExpressionOperationType.NotEquals:
                    op = "!=";
                    break;
                case ExpressionOperationType.Not:
                    op = "!";
                    break;
                case ExpressionOperationType.And:
                    op = "&";
                    break;
                case ExpressionOperationType.Or:
                    op = "|";
                    break;
            }

            #endregion

            siyuanshi.Add(new _Siyuanshi()
            {
                op = op,
                oper1 = oper1,
                oper2 = oper2,
                result = result
            });
            return new object();
        }
        /// <summary>
        /// 引用变量节点，用来判断变量是否定义等状态
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool ReadNode(VariableReferenceExpressionNode node)
        {
            //变量查找结果
            var finRes = vartable.Where(o => o.name == node.VariableName);
            if (!finRes.Any())
            {
                //如果找不到变量
                Errors.HasErros = true;
                Errors.ErrorTexts.Add($"变量{node.VariableName}没有定义");
            }
            return false;
        }

        void ReadNode(WhileLoopNode node)
        {
            //var 

        }

    }
}
