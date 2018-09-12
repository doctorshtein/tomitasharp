using System.Collections.Generic;
using System.Linq;

namespace TomitaSharp.Generation
{
    public class ParserConstructionItemSet
    {
        private int? _hashCode = null;

        public ISet<GrammarRuleOffset> RuleOffsets { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ParserConstructionItemSet otherSet))
            {
                return false;
            }

            return RuleOffsets.SetEquals(otherSet.RuleOffsets);
        }

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                unchecked
                {
                    int hash = 53;
                    foreach (var offset in RuleOffsets.OrderBy(o => o.Rule.Id).ThenBy(o => o.Offset))
                    {
                        hash = hash * 486187739 + offset.GetHashCode();
                    }
                    _hashCode = hash;
                }
            }

            return _hashCode.Value;
        }
    }
}
