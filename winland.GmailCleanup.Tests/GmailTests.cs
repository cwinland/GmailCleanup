using GmailCleanup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace winland.GmailCleanup.Tests
{
    [TestClass]
    public class GmailTests
    {
        private Email emailService;

        [TestInitialize]
        public void Init() =>

            // Sign up for google client ids at: https://console.developers.google.com/
            //            GCreds.CreateCredsConfig("ID GOES HERE", "SECRET GOES HERE");
            emailService = new Email("me");
    }
}
