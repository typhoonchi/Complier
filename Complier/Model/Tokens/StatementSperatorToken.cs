using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    class StatementSperatorToken : Token
    {
        /// <summary>
        /// 分号token
        /// </summary>
        /// <param name="content"></param>
        public StatementSperatorToken(string content, int lineNum)
            : base(content, lineNum)
        {
            if (content != ";")
                throw new ArgumentException("The content is no statement seperator.", "content");
        }
    }
}
