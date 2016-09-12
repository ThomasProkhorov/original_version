using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Text.RegularExpressions;

namespace Uco.Infrastructure
{
    public static class UcoString
    {
        public static List<string> String2List(string value)
        {
            List<string> Items = new List<string>();
            if (!string.IsNullOrEmpty(value))
            {
                foreach (string item in value.Split('|').ToList())
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    Items.Add(item);
                }
            }
            return Items;
        }
        public static string List2String(List<string> Items)
        {
            if (Items.Count == 0) return string.Empty;
            return string.Format("|{0}|", String.Join("|", Items.Select(r => r).ToArray()));
        }

        public static string RemoveHTMLTagsFromString(string Text)
        {
            string noHTML = Regex.Replace(Text, @"<[^>]+>|&nbsp;", "").Trim();
            return Regex.Replace(noHTML, @"\s{2,}", " ");
        }

        public static string ReplaceBrByDot(string Text)
        {
            return Text.Replace("<br />", ". ").Replace("<br/>", ". ").Replace("<br>", ". ");
        }

        public static string ClearForCSV(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;
            return Text.Replace("\"", "“").Replace(",", ".");
        }

        public static string GetUtf8String(string notUtf8String)
        {
            string OutValue = "";
            byte[] encodedBytes = System.Text.Encoding.Default.GetBytes(notUtf8String);
            string notUtf8StringEncoded = System.Text.Encoding.UTF8.GetString(encodedBytes);

            if (notUtf8StringEncoded.Contains("�")) OutValue = notUtf8String;
            else notUtf8String = notUtf8StringEncoded;

            return notUtf8String;
        }
    }

}