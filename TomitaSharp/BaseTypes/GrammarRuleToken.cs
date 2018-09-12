namespace TomitaSharp.BaseTypes
{
    public class GrammarRuleToken
    {
        public bool IsTerminal { get; internal set; }

        public string Value { get; internal set; }

        public virtual bool Matches(GrammarRuleToken otherToken)
        {
            return (IsTerminal == otherToken.IsTerminal) && (Value == otherToken.Value);
        }

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GrammarRuleToken otherToken))
            {
                return false;
            }

            return (IsTerminal == otherToken.IsTerminal) && (Value == otherToken.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 486187739 + Value.GetHashCode();
                hash = hash * 486187739 + (IsTerminal ? 1 : 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return Value;
        }
        #endregion
    }
}
