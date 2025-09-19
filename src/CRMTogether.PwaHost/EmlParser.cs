using System;
using MimeKit;

namespace CRMTogether.PwaHost
{
    public class EmlInfo
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTimeOffset? Date { get; set; }
        public bool HasHtml { get; set; }
        public bool HasText { get; set; }
    }

    internal static class EmlParser
    {
        public static EmlInfo Parse(string path)
        {
            var msg = MimeMessage.Load(path);
            return new EmlInfo
            {
                Subject = msg.Subject ?? "",
                From = msg.From?.ToString() ?? "",
                To = msg.To?.ToString() ?? "",
                Date = msg.Date,
                HasHtml = msg.HtmlBody != null,
                HasText = msg.TextBody != null
            };
        }
    }
}
