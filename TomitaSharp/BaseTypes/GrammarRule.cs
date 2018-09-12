using System.Collections.Generic;
using System.Linq;

namespace TomitaSharp.BaseTypes
{
    public class GrammarRule
    {
        private int? _hashCode = null;

        public int Id { get; internal set; }

        public IReadOnlyList<GrammarRuleToken> SourceTokens { get; internal set; }

        public GrammarRuleToken ResultToken { get; internal set; }

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GrammarRule otherRule))
            {
                return false;
            }

            return ResultToken.Equals(otherRule.ResultToken) && SourceTokens.SequenceEqual(otherRule.SourceTokens);
        }

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                unchecked
                {
                    int hash = 7;
                    hash = hash * 486187739 + ResultToken.GetHashCode();
                    foreach (var token in SourceTokens)
                    {
                        hash = hash * 486187739 + token.GetHashCode();
                    }
                    _hashCode = hash;
                }
            }

            return _hashCode.Value;
        }

        public override string ToString()
        {
            return Id.ToString() + ": " + ResultToken.ToString() + " => " + string.Join(" ", SourceTokens);
        }
        #endregion
    }
}
