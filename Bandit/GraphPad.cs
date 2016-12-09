using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NeoSmart.Bandit
{
    struct BinaryResult
    {
        public readonly long Outcome1;
        public readonly long Outcome2;

        public BinaryResult(long outcome1, long outcome2)
        {
            Outcome1 = outcome1;
            Outcome2 = outcome2;
        }
    }

    static class GraphPad
    {
        static public decimal ChiSquare(BinaryResult variant1, BinaryResult variant2)
        {
            if (variant1.Outcome1 == 0 || variant2.Outcome1 == 0 || variant1.Outcome2 == 0 || variant2.Outcome2 == 0)
            {
                return 0;
            }

            var request = new WebClient();
            var parameters = new NameValueCollection
            {
                {"Outcome1", "outcome1"},
                {"Outcome2", "outcome2"},
                {"Group1", "group1"},
                {"Group2", "group2"},
                {"Test", "ChiWith"},
                {"Tails", "2"},

                {"A", variant1.Outcome1.ToString()},
                {"B", variant1.Outcome2.ToString()},
                {"C", variant2.Outcome1.ToString()},
                {"D", variant2.Outcome2.ToString()}
            };

            try
            {
                var result =
                    Encoding.UTF8.GetString(request.UploadValues("http://graphpad.com/quickcalcs/contingency2/", "POST",
                        parameters));
                result = Regex.Replace(result, @"\s+", " ", RegexOptions.Multiline);
                var rMatch = Regex.Match(result, "The two-tailed P value equals (\\d+\\.\\d+)", RegexOptions.Multiline);
                return decimal.Parse(rMatch.Groups[1].Captures[0].Value.ToString());
            }
            catch
            {
                return -1;
            }
        }
    }
}
