using System.Collections.Generic;
using System.Text;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Runtime
{
    public class ParseExplanation
    {
        public GrammarRule RuleApplied { get; set; }

        public GrammarRuleToken ResultToken { get; set; }

        public List<ParseExplanation> ChildrenExplained { get; set; }

        public override string ToString()
        {
            return _stringValue ?? (_stringValue = GetShortString(new StringBuilder()).ToString());
        }

        private string _stringValue = null;

        private StringBuilder GetShortString(StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(ResultToken.Value);

            if (ChildrenExplained != null && ChildrenExplained.Count > 0)
            {
                builder.Append('(');
                foreach (var child in ChildrenExplained)
                {
                    child.GetShortString(builder);
                }
                builder.Append(" )");
            }

            return builder;
        }
    }
}
