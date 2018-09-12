namespace TomitaSharp.Runtime
{
    public class Edge
    {
        public Node PrevNode { get; set; }

        public ParseExplanation Meta { get; set; }

        public override string ToString()
        {
            return "[ " + PrevNode.ToString() + " ] <- ";
        }
    }
}
