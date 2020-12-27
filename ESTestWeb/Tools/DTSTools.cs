using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESTestWeb.Tools
{
    public class DTSTools
    {
        /// <summary>
        /// Convert string to int.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ConvertToInt(string str, int defaultValue)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            bool br = int.TryParse(str, out int strInt);

            if (br == true)
            {
                return strInt;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
