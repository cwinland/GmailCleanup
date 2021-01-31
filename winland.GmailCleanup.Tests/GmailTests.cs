using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using GmailCleanup;
using GmailCleanup.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace winland.GmailCleanup.Tests
{
    [TestClass]
    public class GmailTests
    {
        private Email emailService;

        [TestInitialize]
        public void Init()
        {
            // Sign up for google client ids at: https://console.developers.google.com/
            GCreds.CreateCredsConfig("ID GOES HERE", "SECRET GOES HERE");

            emailService = new Email();
        }

        [TestMethod]
        public void CanConnect() => GCreds.Connect().Name.Should().NotBeNullOrEmpty();

        [TestMethod]
        [DataRow(1, 10)]
        [DataRow(2, 10)]
        [DataRow(5, 5)]
        [DataRow(2, 75)]
        public void GetEmails(int page, int pageSize)
        {
            var emails = emailService.Get(page, pageSize);
            emails.Should().NotBeNull();
            emails.Count.Should().BeLessOrEqualTo(pageSize);
            emails.Count.Should().BeGreaterOrEqualTo(1);
            emails.Should().OnlyHaveUniqueItems(x => x.Id);
        }

        [TestMethod]
        public void CheckPages()
        {
            const int PAGE_SIZE = 20;
            const int ITERATIONS = 5;
            var list = new List<EmailInfo>();

            for (var i = 0; i < ITERATIONS; i++)
            {
                list.AddRange(emailService.Get(i + 1, PAGE_SIZE));
            }

            list.Count.Should().BeGreaterThan(PAGE_SIZE * (ITERATIONS - 1));
            list.Should().OnlyHaveUniqueItems(x => x.Id);
        }

        [TestMethod]
        public void CheckPages_Duplicates()
        {
            const int PAGE_SIZE = 5;
            const int ITERATIONS = 2;
            var list = new List<EmailInfo>();

            for (var i = 0; i < ITERATIONS; i++)
            {
                list.AddRange(emailService.Get(1, PAGE_SIZE));
            }

            list.Count.Should().BeGreaterThan(PAGE_SIZE * (ITERATIONS - 1));
            list.GroupBy(x => x.Id).Count().Should().NotBe(list.Count);
        }

        [TestMethod]
        public void Test_Query_AfterDate()
        {
            var testDate = DateTime.Today.AddDays(-3);
            emailService.SearchDates.Add(
                new KeyValuePair<SearchDateOperators, DateTime>(SearchDateOperators.After, testDate));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
            list.Any(x => x.Date < testDate).Should().BeFalse();
            list.Any(x => x.Date >= testDate).Should().BeTrue();
        }

        [TestMethod]
        public void Test_Query_BeforeDate()
        {
            var testDate = DateTime.Today.AddDays(-3);
            emailService.SearchDates.Add(
                new KeyValuePair<SearchDateOperators, DateTime>(SearchDateOperators.Before, testDate));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
            list.Any(x => x.Date >= testDate).Should().BeFalse();
            list.Any(x => x.Date < testDate).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(12)]
        public void Test_Query_BeforeRelativeDays(int days)
        {
            var testDate = DateTime.Today.AddDays((days - 1) * -1);
            emailService.SearchStrings.Add(
                new KeyValuePair<SearchStringOperators, string>(SearchStringOperators.OlderThan, $"{days}d"));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
            list.Any(x => x.Date >= testDate).Should().BeFalse();
            list.Any(x => x.Date < testDate).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(12)]
        public void Test_Query_AfterRelativeDays(int days)
        {
            var testDate = DateTime.Today.AddDays((days - 1) * -1);
            emailService.SearchStrings.Add(
                new KeyValuePair<SearchStringOperators, string>(SearchStringOperators.NewerThan, $"{days}d"));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
            list.Any(x => x.Date < testDate).Should().BeFalse();
            list.Any(x => x.Date >= testDate).Should().BeTrue();
        }

        [TestMethod]
        public void GetIds()
        {
            var query = "smaller:1M older_than:3m (in:forums OR in:promotions)";
            emailService.AdvancedQuery = query;

            emailService.GetIdList().Count.Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void Delete()
        {
            var labelName = "GmailTest";
            var label = emailService.CreateGmailLabel(labelName);
            Console.WriteLine(label.Id);

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
            emailService.Get().Count.Should().BeLessThan(emails.Count);
            emailService.DeleteLabel(label.Id);
        }

        [TestMethod]
        public void Delete_old_Emails() =>
            emailService.DeleteByQuery("smaller:1M older_than:3m (in:forums OR in:promotions)", 1000);
    }
}
