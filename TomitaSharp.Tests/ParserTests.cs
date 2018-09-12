using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TomitaSharp.BaseTypes;
using TomitaSharp.Generation;
using TomitaSharp.IO;
using TomitaSharp.Runtime;

namespace TomitaSharp.Tests
{
    [TestClass]
    public class ParserTests
    {
        private static readonly string c_skipGrammar = @"
True !NOMATCH!
TimeStart TimeEnd
TimeStart → at TIME
TimeStart → from TIME
TimeEnd → to TIME
TimeEnd → till TIME
TIME → 9 am
TIME → 5 pm";

        private static readonly string c_noSkipGrammar = @"
False
TimeStart TimeEnd
TimeStart → at TIME
TimeStart → from TIME
TimeEnd → to TIME
TimeEnd → till TIME
TIME → 9 am
TIME → 5 pm";

        [TestMethod]
        public void NoSkipGramarParseTest()
        {
            // get non-skip parse table
            Grammar grammar = GrammarParser.Parse(c_noSkipGrammar);
            var parseTable = ParserTableConstructor.Construct(grammar);
            var grammarStr = string.Join(Environment.NewLine, grammar.Rules);
            var tableStr = new ParserTableFormatter().Format(parseTable);
            Console.Write(grammarStr);
            Console.WriteLine();
            Console.Write(tableStr);
            Console.WriteLine();
            Console.WriteLine();

            var noSkipParser = new Parser(parseTable);
            noSkipParser.Parse("from 9 am".Split(' '));
            noSkipParser.Parse("begin from 9 am".Split(' '));
            noSkipParser.Parse("from 9 am finish".Split(' '));
        }

        [TestMethod]
        public void SkipGramarParseTest()
        {
            // get skip parse table
            Grammar skipGrammar = GrammarParser.Parse(c_skipGrammar);
            var skipParseTable = ParserTableConstructor.Construct(skipGrammar);
            var skipGrammarStr = string.Join(Environment.NewLine, skipGrammar.Rules);
            var skipTableStr = new ParserTableFormatter().Format(skipParseTable);
            Console.Write(skipGrammarStr);
            Console.WriteLine();
            Console.Write(skipTableStr);
            Console.WriteLine();
            Console.WriteLine();

            var skipParser = new Parser(skipParseTable);
            skipParser.Parse("till 5 pm".Split(' '));
            skipParser.Parse("begin at 9 am and run to 5 pm nonstop".Split(' '));
        }
    }
}
