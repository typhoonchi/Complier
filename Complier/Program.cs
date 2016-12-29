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

int func(int b,int bb)
{
    int c = (5*b)+7*bb;
    return c;
}

int main()
{
    a = 16;
    a=func(4,8);
    return a;
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
                    }
                    catch (FileLoadException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("打开文件失败:" + ex.Message);
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
                Console.WriteLine($"\t{i + 1}\t{tokens[i]}");

            }

            var errTokens = tokens.Where(o => o.GetType().Name == "UnKnowToken");
            var enumerable = errTokens as Token[] ?? errTokens.ToArray();
            if (enumerable.Any())
            {
                Console.WriteLine("\r\n语法分析发现错误;");
                foreach (var token in enumerable)
                {
                    var item = (UnKnowToken)token;
                    Console.WriteLine($"\r\n\t 第{item.LineNum}行 {item.Content} 出错：{item.ErrText}");
                }

            }

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

            Console.WriteLine("**********************四元式*************************");

            //Console.WriteLine("序号\t类型\t操作数一\t操作数二\t结果\t");

            //四元式表
            var siyuanshi = new List<QuaternionTypeTable._Siyuanshi>();
            QuaternionTypeTable table = new QuaternionTypeTable(ast, parser.vartable, ref siyuanshi);
            //调用转换方法
            table.PrintAst(ast);
            //输出结果
            for (int i = 0; i < siyuanshi.Count; i++)
            {
                var item = siyuanshi[i];
                Console.WriteLine($"{i + 1}\t( {item.op} , {item.oper1} , {item.oper2} , {item.result} )");
            }
            if (table.Errors.HasErros)
            {
                Console.WriteLine("\r\n错误：");
                for (int i = 0; i < table.Errors.ErrorTexts.Count; i++)
                {
                    Console.WriteLine($"\t{table.Errors.ErrorTexts[i]}");

                }
            }


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("**********************中间代码生成和汇编代码生成*************************");
            Console.ResetColor();
            var codeGenerate = new AssemblyGenerate();
            var AssemblyCode = codeGenerate.Code(siyuanshi);
            Console.WriteLine(AssemblyCode);



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
