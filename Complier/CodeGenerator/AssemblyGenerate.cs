using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complier.Model.Ast;
using System.IO;
using Complier.SyntaxAnalysis;

namespace Complier.CodeGenerator
{
    public class AssemblyGenerate
    {
        #region 将四元式转成汇编代码
        StringBuilder AssemblyCode=new StringBuilder();
        //传人一个四元式，输出对应的汇编代码
        void print_code(QuaternionTypeTable._Siyuanshi siyuanshi)
        {
            AssemblyCode.Append("\r\n");
            /*
             mov eax, 3
                add eax, 4
                mov t+0, eax
            */
            if (siyuanshi.op == "+")
            {
                AssemblyCode.AppendFormat("        mov eax,{0}\r\n", siyuanshi.oper1);
                AssemblyCode.AppendFormat("        add eax,{0}\r\n", siyuanshi.oper2);
                AssemblyCode.AppendFormat("        mov {0},eax\r\n", siyuanshi.result);
            }
            else if (siyuanshi.op == "-")
            {
                AssemblyCode.AppendFormat("        mov eax,{0}\r\n", siyuanshi.oper1);
                AssemblyCode.AppendFormat("        sub eax,{0}\r\n", siyuanshi.oper2);
                AssemblyCode.AppendFormat("        mov {0},eax\r\n", siyuanshi.result);
            }
            /*
            mov eax, 2
               mov ebx, t+0
               mul ebx
               mov t+4, eax
            */
            else if (siyuanshi.op == "*")
            {
                AssemblyCode.AppendFormat("        mov eax,{0}\r\n", siyuanshi.oper1);
                AssemblyCode.AppendFormat("        mov ebx,{0}\r\n", siyuanshi.oper2);
                AssemblyCode.AppendFormat("        mul ebx\r\n");
                AssemblyCode.AppendFormat("        mov {0},eax\r\n", siyuanshi.result);
            }
            else if (siyuanshi.op == "/")
            {//除法的时候不考虑余数
                AssemblyCode.AppendFormat("        mov eax,{0}\r\n", siyuanshi.oper1);
                AssemblyCode.AppendFormat("        mov ebx,{0}\r\n", siyuanshi.oper2);
                AssemblyCode.AppendFormat("        div ebx\r\n");
                AssemblyCode.AppendFormat("        mov {0},eax\r\n", siyuanshi.result);
            }
            else if (siyuanshi.op == "=")
            {
                AssemblyCode.AppendFormat("        mov eax,{0}\r\n", siyuanshi.oper1);
                AssemblyCode.AppendFormat("        mov {0},eax\r\n", siyuanshi.result);

            }
        }
        //输出全部汇编代码
        public string Code(List<QuaternionTypeTable._Siyuanshi> siyuanshiList)
        {
            AssemblyCode = new StringBuilder();
            AssemblyCode.AppendLine("生成的汇编代码如下：\r\n");
            AssemblyCode.AppendLine(".386");
            AssemblyCode.AppendLine(".MODEL FLAT");
            AssemblyCode.AppendLine("ExitProcess PROTO NEAR32 stdcall, dwExitCode:DWORD");
            AssemblyCode.AppendLine("INCLUDE io.h            ; header file for input/output");
            AssemblyCode.AppendLine("cr      EQU     0dh     ; carriage return character");
            AssemblyCode.AppendLine("Lf      EQU     0ah     ; line feed");
            AssemblyCode.AppendLine(".STACK  4096            ; reserve 4096-byte stack");
            AssemblyCode.AppendLine(".DATA                   ; reserve storage for data");
            AssemblyCode.AppendLine("t       DWORD   40 DUP (?)");
            AssemblyCode.AppendLine("label1   BYTE    cr, Lf, \"The result is \"");
            AssemblyCode.AppendLine("result  BYTE    11 DUP (?)");
            AssemblyCode.AppendLine("        BYTE    cr, Lf, 0");
            AssemblyCode.AppendLine(".CODE                           ; start of main program code");
            AssemblyCode.AppendLine("_start:");
            //遍历实验3中的四元式，输出对应的汇编代码
            for (int i=0; i < siyuanshiList.Count; i++)
                print_code(siyuanshiList[i]);
            AssemblyCode.AppendLine("        dtoa    result, eax     ; convert to ASCII characters");
            AssemblyCode.AppendLine("        output  label1          ; output label and sum");
            AssemblyCode.AppendLine("        INVOKE  ExitProcess, 0  ; exit with return code 0");
            AssemblyCode.AppendLine("PUBLIC _start                   ; make entry point public");
            AssemblyCode.AppendLine("END                             ; end of source code");
            return AssemblyCode.ToString();
        }
        #endregion
        #region 遍历语法树打印内容

        //private int _indentationLevel = 0;


        //public AssemblyGenerate()
        //{
        //}

        //#region 输出
        //public void Emit(string code)
        //{
        //    Emit("{0}", code);
        //}

        //public void Emit(string pattern, params object[] args)
        //{
        //    Console.WriteLine(new string(' ', 4 * _indentationLevel) + pattern, args);
        //}

        //public void EmitComment(string comment)
        //{
        //    EmitComment("{0}", comment);
        //}

        //public void EmitComment(string comment, params object[] args)
        //{
        //    Emit("//", comment, args);
        //}
        //#endregion

        //public void Generate(ProgramNode programNode)
        //{
        //    Visit(programNode);
        //}

        ///// <summary>
        ///// 根节点
        ///// </summary>
        ///// <param name="programNode"></param>


        //public void Visit(ProgramNode programNode)
        //{
        //    foreach (var item in programNode.SubNodes)
        //    {
        //        Visit((Object)item);
        //    }
        //}

        //public void Visit(Object node)
        //{
        //    if (node is VariableDeclarationNode)
        //        Visit(node as VariableDeclarationNode);
        //    else if (node is ReturnStatementNode)
        //        Visit(node as ReturnStatementNode);
        //    else if (node is IfStatementNode)
        //        Visit(node as IfStatementNode);
        //    else if (node is WhileLoopNode)
        //        Visit(node as WhileLoopNode);
        //    else if (node is VariableAssingmentNode)
        //        Visit(node as VariableAssingmentNode);
        //    else if (node is FunctionDeclarationNode)
        //        Visit(node as FunctionDeclarationNode);
        //    else if (node is ParameterDeclarationNode)
        //        Visit(node as ParameterDeclarationNode);
        //    else if (node is BinaryOperationNode)
        //        Visit(node as BinaryOperationNode);
        //    else if (node is FunctionCallExpressionNode)
        //        Visit(node as FunctionCallExpressionNode);
        //    else if (node is NumberLiteralNode)
        //        Visit(node as NumberLiteralNode);
        //    else if (node is UnaryOperationNode)
        //        Visit(node as UnaryOperationNode);
        //    else if (node is VariableReferenceExpressionNode)
        //        Visit(node as VariableReferenceExpressionNode);
        //    else
        //        throw new Exception("can't fint correct type.");
        //}

        ///// <summary>
        ///// 变量定义
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(VariableDeclarationNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("变量定义：");
        //    builder.AppendFormat("{0}\t{1} ", node.Type, node.Name);
        //    Console.Write(builder.ToString());
        //    Visit((Object)node.InitialValueExpression);
        //}

        ///// <summary>
        ///// 返回语句
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(ReturnStatementNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.AppendLine("返回语句:");
        //    Console.WriteLine(builder.ToString());
        //    Visit((Object)node.ValueExpression);
        //}

        ///// <summary>
        ///// if条件语句
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(IfStatementNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("if条件语句：");
        //    builder.Append("  条件：");
        //    Visit((Object)node.Condition);
        //    builder.AppendLine("条件语句块:");
        //    Console.WriteLine(builder.ToString());
        //    foreach (var item in node.SubNodes)
        //    {
        //        Visit((Object)item);
        //    }
        //}

        ///// <summary>
        ///// while语句
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(WhileLoopNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("while条件语句：");
        //    builder.Append("  条件：");
        //    Visit((Object)node.Condition);
        //    builder.AppendLine("条件语句块:");
        //    Console.WriteLine(builder.ToString());
        //    foreach (var item in node.SubNodes)
        //    {
        //        Visit((Object)item);
        //    }
        //}

        ///// <summary>
        ///// 变量赋值语句
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(VariableAssingmentNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("变量赋值：");
        //    builder.Append(node.VariableName);
        //    Console.WriteLine(builder.ToString());
        //    Visit((Object)node.ValueExpression);
        //}

        ///// <summary>
        ///// 函数定义
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(FunctionDeclarationNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("\r\n函数定义:\r\n");
        //    builder.AppendFormat("\t函数名称:{0}\r\n", node.FunctionName);
        //    builder.Append("\t函数参数:");
        //    foreach (var item in node.Parameters)
        //    {
        //        builder.Append(item);
        //    }
        //    builder.AppendLine("\r\r\n\t函数体：");
        //    Console.WriteLine(builder.ToString());
        //    foreach (var item in node.SubNodes)
        //    {
        //        Console.Write("\t\t");
        //        Visit((Object)item);
        //    }
        //}

        ///// <summary>
        ///// 函数参数定义
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(ParameterDeclarationNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("函数参数：");
        //    builder.AppendFormat("{0} {1}", node.Type, node.Name);
        //    Console.Write(builder.ToString());
        //}

        ///// <summary>
        ///// 二元操作符
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(BinaryOperationNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("二元操作符:");
        //    Console.WriteLine(builder.ToString());
        //    Visit((Object)node.OperandA);
        //    Console.WriteLine(node.OperationType);
        //    Visit((Object)node.OperandB);
        //}

        ///// <summary>
        ///// 函数调用
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(FunctionCallExpressionNode node)
        //{
        //    var builder = new StringBuilder();
        //    builder.Append("  函数调用：");
        //    builder.AppendFormat("{0}(", node.FunctionName);
        //    foreach (var item in node.Arguments)
        //    {
        //        builder.Append(item.ToString() + ",");
        //    }
        //    builder.Replace(",", ")", builder.Length - 1, 1);
        //    Console.WriteLine(builder.ToString());
        //}

        ///// <summary>
        ///// 数字字面量
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(NumberLiteralNode node)
        //{
        //    Console.Write(node.Value);
        //}

        ///// <summary>
        ///// 一元操作符
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(UnaryOperationNode node)
        //{
        //    Visit((Object)node.OperationType);
        //}

        ///// <summary>
        ///// 变量引用
        ///// </summary>
        ///// <param name="node"></param>
        //public void Visit(VariableReferenceExpressionNode node)
        //{
        //    Console.Write("\r\n变量调用:\t");
        //    Console.Write(node.VariableName);
        //}
        #endregion
    }

}


