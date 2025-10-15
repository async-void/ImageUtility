using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Extensions
{
    public static class EnumToStringExtension
    {
        public static string ToExtensionString(this Enum enumValue)
        {
            return enumValue.ToString().ToLower();
        }
    }
}
