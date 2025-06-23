using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameSDK.Extensions
{
    public static class BuildExtension
    {
        private const string KeyBegin = @"<!-- BEGIN {0} -->";
        private const string KeyEnd = @"<!-- END {0} -->";
        
        public static string Append(string html, HtmlTag tag, string key, string value)
        { 
            html = Remove(html, key);

            var begin = string.Format(KeyBegin, key); // Экранирование ключа не нужно
            var end = string.Format(KeyEnd, key);    // Экранирование ключа не нужно

            var tagKey = $"</{tag.ToString().ToLower()}>";
            
            var tagIndex = html.IndexOf(tagKey, StringComparison.Ordinal);
            
            if (tagIndex == -1)
                return html;
            
            var block = $"\n\t{begin}\n\t{value}\n\t{end}";
            
            var pattern = $@"{Regex.Escape(tagKey)}";
            html = Regex.Replace(html, pattern, $"{block}\n {tagKey}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
            html = RemoveEmptyLines(html);
            
            return html;
        }
        
        public static string RemovePatterns(string html, params string[] patterns) => 
            patterns.Aggregate(html, (current, pattern) => Regex.Replace(current, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline));
        
        public static string RemoveEmptyLines(string html) => Regex.Replace(html, @"^\s*$\n|\r", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        public static string Remove(string html, string key)
        {
            var begin = string.Format(KeyBegin, key);
            var end = string.Format(KeyEnd, key);

            var pattern = $@"{Regex.Escape(begin)}.*{Regex.Escape(end)}\s*";

            html = RemovePatterns(html, pattern);
            
            return html;
        }
    }
}