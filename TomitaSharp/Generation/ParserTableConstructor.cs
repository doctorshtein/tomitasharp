using System.Collections.Generic;
using System.Linq;
using TomitaSharp.BaseTypes;
using TomitaSharp.Runtime;

namespace TomitaSharp.Generation
{
    public class ParserTableConstructor
    {
        private Grammar _grammar = null;

        public ParserTableConstructor(Grammar grammar)
        {
            _grammar = grammar;
        }

        public static ParserTable Construct(Grammar grammar)
        {
            return new ParserTableConstructor(grammar).Construct();
        }

        private ParserTable Construct()
        {
            ParserTable result = new ParserTable()
            {
                Grammar = _grammar,
                StateRows = new List<ParserTableRow>()
            };

            var graph = ConstructStateTransitionGraph();

            var orderedStates = graph.OrderBy(kvp => kvp.Value.Id);
            foreach (var stateKvp in orderedStates)
            {
                var reducableRules = stateKvp.Key.RuleOffsets.Where(r => r.IsFinal).Select(r => r.Rule.Id);

                var row = new ParserTableRow
                {
                    StateId = stateKvp.Value.Id,
                    ShiftActionsMap = stateKvp.Value.ShiftToIds,
                    ReduceRulesIds = new HashSet<int>(reducableRules)
                };

                result.StateRows.Add(row);
            }

            return result;
        }

        private Dictionary<ParserConstructionItemSet, ItemSetGraphInformation> ConstructStateTransitionGraph()
        {
            ParserConstructionItemSet intialStateSet = GetInitialState();
            Dictionary<ParserConstructionItemSet, ItemSetGraphInformation> shiftGraph = new Dictionary<ParserConstructionItemSet, ItemSetGraphInformation>();
            shiftGraph.Add(intialStateSet, new ItemSetGraphInformation { Id = 0 });

            HashSet<ParserConstructionItemSet> processedStates = new HashSet<ParserConstructionItemSet>();
            Queue<ParserConstructionItemSet> setsToProcess = new Queue<ParserConstructionItemSet>();
            setsToProcess.Enqueue(intialStateSet);

            while (setsToProcess.Count > 0)
            {
                var curSet = setsToProcess.Dequeue();
                if (processedStates.Contains(curSet))
                {
                    continue;
                }

                var curStateInfo = shiftGraph[curSet];
                var targetStates = GetShiftedStates(curSet, intialStateSet);
                foreach (var targetState in targetStates)
                {
                    if (shiftGraph.TryGetValue(targetState.Value, out var existingTargetStateInfo))
                    {
                        curStateInfo.ShiftToIds.Add(targetState.Key, existingTargetStateInfo.Id);
                    }
                    else
                    {
                        int newStateId = shiftGraph.Count;
                        shiftGraph.Add(targetState.Value, new ItemSetGraphInformation { Id = newStateId });
                        curStateInfo.ShiftToIds.Add(targetState.Key, newStateId);

                        setsToProcess.Enqueue(targetState.Value);
                    }
                }

                processedStates.Add(curSet);
            }

            return shiftGraph;
        }

        private ParserConstructionItemSet GetInitialState()
        {
            var initialRules = _grammar.Rules.Where(r => _grammar.FinalTokens.Contains(r.ResultToken)).Select(r => new GrammarRuleOffset { Offset = 0, Rule = r }).ToList();
            var setEnclosure = CalculateItemSetEnclosure(initialRules);

            return new ParserConstructionItemSet
            {
                RuleOffsets = setEnclosure
            };
        }

        private Dictionary<GrammarRuleToken, ParserConstructionItemSet> GetShiftedStates(ParserConstructionItemSet currentSet, ParserConstructionItemSet initialSet)
        {
            var result = currentSet
                .RuleOffsets
                .Where(o => !o.IsFinal)
                .GroupBy(o => o.NextToken)
                .ToDictionary(g => g.Key, g => GetShiftedState(g, initialSet));

            if (_grammar.IsSkipGrammar)
            {
                result.Add(_grammar.SkipToken, initialSet);
            }

            return result;
        }

        private ParserConstructionItemSet GetShiftedState(IEnumerable<GrammarRuleOffset> offsets, ParserConstructionItemSet initialSet)
        {
            var shiftedSeedOffsets = offsets.Select(o => o.Shift()).Where(i => i.IsValid);
            var result = CalculateItemSetEnclosure(shiftedSeedOffsets);
            if (_grammar.IsSkipGrammar)
            {
                result.UnionWith(initialSet.RuleOffsets);
            }

            return new ParserConstructionItemSet
            {
                RuleOffsets = result
            };
        }

        private ISet<GrammarRuleOffset> CalculateItemSetEnclosure(IEnumerable<GrammarRuleOffset> items)
        {
            HashSet<GrammarRuleOffset> result = new HashSet<GrammarRuleOffset>(items);

            HashSet<GrammarRuleToken> processedNextTokens = new HashSet<GrammarRuleToken>();
            Queue<GrammarRuleToken> nextTokensQueue = new Queue<GrammarRuleToken>(result.Where(r => !r.IsFinal).Select(r => r.NextToken));

            while (nextTokensQueue.Count > 0)
            {
                var token = nextTokensQueue.Dequeue();
                if (processedNextTokens.Contains(token))
                {
                    continue;
                }

                var newOffsets = _grammar.Rules.Where(r => r.ResultToken == token).Select(r => new GrammarRuleOffset { Offset = 0, Rule = r }).ToList();
                var newNextTokens = newOffsets.Select(o => o.NextToken);
                foreach (var newNextToken in newNextTokens)
                {
                    if (processedNextTokens.Contains(newNextToken))
                    {
                        continue;
                    }
                    nextTokensQueue.Enqueue(newNextToken);
                }

                result.UnionWith(newOffsets);
                processedNextTokens.Add(token);
            }

            return result;
        }
    }
}
