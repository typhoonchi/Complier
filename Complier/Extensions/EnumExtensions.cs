using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="flag"></param> 
        /// <returns></returns>
        public static bool HasAnyFlag(this Enum e,Enum flag)
        {
            if (flag == null)
                throw new ArgumentNullException("flag");
            //确定两个 COM 类型是否具有相同的标识，以及是否符合类型等效的条件
            if (!e.GetType().IsEquivalentTo(flag.GetType()))
                throw new ArgumentException("标识不同，类型不等效", "flag");
            //返回布尔值，指示两个枚举类型是否具有相同标识，具体组合见CharType枚举类型的声明
            return (Convert.ToUInt64(e) & Convert.ToUInt64(flag)) != 0;
        }
    }
}
