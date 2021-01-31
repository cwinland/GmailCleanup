using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailCleanup
{
    public static class GCreds
    {
        private const string APPLICATION_NAME = "GmailCleanup";
        private static readonly string[] scopes = { GmailService.Scope.GmailModify, };
        private const string CRED_FILE = "gmail_gmailCleanup.json";
        private const string SECRETS = "client_secrets.json";
        private const string QUOTE = "\"";
        private const string REDIRECT_URI1 = @"http://localhost";
        private const string REDIRECT_URI2 = @"urn:ietf:wg:oauth:2.0:oob";
        private const string AUTH_URI = @"https://accounts.google.com/o/oauth2/auth";
        private const string TOKEN_URI = @"https://accounts.google.com/o/oauth2/token";

        public static FileInfo CreateCredsConfig(string clientId, string clientSecret, bool overwrite = false)
        {
            if (overwrite || !File.Exists(SECRETS))
            {
                using var file = File.CreateText(SECRETS);
                file.WriteLine("{");
                file.WriteLine($"{QUOTE}Installed{QUOTE}");
                file.WriteLine($"{QUOTE}client_id{QUOTE}: {QUOTE}{clientId}{QUOTE}");
                file.WriteLine($"{QUOTE}client_secret{QUOTE}: {QUOTE}{clientSecret}{QUOTE}");
                file.WriteLine(
                    $"{QUOTE}redirect_uris{QUOTE}: [{QUOTE}{REDIRECT_URI1}{QUOTE}, {QUOTE}{REDIRECT_URI2}{QUOTE}]");
                file.WriteLine($"{QUOTE}auth_uri{QUOTE}: {QUOTE}{AUTH_URI}{QUOTE}");
                file.WriteLine($"{QUOTE}token_uri{QUOTE}: {QUOTE}{TOKEN_URI}{QUOTE}");
                file.WriteLine("}");
                file.Flush();
                file.Close();
            }

            return new FileInfo(SECRETS);
        }

        public static UserCredential Authorize()
        {
            using var stream = new FileStream(SECRETS, FileMode.Open, FileAccess.Read);

            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, $".credentials\\{CRED_FILE}");

            return GoogleWebAuthorizationBroker
                   .AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                                   scopes,
                                   "user",
                                   CancellationToken.None,
                                   new FileDataStore(credPath, true))
                   .Result;
        }

        public static GmailService Connect(UserCredential credential = null) => new GmailService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential ?? Authorize(), ApplicationName = APPLICATION_NAME,
            });
    }
}
