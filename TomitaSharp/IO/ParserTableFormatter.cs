using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomitaSharp.BaseTypes;
using TomitaSharp.Runtime;

namespace TomitaSharp.IO
{
    public class ParserTableFormatter
    {
        StringBuilder _builder = new StringBuilder();

        public string Format(ParserTable table)
        {
            _builder.Clear();

            var allTokens = table.Grammar.GetAllUniqueTokens().OrderBy(t => t.IsTerminal ? 0 : 1).ThenBy(t => t.Value).ToList();
            FormatHeader(allTokens);
            foreach (var row in table.StateRows)
            {
                FormatState(row, allTokens);
            }

            return _builder.ToString();
        }

        private void AppendCell(string value, int width = 6)
        {
            _builder.Append(value.PadLeft(width));
            _builder.Append(" |");
        }

        private void AppendSeparator()
        {
            _builder.Append("|");
        }

        private void CompleteRow()
        {
            _builder.AppendLine();
        }

        private void FormatHeader(IReadOnlyList<GrammarRuleToken> allTokens)
        {
            AppendCell("State");

            foreach (var t in allTokens.Where(t => t.IsTerminal))
            {
                AppendCell(t.ToString());
            }

            AppendSeparator();
            AppendCell("Reductions", 10);
            AppendSeparator();

            foreach (var t in allTokens.Where(t => !t.IsTerminal))
            {
                AppendCell(t.ToString());
            }

            CompleteRow();
        }

        private void FormatState(ParserTableRow row, IReadOnlyList<GrammarRuleToken> allTokens)
        {
            AppendCell(row.StateId.ToString());

            foreach (var t in allTokens.Where(t => t.IsTerminal))
            {
                if (row.ShiftActionsMap.TryGetValue(t, out int nextState))
                {
                    AppendCell("s" + nextState.ToString());
                }
                else
                {
                    AppendCell(string.Empty);
                }
            }

            AppendSeparator();
            AppendCell(string.Join(",", row.ReduceRulesIds.Select(r => "r" + r.ToString())), 10);
            AppendSeparator();

            foreach (var t in allTokens.Where(t => !t.IsTerminal))
            {
                if (row.ShiftActionsMap.TryGetValue(t, out int nextState))
                {
                    AppendCell(nextState.ToString());
                }
                else
                {
                    AppendCell(string.Empty);
                }
            }

            CompleteRow();
        }
    }
}
