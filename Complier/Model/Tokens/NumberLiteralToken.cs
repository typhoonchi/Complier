using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    /// <summary>
    /// 数字token
    /// </summary>
    class NumberLiteralToken : Token
    {
        private int number;
        public int Number => number;


        public NumberLiteralToken(string content, int lineNum)
            : base(content, lineNum)
        {
            number = int.Parse(content);
        }
    }
}
