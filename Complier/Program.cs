using Complier.CodeGenerator;
using Complier.LexicalAnalysis;
using Complier.SyntaxAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complier.Model;
using Complier.Model.Tokens;

namespace Complier
{
    class Program
    {

        static void Main(string[] args)
        {
            //获取.exe文件所在目录
            var dir = Path.GetFullPath(".");
            //args = new []{ dir+"/test.txt" };

            string code = string.Empty;
            string defaultCode = string.Empty;
            if (!args.Any())
            {
                #region 源程序
                defaultCode = @"
int a = 16;

int func(int b)
{
    int c = (5*b)+7;
    return c;
}

int main()
{
    a = 16;
    a=func(4);
    return a*2;
}";
                code = defaultCode;

                defaultCode = @"
int main()
{
    int sum = 0;
    int a = 6;
    while(a > 0)
    {
        sum = sum + a;
        a = a - 1;
    }
    return sum;
}";
                //code = defaultCode;

                #endregion
            }
            else
            {
                if (File.Exists(args[0]))
                {
                    try
                    {
                        code = File.ReadAllText(args[0]);
                    }catch(FileLoadException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("打开文件失败:"+ex.Message);
                        code = defaultCode;
                    }
                }
                else
                {
                    Console.WriteLine("找不到指定文件");
                    code = defaultCode;
                }
            }



            //词法分析
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("***********************词法分析************************");
            Console.ResetColor();
            var lexer = new Tokenizer(code);
            //词法分析结果
            var tokens = lexer.Tokenize();
            Console.Write("\t序号\t单词\t类型\t\t\t行号\r\n");
            for (int i = 0; i < tokens.Length; i++)
            {
                Console.WriteLine($"\t{i+1}\t{tokens[i]}");

            }

            var errTokens = tokens.Where(o => o.GetType().Name == "UnKnowToken");
            var enumerable = errTokens as Token[] ?? errTokens.ToArray();
            if (enumerable.Any())
            {
                Console.WriteLine("\r\n语法分析发现错误;");
                foreach (var token in enumerable)
                {
                    var item = (UnKnowToken) token;
                    Console.WriteLine($"\r\n\t 第{item.LineNum}行 {item.Content} 出错：{item.ErrText}");
                }

            }

            //Console.WriteLine("**********************符号表*************************");
            //Console.WriteLine("序号\t变量名\t类型\t\t\t行号");
            //var iDTokens = tokens.Where(o => o.GetType().Name == "IdentifierToken").ToArray();
            //for (int i = 0; i < iDTokens.Length; i++)
            //{
            //    if (tokens[i].GetType().Name== "KeywordType")
            //    {
            //        //i++;


            //    }
            //    Console.WriteLine($"{i + 1}\t{iDTokens[i]}");
            //}

            //抽象语法树
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("**********************语法分析*************************");
            Console.ResetColor();
            var parser = new Parser(tokens);
            //分析之后的语法树
            var ast = parser.ParseToAst();

            Console.WriteLine("**********************符号表*************************");
            Console.WriteLine("序号\t变量名\t类型\t所在节点");

            for (int i = 0; i < parser.vartable.Count; i++)
            {
                Console.WriteLine($"{i + 1}\t{parser.vartable[i].name}\t{parser.vartable[i].type}\t{parser.vartable[i].nodeType.GetType().Name}");

            }
            //四元式表
            QuaternionTypeTable table = new QuaternionTypeTable();
            table.PrintAst(ast);


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("**********************代码生成*************************");
            Console.ResetColor();
            var codeGenerate = new AssemblyGenerate();
            codeGenerate.Generate(ast);

            //编译
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("************************编译***************************");
            Console.ResetColor();
            Complier.Complier complier = new Complier.Complier();
            var comCode = complier.GenerateCode(code);
            var comResult = complier.Compile(comCode);
            complier.ShowCompileResult(comResult);

            Console.ReadKey();
        }
    }
}
