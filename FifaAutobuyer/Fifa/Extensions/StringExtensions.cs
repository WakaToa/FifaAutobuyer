using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FifaAutobuyer.Fifa.Extensions
{
    public static class StringExtensions
    {
        public static string GetRegexBetween(this string s, string begin, string end)
        {
            var regexS = begin + "(.*?)" + end;

            var result = Regex.Match(s, regexS);
            return result.Groups[1].Value;
        }

        public static IEnumerable<string> Substring(this string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                yield return match.Groups[1].Value;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToFormData(this string s)
        {
            var data = HttpUtility.ParseQueryString(s);
            return data.AllKeys.Select(key => new KeyValuePair<string, string>(key, data[key]));
        }
    }
}
