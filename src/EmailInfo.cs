using System;

namespace GmailCleanup
{
    public class EmailInfo
    {
        public DateTime? Date { get; set; }
        public string From { get; set; }
        public string Id { get; }
        public string Labels { get; set; }
        public string Subject { get; set; }

        public EmailInfo(string id) => Id = id;
    }
}
