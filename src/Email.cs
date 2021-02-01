using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GmailCleanup.Enums;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace GmailCleanup
{
    public class Email
    {
        private string EmailId { get; }
        private const string SEPARATOR = ", ";

        public string AdvancedQuery { get; set; } = "smaller:1M older_than:3m (in:forums OR in:promotions)";

        public List<KeyValuePair<SearchDateOperators, DateTime>> SearchDates { get; set; } =
            new List<KeyValuePair<SearchDateOperators, DateTime>>();

        public List<KeyValuePair<SearchStringOperators, string>> SearchStrings { get; set; } =
            new List<KeyValuePair<SearchStringOperators, string>>();

        protected UserCredential Credential { get; }

        protected GmailService Service { get; }

        public string EmailAddress => Service.Users.GetProfile(EmailId).Execute().EmailAddress;

        public Email(string emailId, UserCredential credential = null, GmailService service = null)
        {
            EmailId = emailId;
            Credential = credential ?? GCreds.Authorize();
            Service = service ?? GCreds.Connect(Credential);
        }

        public List<Label> GetLabels() => Service.Users.Labels.List(EmailId).Execute().Labels.ToList();

        public List<string> GetIdList(int page = 1, int pageSize = 100)
        {
            var currentPage = 1;
            var idList = new List<string>();
            var mailListRequest = Service.Users.Messages.List(EmailId);
            mailListRequest.Q = string.IsNullOrEmpty(AdvancedQuery)
                ? GetQuery()
                : AdvancedQuery;
            mailListRequest.IncludeSpamTrash = false;
            mailListRequest.MaxResults = pageSize;

            while (true)
            {
                //get our emails
                var emailListResponse = mailListRequest.Execute();

                mailListRequest.PageToken = emailListResponse.NextPageToken;

                if (currentPage < page)
                {
                    currentPage++;

                    continue;
                }

                if (currentPage > page ||
                    emailListResponse.Messages == null)
                {
                    break;
                }

                return emailListResponse.Messages.Select(x => x.Id).ToList();
            }

            return idList;
        }

        public List<EmailInfo> Get(int page = 1, int pageSize = 25)
        {
            var currentPage = 1;
            var emailInfoList = new List<EmailInfo>();
            var mailListRequest = Service.Users.Messages.List(EmailId);
            mailListRequest.Q = string.IsNullOrEmpty(AdvancedQuery)
                ? GetQuery()
                : AdvancedQuery;
            mailListRequest.IncludeSpamTrash = false;
            mailListRequest.MaxResults = pageSize;

            while (true)
            {
                //get our emails
                var emailListResponse = mailListRequest.Execute();

                mailListRequest.PageToken = emailListResponse.NextPageToken;

                if (currentPage < page)
                {
                    currentPage++;

                    continue;
                }

                if (currentPage > page ||
                    emailListResponse.Messages == null)
                {
                    break;
                }

                foreach (var email in emailListResponse.Messages)
                {
                    if (email == null)
                    {
                        continue;
                    }

                    var values = email.LabelIds?.ToList() ?? new List<string>();
                    var labels = email.LabelIds == null ? string.Empty : string.Join(SEPARATOR, values);
                    var emailInfoRequest = Service.Users.Messages.Get(EmailId, email.Id);
                    var emailInfoResponse = emailInfoRequest.Execute();

                    var from = string.Empty;
                    DateTime? date = null;
                    var subject = string.Empty;

                    if (emailInfoResponse != null)
                    {
                        //loop through the headers to get from,date,subject, body
                        foreach (var mParts in emailInfoResponse.Payload.Headers)
                        {
                            switch (mParts.Name)
                            {
                                case "Date":
                                    if (DateTime.TryParse(mParts.Value.Split('(', ')')[0].Trim(),
                                                          out var emailDate))
                                    {
                                        date = emailDate;
                                    }
                                    else
                                    {
                                        Console.WriteLine(mParts.Value);
                                    }

                                    break;
                                case "From":
                                    from = mParts.Value;

                                    break;
                                case "Subject":
                                    subject = mParts.Value;

                                    break;
                            }
                        }
                    }

                    if (IncludeNullDates || date != null)
                    {
                        emailInfoList.Add(new EmailInfo(email.Id)
                        {
                            Date = date, From = from, Subject = subject, Labels = labels,
                        });
                    }
                }

                break;
            }

            return emailInfoList;
        }

        public void DeleteAsync(IEnumerable<string> idList)
        {
            var maxThread = new SemaphoreSlim(5);

            foreach (var id in idList)
            {
                maxThread.Wait();
                Task.Factory.StartNew(() => { Service.Users.Messages.Trash(EmailId, id).Execute(); },
                                      TaskCreationOptions.LongRunning)
                    .ContinueWith(task => maxThread.Release());
            }
        }

        public void DeleteByQuery(string query, int max = 500)
        {
            AdvancedQuery = query;
            var ids = new List<string>();
            var currentPage = 1;

            while (true)
            {
                var list = GetIdList(currentPage++, 500);

                if (list.Count == 0 ||
                    ids.Count >= Math.Min(max, 10000))
                {
                    break;
                }

                ids.AddRange(list);
            }

            DeleteAsync(ids.Distinct().ToList());
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
                          .Replace('+', '-')
                          .Replace('/', '_')
                          .Replace("=", "");
        }

        public Label CreateGmailLabel(string labelName)
        {
            Label result = null;
            var labels = Service.Users.Labels.List(EmailId).Execute().Labels.ToList();
            labels.ForEach(label =>
                           {
                               if (label.Name == labelName)
                               {
                                   result = label;
                               }
                           });

            if (result != null)
            {
                return result;
            }

            return Service.Users.Labels.Create(new Label
                                               {
                                                   Name = labelName,
                                                   LabelListVisibility = "labelShow",
                                                   MessageListVisibility = "show",
                                               },
                                               EmailId)
                          .Execute();
        }

        public string DeleteLabel(string id) => Service.Users.Labels.Delete(EmailId, id).Execute();

        public Message InsertMessage(Message message) => Service.Users.Messages.Insert(message, EmailId).Execute();

        public static Message CreateMessageObject(string to, string from, string subject, string bodyText)
        {
            var msg = new MailMessage(from, to, subject, bodyText);

            return new Message { Raw = Base64UrlEncode(msg.RawString()), };
        }

        private string GetQuery()
        {
            var builder = new StringBuilder();

            SearchDates.ForEach(d => builder.Append($"{d.Key.Value}:{d.Value:MM/dd/yyyy} "));
            SearchStrings.ForEach(s => builder.Append($"{s.Key.Value}:{s.Value} "));

            return builder.ToString();
        }

        private bool IncludeNullDates => SearchDates.Count == 0 ||
                                         SearchStrings.All(x => x.Key != SearchStringOperators.OlderThan) ||
                                         SearchStrings.All(x => x.Key != SearchStringOperators.NewerThan);
    }
}
