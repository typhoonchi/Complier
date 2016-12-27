using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    class UnKnowToken:Token
    {
        public string ErrText = "";

        public UnKnowToken(string content, int lineNum, string errText = "不能识别该词")
            : base(content, lineNum)
        {
            this.ErrText = errText;
        }
    }
}
