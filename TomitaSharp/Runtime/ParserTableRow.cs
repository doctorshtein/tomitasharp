using System.Collections.Generic;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Runtime
{
    public class ParserTableRow
    {
        public int StateId { get; internal set; }

        public IReadOnlyDictionary<GrammarRuleToken, int> ShiftActionsMap { get; internal set; }

        public ISet<int> ReduceRulesIds { get; internal set; }
    }
}
