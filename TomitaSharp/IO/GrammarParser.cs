using System;
using System.Collections.Generic;
using System.Linq;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.IO
{
    public class GrammarParser
    {
        private static readonly char[] _lineSeparators = new char[] { '\r', '\n' };
        private static readonly char[] _tokenSeparators = new char[] { ' ' };
        private static readonly string[] _rulePortionsSeparators = new string[] { "\t", "->", "→", "=>" };
        private static readonly string[] _commentPrefix = new string[] { "//", "#" };

        public static Grammar Parse(string grammarAsText)
        {
            var lines =
                grammarAsText
                .Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Where(l => !_commentPrefix.Any(prefix => l.StartsWith(prefix)))
                .ToList();

            Dictionary<string, GrammarRuleToken> knownTokens = new Dictionary<string, GrammarRuleToken>();

            var skipDefinition = lines[0].Split(_tokenSeparators, StringSplitOptions.RemoveEmptyEntries);
            bool isSkipGrammar = bool.Parse(skipDefinition[0]);
            GrammarRuleToken skipToken = isSkipGrammar ? new GrammarRuleToken { IsTerminal = true, Value = skipDefinition[1].Trim() } : null;

            var acceptingTokens = lines[1].Split(_tokenSeparators, StringSplitOptions.RemoveEmptyEntries).Select(t => ConvertStringToToken(t, knownTokens)).ToList();
            var rules = lines.Skip(2).Select((str, index) => ConvertStringToRule(str, index, knownTokens)).ToList();

            return new Grammar
            {
                FinalTokens = new HashSet<GrammarRuleToken>(acceptingTokens),
                Rules = rules,
                IsSkipGrammar = isSkipGrammar,
                SkipToken = skipToken
            };
        }

        private static GrammarRule ConvertStringToRule(string ruleString, int ruleId, Dictionary<string, GrammarRuleToken> knownTokens)
        {
            var rulePortions = ruleString.Split(_rulePortionsSeparators, StringSplitOptions.None);
            if (rulePortions.Length != 2)
            {
                throw new InvalidOperationException();
            }

            var targetToken = ConvertStringToToken(rulePortions[0], knownTokens);
            var sourceTokens = rulePortions[1].Split(_tokenSeparators, StringSplitOptions.RemoveEmptyEntries).Select(t => ConvertStringToToken(t, knownTokens)).ToList();

            GrammarRule rule = new GrammarRule
            {
                Id = ruleId,
                ResultToken = targetToken,
                SourceTokens = sourceTokens
            };

            return rule;
        }

        private static GrammarRuleToken ConvertStringToToken(string str, Dictionary<string, GrammarRuleToken> knownTokens)
        {
            str = str.Trim();

            if (knownTokens.TryGetValue(str, out var existingToken))
            {
                return existingToken;
            }

            var newToken = new GrammarRuleToken
            {
                IsTerminal = str.Any(c => char.IsLower(c) || !char.IsUpper(c)),
                Value = str
            };

            knownTokens.Add(str, newToken);
            return newToken;
        }
    }
}
