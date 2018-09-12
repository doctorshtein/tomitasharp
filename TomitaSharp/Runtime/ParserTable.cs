using System.Collections.Generic;
using TomitaSharp.BaseTypes;

namespace TomitaSharp.Runtime
{
    public class ParserTable
    {
        public Grammar Grammar { get; internal set; }

        public List<ParserTableRow> StateRows { get; internal set; }

        public ParserTableRow this[int id] => StateRows[id];
    }
}
