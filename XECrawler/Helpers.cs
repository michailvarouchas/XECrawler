using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XECrawler
{
    static class Helper
    {
        public static string GetStartOrEmpty(this string text, string stopAt = ",")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static string GetEndOrEmpty(this string text, string stopAt = ",")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal) + stopAt.Length;

                if (charLocation > 0)
                {
                    return text.Substring(charLocation);
                }
            }

            return String.Empty;
        }

        public static string GetMiddleOrEmpty(this string text, string start = ",", string stop = ".")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int startCharLocation = text.IndexOf(start, StringComparison.Ordinal);
                int stopCharLocation = text.IndexOf(stop, StringComparison.Ordinal);

                if (startCharLocation > 0 && stopCharLocation > 0 && stopCharLocation - startCharLocation > 3)
                {
                    return text.Substring(startCharLocation + 1, stopCharLocation - startCharLocation - 3);
                }
            }

            return String.Empty;
        }
    }
}
