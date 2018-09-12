using System.Collections.Generic;

namespace TomitaSharp.BaseTypes
{
    public class Grammar
    {
        public bool IsSkipGrammar { get; internal set; }

        public GrammarRuleToken SkipToken { get; internal set; }

        public IReadOnlyList<GrammarRule> Rules { get; internal set; }

        public ISet<GrammarRuleToken> FinalTokens { get; internal set; }

        public ISet<GrammarRuleToken> GetAllUniqueTokens()
        {
            HashSet<GrammarRuleToken> result = new HashSet<GrammarRuleToken>();
            result.UnionWith(FinalTokens);

            foreach (var rule in Rules)
            {
                result.Add(rule.ResultToken);
                result.UnionWith(rule.SourceTokens);
            }

            return result;
        }
    }
}
