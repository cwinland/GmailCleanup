using EnhancedEnum;
using EnhancedEnum.Attributes;

namespace GmailCleanup.Enums
{
    public class SearchStringOperators : EnhancedEnum<string, SearchStringOperators>
    {
        [Value("from")]public static readonly SearchStringOperators Sender = new SearchStringOperators();

        [Value("to")]public static readonly SearchStringOperators Recipient = new SearchStringOperators();

        [Description("Carbon Copy")]
        [Value("cc")]
        public static readonly SearchStringOperators CarbonCopy = new SearchStringOperators();

        [Description("Blind Carbon Copy")]
        [Value("bcc")]
        public static readonly SearchStringOperators BlindCarbonCopy = new SearchStringOperators();

        [Value("subject")]public static readonly SearchStringOperators Subject = new SearchStringOperators();

        [Value("label")]public static readonly SearchStringOperators Label = new SearchStringOperators();

        [Description("Older Than")]
        [Value("older_than")]
        public static readonly SearchStringOperators OlderThan = new SearchStringOperators();

        [Description("Newer Than")]
        [Value("newer_than")]
        public static readonly SearchStringOperators NewerThan = new SearchStringOperators();

        [Value("size")]public static readonly SearchStringOperators Size = new SearchStringOperators();
        [Value("smaller")]public static readonly SearchStringOperators Smaller = new SearchStringOperators();
        [Value("larger")]public static readonly SearchStringOperators Larger = new SearchStringOperators();
    }
}
