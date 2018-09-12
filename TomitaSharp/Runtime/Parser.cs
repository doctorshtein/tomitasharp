using System.Collections.Generic;
using System.Linq;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Runtime
{
    public class Parser
    {
        private ParserTable Table { get; set; }

        public Parser(ParserTable table)
        {
            Table = table;
        }

        public void Parse(string[] strTokens)
        {
            var tokens = strTokens.Select(s => new GrammarRuleToken { IsTerminal = true, Value = s.Trim() }).ToList();

            int curTokenIndex = 0;
            var graph = new StackGraph();
            graph.Heads.Add(new Node(null, 0));

            while (curTokenIndex <= tokens.Count)
            {
                Reduce(graph);
                if (curTokenIndex < tokens.Count)
                {
                    Shift(graph, tokens[curTokenIndex]);
                }
                curTokenIndex++;
            }
        }

        private void Reduce(StackGraph graph)
        {
            var newHeads = new List<Node>();
            var curHeadList = graph.Heads;

            while (curHeadList.Count > 0)
            {
                var headsWithShifts = curHeadList
                    .Where(h => Table[h.State].ShiftActionsMap.Count > 0)
                    .ToList();

                newHeads.AddRange(headsWithShifts);

                var reducedHeads = curHeadList
                    .Where(h => Table[h.State].ReduceRulesIds.Count > 0)
                    .SelectMany(h => Table[h.State].ReduceRulesIds
                        .Select(r => new { Head = h, Rule = Table.Grammar.Rules[r] }))
                    .SelectMany(t => FindAllPathsBack(t.Head, t.Rule.SourceTokens.Count + 1)
                        .Select(p => new { Head = t.Head, Rule = t.Rule, Path = p }))
                    .Select(t => CreateReduceNode(t.Path, t.Rule))
                    .Where(rn => rn != null)
                    .ToList();

                curHeadList = reducedHeads;
            }

            newHeads = CombineNodes(newHeads).ToList();
            graph.Heads = newHeads;
        }

        private void Shift(StackGraph graph, GrammarRuleToken token)
        {
            List<Node> resultNodes = new List<Node>(graph.Heads.Count);
            foreach (var head in graph.Heads)
            {
                if (Table[head.State].ShiftActionsMap.TryGetValue(token, out int nextState))
                {
                    var newNode = new Node(token, nextState);
                    newNode.PrevNodes.Add(new Edge { PrevNode = head, Meta = new ParseExplanation { ResultToken = token, RuleApplied = null, ChildrenExplained = null } });
                    resultNodes.Add(newNode);
                }
                else if (Table.Grammar.IsSkipGrammar && Table[head.State].ShiftActionsMap.TryGetValue(Table.Grammar.SkipToken, out int skipState))
                {
                    var newNode = new Node(token, nextState);
                    newNode.PrevNodes.Add(new Edge { PrevNode = head, Meta = new ParseExplanation { ResultToken = token, RuleApplied = null, ChildrenExplained = null } });
                    resultNodes.Add(newNode);
                }
            }

            graph.Heads = CombineNodes(resultNodes).ToList();
        }

        private class StackFrame
        {
            public Node currentNode;
            public int currentDepth;
            public int nextParentId;
        }

        private class NodeWithIncomingEdge
        {
            public Node targetNode;
            public Edge incomingEdge;
        }

        private class PathBack
        {
            public Node previousNode;
            public List<NodeWithIncomingEdge> pathNodes;
        }

        private List<PathBack> FindAllPathsBack(Node head, int length)
        {
            List<PathBack> paths = new List<PathBack>();

            Stack<StackFrame> stack = new Stack<StackFrame>();
            stack.Push(new StackFrame { currentDepth = 1, currentNode = head, nextParentId = 0 });
            while (stack.Count > 0)
            {
                var frame = stack.Peek();
                if (frame.currentDepth == length)
                {
                    var pathBack = new PathBack
                    {
                        previousNode = frame.currentNode,
                        pathNodes = stack.Skip(1).Select(fr => new NodeWithIncomingEdge { targetNode = fr.currentNode, incomingEdge = fr.currentNode.PrevNodes[fr.nextParentId] }).ToList()
                    };
                    paths.Add(pathBack);

                    stack.Pop();
                    if (stack.Count > 0)
                    {
                        stack.Peek().nextParentId++;
                    }
                    continue;
                }
                else
                {
                    if (frame.nextParentId >= frame.currentNode.PrevNodes.Count)
                    {
                        stack.Pop();
                        if (stack.Count > 0)
                        {
                            stack.Peek().nextParentId++;
                        }
                        continue;
                    }
                    else
                    {
                        stack.Push(new StackFrame { currentDepth = frame.currentDepth + 1, currentNode = frame.currentNode.PrevNodes[frame.nextParentId].PrevNode, nextParentId = 0 });
                        continue;
                    }
                }
            }

            return paths;
        }

        private Node CreateReduceNode(PathBack pathBack, GrammarRule rule)
        {
            if (!Table[pathBack.previousNode.State].ShiftActionsMap.TryGetValue(rule.ResultToken, out int newState))
            {
                // after we reduced, there is no non-terminal-shift available. let's return null as a new node to signify no parse available
                return null;
            }

            var result = new Node(rule.ResultToken, newState);
            result.PrevNodes.Add(new Edge { PrevNode = pathBack.previousNode, Meta = BuildParseRuleExplanation(pathBack, rule) });

            return result;
        }

        private IEnumerable<Node> CombineNodes(IEnumerable<Node> nodes)
        {
            return nodes
                .GroupBy(rn => rn.Token)
                .SelectMany(g => g
                    .GroupBy(rn => rn.State)
                    .Select(sg => new Node(g.Key, sg.Key)
                    {
                        PrevNodes = sg
                            .SelectMany(rn => rn.PrevNodes)
                            .ToList()
                    }));
        }

        private ParseExplanation BuildParseRuleExplanation(PathBack pathBack, GrammarRule rule)
        {
            return new ParseExplanation
            {
                RuleApplied = rule,
                ResultToken = rule.ResultToken,
                ChildrenExplained = pathBack.pathNodes.Select(pn => pn.incomingEdge.Meta).ToList()
            };
        }
    }
}
