using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model
{
    /// <summary>
    /// 单词类
    /// </summary>
    public class Token
    {
        public Token()
        {
        }

        public Token(string content,int lineNum)
        {
            this.Content = content;
            this.LineNum = lineNum;
        }
        /// <summary>
        /// 所在行号
        /// </summary>
        public int LineNum { get; }

        /// <summary>
        /// 单词内容
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// 词法分析的结果，每一个单词识别之后的显示格式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t\t{2}", Content, this.GetType().Name,LineNum );
        }
    }
}
