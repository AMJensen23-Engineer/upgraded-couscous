using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TPS9266xEvaluationModule
{
    class Utilities
    {
        public string sciNotToTime(double valueDbl, ref Label labelStr)
        {
            // value passed in must be exponential
            if (!HasValue(valueDbl) || (labelStr == null))
                return "";
            string valuStr = valueDbl.ToString("E12");
            char lastChar = valuStr[valuStr.Length - 1];
            int exp = int.Parse(lastChar.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            if (exp == 0)
            {
                labelStr.Content = "";
            }
            else if (exp < 4)
            {
                valueDbl *= 1000;
                labelStr.Content = "m";  // milli
            }
            else if (exp < 7)
            {
                valueDbl *= 1000000;
                labelStr.Content = "u";  // micro
            }
            else if (exp < 10)
            {
                valueDbl *= 1000000000;
                labelStr.Content = "n";  // nano
            }
            else if (exp < 13)
            {
                valueDbl *= 1000000000000;  // pico
                labelStr.Content = "p";
            }

            return valueDbl.ToString("0.0");
        }

        private bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }
    }
}
