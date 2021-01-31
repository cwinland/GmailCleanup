using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace GmailCleanup
{
    /// <summary>
    /// Uses reflection to get the raw content out of a MailMessage.
    /// </summary>
    public static class MailMessageExtensions
    {
        private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly Type mailWriter = typeof(SmtpClient).Assembly.GetType("System.Net.Mail.MailWriter");

        private static readonly ConstructorInfo mailWriterConstructor =
            mailWriter.GetConstructor(FLAGS, null, new[] { typeof(Stream), }, null);

        private static readonly MethodInfo closeMethod = mailWriter.GetMethod("Close", FLAGS);
        private static readonly MethodInfo sendMethod = typeof(MailMessage).GetMethod("Send", FLAGS);

        /// <summary>
        /// A little hack to determine the number of parameters that we
        /// need to pass to the SaveMethod.
        /// </summary>
        private static readonly bool isRunningInDotNetFourPointFive = sendMethod.GetParameters().Length == 3;

        /// <summary>
        /// The raw contents of this MailMessage as a MemoryStream.
        /// </summary>
        /// <param name="self">The caller.</param>
        /// <returns>A MemoryStream with the raw contents of this MailMessage.</returns>
        public static MemoryStream RawMessage(this MailMessage self)
        {
            var result = new MemoryStream();
            var writer = mailWriterConstructor.Invoke(new object[] { result, });
            sendMethod.Invoke(self,
                              FLAGS,
                              null,
                              isRunningInDotNetFourPointFive
                                  ? new[] { writer, true, true, }
                                  : new[] { writer, true, },
                              CultureInfo.CurrentCulture);
            result = new MemoryStream(result.ToArray());
            closeMethod.Invoke(writer, FLAGS, null, Array.Empty<object>(), CultureInfo.CurrentCulture);

            return result;
        }

        public static string RawString(this MailMessage self)
        {
            using var streamReader = new StreamReader(RawMessage(self));

            return streamReader.ReadToEnd();
        }
    }
}
