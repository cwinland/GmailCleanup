using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GmailCleanup;
using GmailCleanup.Enums;

namespace winland.GmailCleanup.Console
{
    class Program
    {
        private readonly Email emailService = new Email("me");

        static void Main(string[] args) { }

        public void GetEmails(int page, int pageSize)
        {
            var emails = emailService.Get(page, pageSize);
        }

        public void CheckPages()
        {
            const int PAGE_SIZE = 20;
            const int ITERATIONS = 5;
            var list = new List<EmailInfo>();

            for (var i = 0; i < ITERATIONS; i++)
            {
                list.AddRange(emailService.Get(i + 1, PAGE_SIZE));
            }
        }

        public void CheckPages_Duplicates()
        {
            const int PAGE_SIZE = 5;
            const int ITERATIONS = 2;
            var list = new List<EmailInfo>();

            for (var i = 0; i < ITERATIONS; i++)
            {
                list.AddRange(emailService.Get(1, PAGE_SIZE));
            }
        }

        public void Test_Query_AfterDate()
        {
            var testDate = DateTime.Today.AddDays(-3);
            emailService.SearchDates.Add(
                new KeyValuePair<SearchDateOperators, DateTime>(SearchDateOperators.After, testDate));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
        }

        public void Test_Query_BeforeDate()
        {
            var testDate = DateTime.Today.AddDays(-3);
            emailService.SearchDates.Add(
                new KeyValuePair<SearchDateOperators, DateTime>(SearchDateOperators.Before, testDate));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
        }

        public void Test_Query_BeforeRelativeDays(int days)
        {
            var testDate = DateTime.Today.AddDays((days - 1) * -1);
            emailService.SearchStrings.Add(
                new KeyValuePair<SearchStringOperators, string>(SearchStringOperators.OlderThan, $"{days}d"));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
        }

        public void Test_Query_AfterRelativeDays(int days)
        {
            var testDate = DateTime.Today.AddDays((days - 1) * -1);
            emailService.SearchStrings.Add(
                new KeyValuePair<SearchStringOperators, string>(SearchStringOperators.NewerThan, $"{days}d"));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
        }

        public void GetIds()
        {
            var query = "smaller:1M older_than:3m (in:forums OR in:promotions)";
            emailService.AdvancedQuery = query;

            emailService.GetIdList();
        }

        public void Delete()
        {
            var labelName = "GmailTest";
            var label = emailService.CreateGmailLabel(labelName);

            var message =
                Email.CreateMessageObject($"{emailService.EmailAddress}",
                                          $"{emailService.EmailAddress}",
                                          "Test Message",
                                          "Test Gmail Service");
            message.LabelIds = new List<string> { label.Id, };
            var msg = emailService.InsertMessage(message);
            emailService.AdvancedQuery = "label:gmailtest";
            var emails = emailService.Get();

            emailService.DeleteAsync(new List<string> { msg.Id, });
            Thread.Sleep(1000);
            emailService.Get();
            emailService.DeleteLabel(label.Id);
        }
    }
}
