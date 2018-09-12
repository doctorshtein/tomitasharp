using System.Collections.Generic;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Generation
{
    class ItemSetGraphInformation
    {
        public int Id { get; set; }
        public Dictionary<GrammarRuleToken, int> ShiftToIds { get; } = new Dictionary<GrammarRuleToken, int>();
    }
}
