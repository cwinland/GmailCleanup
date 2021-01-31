# Gmail Cleanup

API to Query and Automatically Cleanup your Google Gmail Email account.

## Features (In Progress)

1. Connect to gmail account.
2. Create Credential File / Client connection information.
3. Get email headers by query (same query used in the Google Gmail search bar).
4. Get email headers by custom options (After, Before, older, newer, from, to, cc, bcc, subject, label, size, smaller, larger, etc.).
5. Paged Results.
6. Get list of labels
7. Get Id List based on query
8. Delete by query
9. Create and Remove labels
10. Insert Message (put in the inbox without sending).

## Examples

### Delete Old Emails

Running this command deletes 1000 emails that are smaller than 1MB and older than 3 months old, only in the forums or promotions category.

```c#
emailService.DeleteByQuery("smaller:1M older_than:3m (in:forums OR in:promotions)", 1000);
```

### Get by Label

Running this command gets all email headers with the label gmailtest

```c#
emailService.AdvancedQuery = "label:gmailtest";
var emails = emailService.Get();
```

### Get 100 emails in the last 2 days

Running this command gets up to 100 emails received in the last 2 days.

```c#
emailService.SearchStrings.Add(
                new KeyValuePair<SearchStringOperators, string>(SearchStringOperators.NewerThan, "2d"));
            emailService.AdvancedQuery = string.Empty;
            var list = emailService.Get(1, 100).OrderBy(x => x.Date);
```

## License

   Copyright 2021 Christopher Winland

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
