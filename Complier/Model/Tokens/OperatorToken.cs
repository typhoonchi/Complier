using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    /// <summary>
    /// 运算符
    /// </summary>
    class OperatorToken : Token
    {
        static readonly Dictionary<string, OperatorType> validOperators = new Dictionary<string, OperatorType>()
        {
            { "+", OperatorType.Add },
            { "&", OperatorType.And },
            { "=", OperatorType.Assignment },
            { "/", OperatorType.Divide },
            { "==", OperatorType.Equals },
            { ">=", OperatorType.GreaterEquals },
            { ">", OperatorType.GreaterThan },
            { "<=", OperatorType.LessEquals },
            { "<", OperatorType.LessThan },
            { "%", OperatorType.Modulo },
            { "*", OperatorType.Multiply },
            { "!", OperatorType.Not },
            { "!=", OperatorType.NotEquals },
            { "|", OperatorType.Or },
            { "-", OperatorType.SubstractNegate },
        };

        public OperatorType OperatorType { get; private set; }

        public OperatorToken(string content, int lineNum)
            : base(content, lineNum)
        {
            if (!validOperators.ContainsKey(content))
                throw new ArgumentException("暂未支持识别该运算符", "content");

            OperatorType = validOperators[content];
        }
    }
}
