using Complier.Extensions;
using Complier.Model;
using Complier.Model.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.LexicalAnalysis
{
    public class Tokenizer
    {
        /// <summary>
        /// 正在读取的位置
        /// </summary>
        private int readingPosition;
        public Tokenizer(string code)
        {
            this.Code = code;
            createLineNum();
            readingPosition = 0;
        }

        /// <summary>
        /// 源代码
        /// </summary>
        public string Code { get; private set; }
        /// <summary>
        /// 每个字符所在行数
        /// </summary>
        public List<int> lineNum = new List<int>();

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <returns></returns>
        public Token[] Tokenize()
        {
            var tokens = new List<Token>();
            var builder = new StringBuilder();
            while (!Eof())
            {
                //跳过空白符
                Skip(CharType.WhiteSpace);
                if (Eof()) break;
                //根据每个token的第一个字符判断token类型，以便添加到tokens的时候选择相应的类型
                switch (PeekType())
                {
                    case CharType.Alpha:
                        ReadToken(builder, CharType.AlphaNumeric);
                        string s = builder.ToString();
                        //判断是否为关键字
                        if (KeywordToken.IsKeyword(s))
                            tokens.Add(new KeywordToken(s, lineNum[readingPosition - 1]));
                        else
                            tokens.Add(new IdentifierToken(s, lineNum[readingPosition - 1]));
                        builder.Clear();
                        break;
                    case CharType.Numeric:
                        ReadToken(builder, CharType.AlphaNumeric);
                        decimal number;
                        //判断是否是数字开头的字母数字混合token
                        if (decimal.TryParse(builder.ToString(), out number))
                        tokens.Add(new NumberLiteralToken(builder.ToString(), lineNum[readingPosition - 1]));
                        else tokens.Add(new UnKnowToken(builder.ToString(), lineNum[readingPosition - 1],"标识符不能以数字开头"));
                        builder.Clear();
                        break;
                    case CharType.Operator:
                        ReadToken(builder, CharType.Operator);
                        tokens.Add(new OperatorToken(builder.ToString(), lineNum[readingPosition - 1]));
                        builder.Clear();
                        break;
                    case CharType.OpenBrace:
                        tokens.Add(new OpenBraceToken(Next().ToString(), lineNum[readingPosition - 1]));
                        break;
                    case CharType.CloseBrace:
                        tokens.Add(new CloseBraceToken(Next().ToString(), lineNum[readingPosition - 1]));
                        break;
                    case CharType.ArgSeperator:
                        tokens.Add(new ArgSeperatorToken(Next().ToString(),lineNum[readingPosition-1]));
                        break;
                    case CharType.StatementSeperator:
                        tokens.Add(new StatementSperatorToken(Next().ToString(), lineNum[readingPosition - 1]));
                        break;
                    default:
                        tokens.Add(new UnKnowToken(Next().ToString(),lineNum[readingPosition-1]));
                        break;
                        //throw new Exception("不能识别该字符");
                }
            }

            return tokens.ToArray();
        }

        /// <summary>
        /// 判断字符类型
        /// </summary>
        /// <param name="c">需要判断的字符</param>
        /// <returns>CharType枚举类型</returns>
        private CharType CharTypeOf(char c)
        {
            switch (c)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                case '&':
                case '|':
                case '=':
                case '>':
                case '<':
                    return CharType.Operator;
                case '(':
                case '[':
                case '{':
                    return CharType.OpenBrace;
                case ')':
                case ']':
                case '}':
                    return CharType.CloseBrace;
                case ',':
                    return CharType.ArgSeperator;
                case ';':
                    return CharType.StatementSeperator;
                case '\r': //\r and \n have UnicodeCategory.Control, not LineSeperator...
                case '\n':
                case '\t':
                    return CharType.NewLine;
            }
            //判断字符类型
            switch (char.GetUnicodeCategory(c))
            {
                //数字
                case UnicodeCategory.DecimalDigitNumber:
                    return CharType.Numeric;
                //分隔符，行
                case UnicodeCategory.LineSeparator: //just in case... (see above)
                    return CharType.NewLine;
                //分隔符，段落
                case UnicodeCategory.ParagraphSeparator:
                //小写字母
                case UnicodeCategory.LowercaseLetter:
                //不属于大写字母、小写字母、词首字母大写或修饰符字母的字母
                case UnicodeCategory.OtherLetter:
                //大写字母
                case UnicodeCategory.UppercaseLetter:
                    return CharType.Alpha;
                //空格
                case UnicodeCategory.SpaceSeparator:
                    return CharType.LineSpace;
            }
            //未知字符类型
            return CharType.Unknown;
        }

        /// <summary>
        /// 获取当前字符
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            return Code[readingPosition];
        }

        /// <summary>
        /// 获取下一字符
        /// </summary>
        /// <returns></returns>
        private char Next()
        {
            var result = Peek();
            readingPosition++;
            return result;
        }

        /// <summary>
        /// 获取当前字符类型
        /// </summary>
        /// <returns></returns>
        private CharType PeekType()
        {
            return CharTypeOf(Peek());
        }

        /// <summary>
        /// 获取下一字符类型
        /// </summary>
        /// <returns></returns>
        private CharType NextType()
        {
            return CharTypeOf(Next());
        }

        /// <summary>
        /// 跳过字符
        /// </summary>
        /// <param name="type"></param>
        private void Skip(CharType type)
        {

            while (!Eof() && PeekType().HasAnyFlag(type))
                Next();
        }

        /// <summary>
        /// 是否到达结尾
        /// </summary>
        /// <returns></returns>
        private bool Eof()
        {
            return readingPosition >= Code.Length;
        }

        /// <summary>
        /// 识别出一个Token
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        private void ReadToken(StringBuilder builder, CharType type)
        {
            //通过判断传入的字符类型与下一字符类型是否具有同一标识，识别每一个token
            while (!Eof() && PeekType().HasAnyFlag(type))
                builder.Append(Next());
        }
        /// <summary>
        /// 构造行号
        /// </summary>
        void createLineNum()
        {
            int temp = 1;
            for (int i = 0; i < Code.Length; i++)
            {
                lineNum.Add(temp);
                if (Code[i] == '\n')
                    ++temp;
            }
        }
    }
}
