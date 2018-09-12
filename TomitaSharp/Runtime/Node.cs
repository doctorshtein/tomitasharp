using System.Collections.Generic;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Runtime
{
    public class Node
    {
        public List<Edge> PrevNodes { get; set; } = new List<Edge>();

        public GrammarRuleToken Token { get; set; }

        public int State { get; set; }

        public Node(GrammarRuleToken token, int state)
        {
            Token = token;
            State = state;
        }


        public override string ToString()
        {
            return (Token?.ToString() ?? "null") + " <- " + State.ToString();
        }
    }
}
