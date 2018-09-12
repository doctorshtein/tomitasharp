using System;
using System.Linq;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Generation
{
    public class GrammarRuleOffset
    {
        public GrammarRule Rule { get; set; }

        public int Offset { get; set; }

        public GrammarRuleToken NextToken
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException();
                }

                if (IsFinal)
                {
                    return null;
                }

                return Rule.SourceTokens[Offset];
            }
        }

        public bool IsValid => Offset >= 0 && Offset <= Rule.SourceTokens.Count;

        public bool IsFinal => Offset == Rule.SourceTokens.Count;

        public GrammarRuleOffset Shift()
        {
            return new GrammarRuleOffset { Rule = this.Rule, Offset = this.Offset + 1 };
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GrammarRuleOffset otherOffset))
            {
                return false;
            }

            return Rule.Equals(otherOffset.Rule) && (Offset == otherOffset.Offset);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 31;
                hash = hash * 486187739 + Rule.GetHashCode();
                hash = hash * 486187739 + Offset;
                return hash;
            }
        }

        public override string ToString()
        {
            return Rule.Id.ToString() + ": " + Rule.ResultToken.ToString() + " => " + string.Join(" ", Rule.SourceTokens.Take(Offset)) + " * " + string.Join(" ", Rule.SourceTokens.Skip(Offset));
        }
    }
}
