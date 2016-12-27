using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    /// <summary>
    /// 逗号token
    /// </summary>
    public class ArgSeperatorToken : Token
    {
        public ArgSeperatorToken(string content, int lineNum)
            : base(content, lineNum)
        {
            if (content != ",")
                throw new ArgumentException("The content is no argument seperator.", "content");
        }
    }
}
