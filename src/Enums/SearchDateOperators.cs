using EnhancedEnum;
using EnhancedEnum.Attributes;

namespace GmailCleanup.Enums
{
    public class SearchDateOperators : EnhancedEnum<string, SearchDateOperators>
    {
        [Value("after")]public static readonly SearchDateOperators After = new SearchDateOperators();
        [Value("before")]public static readonly SearchDateOperators Before = new SearchDateOperators();
        [Value("older")]public static readonly SearchDateOperators Older = new SearchDateOperators();
        [Value("newer")]public static readonly SearchDateOperators Newer = new SearchDateOperators();
    }
}
